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

        [EnableCors("_myAllowSpecificOrigins")]
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] MediaSearchQuery searchQuery)
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

        [HttpGet("{id}")]
        public IActionResult Info(int id)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var media = unitOfWork.Media
                                      .Get()
                                      .Include(m => m.Tags)
                                      .FirstOrDefault(m => m.ID == id);

                if (media == null)
                {
                    return NotFound();
                }

                return Ok(new BaseResponse(this.Mapper.Map<MediaViewModel>(media)));
            }
        }

        [EnableCors("_myAllowSpecificOrigins")]
        [HttpPut("{id}/Edit")]
        public IActionResult Edit([FromBody] MediaEditRequest editRequest, int id)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                Media media = unitOfWork.Media.Get().Include(m => m.Tags).FirstOrDefault(m => m.ID == id);
                if (media == null)
                {
                    return NotFound();
                }

                Media newMedia;
                try
                {
                    newMedia = editRequest.UpdateMedia(unitOfWork, media);
                }
                catch (MediaEditRequest.BadRequestException)
                {
                    return BadRequest();
                }
                unitOfWork.Save();
                return Ok(new BaseResponse(this.Mapper.Map<MediaViewModel>(newMedia)));
            }
        }

        [HttpGet("{id}/File")]
        public IActionResult File(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var media = unitOfWork.Media.Get(m => m.ID == id).FirstOrDefault();
                if (media == null)
                {
                    return NotFound();
                }


                try
                {
                    return File(this.GetMediaImageBytes(media), media.Type == MediaType.Video ? "video/mp4" : "image/png");
                }
                catch (Exception)
                {
                    return StatusCode(500);
                }
            }
        }

        [HttpGet("{id}/Thumbnail")]
        public async Task<IActionResult> Thumbnail(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var media = unitOfWork.Media.Get(m => m.ID == id).FirstOrDefault();
                if (media == null)
                {
                    return NotFound();
                }

                try
                {
                    if (this.Thumbnailer.HasThumbnail(media) || await this.Thumbnailer.CreateThumbnail(media))
                    {
                        return File(this.GetMediaThumbnailBytes(media), "image/png");
                    }

                    return File(this.GetDefaultThumbnailBytes(), "image/png");
                }
                catch (Exception)
                {
                    return StatusCode(500);
                }
            }
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest(new BaseResponse(null, "No File"));
            }
            Media media = new Media();
            try
            {
                using (var stream = file.OpenReadStream())
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
                if (unitOfWork.Media.Get().Any(m => m.Hash == media.Hash))
                {
                    return BadRequest(new BaseResponse(null, "Duplicate"));
                }

                string filePath = $"{this.FSController.MediaDirectory}{media.Hash}";
                while (System.IO.File.Exists(filePath))
                {
                    filePath += "_";
                }

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                unitOfWork.Media.Insert(media);
                unitOfWork.Save();
            }

            return Ok(media);
        }

        protected byte[] GetMediaImageBytes(Media media)
        {
            return this.readFile(this.FSController.GetMediaFullPath(media));
        }

        protected byte[] GetMediaThumbnailBytes(Media media)
        {
            return this.readFile(this.Thumbnailer.GetThumbnailFullPath(media));
        }

        protected byte[] GetDefaultThumbnailBytes()
        {
            return this.readFile(this.Thumbnailer.GetDefaultThumbnailFullPath());
        }

        private byte[] readFile(string filePath)
        {
            return System.IO.File.ReadAllBytes(filePath);
        }

        #endregion
    }
}