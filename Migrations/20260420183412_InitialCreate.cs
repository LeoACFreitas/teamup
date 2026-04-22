using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Teamup.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Game",
                columns: table => new
                {
                    Game_id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(10,10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Game", x => x.Game_id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    User_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Nickname = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Sub = table.Column<decimal>(type: "decimal(30,0)", nullable: true),
                    Country = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.User_id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Request",
                columns: table => new
                {
                    Request_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    User_id = table.Column<int>(type: "int", nullable: false),
                    Game_id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Request", x => x.Request_id);
                    table.ForeignKey(
                        name: "FK_Request_Game_Game_id",
                        column: x => x.Game_id,
                        principalTable: "Game",
                        principalColumn: "Game_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Request_User_User_id",
                        column: x => x.User_id,
                        principalTable: "User",
                        principalColumn: "User_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Request_Game_id",
                table: "Request",
                column: "Game_id");

            migrationBuilder.CreateIndex(
                name: "IX_Request_User_id",
                table: "Request",
                column: "User_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Request");

            migrationBuilder.DropTable(
                name: "Game");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
