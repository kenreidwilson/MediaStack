using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models.Requests;
using MediaStack_API.Models.Responses;
using MediaStack_API.Models.ViewModels;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class MediaController : Controller
    {
        #region Properties

        protected IUnitOfWorkService UnitOfWorkService { get; }

        protected IMapper Mapper { get; }

        #endregion

        #region Constructors

        public MediaController(IUnitOfWorkService unitOfWorkService, IMapper mapper)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.Mapper = mapper;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Index([FromBody] MediaSearchQuery searchQuery)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var query = searchQuery.GetQuery(unitOfWork);
                var total = query.Count();
                var response = new MediaSearchResponse(await query
                                                             .Skip(searchQuery.Offset)
                                                             .Take(searchQuery.Count)
                                                             .Select(m => this.Mapper.Map<MediaViewModel>(m))
                                                             .ToListAsync()) {
                    Offset = searchQuery.Offset,
                    Count = searchQuery.Count,
                    Total = total
                };
                return Ok(response);
            }
        }

        [HttpPost("Edit")]
        public IActionResult Edit([FromBody] MediaEditRequest editRequest)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                if (!unitOfWork.Media.Get().Any(m => m.ID == editRequest.MediaID))
                {
                    return NotFound();
                }

                return Ok(new BaseResponse(editRequest.UpdateMedia(unitOfWork)));
            }
        }

        #endregion
    }
}