using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NtsHotels.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHotelImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TEMP TABLE "__image_conversion_guard" ("ok" INTEGER NOT NULL CHECK ("ok" = 1));
                INSERT INTO "__image_conversion_guard" ("ok")
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM "Hotels"
                    WHERE "ImageUrl" IS NOT NULL AND trim("ImageUrl") <> '' AND "ImageData" IS NULL
                ) THEN 0 ELSE 1 END;
                DROP TABLE "__image_conversion_guard";
                """);
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Hotels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Hotels",
                type: "TEXT",
                nullable: true);
        }
    }
}
