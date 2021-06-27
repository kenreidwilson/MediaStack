using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models.Requests;
using MediaStack_API.Models.Responses;
using MediaStack_API.Models.ViewModels;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediaStack_API.Controllers
{
    [EnableCors]
    [Route("/[controller]")]
    public class CategoriesController : Controller
    {
        #region Properties

        protected IUnitOfWorkService UnitOfWorkService { get; }

        protected IMapper Mapper { get; }

        #endregion

        #region Constructors

        public CategoriesController(IUnitOfWorkService unitOfWorkService, IMapper mapper)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.Mapper = mapper;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Read([FromQuery] CategoryViewModel potentialCategory)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var category = await unitOfWork.Categories
                                               .Get()
                                               .FirstOrDefaultAsync(c => c.ID == potentialCategory.ID);

                if (category == null)
                {
                    return NotFound();
                }

                return Ok(new BaseResponse(this.Mapper.Map<CategoryViewModel>(category)));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] CategoryViewModel category)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid || category.ID != 0)
                {
                    return BadRequest();
                }

                if (await unitOfWork.Categories.Get().AnyAsync(c => c.Name == category.Name))
                {
                    return BadRequest(new BaseResponse(null, "Duplicate."));
                }

                await unitOfWork.Categories.InsertAsync(this.Mapper.Map<Category>(category));
                await unitOfWork.SaveAsync();
                var createdCategory = await unitOfWork.Categories.Get(c => c.Name == category.Name).FirstAsync();
                return Ok(new BaseResponse(this.Mapper.Map<CategoryViewModel>(createdCategory)));
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] CategorySearchQuery categoryQuery)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var query = categoryQuery.GetQuery(unitOfWork);
                var total = await query.CountAsync();
                var categories = await query
                                 .Skip(categoryQuery.Offset)
                                 .Take(categoryQuery.Count)
                                 .Select(t => this.Mapper.Map<CategoryViewModel>(t))
                                 .ToListAsync();

                return Ok(new CategorySearchResponse(categories, categoryQuery.Offset, categoryQuery.Count, total));
            }
        }

        #endregion
    }
}