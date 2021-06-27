using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models.Requests;
using MediaStack_API.Models.Responses;
using MediaStack_API.Models.ViewModels;
using MediaStack_API.Services.Thumbnailer;
using MediaStackCore.Controllers;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
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
        #region Properties

        protected IMediaFileSystemController FSController { get; }

        protected IUnitOfWorkService UnitOfWorkService { get; }

        protected IMapper Mapper { get; }

        protected IThumbnailer Thumbnailer { get; }

        #endregion

        #region Constructors

        public MediaController(IUnitOfWorkService unitOfWorkService,
            IMapper mapper,
            IMediaFileSystemController fsController,
            IThumbnailer thumbnailer)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.Mapper = mapper;
            this.FSController = fsController;
            this.Thumbnailer = thumbnailer;
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<IActionResult> Read([FromQuery] MediaViewModel potentialMedia)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
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
            Media media = new Media();
            try
            {
                await using (var stream = file.OpenReadStream())
                {
                    media.Hash = this.FSController.CalculateHash(stream);
                    stream.Position = 0;
                    media.Type = this.FSController.DetermineMediaType(stream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(500);
            }

            if (media.Type == null)
            {
                return BadRequest(new BaseResponse(null, "Invalid File Type"));
            }

            using (IUnitOfWork unitOfWork = this.UnitOfWorkService.Create())
            {
                if (await unitOfWork.Media.Get().AnyAsync(m => m.Hash == media.Hash))
                {
                    return BadRequest(new BaseResponse(null, "Duplicate"));
                }

                string filePath = $"{media.Hash}";
                while (System.IO.File.Exists(filePath))
                {
                    filePath += "_";
                }

                media.Path = filePath;

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                await unitOfWork.Media.InsertAsync(media);
                await unitOfWork.SaveAsync();
            }

            return Ok(new BaseResponse(this.Mapper.Map<MediaViewModel>(media)));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] MediaEditRequest editRequest)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                Media newMedia;
                try
                {
                    newMedia = await editRequest.UpdateMedia(unitOfWork);
                }
                catch (MediaEditRequest.BadRequestException)
                {
                    return BadRequest();
                }
                catch (MediaEditRequest.MediaNotFoundException)
                {
                    return NotFound();
                }

                unitOfWork.Media.Update(newMedia);
                await unitOfWork.SaveAsync();

                /**
                if (editRequest.CategoryID != null || editRequest.ArtistID != null || editRequest.AlbumID != null)
                {
                    newMedia.Path = this.FSController.MoveMedia(
                        unitOfWork.Media.Get()
                                  .Include(m => m.Category)
                                  .Include(m => m.Artist)
                                  .Include(m => m.Album)
                                  .First(m => m.ID == newMedia.ID)
                    ).Substring(this.FSController.MediaDirectory.Length);
                }

                unitOfWork.Media.Update(newMedia);
                unitOfWork.Save();
                */

                return Ok(new BaseResponse(this.Mapper.Map<MediaViewModel>(newMedia)));
            }
        }

        [HttpPost("Search")]
        public async Task<IActionResult> Search([FromBody] MediaSearchQuery searchQuery)
        {
            searchQuery ??= new MediaSearchQuery();
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var query = searchQuery.GetQuery(unitOfWork);
                var total = query.Count();
                var medias = await query
                                   .Include(m => m.Tags)
                                   .Skip(searchQuery.Offset)
                                   .Take(searchQuery.Count)
                                   .Select(m => this.Mapper.Map<MediaViewModel>(m))
                                   .ToListAsync();
                return Ok(new MediaSearchResponse(medias, searchQuery.Offset, searchQuery.Count, total));
            }
        }

        [HttpGet("File")]
        public async Task<IActionResult> File([FromQuery] MediaViewModel potentialMedia)
        {
            if (potentialMedia.ID == 0)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var media = await unitOfWork.Media.Get(m => m.ID == potentialMedia.ID).FirstOrDefaultAsync();
                if (media == null)
                {
                    return NotFound();
                }

                try
                {
                    return File(await this.GetMediaImageBytes(media), media.Type == MediaType.Video ? "video/mp4" : "image/png");
                }
                catch (Exception)
                {
                    return StatusCode(500);
                }
            }
        }

        [HttpGet("Thumbnail")]
        public async Task<IActionResult> Thumbnail([FromQuery] MediaViewModel potentialMedia)
        {
            if (potentialMedia.ID == 0)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkService.Create())
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
                        return File(await this.GetMediaThumbnailBytes(media), "image/png");
                    }

                    return File(await this.GetDefaultThumbnailBytes(), "image/png");
                }
                catch (Exception)
                {
                    return StatusCode(500);
                }
            }
        }

        protected async Task<byte[]> GetMediaImageBytes(Media media)
        {
            return await this.readFile(this.FSController.GetMediaFullPath(media));
        }

        protected async Task<byte[]> GetMediaThumbnailBytes(Media media)
        {
            return await this.readFile(this.Thumbnailer.GetThumbnailFullPath(media));
        }

        protected async Task<byte[]> GetDefaultThumbnailBytes()
        {
            return await this.readFile(this.Thumbnailer.GetDefaultThumbnailFullPath());
        }

        private async Task<byte[]> readFile(string filePath)
        {
            return await System.IO.File.ReadAllBytesAsync(filePath);
        }

        #endregion
    }
}