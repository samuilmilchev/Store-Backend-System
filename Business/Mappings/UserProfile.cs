using AutoMapper;
using DAL.Entities;
using Shared.DTOs;
using Shared.Models;

namespace Business.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserProfileModel>()
            .ForMember(dest => dest.AddressDelivery, opt => opt.MapFrom(src => src.AddressDelivery.AddressDelivery));

            CreateMap<UserUpdateModel, ApplicationUser>()
           .ForMember(dest => dest.AddressDelivery, opt => opt.Condition(src => src.AddressDelivery != null));

            CreateMap<string, UserAddress>()
           .ForMember(dest => dest.AddressDelivery, opt => opt.MapFrom(src => src));

            CreateMap<UserAddressDTO, UserAddress>().ReverseMap();
        }
    }
}
