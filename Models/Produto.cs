using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MinhaAPI.Models
{
    [Table("Produtos")] //indica par onde ela vai ser mapeada

    public class Produto :IValidatableObject
    {
        [Key]
        public int ProdutoId { get; set; }

        [Required(ErrorMessage ="O nme é obrigatório")]
        [StringLength(80, ErrorMessage ="O nome deve ter no máximo {1} e no mínimo {2} caracteres.", MinimumLength =5)]
        public string? Nome { get; set; }

        [Required]
        [StringLength(300)]
        public string? Descricao { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }

        [Required]
        [StringLength(300)]
        public string? ImagemUrl { get; set; }
        public float Estoque { get; set; }
        public DateTime DataCadastro { get; set; }
        public int CategoriaId { get; set; }

        [JsonIgnore]
        public Categoria? Categoria { get; set; } //Propriedade de navegação Categoria

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(this.Nome))
            {
                var primeiraLetra = this.Nome[0].ToString();
                if (primeiraLetra != primeiraLetra.ToUpper())
                {
                    yield return new // yielder indica q o método ou operador é um hiterador e usamos para retornar cada elemento individualmetne
                        ValidationResult("A primeira letra do produto deve ser maiúscula", new[] { nameof(this.Nome) });
                }
            }


            if (this.Estoque <= 0)
            {
                yield return new // yielder indica q o método ou operador é um hiterador e usamos para retornar cada elemento individualmetne
                       ValidationResult("O estoque necessita ser maior do que 0", new[] { nameof(this.Estoque) });
            }
        }    
    }

}



