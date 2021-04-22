using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class AlbumsController : Controller
    {
        protected IUnitOfWorkService UnitOfWorkService { get; }

        protected IMapper Mapper { get; }

        public AlbumsController(IUnitOfWorkService unitOfWorkService, IMapper mapper)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.Mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                return Ok(await unitOfWork.Albums.Get().Select(a => this.Mapper.Map<AlbumDto>(a)).ToListAsync());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                Album album = unitOfWork.Albums
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
        public async Task<IActionResult> Index([FromBody] Album album)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (unitOfWork.Albums.Get().Any(a => a.ArtistID == album.ArtistID && a.Name == album.Name))
                {
                    return BadRequest("Duplicate.");
                }
                unitOfWork.Albums.Insert(album);
                unitOfWork.Save();
                unitOfWork.Albums.Get(a => a.ArtistID == album.ArtistID && a.Name == album.Name).First();
                return Ok(this.Mapper.Map<AlbumDto>(album));
            }
        }
    }
}
