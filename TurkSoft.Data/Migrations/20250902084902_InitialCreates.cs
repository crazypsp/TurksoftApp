using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurkSoft.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MaliMusavirId",
                table: "Luca",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaliMusavirId",
                table: "KeyAccount",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaliMusavirId",
                table: "Luca");

            migrationBuilder.DropColumn(
                name: "MaliMusavirId",
                table: "KeyAccount");
        }
    }
}
