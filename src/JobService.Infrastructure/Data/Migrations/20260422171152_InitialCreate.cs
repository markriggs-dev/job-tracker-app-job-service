using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_requisitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RoleTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CompanyCareerPortalUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    JobDescription = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DateDiscovered = table.Column<DateOnly>(type: "date", nullable: false),
                    ApplicationExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DateSubmitted = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_requisitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_job_requisitions_UserId",
                table: "job_requisitions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_job_requisitions_UserId_IsDeleted",
                table: "job_requisitions",
                columns: new[] { "UserId", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_requisitions");
        }
    }
}
