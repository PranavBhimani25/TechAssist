using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechAssist.Migrations
{
    /// <inheritdoc />
    public partial class secondmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "Role" },
                values: new object[] { 1, new DateTime(2025, 10, 22, 14, 54, 40, 758, DateTimeKind.Utc).AddTicks(1741), "admin@example.com", "Super Admin", true, "admin@123", "Admin" });
        }
    }
}
