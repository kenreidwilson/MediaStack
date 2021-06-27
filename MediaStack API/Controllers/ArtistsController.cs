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
    public class ArtistsController : Controller
    {
        #region Properties

        protected IUnitOfWorkService UnitOfWorkService { get; }

        protected IMapper Mapper { get; }

        #endregion

        #region Constructors

        public ArtistsController(IUnitOfWorkService unitOfWorkService, IMapper mapper)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.Mapper = mapper;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Read([FromQuery] ArtistViewModel potentialArtist)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
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
        public async Task<IActionResult> Create([FromQuery] ArtistViewModel artist)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid || artist.ID != 0)
                {
                    return BadRequest();
                }

                if (await unitOfWork.Artists.Get().AnyAsync(a => a.Name == artist.Name))
                {
                    return BadRequest(new BaseResponse(null, "Duplicate."));
                }

                await unitOfWork.Artists.InsertAsync(this.Mapper.Map<Artist>(artist));
                await unitOfWork.SaveAsync();
                var createdArtist = await unitOfWork.Artists.Get(t => t.Name == artist.Name).FirstAsync();
                return Ok(new BaseResponse(this.Mapper.Map<ArtistViewModel>(createdArtist)));
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] ArtistSearchQuery artistQuery)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
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