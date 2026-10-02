using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.WebApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationChallengeRevokedAtUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAtUtc",
                table: "VerificationChallenges",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RevokedAtUtc",
                table: "VerificationChallenges");
        }
    }
}
