using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APICatalog.Migrations
{
    /// <inheritdoc />
    public partial class AddCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder mb)
        {
            mb.Sql("INSERT INTO Categories(Name, ImageUrl) VALUES('Bebida', 'Bebida.jpg')");
            mb.Sql("INSERT INTO Categories(Name, ImageUrl) VALUES('Lanche', 'Lanche.jpg')");
            mb.Sql("INSERT INTO Categories(Name, ImageUrl) VALUES('Sobremesas', 'Sobremesas.jpg')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder mb)
        {
            mb.Sql("DELETE FROM Categories");
        }
    }
}
