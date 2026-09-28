using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReExamManagementSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FilterActiveReExamApplicationUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReExamApplications_StudentId_CourseId_AcademicYearId_SemesterId",
                table: "ReExamApplications");

            migrationBuilder.CreateIndex(
                name: "IX_ReExamApplications_StudentId_CourseId_AcademicYearId_SemesterId",
                table: "ReExamApplications",
                columns: new[] { "StudentId", "CourseId", "AcademicYearId", "SemesterId" },
                unique: true,
                filter: "[Status] IN (1, 2, 5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReExamApplications_StudentId_CourseId_AcademicYearId_SemesterId",
                table: "ReExamApplications");

            migrationBuilder.CreateIndex(
                name: "IX_ReExamApplications_StudentId_CourseId_AcademicYearId_SemesterId",
                table: "ReExamApplications",
                columns: new[] { "StudentId", "CourseId", "AcademicYearId", "SemesterId" },
                unique: true);
        }
    }
}
