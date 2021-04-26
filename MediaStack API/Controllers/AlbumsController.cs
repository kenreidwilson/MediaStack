﻿using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediaStack_API.Models.Requests;
using MediaStack_API.Models.Responses;
using MediaStack_API.Models.ViewModels;
using MediaStackCore.Models;
using MediaStackCore.Services.UnitOfWorkService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class AlbumsController : Controller
    {
        #region Properties

        protected IUnitOfWorkService UnitOfWorkService { get; }

        protected IMapper Mapper { get; }

        #endregion

        #region Constructors

        public AlbumsController(IUnitOfWorkService unitOfWorkService, IMapper mapper)
        {
            this.UnitOfWorkService = unitOfWorkService;
            this.Mapper = mapper;
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                return Ok(await unitOfWork.Albums.Get().Select(a => this.Mapper.Map<AlbumViewModel>(a)).ToListAsync());
            }
        }

        [HttpPost]
        public IActionResult Index([FromBody] AlbumViewModel album)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid || album.ID != 0 || !unitOfWork.Artists.Get().Any(a => a.ID == album.ArtistID))
                {
                    return BadRequest();
                }

                if (unitOfWork.Albums.Get().Any(a => a.ArtistID == album.ArtistID && a.Name == album.Name))
                {
                    return BadRequest(new BaseResponse(null, "Duplicate."));
                }

                unitOfWork.Albums.Insert(this.Mapper.Map<Album>(album));
                unitOfWork.Save();
                Album createdAlbum = unitOfWork.Albums.Get(a => a.ArtistID == album.ArtistID && a.Name == album.Name).First();
                return Ok(new BaseResponse(this.Mapper.Map<AlbumViewModel>(createdAlbum)));
            }
        }

        [HttpGet("{id}")]
        public IActionResult Details(int id)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                var album = unitOfWork.Albums
                                      .Get()
                                      .FirstOrDefault(a => a.ID == id);

                if (album == null)
                {
                    return NotFound();
                }

                return Ok(this.Mapper.Map<AlbumViewModel>(album));
            }
        }

        [HttpPost("Sort")]
        public IActionResult Sort([FromBody] AlbumSortRequest request)
        {
            using (var unitOfWork = this.UnitOfWorkService.Create())
            {
                if (!ModelState.IsValid || !unitOfWork.Albums.Get().Any(a => a.ID == request.AlbumID))
                {
                    return BadRequest();
                }

                return Ok(this.Mapper.Map<AlbumViewModel>(request.SortRequestedAlbum(unitOfWork)));
            }
        }

        #endregion
    }
}