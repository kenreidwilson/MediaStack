using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models.Requests;
using MediaStack_API.Models.Responses;
using MediaStack_API.Models.ViewModels;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediaStack_API.Controllers
{
    [EnableCors]
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

        [HttpGet]
        public async Task<IActionResult> IndexAsync([FromQuery] TagViewModel potentialTag)
        {
            if (potentialTag.ID == 0 && potentialTag.Name == null)
            {
                return BadRequest("You must provide a Tag ID or Name.");
            }

            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                Tag tag = null;

                if (potentialTag.Name != null)
                {
                    tag = await unitOfWork.Tags
                                    .Get()
                                    .FirstOrDefaultAsync(t => t.Name == potentialTag.Name);
                }

                if (potentialTag.ID != 0)
                {
                    tag = await unitOfWork.Tags
                                    .Get()
                                    .FirstOrDefaultAsync(t => t.ID == potentialTag.ID);
                }

                if (tag == null)
                {
                    return NotFound();
                }

                return Ok(new BaseResponse(this.Mapper.Map<TagViewModel>(tag)));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] TagViewModel potentialTag)
        {
            if (potentialTag.Name == null)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid || potentialTag.ID != 0)
                {
                    return BadRequest();
                }

                if (unitOfWork.Tags.Get().Any(t => t.Name == potentialTag.Name))
                {
                    return BadRequest(new BaseResponse(null, "Duplicate."));
                }

                await unitOfWork.Tags.InsertAsync(this.Mapper.Map<Tag>(potentialTag));
                await unitOfWork.SaveAsync();
                var createdTag = unitOfWork.Tags.Get(t => t.Name == potentialTag.Name).First();
                return Ok(new BaseResponse(this.Mapper.Map<TagViewModel>(createdTag)));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromQuery] TagViewModel potentialTag)
        {
            if (potentialTag.Name == null)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var tagModel = await unitOfWork.Tags
                                    .Get()
                                    .FirstOrDefaultAsync(t => t.ID == potentialTag.ID);

                if (tagModel == null || await unitOfWork.Tags.Get().AnyAsync(t => t.Name == potentialTag.Name))
                {
                    return BadRequest();
                }

                tagModel.Name = potentialTag.Name;

                unitOfWork.Tags.Update(tagModel);
                await unitOfWork.SaveAsync();

                return Ok(new BaseResponse(this.Mapper.Map<TagViewModel>(tagModel)));
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] TagViewModel potentialTag)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var tag = await unitOfWork.Tags
                                         .Get()
                                         .FirstOrDefaultAsync(t => t.ID == potentialTag.ID);

                if (tag == null)
                {
                    return BadRequest();
                }

                unitOfWork.Tags.Delete(tag);
                await unitOfWork.SaveAsync();

                return Ok();
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] TagSearchQuery tagsQuery)
        {
            using (IUnitOfWork unitOfWork = this.UnitOfWorkService.Create())
            {
                IQueryable<Tag> query = tagsQuery.GetQuery(unitOfWork);
                int total = await query.CountAsync();
                var tags = await query
                                 .Skip(tagsQuery.Offset)
                                 .Take(tagsQuery.Count)
                                 .Select(t => this.Mapper.Map<TagViewModel>(t))
                                 .ToListAsync();

                return Ok(new TagsSearchResponse(tags, tagsQuery.Offset, tagsQuery.Count, total));
            }
        }

        #endregion
    }
}