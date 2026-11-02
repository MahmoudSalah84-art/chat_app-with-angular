using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace chatme.infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantReceipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastDeliveredAt",
                table: "ChatParticipants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReadAt",
                table: "ChatParticipants",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastDeliveredAt",
                table: "ChatParticipants");

            migrationBuilder.DropColumn(
                name: "LastReadAt",
                table: "ChatParticipants");
        }
    }
}
