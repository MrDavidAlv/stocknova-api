using StockNova.Domain.Entities;

namespace StockNova.Domain.Specifications;

public class ProductFilterSpecification : BaseSpecification<Product>
{
    public ProductFilterSpecification(
        int? ProductId = null,
        string? search = null,
        int? categoryId = null,
        int? supplierId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? discontinued = null,
        int page = 1,
        int pageSize = 10,
        string sortBy = "ProductName",
        string sortOrder = "asc")
        : base(p =>
            !p.IsDeleted &&
            (string.IsNullOrEmpty(search) || p.ProductName.ToLower().Contains(search.ToLower())) &&
            (!ProductId.HasValue || p.ProductId == ProductId) &&
            (!categoryId.HasValue || p.CategoryId == categoryId) &&
            (!supplierId.HasValue || p.SupplierId == supplierId) &&
            (!minPrice.HasValue || p.UnitPrice >= minPrice) &&
            (!maxPrice.HasValue || p.UnitPrice <= maxPrice) &&
            (!discontinued.HasValue || p.Discontinued == discontinued))
    {
        AddInclude(p => p.Category!);
        AddInclude(p => p.Supplier!);

        ApplyPaging((page - 1) * pageSize, pageSize);

        var isDescending = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

        switch (sortBy.ToLower())
        {
            case "unitprice":
                if (isDescending) ApplyOrderByDescending(p => p.UnitPrice!);
                else ApplyOrderBy(p => p.UnitPrice!);
                break;
            case "categoryid":
                if (isDescending) ApplyOrderByDescending(p => p.CategoryId!);
                else ApplyOrderBy(p => p.CategoryId!);
                break;
            case "createdat":
                if (isDescending) ApplyOrderByDescending(p => p.CreatedAt);
                else ApplyOrderBy(p => p.CreatedAt);
                break;
            default:
                if (isDescending) ApplyOrderByDescending(p => p.ProductName);
                else ApplyOrderBy(p => p.ProductName);
                break;
        }
    }
}

public class ProductByIdSpecification : BaseSpecification<Product>
{
    public ProductByIdSpecification(int productId)
        : base(p => p.ProductId == productId && !p.IsDeleted)
    {
        AddInclude(p => p.Category!);
        AddInclude(p => p.Supplier!);
    }
}
