using AutoMapper;
using ProductOrderManagementSystem.Infrastructure.DTOs;
using ProductOrderManagementSystem.Infrastructure.Entities;

namespace ProductOrderManagementSystem.Infrastructure.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDTO>();
            CreateMap<ProductDTO, Product>();

            CreateMap<Order, OrderDTO>();
            CreateMap<OrderDTO, Order>();

            CreateMap<OrderDetail, OrderDetailDTO>();
            CreateMap<OrderDetailDTO, OrderDetail>()
                 .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));
        }
    }
}
