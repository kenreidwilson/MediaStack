using AutoMapper;
using MediaStack_API.Models;
using MediaStack_API.Models.ViewModels;
using MediaStackCore.Models;

namespace MediaStack_API.Infrastructure
{
    public class DefaultAutoMapperProfile : Profile
    {
        public DefaultAutoMapperProfile()
        {
            CreateMap<Album, AlbumViewModel>();
            CreateMap<Tag, TagViewModel>();
            CreateMap<Artist, ArtistViewModel>();
            CreateMap<Category, CategoryViewModel>();
            CreateMap<Media, MediaViewModel>();
        }
    }
}
