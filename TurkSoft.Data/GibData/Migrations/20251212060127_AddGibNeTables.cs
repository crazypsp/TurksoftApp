using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TurkSoft.Data.GibData.Migrations
{
    /// <inheritdoc />
    public partial class AddGibNeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorporateEmail",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByPersonFirstName",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByPersonLastName",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByPersonMobilePhone",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerRepresentative",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "InstitutionType",
                table: "GibFirm",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "KepAddress",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PersonalFirstName",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PersonalLastName",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleEmail",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleFirstName",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleLastName",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleMobilePhone",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleTckn",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TaxOfficeProvince",
                table: "GibFirm",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "GibFirmServices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GibFirmId = table.Column<long>(type: "bigint", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TariffType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeleteDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GibFirmServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GibFirmServices_GibFirm_GibFirmId",
                        column: x => x.GibFirmId,
                        principalTable: "GibFirm",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GibFirmServiceAliases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GibFirmServiceId = table.Column<long>(type: "bigint", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Alias = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DeleteDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GibFirmServiceAliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GibFirmServiceAliases_GibFirmServices_GibFirmServiceId",
                        column: x => x.GibFirmServiceId,
                        principalTable: "GibFirmServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GibFirmServiceAliases_GibFirmServiceId",
                table: "GibFirmServiceAliases",
                column: "GibFirmServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_GibFirmServices_GibFirmId",
                table: "GibFirmServices",
                column: "GibFirmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GibFirmServiceAliases");

            migrationBuilder.DropTable(
                name: "GibFirmServices");

            migrationBuilder.DropColumn(
                name: "CorporateEmail",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonFirstName",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonLastName",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonMobilePhone",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "CustomerRepresentative",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "InstitutionType",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "KepAddress",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "PersonalFirstName",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "PersonalLastName",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "ResponsibleEmail",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "ResponsibleFirstName",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "ResponsibleLastName",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "ResponsibleMobilePhone",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "ResponsibleTckn",
                table: "GibFirm");

            migrationBuilder.DropColumn(
                name: "TaxOfficeProvince",
                table: "GibFirm");
        }
    }
}
