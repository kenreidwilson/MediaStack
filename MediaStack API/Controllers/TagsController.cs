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
    public class TagsController : Controller
    {
        #region Properties

        protected IUnitOfWorkService UnitOfWorkService { get; }

        protected IMapper Mapper { get; }

        #endregion

        #region Constructors

        public TagsController(IUnitOfWorkService unitOfWorkService, IMapper mapper)
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
                return Ok(await unitOfWork.Tags.Get().Select(t => this.Mapper.Map<TagViewModel>(t)).ToListAsync());
            }
        }

        [HttpPost]
        public IActionResult Index([FromBody] TagViewModel tag)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid || tag.ID != 0)
                {
                    return BadRequest();
                }

                if (unitOfWork.Tags.Get().Any(t => t.Name == tag.Name))
                {
                    return BadRequest(new BaseResponse(null, "Duplicate."));
                }

                unitOfWork.Tags.Insert(this.Mapper.Map<Tag>(tag));
                unitOfWork.Save();
                var createdTag = unitOfWork.Tags.Get(t => t.Name == tag.Name).First();
                return Ok(new BaseResponse(this.Mapper.Map<TagViewModel>(createdTag)));
            }
        }

        [HttpGet("{id}")]
        public IActionResult Details(int id)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var tag = unitOfWork.Tags
                                    .Get()
                                    .FirstOrDefault(t => t.ID == id);

                if (tag == null)
                {
                    return NotFound();
                }

                return Ok(this.Mapper.Map<TagViewModel>(tag));
            }
        }

        #endregion
    }
}