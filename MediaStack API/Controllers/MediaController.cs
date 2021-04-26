using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models.Requests;
using MediaStack_API.Models.Responses;
using MediaStack_API.Models.ViewModels;
using MediaStack_API.Services.Thumbnailer;
using MediaStackCore.Controllers;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
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

        public async Task<IActionResult> Index([FromBody] MediaSearchQuery searchQuery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var query = searchQuery.GetQuery(unitOfWork);
                var total = query.Count();
                var response = new MediaSearchResponse(await query
                                                             .Skip(searchQuery.Offset)
                                                             .Take(searchQuery.Count)
                                                             .Select(m => this.Mapper.Map<MediaViewModel>(m))
                                                             .ToListAsync()) {
                    Offset = searchQuery.Offset,
                    Count = searchQuery.Count,
                    Total = total
                };
                return Ok(response);
            }
        }

        [HttpPost("Edit")]
        public IActionResult Edit([FromBody] MediaEditRequest editRequest)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                if (!unitOfWork.Media.Get().Any(m => m.ID == editRequest.MediaID))
                {
                    return NotFound();
                }

                return Ok(new BaseResponse(editRequest.UpdateMedia(unitOfWork)));
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

                return File(this.GetMediaImageBytes(media), "image/png");
            }
        }

        [HttpGet("{id}/Thumbnail")]
        public IActionResult Thumbnail(int id)
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

                if (this.Thumbnailer.HasThumbnail(media) || this.Thumbnailer.CreateThumbnail(media))
                {
                    return File(this.GetMediaThumbnailBytes(media), "image/png");
                }

                return File(this.GetDefaultThumbnailBytes(), "image/png");
            }
        }

        protected byte[] GetMediaImageBytes(Media media)
        {
            return System.IO.File.ReadAllBytes(this.FSController.GetMediaFullPath(media));
        }

        protected byte[] GetMediaThumbnailBytes(Media media)
        {
            return System.IO.File.ReadAllBytes(this.Thumbnailer.GetThumbnailFullPath(media));
        }

        protected byte[] GetDefaultThumbnailBytes()
        {
            return System.IO.File.ReadAllBytes(this.Thumbnailer.GetDefaultThumbnailFullPath());
        }

        #endregion
    }
}