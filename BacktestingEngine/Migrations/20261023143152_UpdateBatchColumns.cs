using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacktestingEngine.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBatchColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                schema: "public",
                table: "backtest_batches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Interval",
                schema: "public",
                table: "backtest_batches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RunsCount",
                schema: "public",
                table: "backtest_batches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                schema: "public",
                table: "backtest_batches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                schema: "public",
                table: "backtest_batches",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                schema: "public",
                table: "backtest_batches");

            migrationBuilder.DropColumn(
                name: "Interval",
                schema: "public",
                table: "backtest_batches");

            migrationBuilder.DropColumn(
                name: "RunsCount",
                schema: "public",
                table: "backtest_batches");

            migrationBuilder.DropColumn(
                name: "StartDate",
                schema: "public",
                table: "backtest_batches");

            migrationBuilder.DropColumn(
                name: "Symbol",
                schema: "public",
                table: "backtest_batches");
        }
    }
}
