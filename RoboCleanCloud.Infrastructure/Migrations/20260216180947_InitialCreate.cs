using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoboCleanCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Robots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FriendlyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConnectionStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BatteryLevel = table.Column<int>(type: "integer", nullable: false),
                    FirmwareVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DustbinLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Robots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CleaningSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RobotId = table.Column<Guid>(type: "uuid", nullable: false),
                    CronExpression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ZoneIds = table.Column<string>(type: "jsonb", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastTriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "UTC"),
                    QuietHoursStart = table.Column<int>(type: "integer", nullable: true),
                    QuietHoursEnd = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleaningSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CleaningSchedules_Robots_RobotId",
                        column: x => x.RobotId,
                        principalTable: "Robots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RobotId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CurrentHealth = table.Column<int>(type: "integer", nullable: false),
                    LastReplacedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstimatedDaysLeft = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceItems_Robots_RobotId",
                        column: x => x.RobotId,
                        principalTable: "Robots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RobotStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RobotId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NewStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RobotStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RobotStatusHistories_Robots_RobotId",
                        column: x => x.RobotId,
                        principalTable: "Robots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CleaningSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RobotId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ZoneIds = table.Column<string>(type: "jsonb", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AreaCleaned = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true),
                    EnergyConsumed = table.Column<double>(type: "double precision", precision: 10, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleaningSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CleaningSessions_CleaningSchedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "CleaningSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CleaningSessions_Robots_RobotId",
                        column: x => x.RobotId,
                        principalTable: "Robots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CleaningErrors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Resolution = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleaningErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CleaningErrors_CleaningSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "CleaningSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CleaningErrors_ErrorCode",
                table: "CleaningErrors",
                column: "ErrorCode");

            migrationBuilder.CreateIndex(
                name: "IX_CleaningErrors_SessionId",
                table: "CleaningErrors",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CleaningErrors_Timestamp",
                table: "CleaningErrors",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_CleaningSchedules_IsActive",
                table: "CleaningSchedules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CleaningSchedules_LastTriggeredAt",
                table: "CleaningSchedules",
                column: "LastTriggeredAt");

            migrationBuilder.CreateIndex(
                name: "IX_CleaningSchedules_RobotId",
                table: "CleaningSchedules",
                column: "RobotId");

            migrationBuilder.CreateIndex(
                name: "IX_CleaningSessions_RobotId",
                table: "CleaningSessions",
                column: "RobotId");

            migrationBuilder.CreateIndex(
                name: "IX_CleaningSessions_ScheduleId",
                table: "CleaningSessions",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_CleaningSessions_StartedAt",
                table: "CleaningSessions",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CleaningSessions_Status",
                table: "CleaningSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceItems_CurrentHealth",
                table: "MaintenanceItems",
                column: "CurrentHealth");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceItems_RobotId",
                table: "MaintenanceItems",
                column: "RobotId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceItems_Type",
                table: "MaintenanceItems",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Robots_ConnectionStatus",
                table: "Robots",
                column: "ConnectionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Robots_LastSeenAt",
                table: "Robots",
                column: "LastSeenAt");

            migrationBuilder.CreateIndex(
                name: "IX_Robots_OwnerId",
                table: "Robots",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Robots_SerialNumber",
                table: "Robots",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RobotStatusHistories_RobotId",
                table: "RobotStatusHistories",
                column: "RobotId");

            migrationBuilder.CreateIndex(
                name: "IX_RobotStatusHistories_Timestamp",
                table: "RobotStatusHistories",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CleaningErrors");

            migrationBuilder.DropTable(
                name: "MaintenanceItems");

            migrationBuilder.DropTable(
                name: "RobotStatusHistories");

            migrationBuilder.DropTable(
                name: "CleaningSessions");

            migrationBuilder.DropTable(
                name: "CleaningSchedules");

            migrationBuilder.DropTable(
                name: "Robots");
        }
    }
}
