using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models;
using MediaStack_API.Responses;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediaStack_API.Controllers
{
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
                return Ok(await unitOfWork.Artists.Get().Select(a => this.Mapper.Map<ArtistViewModel>(a)).ToListAsync());
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

                return Ok(this.Mapper.Map<ArtistViewModel>(artist));
            }
        }

        [HttpPost]
        public IActionResult Index([FromBody] Artist artist)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (unitOfWork.Artists.Get().Any(a => a.Name == artist.Name))
                {
                    return BadRequest(new ResponseWrapper(null, "Duplicate."));
                }

                unitOfWork.Artists.Insert(artist);
                unitOfWork.Save();
                var createdArtist = unitOfWork.Artists.Get(t => t.Name == artist.Name).First();
                return Ok(new ResponseWrapper(this.Mapper.Map<ArtistViewModel>(createdArtist)));
            }
        }

        #endregion
    }
}