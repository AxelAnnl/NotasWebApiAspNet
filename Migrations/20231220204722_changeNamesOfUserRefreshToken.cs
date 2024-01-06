using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotasWebApi.Migrations
{
    /// <inheritdoc />
    public partial class changeNamesOfUserRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefresToken",
                table: "UsersRefreshTokens",
                newName: "RefreshToken");

            migrationBuilder.RenameColumn(
                name: "Expiracion",
                table: "UsersRefreshTokens",
                newName: "Expiration");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "UsersRefreshTokens",
                newName: "RefresToken");

            migrationBuilder.RenameColumn(
                name: "Expiration",
                table: "UsersRefreshTokens",
                newName: "Expiracion");
        }
    }
}
