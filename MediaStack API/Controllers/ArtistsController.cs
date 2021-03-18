using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class ArtistsController : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            using (var unitOfWork = new UnitOfWork<MediaStackContext>())
            {
                return Ok(await unitOfWork.Artists.Get().ToListAsync());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            using (var unitOfWork = new UnitOfWork<MediaStackContext>())
            {
                Artist artist = unitOfWork.Artists.Get()
                    .Where(artist => artist.ID == id)
                    .FirstOrDefault();

                if (artist == null)
                {
                    return NotFound();
                }
                return Ok(artist);
            }
        }
    }
}
