using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class CategoriesController : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            using (var unitOfWork = new UnitOfWork<MediaStackContext>())
            {
                return Ok(await unitOfWork.Categories.Get().ToListAsync());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            using (var unitOfWork = new UnitOfWork<MediaStackContext>())
            {
                Category category = unitOfWork.Categories.Get()
                    .Where(category => category.ID == id)
                    .FirstOrDefault();

                if (category == null)
                {
                    return NotFound();
                }
                return Ok(category);
            }
        }
    }
}
