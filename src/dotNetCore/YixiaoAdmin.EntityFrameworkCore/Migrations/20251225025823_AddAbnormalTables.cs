using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YixiaoAdmin.EntityFrameworkCore.Migrations
{
    public partial class AddAbnormalTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AbnormalConfig",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConfigType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConfigName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinValue = table.Column<float>(type: "real", nullable: false),
                    MaxValue = table.Column<float>(type: "real", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AbnormalConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BraceletAbnormal",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkOrderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    HeartRate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryExitStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_BraceletAbnormal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BraceletAbnormal_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GasAbnormal",
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
                    table.PrimaryKey("PK_GasAbnormal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GasAbnormal_WorkOrder_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrder",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BraceletAbnormal_WorkOrderId",
                table: "BraceletAbnormal",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GasAbnormal_WorkOrderId",
                table: "GasAbnormal",
                column: "WorkOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbnormalConfig");

            migrationBuilder.DropTable(
                name: "BraceletAbnormal");

            migrationBuilder.DropTable(
                name: "GasAbnormal");
        }
    }
}



