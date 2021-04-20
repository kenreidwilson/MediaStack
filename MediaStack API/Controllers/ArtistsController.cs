using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class ArtistsController : Controller
    {
        protected IUnitOfWorkService UnitOfWorkService { get; }

        public ArtistsController(IUnitOfWorkService unitOfWorkService)
        {
            this.UnitOfWorkService = unitOfWorkService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                return Ok(await unitOfWork.Artists.Get().ToListAsync());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                Artist artist = unitOfWork.Artists
                                          .Get()
                                          .FirstOrDefault(a => a.ID == id);

                if (artist == null)
                {
                    return NotFound();
                }
                return Ok(artist);
            }
        }
    }
}
