using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Recipedia.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRecipeModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Recipes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "CookTimeMinutes",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Ingredients",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Recipes");

            migrationBuilder.RenameColumn(
                name: "Difficulty",
                table: "Recipes",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Recipes",
                newName: "Source");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Recipes",
                newName: "Difficulty");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "Recipes",
                newName: "Category");

            migrationBuilder.AddColumn<int>(
                name: "CookTimeMinutes",
                table: "Recipes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ingredients",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Recipes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Recipes",
                columns: new[] { "Id", "Category", "CookTimeMinutes", "Difficulty", "ImageUrl", "Ingredients", "Instructions", "Title" },
                values: new object[,]
                {
                    { 1, null, null, null, null, "", "", "Spaghetti Bolognese" },
                    { 2, null, null, null, null, "", "", "Chicken Curry" },
                    { 3, null, null, null, null, "", "", "Grilled Cheese Sandwich" },
                    { 4, null, null, null, null, "", "", "Veggie Stir Fry" }
                });
        }
    }
}
