using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chatme.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPushReceiptTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PushReceiptTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushReceiptTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PushReceiptTokens_ReceiverId_ChatId_MessageId",
                table: "PushReceiptTokens",
                columns: new[] { "ReceiverId", "ChatId", "MessageId" });

            migrationBuilder.CreateIndex(
                name: "IX_PushReceiptTokens_TokenHash",
                table: "PushReceiptTokens",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PushReceiptTokens");
        }
    }
}
