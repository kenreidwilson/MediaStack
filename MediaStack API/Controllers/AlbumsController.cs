using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;

namespace MediaStack_API.Controllers
{
    [Route("/[controller]")]
    public class AlbumsController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            using (var unitOfWork = new UnitOfWork<MediaStackContext>())
            {
                var albums = unitOfWork.Albums.Get();
                return Ok(await albums.ToListAsync());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            using (var unitOfWork = new UnitOfWork<MediaStackContext>())
            {
                Album album = unitOfWork.Albums.Get()
                    .Where(album => album.ID == id)
                    .FirstOrDefault();

                if (album == null)
                {
                    return NotFound();
                }
                return Ok(album);
            }
        }
    }
}
