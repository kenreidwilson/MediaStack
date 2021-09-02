using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models.Requests;
using MediaStack_API.Models.Responses;
using MediaStack_API.Models.ViewModels;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkFactoryService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediaStack_API.Controllers
{
    [EnableCors]
    [Route("/[controller]")]
    public class ArtistsController : Controller
    {
        #region Properties

        protected IUnitOfWorkFactory UnitOfWorkFactory { get; }

        protected IMapper Mapper { get; }

        private static readonly object WriteLock = new();

        #endregion

        #region Constructors

        public ArtistsController(IUnitOfWorkFactory unitOfWorkFactory, IMapper mapper)
        {
            this.UnitOfWorkFactory = unitOfWorkFactory;
            this.Mapper = mapper;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Read([FromQuery] ArtistViewModel potentialArtist)
        {
            using (var unitOfWork = this.UnitOfWorkFactory.Create())
            {
                var artist = await unitOfWork.Artists
                                             .Get()
                                             .FirstOrDefaultAsync(a => a.ID == potentialArtist.ID);

                if (artist == null)
                {
                    return NotFound();
                }

                return Ok(new BaseResponse(this.Mapper.Map<ArtistViewModel>(artist)));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] ArtistViewModel artistVm)
        {
            if (!ModelState.IsValid || artistVm.ID != 0)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkFactory.Create())
            {
                Artist artist = await unitOfWork.Artists.Get().FirstOrDefaultAsync(a => a.Name == artistVm.Name);
                if (artist == null)
                {
                    lock (WriteLock)
                    {
                        unitOfWork.Artists.Insert(this.Mapper.Map<Artist>(artistVm));
                        unitOfWork.Save();
                    }
                    artist = await unitOfWork.Artists.Get(t => t.Name == artistVm.Name).FirstAsync();
                }
                return Ok(new BaseResponse(this.Mapper.Map<ArtistViewModel>(artist)));
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] ArtistSearchQuery artistQuery)
        {
            using (var unitOfWork = this.UnitOfWorkFactory.Create())
            {
                var query = artistQuery.GetQuery(unitOfWork);
                var total = await query.CountAsync();
                var artists = await query
                                       .Skip(artistQuery.Offset)
                                       .Take(artistQuery.Count)
                                       .Select(t => this.Mapper.Map<ArtistViewModel>(t))
                                       .ToListAsync();

                return Ok(new ArtistsSearchResponse(artists, artistQuery.Offset, artistQuery.Count, total));
            }
        }

        #endregion
    }
}