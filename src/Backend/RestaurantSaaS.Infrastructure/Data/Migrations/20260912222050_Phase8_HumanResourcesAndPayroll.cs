using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantSaaS.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase8_HumanResourcesAndPayroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Designation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    NationalId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BiometricUserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateOfJoining = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BaseMonthlySalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    HourlyOvertimeRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_employees_branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_employees_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "payroll_runs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalGrossPay = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDeductions = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalNetPay = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "attendance_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClockInTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClockOutTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalHoursWorked = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    OvertimeHours = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    DeviceIdentifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_attendance_records_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staff_advances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MonthlyDeductionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsFullyRepaid = table.Column<bool>(type: "boolean", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_advances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_staff_advances_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payroll_details",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DaysPresent = table.Column<int>(type: "integer", nullable: false),
                    DaysAbsent = table.Column<int>(type: "integer", nullable: false),
                    OvertimePay = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AdvanceDeduction = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OtherDeductions = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetSalaryPayable = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payroll_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payroll_details_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payroll_details_payroll_runs_PayrollRunId",
                        column: x => x.PayrollRunId,
                        principalTable: "payroll_runs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_records_EmployeeId_WorkDate",
                table: "attendance_records",
                columns: new[] { "EmployeeId", "WorkDate" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_records_RestaurantId_BranchId_WorkDate",
                table: "attendance_records",
                columns: new[] { "RestaurantId", "BranchId", "WorkDate" });

            migrationBuilder.CreateIndex(
                name: "IX_employees_BranchId",
                table: "employees",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_employees_RestaurantId_EmployeeCode",
                table: "employees",
                columns: new[] { "RestaurantId", "EmployeeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_employees_UserId",
                table: "employees",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_details_EmployeeId",
                table: "payroll_details",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_payroll_details_PayrollRunId_EmployeeId",
                table: "payroll_details",
                columns: new[] { "PayrollRunId", "EmployeeId" });

            migrationBuilder.CreateIndex(
                name: "IX_payroll_runs_RestaurantId_BranchId_Year_Month",
                table: "payroll_runs",
                columns: new[] { "RestaurantId", "BranchId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_advances_EmployeeId",
                table: "staff_advances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_staff_advances_RestaurantId_EmployeeId",
                table: "staff_advances",
                columns: new[] { "RestaurantId", "EmployeeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_records");

            migrationBuilder.DropTable(
                name: "payroll_details");

            migrationBuilder.DropTable(
                name: "staff_advances");

            migrationBuilder.DropTable(
                name: "payroll_runs");

            migrationBuilder.DropTable(
                name: "employees");
        }
    }
}
