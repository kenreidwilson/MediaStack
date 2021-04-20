using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class TagsController : Controller
    {
        protected IUnitOfWorkService UnitOfWorkService { get; }

        public TagsController(IUnitOfWorkService unitOfWorkService)
        {
            this.UnitOfWorkService = unitOfWorkService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                return Ok(await unitOfWork.Tags.Get().ToListAsync());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                Tag tag = unitOfWork.Tags
                                    .Get()
                                    .FirstOrDefault(t => t.ID == id);

                if (tag == null)
                {
                    return NotFound();
                }
                return Ok(tag);
            }
        }
    }
}
