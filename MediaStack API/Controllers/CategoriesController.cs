using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class CategoriesController : Controller
    {
        protected IUnitOfWorkService UnitOfWorkService { get; }

        public CategoriesController(IUnitOfWorkService unitOfWorkService)
        {
            this.UnitOfWorkService = unitOfWorkService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                return Ok(await unitOfWork.Categories.Get().ToListAsync());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                Category category = unitOfWork.Categories
                                              .Get()
                                              .FirstOrDefault(c => c.ID == id);

                if (category == null)
                {
                    return NotFound();
                }
                return Ok(category);
            }
        }
    }
}
