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

        private static readonly object WriteLock = new();

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
        public async Task<IActionResult> Create([FromQuery] AlbumViewModel albumVm)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid || albumVm.ID != 0 || !await unitOfWork.Artists.Get().AnyAsync(a => a.ID == albumVm.ArtistID))
                {
                    return BadRequest();
                }

                Album album;
                lock (WriteLock)
                {
                    album = unitOfWork.Albums.Get()
                                                  .FirstOrDefault(a => a.ArtistID == albumVm.ArtistID && a.Name == albumVm.Name);
                    if (album == null)
                    {
                        unitOfWork.Albums.Insert(this.Mapper.Map<Album>(albumVm));
                        unitOfWork.Save();
                        album = unitOfWork.Albums
                                                .Get(a => a.ArtistID == albumVm.ArtistID && a.Name == albumVm.Name)
                                                .First();
                    }
                }
                

                return Ok(new BaseResponse(this.Mapper.Map<AlbumViewModel>(album)));
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
