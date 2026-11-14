using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIngredientCurrentStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStock",
                table: "Ingredients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentStock",
                table: "Ingredients",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
