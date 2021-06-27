using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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

        public async Task<IActionResult> IndexAsync()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                return Ok(new BaseResponse(await unitOfWork.Categories.Get().Select(c => this.Mapper.Map<CategoryViewModel>(c)).ToListAsync()));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] CategoryViewModel category)
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

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var category = await unitOfWork.Categories
                                         .Get()
                                         .FirstOrDefaultAsync(c => c.ID == id);

                if (category == null)
                {
                    return NotFound();
                }

                return Ok(new BaseResponse(this.Mapper.Map<CategoryViewModel>(category)));
            }
        }

        #endregion
    }
}