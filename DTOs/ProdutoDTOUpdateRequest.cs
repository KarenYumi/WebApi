using System.ComponentModel.DataAnnotations;

namespace MinhaAPI.DTOs
{
    public class ProdutoDTOUpdateRequest :IValidatableObject
    {
        [Range(1,99999, ErrorMessage ="Tente outra quantidade")]
        public float Estoque { get; set; }
        public DateTime DataCadastro { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DataCadastro.Date <= DateTime.Now.Date)
            {
                yield return new ValidationResult("A data deve ser maior que a data atual", new[] {nameof(this.DataCadastro)});
            }
        }
    }
}
