using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProiectASP.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedToProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Profiles",
                newName: "ProfileImage");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Profiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Profiles");

            migrationBuilder.RenameColumn(
                name: "ProfileImage",
                table: "Profiles",
                newName: "Username");
        }
    }
}
