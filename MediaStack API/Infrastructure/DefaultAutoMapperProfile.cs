using AutoMapper;
using MediaStack_API.Models.ViewModels;
using MediaStackCore.Models;

namespace MediaStack_API.Infrastructure
{
    public class DefaultAutoMapperProfile : Profile
    {
        #region Constructors

        public DefaultAutoMapperProfile()
        {
            CreateMap<Album, AlbumViewModel>();
            CreateMap<AlbumViewModel, Album>();
            CreateMap<Tag, TagViewModel>();
            CreateMap<TagViewModel, Tag>();
            CreateMap<Artist, ArtistViewModel>();
            CreateMap<ArtistViewModel, Artist>();
            CreateMap<Category, CategoryViewModel>();
            CreateMap<CategoryViewModel, Category>();
            CreateMap<Media, MediaViewModel>();
            CreateMap<MediaViewModel, Media>();
        }

        #endregion
    }
}