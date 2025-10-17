using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YixiaoAdmin.EntityFrameworkCore.Migrations
{
    public partial class NewCreat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "WorkOrder",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "WorkOrder",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GasAlarm",
                table: "WorkOrder",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ToxicGasAlarmOnlineStatus",
                table: "WorkOrder",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Bracelet",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BraceletNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BraceletRecord = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Bracelet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personnel",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Personnel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkBracelet",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryExitStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyCallStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExitTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_WorkBracelet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkRecord",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkOrderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    WorkBraceletId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HeartRate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryExitStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyCallStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_WorkRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkRecord_WorkBracelet_WorkBraceletId",
                        column: x => x.WorkBraceletId,
                        principalTable: "WorkBracelet",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WorkRecord_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkRecord_WorkBraceletId",
                table: "WorkRecord",
                column: "WorkBraceletId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkRecord_WorkOrderId",
                table: "WorkRecord",
                column: "WorkOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bracelet");

            migrationBuilder.DropTable(
                name: "Personnel");

            migrationBuilder.DropTable(
                name: "WorkRecord");

            migrationBuilder.DropTable(
                name: "WorkBracelet");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "GasAlarm",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "ToxicGasAlarmOnlineStatus",
                table: "WorkOrder");
        }
    }
}
