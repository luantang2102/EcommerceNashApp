using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceNashApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ADded_Delete_Behavior_ProductCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_Categories_CategoriesId",
                table: "ProductCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_Products_ProductsId",
                table: "ProductCategories");

            migrationBuilder.RenameColumn(
                name: "ProductsId",
                table: "ProductCategories",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "CategoriesId",
                table: "ProductCategories",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductCategories_ProductsId",
                table: "ProductCategories",
                newName: "IX_ProductCategories_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_Categories_CategoryId",
                table: "ProductCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_Products_ProductId",
                table: "ProductCategories",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_Categories_CategoryId",
                table: "ProductCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_Products_ProductId",
                table: "ProductCategories");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ProductCategories",
                newName: "ProductsId");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "ProductCategories",
                newName: "CategoriesId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductCategories_ProductId",
                table: "ProductCategories",
                newName: "IX_ProductCategories_ProductsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_Categories_CategoriesId",
                table: "ProductCategories",
                column: "CategoriesId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_Products_ProductsId",
                table: "ProductCategories",
                column: "ProductsId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
