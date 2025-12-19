using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YixiaoAdmin.EntityFrameworkCore.Migrations
{
    public partial class UpdateTablesAndAddGasAlarmRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkBracelet_Bracelet_BraceletId",
                table: "WorkBracelet");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkRecord_WorkBracelet_WorkBraceletId",
                table: "WorkRecord");

            migrationBuilder.DropIndex(
                name: "IX_WorkRecord_WorkBraceletId",
                table: "WorkRecord");

            migrationBuilder.DropColumn(
                name: "EmergencyCallStatus",
                table: "WorkRecord");

            migrationBuilder.DropColumn(
                name: "WorkBraceletId",
                table: "WorkRecord");

            migrationBuilder.DropColumn(
                name: "GasAlarm",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "EmergencyCallStatus",
                table: "WorkBracelet");

            migrationBuilder.DropColumn(
                name: "EntryAreaScanner",
                table: "Device");

            migrationBuilder.DropColumn(
                name: "WorkAreaScanner",
                table: "Device");

            migrationBuilder.RenameColumn(
                name: "BraceletId",
                table: "WorkBracelet",
                newName: "WorkOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkBracelet_BraceletId",
                table: "WorkBracelet",
                newName: "IX_WorkBracelet_WorkOrderId");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "WorkOrder",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GasAlarmRecord",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkOrderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Gas1 = table.Column<float>(type: "real", nullable: false),
                    Gas2 = table.Column<float>(type: "real", nullable: false),
                    Gas3 = table.Column<float>(type: "real", nullable: false),
                    Gas4 = table.Column<float>(type: "real", nullable: false),
                    Gas5 = table.Column<float>(type: "real", nullable: false),
                    Gas6 = table.Column<float>(type: "real", nullable: false),
                    Gas7 = table.Column<float>(type: "real", nullable: false),
                    Gas8 = table.Column<float>(type: "real", nullable: false),
                    Gas9 = table.Column<float>(type: "real", nullable: false),
                    Gas10 = table.Column<float>(type: "real", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationUsername = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModificationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ParentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortCode = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GasAlarmRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GasAlarmRecord_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GasAlarmRecord_WorkOrderId",
                table: "GasAlarmRecord",
                column: "WorkOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkBracelet_WorkOrder_WorkOrderId",
                table: "WorkBracelet",
                column: "WorkOrderId",
                principalTable: "WorkOrder",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkBracelet_WorkOrder_WorkOrderId",
                table: "WorkBracelet");

            migrationBuilder.DropTable(
                name: "GasAlarmRecord");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "WorkOrder");

            migrationBuilder.RenameColumn(
                name: "WorkOrderId",
                table: "WorkBracelet",
                newName: "BraceletId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkBracelet_WorkOrderId",
                table: "WorkBracelet",
                newName: "IX_WorkBracelet_BraceletId");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyCallStatus",
                table: "WorkRecord",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkBraceletId",
                table: "WorkRecord",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GasAlarm",
                table: "WorkOrder",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyCallStatus",
                table: "WorkBracelet",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntryAreaScanner",
                table: "Device",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkAreaScanner",
                table: "Device",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkRecord_WorkBraceletId",
                table: "WorkRecord",
                column: "WorkBraceletId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkBracelet_Bracelet_BraceletId",
                table: "WorkBracelet",
                column: "BraceletId",
                principalTable: "Bracelet",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkRecord_WorkBracelet_WorkBraceletId",
                table: "WorkRecord",
                column: "WorkBraceletId",
                principalTable: "WorkBracelet",
                principalColumn: "Id");
        }
    }
}
