using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models;
using MediaStack_API.Models.Requests;
using MediaStack_API.Models.Responses;
using MediaStack_API.Models.ViewModels;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class MediaController : Controller
    {
        protected IUnitOfWorkService UnitOfWorkService { get; }

        protected IMapper Mapper { get; }

        public MediaController(IUnitOfWorkService unitOfWorkService, IMapper mapper)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.Mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] MediaSearchQuery searchQuery)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                IQueryable<Media> query = searchQuery.GetQuery(unitOfWork);
                int total = query.Count();
                var response = new MediaSearchResponse(await query
                                                        .Skip(searchQuery.Offset)
                                                        .Take(searchQuery.Count)
                                                        .Select(m => this.Mapper.Map<MediaViewModel>(m))
                                                        .ToListAsync()) 
                {
                    Offset = searchQuery.Offset,
                    Count = searchQuery.Count,
                    Total = total
                };
                return Ok(response);
            }
        }
    }
}
