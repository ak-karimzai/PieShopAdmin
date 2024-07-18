using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PieShopAdmin.Migrations
{
    public partial class PieVersioningAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Pies",
                type: "bytea",
                rowVersion: true,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Pies");
        }
    }
}
