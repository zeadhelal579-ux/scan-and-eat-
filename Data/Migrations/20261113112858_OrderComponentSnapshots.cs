using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScanEat.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrderComponentSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComponentsJson",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComponentsJson",
                table: "OrderItems");
        }
    }
}
