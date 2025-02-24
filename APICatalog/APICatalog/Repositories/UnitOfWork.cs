using APICatalog.Context;

namespace APICatalog.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private IProductRepository? _productRepository;
    private ICategoryRepository? _categoryRepository;
    public AppDbContext _context;

    public UnitOfWork(AppDbContext appDbContext)
    {
        _context = appDbContext;
    }

    public IProductRepository ProductRepository
    {
        get
        {
            return _productRepository ??= new ProductRepository(_context);
        }
    }

    public ICategoryRepository CategoryRepository
    {
        get
        {
            return _categoryRepository ??= new CategoryRepository(_context);
        }
    }

    public async Task CommitAsync()
    {
       await _context.SaveChangesAsync();
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
