using AutoMapper;
using DAL.Entities;
using Shared.DTOs;

namespace Business.Mappings
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, SearchResultDto>()
                .ForMember(dest => dest.Platform, opt => opt.MapFrom(src => src.Platform.ToString()));
        }
    }
}
