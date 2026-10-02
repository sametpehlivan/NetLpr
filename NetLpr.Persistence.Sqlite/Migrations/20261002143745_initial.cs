using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetLpr.Persistence.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RtspSources",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false),
                    PathAndQuery = table.Column<string>(type: "TEXT", nullable: false),
                    TransportType = table.Column<string>(type: "TEXT", nullable: false),
                    RoiX = table.Column<float>(type: "REAL", nullable: false),
                    RoiY = table.Column<float>(type: "REAL", nullable: false),
                    RoiHeight = table.Column<float>(type: "REAL", nullable: false),
                    RoiWidth = table.Column<float>(type: "REAL", nullable: false),
                    IsAnalysesOpen = table.Column<bool>(type: "INTEGER", nullable: false),
                    AnalysesType = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RtspSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackedInfos",
                columns: table => new
                {
                    TrackedId = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<string>(type: "TEXT", nullable: false),
                    DateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", nullable: false),
                    PlateRectangle = table.Column<string>(type: "TEXT", nullable: false),
                    TrackingValues = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedInfos", x => x.TrackedId);
                });

            migrationBuilder.CreateTable(
                name: "StreamAnalysesAreaPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceId = table.Column<string>(type: "TEXT", nullable: false),
                    X = table.Column<float>(type: "REAL", nullable: false),
                    Y = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamAnalysesAreaPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StreamAnalysesAreaPoints_RtspSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "RtspSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StreamAnalysesAreaPoints_SourceId",
                table: "StreamAnalysesAreaPoints",
                column: "SourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StreamAnalysesAreaPoints");

            migrationBuilder.DropTable(
                name: "TrackedInfos");

            migrationBuilder.DropTable(
                name: "RtspSources");
        }
    }
}
