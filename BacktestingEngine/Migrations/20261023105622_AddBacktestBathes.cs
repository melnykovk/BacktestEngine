using System;
using System.Collections.Generic;
using BacktestingEngine.DTO;
using BacktestingEngine.Engine.Core;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacktestingEngine.Migrations
{
    /// <inheritdoc />
    public partial class AddBacktestBathes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "backtest_batches",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RangeData = table.Column<RangeBacktestData>(type: "jsonb", nullable: false),
                    Stats = table.Column<List<BacktestStats>>(type: "jsonb", nullable: false),
                    ExecutionTimeMs = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_backtest_batches", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "backtest_batches",
                schema: "public");
        }
    }
}
