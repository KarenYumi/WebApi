using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MinhaAPI.Models
{
    [Table("Categorias")]

    public class Categoria
    {
        public Categoria()
        {
            Produtos = new Collection<Produto>(); //inicializando a propriedade Produtos que é uma coleção de Colletion<Produtos>
        }

        [Key]
        public int CategoriaId { get; set; }

        [Required]
        [StringLength(80)]
        public string Nome { get; set; }


        [Required]
        [StringLength(300)]
        public string? ImagemUrl { get; set; }

        [JsonIgnore]
        public ICollection<Produto>? Produtos { get; set; } //Propriedade de navegação Produtos que é uma coleção de objetos produto



    }
}
