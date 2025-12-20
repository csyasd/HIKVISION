using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YixiaoAdmin.EntityFrameworkCore.Migrations
{
    public partial class AddDeviceOnlineStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OnlineStatus",
                table: "Device",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnlineStatus",
                table: "Device");
        }
    }
}

