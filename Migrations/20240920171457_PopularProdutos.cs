using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MinhaAPI.Migrations
{
    /// <inheritdoc />
    public partial class PopularProdutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            mb.Sql("INSERT INTO \"Produtos\" (\"Nome\", \"Descricao\", \"Preco\", \"ImagemUrl\", \"Estoque\", \"DataCadastro\", \"CategoriaId\") VALUES ('Coca-Cola', 'Refrigerante', 5.45, 'cocacola.jpg', 50, now(), 1);");

            mb.Sql("INSERT INTO \"Produtos\" (\"Nome\", \"Descricao\", \"Preco\", \"ImagemUrl\", \"Estoque\", \"DataCadastro\", \"CategoriaId\") VALUES ('Suco de Laranja', 'Suco', 7.50, 'sucodelaranja.jpg', 320, now(), 1);");

            mb.Sql("INSERT INTO \"Produtos\" (\"Nome\", \"Descricao\", \"Preco\", \"ImagemUrl\", \"Estoque\", \"DataCadastro\", \"CategoriaId\") VALUES ('Coxinha', 'Salgado com recheio de frango', 10.00, 'coxinha.jpg', 12, now(), 2);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder mb)
        {
            mb.Sql("DELETE FROM Produtos");
        }
    }
}
