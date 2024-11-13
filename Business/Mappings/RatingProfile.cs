using AutoMapper;
using DAL.Entities;
using Shared.DTOs;

namespace Business.Mappings
{
    public class RatingProfile : Profile
    {
        public RatingProfile()
        {
            CreateMap<CreateRatingDto, ProductRating>()
           .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
           .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating));

            CreateMap<ProductRating, RatingResponseDto>()
           .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Rating))
           .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
           .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));
        }
    }
}
