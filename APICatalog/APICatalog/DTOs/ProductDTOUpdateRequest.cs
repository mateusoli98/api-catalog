using System.ComponentModel.DataAnnotations;

namespace APICatalog.DTOs;

public class ProductDTOUpdateRequest : IValidatableObject
{
    [Range(1, 9999, ErrorMessage = "Estoque deve estar entre 1 e 9999")]
    public float Stock { get; set; }


    public DateTime CreatedAt { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CreatedAt <= DateTime.MinValue)
        {
            yield return new ValidationResult("A data deve ser maior que a data atual", new[] { nameof(this.CreatedAt) });
        }
    }
}
