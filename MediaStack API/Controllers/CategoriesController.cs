﻿using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models;
using MediaStack_API.Responses;
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
                return Ok(await unitOfWork.Categories.Get().ToListAsync());
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

                return Ok(category);
            }
        }

        [HttpPost]
        public IActionResult Index([FromBody] Category category)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (unitOfWork.Categories.Get().Any(c => c.Name == category.Name))
                {
                    return BadRequest(new ResponseWrapper(null, "Duplicate."));
                }

                unitOfWork.Categories.Insert(category);
                unitOfWork.Save();
                var createdCategory = unitOfWork.Categories.Get(c => c.Name == category.Name).First();
                return Ok(new ResponseWrapper(this.Mapper.Map<CategoryViewModel>(createdCategory)));
            }
        }

        #endregion
    }
}