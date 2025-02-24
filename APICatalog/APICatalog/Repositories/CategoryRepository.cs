using APICatalog.Context;
using APICatalog.Models;
using APICatalog.Pagination;
using X.PagedList;

namespace APICatalog.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext appDbContext) : base(appDbContext)
    { }

    public async Task<IPagedList<Category>> GetCategoriesAsync(CategoryParameters categoryParameters)
    {
        var categories = await GetAllAsync();
        var catogoriesOrder = categories.OrderBy(x => x.CategoryId).AsQueryable();

        //var result = PagedList<Category>.ToPagedList(catogoriesOrder, categoryParameters.PageNumber, categoryParameters.PageSize);

        var result = await categories.ToPagedListAsync(categoryParameters.PageNumber, categoryParameters.PageSize);

        return result;
    }

    public async Task<IPagedList<Category>> GetCategoriesFilterNameAsync(CategoryFilterName filter)
    {
        var categories = await GetAllAsync();

        if (!string.IsNullOrEmpty(filter.Name))
        {
            categories = categories.Where(p => p.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));
        }

        //var resultFilter = PagedList<Category>.ToPagedList(categories.AsQueryable(), filter.PageNumber, filter.PageSize);
        var resultFilter = await categories.ToPagedListAsync(filter.PageNumber, filter.PageSize);

        return resultFilter;
    }
}
