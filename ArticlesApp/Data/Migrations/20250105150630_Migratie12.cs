using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArticlesApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class Migratie12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isInCart",
                table: "Articles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isInCart",
                table: "Articles");
        }
    }
}
