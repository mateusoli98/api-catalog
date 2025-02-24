using APICatalog.Context;
using APICatalog.Models;
using APICatalog.Pagination;
using X.PagedList;

namespace APICatalog.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext appDbContext) : base(appDbContext)
    { }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int id)
    {
        var categories = await GetAllAsync();
        var categoriesFilter = categories.Where(x => x.CategoryId == id);

        return categoriesFilter;
    }

    //public IEnumerable<Product> GetProducts(ProductsParameters productsParameters)
    //{
    //    return GetAll()
    //        .OrderBy(x => x.Name)
    //        .Skip((productsParameters.PageNumber - 1) * productsParameters.PageSize)
    //        .Take(productsParameters.PageSize)
    //        .ToList();
    //}

    public async Task<IPagedList<Product>> GetProductsAsync(ProductParameters productsParameters)
    {
        var products = await GetAllAsync();
        var productsOrder = products.OrderBy(x => x.ProductId).AsQueryable();
        //var result = PagedList<Product>.ToPagedList(productsOrder, productsParameters.PageNumber, productsParameters.PageSize);
        var result = await productsOrder.ToPagedListAsync(productsParameters.PageNumber, productsParameters.PageSize);
        return result;
    }

    public async Task<IPagedList<Product>> GetProductsFilterPriceAsync(ProductsFilterPrice filter)
    {
        var products = await GetAllAsync();

        if (filter.Price.HasValue && !string.IsNullOrEmpty(filter.PriceCritery))
        {
            if (filter.PriceCritery.Equals("maior", StringComparison.OrdinalIgnoreCase))
            {
                products = products.Where(p => p.Price > filter.Price.Value).OrderBy(x => x.Price);
            }

            if (filter.PriceCritery.Equals("menor", StringComparison.OrdinalIgnoreCase))
            {
                products = products.Where(p => p.Price < filter.Price.Value).OrderBy(x => x.Price);
            }

            if (filter.PriceCritery.Equals("igual", StringComparison.OrdinalIgnoreCase))
            {
                products = products.Where(p => p.Price == filter.Price.Value).OrderBy(x => x.Price);
            }
        }

        // var resultFilter = PagedList<Product>.ToPagedList(products.AsQueryable(), filter.PageNumber, filter.PageSize);
        var resultFilter = await products.ToPagedListAsync(filter.PageNumber, filter.PageSize);

        return resultFilter;
    }
}