using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JsonMart.Migrations
{
    /// <inheritdoc />
    public partial class AddProductQuantityInOrderProductEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductQuantity",
                table: "OrderProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductQuantity",
                table: "OrderProducts");
        }
    }
}
