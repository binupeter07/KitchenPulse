using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDishes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dishes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Station = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes", x => x.Id);
                    table.CheckConstraint("CK_Dishes_Price", "[Price] > 0");
                    table.CheckConstraint("CK_Dishes_Station", "[Station] BETWEEN 1 AND 3");
                });

            migrationBuilder.CreateTable(
                name: "RecipeLines",
                columns: table => new
                {
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeLines", x => new { x.DishId, x.IngredientId });
                    table.CheckConstraint("CK_RecipeLines_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_RecipeLines_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeLines_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_Name",
                table: "Dishes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeLines_IngredientId",
                table: "RecipeLines",
                column: "IngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeLines");

            migrationBuilder.DropTable(
                name: "Dishes");
        }
    }
}
