using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BacktestingEngine.Migrations
{
    /// <inheritdoc />
    public partial class CreateBinanceKlinesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "binance_klines",
                schema: "public",
                columns: table => new
                {
                    symbol = table.Column<string>(type: "text", nullable: false),
                    interval = table.Column<string>(type: "text", nullable: false),
                    open_time_ms = table.Column<long>(type: "bigint", nullable: false),
                    open = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    high = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    low = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    close = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    volume = table.Column<decimal>(type: "numeric(28,8)", precision: 28, scale: 8, nullable: false),
                    close_time_ms = table.Column<long>(type: "bigint", nullable: false),
                    quote_asset_vol = table.Column<decimal>(type: "numeric(28,8)", precision: 28, scale: 8, nullable: false),
                    trades_count = table.Column<long>(type: "bigint", nullable: false),
                    taker_buy_base = table.Column<decimal>(type: "numeric(28,8)", precision: 28, scale: 8, nullable: false),
                    taker_buy_quote = table.Column<decimal>(type: "numeric(28,8)", precision: 28, scale: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_binance_klines", x => new { x.symbol, x.interval, x.open_time_ms });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "binance_klines",
                schema: "public");
        }
    }
}
