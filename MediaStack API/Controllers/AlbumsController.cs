using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models.Requests;
using MediaStack_API.Models.Responses;
using MediaStack_API.Models.ViewModels;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediaStack_API.Controllers
{
    [EnableCors]
    [Route("/[controller]")]
    public class AlbumsController : Controller
    {
        #region Properties

        protected IUnitOfWorkService UnitOfWorkService { get; }

        protected IMapper Mapper { get; }

        #endregion

        #region Constructors

        public AlbumsController(IUnitOfWorkService unitOfWorkService, IMapper mapper)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.Mapper = mapper;
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<IActionResult> Read([FromQuery] AlbumViewModel potentialAlbum)
        {
            if (potentialAlbum.ID == 0)
            {
                return BadRequest("You must provide a valid Album ID.");
            }

            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var album = await unitOfWork.Albums
                                            .Get()
                                            .FirstOrDefaultAsync(a => a.ID == potentialAlbum.ID);

                if (album == null)
                {
                    return NotFound();
                }

                return Ok(new BaseResponse(this.Mapper.Map<AlbumViewModel>(album)));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] AlbumViewModel album)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid || album.ID != 0 || !unitOfWork.Artists.Get().Any(a => a.ID == album.ArtistID))
                {
                    return BadRequest();
                }

                if (unitOfWork.Albums.Get().Any(a => a.ArtistID == album.ArtistID && a.Name == album.Name))
                {
                    return BadRequest(new BaseResponse(null, "Duplicate."));
                }

                await unitOfWork.Albums.InsertAsync(this.Mapper.Map<Album>(album));
                await unitOfWork.SaveAsync();
                var createdAlbum = unitOfWork.Albums.Get(a => a.ArtistID == album.ArtistID && a.Name == album.Name)
                                             .First();
                return Ok(new BaseResponse(this.Mapper.Map<AlbumViewModel>(createdAlbum)));
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] AlbumSearchQuery albumQuery)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var query = albumQuery.GetQuery(unitOfWork);
                var total = await query.CountAsync();
                var tags = await query
                                 .Skip(albumQuery.Offset)
                                 .Take(albumQuery.Count)
                                 .Select(t => this.Mapper.Map<AlbumViewModel>(t))
                                 .ToListAsync();

                return Ok(new AlbumsSearchResponse(tags, albumQuery.Offset, albumQuery.Count, total));
            }
        }

        [HttpPut("Sort")]
        public async Task<IActionResult> Sort([FromQuery] AlbumSortRequest request)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid || !unitOfWork.Albums.Get().Any(a => a.ID == request.AlbumID))
                {
                    return BadRequest();
                }

                return Ok(new BaseResponse(
                    this.Mapper.Map<AlbumViewModel>(await request.SortRequestedAlbum(unitOfWork))));
            }
        }

        #endregion
    }
}
