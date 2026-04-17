using AutoMapper;
using StockNova.Application.DTOs.AuditLogs;
using StockNova.Application.DTOs.Categories;
using StockNova.Application.DTOs.Products;
using StockNova.Domain.Entities;

namespace StockNova.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.CompanyName : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null));

        CreateMap<Product, ProductDetailResponse>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.CompanyName : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
            .ForMember(dest => dest.CategoryPicture, opt => opt.MapFrom(src => src.Category != null ? src.Category.Picture : null))
            .ForMember(dest => dest.CategoryDescription, opt => opt.MapFrom(src => src.Category != null ? src.Category.Description : null))
            .ForMember(dest => dest.SupplierContactName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.ContactName : null))
            .ForMember(dest => dest.SupplierPhone, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Phone : null))
            .ForMember(dest => dest.SupplierCountry, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Country : null));

        CreateMap<CreateProductRequest, Product>();
        CreateMap<UpdateProductRequest, Product>();

        // Category mappings
        CreateMap<Category, CategoryResponse>();
        CreateMap<CreateCategoryRequest, Category>();

        // AuditLog mappings
        CreateMap<AuditLog, AuditLogResponse>();
    }
}
