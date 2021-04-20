using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class MediaController : Controller
    {
        protected IUnitOfWorkService UnitOfWorkService { get; }

        public MediaController(IUnitOfWorkService unitOfWorkService)
        {
            this.UnitOfWorkService = unitOfWorkService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                return Ok(await unitOfWork.Media.Get()
                    .Where(media => media.Path != null)
                    .Include(media => media.Tags)
                    .ToListAsync());
            }
        }
    }
}
