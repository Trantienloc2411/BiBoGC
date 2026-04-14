using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_OrderNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderNumberSequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    CurrentSequence = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderNumberSequences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderNumberSequences_Date",
                table: "OrderNumberSequences",
                column: "Date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderNumberSequences");
        }
    }
}
