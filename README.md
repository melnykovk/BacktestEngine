BacktestEngine

Event-driven backtesting engine for grid trading strategies in C#/.NET, with conservative intrabar fill modelling and no-lookahead guarantees.

Overview

Simulates grid trading strategies against historical market data candle-by-candle. Data is fetched from Binance and stored locally, then backtested over any symbol, timeframe, and date range. Every decision is computed on candle close using only past data — no-lookahead is enforced structurally, not by discipline.

This is a research/simulation tool, not a live trading system: it prioritizes correctness and reproducibility, and documents its simplifying assumptions explicitly.

Key design decisions


Event-driven, not vectorized — each candle is a discrete event processed in strict chronological order. No-lookahead becomes a structural property: a candle physically cannot see a future candle.
Conservative intrabar fills — limit orders fill on intrabar touch (Low <= price for buys, High >= price for sells), not on close. Fills execute at the order price — a never-optimistic assumption for limit orders.
Timeframe decoupling — indicator timeframes are independent of the engine timeframe. Intervals are encoded as seconds, reducing "base candles per metric candle" to arithmetic (target / base) with a divisibility check. Any timeframe combination is configured, not coded.
Separated data models — distinct types for the runtime candle (EngineCandle, an immutable record struct on the hot path), the stored entity, and a denormalized dataset summary holding precomputed aggregates — so listing datasets never runs COUNT over millions of rows.
Layered strategy model (in progress) — a Metrics → Rules → Policy hierarchy separates computing signals from reacting to them from managing decisions (a position/mode state machine). Seams follow axes of change, so adding a rule doesn't touch the infrastructure.


Architecture

### Components

- **Engine** - core execution environment responsible for running backtests.
- **Idea** - defines a trading hypothesis and combines strategy configuration.
- **Strategy** - generates trading decisions.
- **Decision System** - evaluates market conditions using metrics, rules and policies.
- **Execution Model** - simulates order execution.
- **Portfolio** - tracks positions, balance and state.
- **Statistics Collector** - aggregates backtest results.

![Architecture](docs/Engine%20architecture.svg)

Data flows one way; lower layers know nothing about higher ones. RollingMean takes a decimal and knows nothing about candles; the aggregator collapses base candles into a metric candle and knows nothing about SMA; execution knows nothing about strategy logic. Each layer is testable in isolation.

Tech stack

.NET 10 · ASP.NET Core (Web API + web UI) · PostgreSQL via EF Core (code-first, Npgsql) · Binance.Net (REST + websocket) · SignalR · xUnit

Features

Working


Ingest OHLCV candles from Binance into PostgreSQL for any symbol / timeframe / range; add or remove datasets on demand.
Backtest a grid strategy over any stored dataset, by timeframe and date range.
Configurable per run: fee rate, order size, starting balance, grid step.
Web UI for dataset management and running backtests, with real-time updates.


In progress


Metrics → Rules → Policy layer for expressing strategy behaviour.
SMA indicators with multi-timeframe aggregation.


Getting started

Prerequisites: .NET 10 SDK, PostgreSQL.


Set the connection string in appsettings.json (ConnectionStrings:DefaultConnection).
Apply migrations: dotnet ef database update
Run: dotnet run
Open the web UI in a browser.


Testing

Unit tests (xUnit) cover the correctness-critical paths:


RollingMean — average correctness, null before the window fills, window sliding, invalid-period guard.
Timeframe — base-candle ratio across intervals, divisibility and ordering guards.
GridStrategy — geometric level construction (buy/sell prices).
Portfolio — starting balance and invariants.
ExecutionModel — intrabar fill on both sides (buy on low-touch, sell on high-touch, and the negative cases), plus the guard against selling with no open position.


bashdotnet test

Assumptions & scope

Deliberate simplifications of a simulation engine:


No partial fills, no slippage — fills are full, at the order price.
Execution price equals order price; actual execution price can't be reconstructed from OHLC (no order book / tick data) and would come from the exchange in a live system.
Data is assumed gap-free (validated on ingestion), so higher-timeframe aggregation uses candle counts, not wall-clock boundaries.


Roadmap

v2


Complete the Metrics → Rules → Policy layer; multiple indicators (EMA, RSI, …), each independently parameterized, sharing a reusable rolling-window primitive.
Distributed parameter sweeps. With several parameterized indicators plus fine-grained timeframes (down to 1s), the parameter space grows combinatorially and individual runs get heavier. Since each run is an independent, stateless unit (candles in, stats out), the workload is embarrassingly parallel — designed to fan out across cloud workers (AWS/Azure) over a shared dataset store, so large sweeps run in parallel rather than sequentially.


v3


Hedging / short-side rules and capital allocation across sub-strategies.
Richer statistics and reporting.
