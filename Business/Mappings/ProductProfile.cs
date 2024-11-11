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
                .ForMember(dest => dest.Platform, opt => opt.MapFrom(src => src.Platform.ToString()))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating.ToString()))
                .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => src.Genre))
                .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => src.Logo))
                .ForMember(dest => dest.Background, opt => opt.MapFrom(src => src.Background))
                .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count));

            CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.DateCreated, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.TotalRating, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.Logo, opt => opt.MapFrom(src => src.Logo))
            .ForMember(dest => dest.Background, opt => opt.MapFrom(src => src.Background));

            CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.DateCreated, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.Condition(src => src.Name != null))
            .ForMember(dest => dest.Platform, opt => opt.Condition(src => src.Platform != null))
            .ForMember(dest => dest.Price, opt => opt.Condition(src => src.Price != null))
            .ForMember(dest => dest.Genre, opt => opt.Condition(src => src.Genre != null))
            .ForMember(dest => dest.Rating, opt => opt.Condition(src => src.Rating != null))
            .ForMember(dest => dest.Count, opt => opt.Condition(src => src.Count != null))
            .ForMember(dest => dest.Logo, opt => opt.Condition(src => !string.IsNullOrEmpty(src.LogoUrl)))
            .ForMember(dest => dest.Background, opt => opt.Condition(src => !string.IsNullOrEmpty(src.BackgroundUrl)));
        }
    }
}
