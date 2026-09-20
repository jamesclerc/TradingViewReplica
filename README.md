<div align="center">

# TradingView Replica

**A TradingView-style market dashboard, built from scratch with .NET and React — backed by Yahoo Finance.**

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react&logoColor=black)
![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript&logoColor=white)
![Vite](https://img.shields.io/badge/Vite-8-646CFF?logo=vite&logoColor=white)
![klinecharts](https://img.shields.io/badge/klinecharts-10-26A69A?logo=chartdotjs&logoColor=white)

</div>

---

## Contents

- [What's in here](#whats-in-here)
- [Features](#features)
- [Tech stack](#tech-stack)
- [Getting started](#getting-started)
- [Project structure](#project-structure)
- [API overview](#api-overview)
- [Data source & limitations](#data-source--limitations)
- [Roadmap](#roadmap)

## What's in here

A market screener and interactive chart in the style of TradingView: browse stocks, ETFs,
crypto and funds through Yahoo Finance's predefined screens, customize the table to your
liking, and pop open a live, indicator-capable candlestick chart for any symbol — all built
as a full-stack .NET + React project from an empty repo.

## Features

**Screener**
- Category tabs — **Stocks · ETFs · Crypto · Funds** — each backed by real Yahoo Finance
  screens (Most Active, Day Gainers/Losers, Growth Technology, Undervalued Large/Growth Caps,
  Most Shorted, Top ETFs, All Cryptocurrencies, six fund screens, and more)
- Fully customizable table: show/hide any of 15 columns, **drag to reorder**, **drag to
  resize** — all backed by real fields from Yahoo (price, change %, volume, market cap, P/E,
  forward P/E, dividend yield, analyst rating, exchange, 52-week high/low, day high/low,
  average volume, EPS)
- Company logos next to every symbol, with a clean fallback for tickers that don't have one
- Color-coded analyst rating badges (Strong Buy → Sell)

**Chart**
- Candlestick chart in a side panel you can open, close, and **freely resize** by dragging
- **27 built-in technical indicators** (MA, EMA, BOLL, MACD, RSI, KDJ, and more) via a
  searchable picker — toggle them without reloading the chart
- Updates live: polls for the latest bar every 10 seconds so the current candle moves without
  a manual refresh
- Multiple timeframes (5m → 1M), each backed by real Yahoo intraday/daily data

**Search**
- Live symbol autocomplete with logos, across equities, ETFs, futures, and more

Dark theme throughout, styled after TradingView's own palette.

## Tech stack

| | |
|---|---|
| **Backend** | .NET 10 · ASP.NET Core Minimal APIs · EF Core + SQLite · xUnit |
| **Frontend** | React 19 + TypeScript · Vite · [klinecharts](https://klinecharts.com) · TanStack React Query · Zustand |
| **Data** | Yahoo Finance (unofficial endpoints), isolated behind an `IMarketDataProvider` interface |

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org) (LTS)

### Setup & run

The backend and frontend run as two separate processes in development.

```bash
# Terminal 1 — API (applies EF Core migrations automatically on first run)
dotnet run --project src/TradingViewReplica.Api
```

```bash
# Terminal 2 — frontend
cd frontend
npm install
npm run dev
```

Then open **http://localhost:5173**. The Vite dev server proxies `/api` requests through to
the backend, so no extra configuration is needed.

| Service | URL |
|---|---|
| Frontend | http://localhost:5173 |
| API (HTTPS) | https://localhost:5250 |
| API (HTTP) | http://localhost:5251 |

### Running the tests

```bash
dotnet test
```

## Project structure

```
TradingViewReplica.slnx
src/
  TradingViewReplica.Api/          ASP.NET Core Web API, EF Core, caching, endpoints
  TradingViewReplica.MarketData/   Yahoo Finance integration, isolated behind IMarketDataProvider
tests/
  TradingViewReplica.MarketData.Tests/
frontend/
  src/
    components/                    screener, chart, and search UI
    state/                         Zustand stores (UI state, screener columns)
    hooks/, api/                   React Query hooks + REST client
```

## API overview

| Endpoint | Description |
|---|---|
| `GET /api/screener/definitions` | List available screens, grouped by category |
| `GET /api/screener/{screenerId}` | Rows for a given screen (Most Active, Top ETFs, ...) |
| `GET /api/symbols/search?q=` | Symbol/company search |
| `GET /api/candles/{symbol}` | Historical candles for a symbol/interval/range |
| `GET /api/candles/{symbol}/latest` | Most recent bar — polled by the chart to stay live |

## Data source & limitations

This project uses Yahoo Finance's **unofficial**, undocumented endpoints — there is no
official free Yahoo Finance API. A few things follow from that:

- Yahoo can change or rate-limit these endpoints without notice. The whole integration lives
  behind `IMarketDataProvider` in `TradingViewReplica.MarketData` specifically so the data
  source could be swapped later without touching the rest of the app.
- Data is delayed (~15–20 minutes), same as Yahoo's own free tier.
- This is a personal/learning project — not intended for production or commercial use.
- Single-user, no authentication.
- Company logos come from [Financial Modeling Prep](https://financialmodelingprep.com)'s free
  logo endpoint.

## Roadmap

- [ ] Persist UI preferences (columns, chart panel width, indicators) across reloads
- [ ] Watchlist
- [ ] Drawing tools on the chart (trendlines, Fibonacci retracement)
- [ ] Frontend test coverage + CI
