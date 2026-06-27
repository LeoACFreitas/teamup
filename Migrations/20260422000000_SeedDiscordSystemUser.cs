using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Teamup.Migrations
{
    /// <inheritdoc />
    public partial class SeedDiscordSystemUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Nickname", "Sub", "Country" },
                values: new object[] { "Discord", null, "US" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM User WHERE Nickname = 'Discord' AND Sub IS NULL LIMIT 1;");
        }
    }
}
