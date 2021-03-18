using MediaStack_Library.Data_Access_Layer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class MediaController : Controller
    {
        public async Task<IActionResult> IndexAsync()
        {
            using (var unitOfWork = new UnitOfWork<MediaStackContext>())
            {
                return Ok(await unitOfWork.Media.Get()
                    .Where(media => media.Path != null)
                    .Include(media => media.Tags)
                    .ToListAsync());
            }
        }
    }
}
