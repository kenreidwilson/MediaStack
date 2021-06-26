using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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

        public async Task<IActionResult> IndexAsync()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                return Ok(new BaseResponse(await unitOfWork.Artists.Get().Select(a => this.Mapper.Map<ArtistViewModel>(a)).ToListAsync()));
            }
        }

        [HttpPost]
        public IActionResult Index([FromBody] ArtistViewModel artist)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid || artist.ID != 0)
                {
                    return BadRequest();
                }

                if (unitOfWork.Artists.Get().Any(a => a.Name == artist.Name))
                {
                    return BadRequest(new BaseResponse(null, "Duplicate."));
                }

                unitOfWork.Artists.Insert(this.Mapper.Map<Artist>(artist));
                unitOfWork.Save();
                var createdArtist = unitOfWork.Artists.Get(t => t.Name == artist.Name).First();
                return Ok(new BaseResponse(this.Mapper.Map<ArtistViewModel>(createdArtist)));
            }
        }

        [HttpGet("{id}")]
        public IActionResult Details(int id)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var artist = unitOfWork.Artists
                                       .Get()
                                       .FirstOrDefault(a => a.ID == id);

                if (artist == null)
                {
                    return NotFound();
                }

                return Ok(new BaseResponse(this.Mapper.Map<ArtistViewModel>(artist)));
            }
        }

        #endregion
    }
}