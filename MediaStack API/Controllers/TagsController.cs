using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class TagsController : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            using (var unitOfWork = new UnitOfWork<MediaStackContext>())
            {
                return Ok(await unitOfWork.Tags.Get().ToListAsync());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            using (var unitOfWork = new UnitOfWork<MediaStackContext>())
            {
                Tag tag = unitOfWork.Tags.Get()
                    .Where(tag => tag.ID == id)
                    .FirstOrDefault();

                if (tag == null)
                {
                    return NotFound();
                }
                return Ok(tag);
            }
        }
    }
}
