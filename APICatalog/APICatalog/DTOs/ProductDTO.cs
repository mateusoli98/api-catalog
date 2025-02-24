using System.ComponentModel.DataAnnotations;

namespace APICatalog.DTOs;

public class ProductDTO
{
    [Key]
    public int ProductId { get; set; }

    [Required]
    [StringLength(80)]
    public string? Name { get; set; }

    [Required]
    [StringLength(300)]
    public string? Description { get; set; }

    [Required]
    [StringLength(300)]
    public string? ImageUrl { get; set; }

    [Required]
    public decimal Price { get; set; }

    public decimal CategoryId { get; set; }
}
