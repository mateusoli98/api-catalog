using APICatalog.Models;
using APICatalog.Pagination;
using X.PagedList;

namespace APICatalog.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IPagedList<Category>> GetCategoriesAsync(CategoryParameters categoryParameters);
    Task<IPagedList<Category>> GetCategoriesFilterNameAsync(CategoryFilterName filter);
}
