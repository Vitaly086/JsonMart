using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JsonMart.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Orders",
                type: "longtext",
                nullable: false);
        }
    }
}
