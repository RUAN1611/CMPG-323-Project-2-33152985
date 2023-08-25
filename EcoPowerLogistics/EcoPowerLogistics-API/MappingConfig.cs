using AutoMapper;
using EcoPowerLogistics_API.Models;
using EcoPowerLogistics_API.Models.DTO;

namespace EcoPowerLogistics_API
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Order, OrderDTO>().ReverseMap();
            CreateMap<Product, ProductDTO>().ReverseMap();
            CreateMap<Customer, CustomerDTO>().ReverseMap();
            CreateMap<OrderDetail, OrderDetailDTO>().ReverseMap();
        }
    }
}
