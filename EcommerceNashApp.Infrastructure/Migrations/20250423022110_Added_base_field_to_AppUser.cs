using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcommerceNashApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_base_field_to_AppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "AppUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "AppUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "AppUsers");
        }
    }
}
