using System.Linq;
using System.Threading.Tasks;
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

        public AlbumsController(IUnitOfWorkService unitOfWorkService)
        {
            this.UnitOfWorkService = unitOfWorkService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                return Ok(await unitOfWork.Albums.Get().ToListAsync());
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
                return Ok(album);
            }
        }
    }
}
