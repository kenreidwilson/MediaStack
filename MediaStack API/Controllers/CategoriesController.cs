using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models.Responses;
using MediaStack_API.Models.ViewModels;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediaStack_API.Controllers
{
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
        public IActionResult Index([FromBody] CategoryViewModel category)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid || category.ID != 0)
                {
                    return BadRequest();
                }

                if (unitOfWork.Categories.Get().Any(c => c.Name == category.Name))
                {
                    return BadRequest(new BaseResponse(null, "Duplicate."));
                }

                unitOfWork.Categories.Insert(this.Mapper.Map<Category>(category));
                unitOfWork.Save();
                var createdCategory = unitOfWork.Categories.Get(c => c.Name == category.Name).First();
                return Ok(new BaseResponse(this.Mapper.Map<CategoryViewModel>(createdCategory)));
            }
        }

        [HttpGet("{id}")]
        public IActionResult Details(int id)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var category = unitOfWork.Categories
                                         .Get()
                                         .FirstOrDefault(c => c.ID == id);

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