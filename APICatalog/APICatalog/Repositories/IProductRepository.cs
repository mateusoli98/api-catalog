using APICatalog.Models;
using APICatalog.Pagination;
using X.PagedList;

namespace APICatalog.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(int id);
    Task<IPagedList<Product>> GetProductsAsync(ProductParameters productsParameters);
    Task<IPagedList<Product>> GetProductsFilterPriceAsync(ProductsFilterPrice filter);
}
