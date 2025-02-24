namespace APICatalog.Pagination;

public class ProductsFilterPrice : QueryStringParameters
{
    public decimal? Price { get; set; }
    public string? PriceCritery { get; set; }
}
