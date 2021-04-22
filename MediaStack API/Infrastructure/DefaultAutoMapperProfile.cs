using AutoMapper;
using MediaStack_API.Models;
using MediaStackCore.Models;

namespace MediaStack_API.Infrastructure
{
    public class DefaultAutoMapperProfile : Profile
    {
        public DefaultAutoMapperProfile()
        {
            CreateMap<Album, AlbumDto>();

        }
    }
}
