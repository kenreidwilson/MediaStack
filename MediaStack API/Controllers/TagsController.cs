using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models.Requests;
using MediaStack_API.Models.Responses;
using MediaStack_API.Models.ViewModels;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkFactoryService;
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

        protected IUnitOfWorkFactory UnitOfWorkFactory { get; }

        protected IMapper Mapper { get; }

        private static readonly object WriteLock = new();

        #endregion

        #region Constructors

        public TagsController(IUnitOfWorkFactory unitOfWorkFactory, IMapper mapper)
        {
            this.UnitOfWorkFactory = unitOfWorkFactory;
            this.Mapper = mapper;
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<IActionResult> Read([FromQuery] TagViewModel tagVm)
        {
            if (tagVm.ID == 0 && tagVm.Name == null)
            {
                return BadRequest(new BaseResponse(null, "You must provide a valid Tag ID or Name."));
            }

            using (var unitOfWork = this.UnitOfWorkFactory.Create())
            {
                Tag tag = await unitOfWork.Tags
                                          .Get()
                                          .FirstOrDefaultAsync(t => t.ID == tagVm.ID || t.Name == tagVm.Name);

                if (tag == null)
                {
                    return BadRequest(new BaseResponse(null, "Not Found"));
                }

                return Ok(new BaseResponse(this.Mapper.Map<TagViewModel>(tag)));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] TagViewModel tagVm)
        {
            if (!ModelState.IsValid || tagVm.ID != 0)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkFactory.Create())
            {
                Tag tag = await unitOfWork.Tags.Get().FirstOrDefaultAsync(t => t.Name == tagVm.Name);

                if (tag == null)
                {
                    lock (WriteLock)
                    {
                        unitOfWork.Tags.Insert(this.Mapper.Map<Tag>(tagVm));
                        unitOfWork.Save();
                    }
                    tag = await unitOfWork.Tags.Get(t => t.Name == tagVm.Name).FirstAsync();
                }

                return Ok(new BaseResponse(this.Mapper.Map<TagViewModel>(tag)));
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromQuery] TagViewModel potentialTag)
        {
            if (potentialTag.Name == null)
            {
                return BadRequest();
            }

            using (var unitOfWork = this.UnitOfWorkFactory.Create())
            {
                var tagModel = await unitOfWork.Tags
                                    .Get()
                                    .FirstOrDefaultAsync(t => t.ID == potentialTag.ID);

                lock (WriteLock)
                {
                    if (tagModel == null || unitOfWork.Tags.Get().Any(t => t.Name == potentialTag.Name))
                    {
                        return BadRequest();
                    }

                    tagModel.Name = potentialTag.Name;

                    unitOfWork.Tags.Update(tagModel);
                    unitOfWork.Save();
                }
                
                return Ok(new BaseResponse(this.Mapper.Map<TagViewModel>(tagModel)));
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] TagViewModel potentialTag)
        {
            using (var unitOfWork = this.UnitOfWorkFactory.Create())
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
            using (IUnitOfWork unitOfWork = this.UnitOfWorkFactory.Create())
            {
                IQueryable<Tag> query = tagsQuery.GetQuery(unitOfWork);
                int total = await query.CountAsync();
                var tags = await query
                                 .Skip(tagsQuery.Offset)
                                 .Take(tagsQuery.Count)
                                 .Select(t => this.Mapper.Map<TagViewModel>(t))
                                 .ToListAsync();

                return Ok(new SearchResponse<TagViewModel>(tags, tagsQuery.Offset, tagsQuery.Count, total));
            }
        }

        #endregion
    }
}