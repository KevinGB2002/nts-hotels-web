using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NtsHotels.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelImageData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "Hotels",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "Hotels");
        }
    }
}
