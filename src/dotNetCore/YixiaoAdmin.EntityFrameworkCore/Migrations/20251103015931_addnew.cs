using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YixiaoAdmin.EntityFrameworkCore.Migrations
{
    public partial class addnew : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BraceletId",
                table: "WorkBracelet",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeartRate",
                table: "WorkBracelet",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntryAreaScanner",
                table: "Device",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GpsLatitude",
                table: "Device",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GpsLongitude",
                table: "Device",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GpsOnlineStatus",
                table: "Device",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToxicGasAlarmOnlineStatus",
                table: "Device",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkAreaScanner",
                table: "Device",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceNumber",
                table: "Bracelet",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkBracelet_BraceletId",
                table: "WorkBracelet",
                column: "BraceletId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkBracelet_Bracelet_BraceletId",
                table: "WorkBracelet",
                column: "BraceletId",
                principalTable: "Bracelet",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkBracelet_Bracelet_BraceletId",
                table: "WorkBracelet");

            migrationBuilder.DropIndex(
                name: "IX_WorkBracelet_BraceletId",
                table: "WorkBracelet");

            migrationBuilder.DropColumn(
                name: "BraceletId",
                table: "WorkBracelet");

            migrationBuilder.DropColumn(
                name: "HeartRate",
                table: "WorkBracelet");

            migrationBuilder.DropColumn(
                name: "EntryAreaScanner",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "GpsLatitude",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "GpsLongitude",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "GpsOnlineStatus",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "ToxicGasAlarmOnlineStatus",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "WorkAreaScanner",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "DeviceNumber",
                table: "Bracelet");
        }
    }
}
