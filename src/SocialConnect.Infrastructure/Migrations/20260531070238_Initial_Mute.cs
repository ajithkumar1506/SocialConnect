using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialConnect.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Mute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mutes",
                columns: table => new
                {
                    MuterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MutedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "datetimeoffset",
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mutes", x => new { x.MuterId, x.MutedId });
                    table.ForeignKey(
                        name: "FK_Mutes_Users_MutedId",
                        column: x => x.MutedId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_Mutes_Users_MuterId",
                        column: x => x.MuterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Mutes_MutedId",
                table: "Mutes",
                column: "MutedId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Mutes");
        }
    }
}
