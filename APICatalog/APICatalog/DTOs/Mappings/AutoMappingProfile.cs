using APICatalog.Models;
using AutoMapper;

namespace APICatalog.DTOs.Mappings;

public class AutoMappingProfile : Profile
{
    public AutoMappingProfile()
    {
        CreateMap<Category, CategoryDTO>().ReverseMap();
        CreateMap<ProductDTO, Product>().ReverseMap();
        CreateMap<ProductDTO, ProductDTOUpdateRequest>().ReverseMap();
        CreateMap<ProductDTO, ProductDTOUpdateResponse>().ReverseMap();
    }
}
