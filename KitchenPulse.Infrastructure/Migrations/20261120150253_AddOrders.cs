using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitchenPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<int>(type: "int", nullable: false),
                    OrderType = table.Column<short>(type: "smallint", nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Table = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PlacedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCollected = table.Column<bool>(type: "bit", nullable: false),
                    CollectedCourses = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.CheckConstraint("CK_Orders_OrderNumber", "[OrderNumber] > 0");
                    table.CheckConstraint("CK_Orders_OrderType", "[OrderType] BETWEEN 1 AND 2");
                    table.ForeignKey(
                        name: "FK_Orders_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DishId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Course = table.Column<short>(type: "smallint", nullable: false),
                    IsDone = table.Column<bool>(type: "bit", nullable: false),
                    IsSent = table.Column<bool>(type: "bit", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLines", x => x.Id);
                    table.CheckConstraint("CK_OrderLines_Course", "[Course] BETWEEN 1 AND 2");
                    table.CheckConstraint("CK_OrderLines_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_OrderLines_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderLines_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_DishId",
                table: "OrderLines",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_OrderId",
                table: "OrderLines",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SiteId_BusinessDate_OrderType_OrderNumber",
                table: "Orders",
                columns: new[] { "SiteId", "BusinessDate", "OrderType", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SiteId_PlacedAtUtc",
                table: "Orders",
                columns: new[] { "SiteId", "PlacedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderLines");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
