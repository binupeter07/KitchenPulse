using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllergenCheckAndIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_IngredientAllergens_AllergenId",
                table: "IngredientAllergens",
                column: "AllergenId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_IngredientAllergens_AllergenId",
                table: "IngredientAllergens",
                sql: "[AllergenId] BETWEEN 1 AND 14");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IngredientAllergens_AllergenId",
                table: "IngredientAllergens");

            migrationBuilder.DropCheckConstraint(
                name: "CK_IngredientAllergens_AllergenId",
                table: "IngredientAllergens");
        }
    }
}
