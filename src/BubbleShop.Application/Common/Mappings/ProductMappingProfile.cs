using AutoMapper;
using BubbleShop.Application.DTOs;
using BubbleShop.Application.Features.Products.Queries.GetAllProducts;
using BubbleShop.Domain.Entities;

namespace BubbleShop.Application.Common.Mappings;

public sealed class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductDto>();
    }
}
