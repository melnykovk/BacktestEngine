using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacktestingEngine.Migrations
{
    /// <inheritdoc />
    public partial class updateEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExecutionTimeMs",
                schema: "public",
                table: "backtest_batches",
                newName: "ExecutionTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExecutionTime",
                schema: "public",
                table: "backtest_batches",
                newName: "ExecutionTimeMs");
        }
    }
}
