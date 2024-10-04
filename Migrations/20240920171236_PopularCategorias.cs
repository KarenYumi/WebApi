
using Microsoft.EntityFrameworkCore.Migrations;

namespace MinhaAPI.Migrations
{
    /// <inheritdoc />
    public partial class PopularCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            mb.Sql("INSERT INTO \"Categorias\" (\"Nome\", \"ImagemUrl\") VALUES ('Bebida', 'bebidas');");
            mb.Sql("INSERT INTO \"Categorias\" (\"Nome\", \"ImagemUrl\") VALUES ('Lanches', 'lanches');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder mb)
        {
            mb.Sql("DELETE FROM Categorias WHERE Nome IN ('Bebida', 'Sobremesas', 'Lanches')");
        }
    }
}
