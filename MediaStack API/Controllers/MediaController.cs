﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models.Requests;
using MediaStack_API.Models.Responses;
using MediaStack_API.Models.ViewModels;
using MediaStack_API.Services.Thumbnailer;
using MediaStackCore.Models;
using MediaStackCore.Services.MediaFilesService;
using MediaStackCore.Services.MediaService;
using MediaStackCore.Services.UnitOfWorkFactoryService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediaStack_API.Controllers
{
    [EnableCors]
    [Route("/[controller]")]
    public class MediaController : Controller
    {
        #region Data members

        private static readonly object WriteLock = new();

        protected readonly bool MoveFile = true; //TODO: Pull from env variable.

        #endregion

        #region Properties

        protected IUnitOfWorkFactory UnitOfWorkFactory { get; }

        protected IMapper Mapper { get; }

        protected IMediaService MediaService { get; }

        protected IMediaFilesService MediaFilesService { get; }

        protected IThumbnailer Thumbnailer { get; }

        #endregion

        #region Constructors

        public MediaController(IUnitOfWorkFactory unitOfWorkFactory,
            IMapper mapper,
            IMediaService mediaService,
            IMediaFilesService mediaFilesService,
            IThumbnailer thumbnailer)
        {
            this.UnitOfWorkFactory = unitOfWorkFactory;
            this.Mapper = mapper;
            this.MediaService = mediaService;
            this.MediaFilesService = mediaFilesService;
            this.Thumbnailer = thumbnailer;
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<IActionResult> Read([FromQuery] MediaViewModel potentialMedia)
        {
            using (var unitOfWork = this.UnitOfWorkFactory.Create())
            {
                var media = await unitOfWork.Media
                                            .Get()
                                            .Include(m => m.Tags)
                                            .FirstOrDefaultAsync(m => m.ID == potentialMedia.ID);

                if (media == null)
                {
                    return NotFound();
                }

                return Ok(new BaseResponse(this.Mapper.Map<MediaViewModel>(media)));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest(new BaseResponse(null, "No File"));
            }

            Media media;
            using (var unitOfWork = this.UnitOfWorkFactory.Create())
            {
                await using (var stream = file.OpenReadStream())
                {
                    media = await this.MediaService.WriteStreamAndCreateMediaAsync(stream);
                }

                lock (WriteLock)
                {
                    var potentialDuplicateMedia = unitOfWork.Media.Get().FirstOrDefault(m => m.Hash == media.Hash);
                    if (potentialDuplicateMedia != null)
                    {
                        this.MediaFilesService.DeleteMediaFile(media);
                        return Ok(new BaseResponse(this.Mapper.Map<MediaViewModel>(potentialDuplicateMedia)));
                    }

                    unitOfWork.Media.Insert(media);
                    unitOfWork.Save();
                }
            }

            return Ok(new BaseResponse(this.Mapper.Map<MediaViewModel>(media)));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] MediaEditRequest editRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Media updateMedia;
            using (var unitOfWork = this.UnitOfWorkFactory.Create())
            {
                try
                {
                    updateMedia = await editRequest.UpdateMedia(unitOfWork);
                }
                catch (MediaEditRequest.BadRequestException)
                {
                    return BadRequest();
                }
                catch (MediaEditRequest.MediaNotFoundException)
                {
                    return NotFound();
                }

                if (this.MoveFile &&
                    (editRequest.CategoryID != 0 || editRequest.ArtistID != 0 || editRequest.AlbumID != 0))
                {
                    updateMedia.Category ??= updateMedia.CategoryID == null
                        ? null
                        : unitOfWork.Categories.Get().First(c => c.ID == updateMedia.CategoryID);
                    updateMedia.Artist ??= updateMedia.ArtistID == null
                        ? null
                        : unitOfWork.Artists.Get().First(a => a.ID == updateMedia.ArtistID);
                    updateMedia.Album ??= updateMedia.AlbumID == null
                        ? null
                        : unitOfWork.Albums.Get().First(a => a.ID == updateMedia.AlbumID);
                    updateMedia.Path =
                        this.MediaFilesService.GetRelativePath(
                            this.MediaFilesService.MoveMediaFileToProperLocation(updateMedia));
                }

                unitOfWork.Media.Update(updateMedia);
                await unitOfWork.SaveAsync();
            }

            return Ok(new BaseResponse(this.Mapper.Map<MediaViewModel>(updateMedia)));
        }

        [HttpPost("Search")]
        public async Task<IActionResult> Search([FromBody] MediaSearchQuery searchQuery)
        {
            searchQuery ??= new MediaSearchQuery();
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkFactory.Create())
            {
                var query = searchQuery.GetQuery(unitOfWork);
                var total = query.Count();
                var medias = await query
                                   .Include(m => m.Tags)
                                   .Skip(searchQuery.Offset)
                                   .Take(searchQuery.Count)
                                   .Select(m => this.Mapper.Map<MediaViewModel>(m))
                                   .ToListAsync();
                return Ok(new SearchResponse<MediaViewModel>(medias, searchQuery.Offset, searchQuery.Count, total));
            }
        }

        [HttpGet("File")]
        public async Task<IActionResult> File([FromQuery] MediaViewModel potentialMedia)
        {
            if (potentialMedia.ID == 0)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkFactory.Create())
            {
                var media = await unitOfWork.Media.Get(m => m.ID == potentialMedia.ID).FirstOrDefaultAsync();
                if (media == null)
                {
                    return NotFound();
                }

                return File(this.MediaFilesService.GetMediaFileInfo(media).OpenRead(),
                    media.Type == MediaType.Video ? "video/mp4" : "image/png");
            }
        }

        [HttpGet("Thumbnail")]
        public async Task<IActionResult> Thumbnail([FromQuery] MediaViewModel potentialMedia)
        {
            if (potentialMedia.ID == 0)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkFactory.Create())
            {
                var media = await unitOfWork.Media.Get(m => m.ID == potentialMedia.ID).FirstOrDefaultAsync();
                if (media == null)
                {
                    return NotFound();
                }

                try
                {
                    if (this.Thumbnailer.HasThumbnail(media) || await this.Thumbnailer.CreateThumbnail(media))
                    {
                        return File(await this.Thumbnailer.GetMediaThumbnailBytesAsync(media), "image/png");
                    }

                    return File(await this.Thumbnailer.GetDefaultThumbnailBytesAsync(), "image/png");
                }
                catch (Exception)
                {
                    return StatusCode(500);
                }
            }
        }

        #endregion
    }
}