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
        public async Task<IActionResult> Index()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                return Ok(await unitOfWork.Albums.Get().Select(a => this.Mapper.Map<AlbumDto>(a)).ToListAsync());
            }
        }

        [HttpGet("{id}")]
        public IActionResult Details(int id)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var album = unitOfWork.Albums
                                      .Get()
                                      .FirstOrDefault(a => a.ID == id);

                if (album == null)
                {
                    return NotFound();
                }

                return Ok(this.Mapper.Map<AlbumDto>(album));
            }
        }

        [HttpPost]
        public IActionResult Index([FromBody] Album album)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (unitOfWork.Albums.Get().Any(a => a.ArtistID == album.ArtistID && a.Name == album.Name))
                {
                    return BadRequest(new ResponseWrapper(null, "Duplicate."));
                }

                unitOfWork.Albums.Insert(album);
                unitOfWork.Save();
                Album createdAlbum = unitOfWork.Albums.Get(a => a.ArtistID == album.ArtistID && a.Name == album.Name).First();
                return Ok(new ResponseWrapper(this.Mapper.Map<AlbumDto>(createdAlbum)));
            }
        }

        #endregion
    }
}