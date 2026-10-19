using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RaceDayAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRaceDayModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrolments_Events_EventId",
                table: "Enrolments");

            migrationBuilder.DropIndex(
                name: "IX_Enrolments_EventId",
                table: "Enrolments");

            migrationBuilder.DropColumn(
                name: "RouteMapUrl",
                table: "Routes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Enrolments");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Routes",
                newName: "RouteId");

            migrationBuilder.RenameColumn(
                name: "ResultStatus",
                table: "Results",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Results",
                newName: "ResultId");

            migrationBuilder.RenameColumn(
                name: "DistanceKm",
                table: "Events",
                newName: "Distance");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Events",
                newName: "EventId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Enrolments",
                newName: "EnrolmentId");

            migrationBuilder.RenameColumn(
                name: "MaxParticipants",
                table: "Categories",
                newName: "MaximumParticipants");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Categories",
                newName: "CategoryId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Distance",
                table: "Events",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "RouteId",
                table: "Routes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Results",
                newName: "ResultStatus");

            migrationBuilder.RenameColumn(
                name: "ResultId",
                table: "Results",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "Distance",
                table: "Events",
                newName: "DistanceKm");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "Events",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "EnrolmentId",
                table: "Enrolments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "MaximumParticipants",
                table: "Categories",
                newName: "MaxParticipants");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Categories",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "RouteMapUrl",
                table: "Routes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<double>(
                name: "DistanceKm",
                table: "Events",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "Enrolments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Enrolments_EventId",
                table: "Enrolments",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrolments_Events_EventId",
                table: "Enrolments",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id");
        }
    }
}
