// Mirrors the backend's Contracts DTOs (src/TradingViewReplica.Api/Contracts) - kept in sync by hand.

export interface Candle {
  timestampMs: number
  open: number
  high: number
  low: number
  close: number
  volume: number
}

export interface SymbolSearchResult {
  symbol: string
  displayName: string
  longName: string | null
  exchangeDisplay: string | null
  quoteType: string | null
}

// String values match TradingViewReplica.MarketData.Models.Interval exactly - the API binds
// query params via Enum.TryParse, so these must stay in lockstep with the C# enum names.
export type Interval =
  | 'OneMinute'
  | 'TwoMinutes'
  | 'FiveMinutes'
  | 'FifteenMinutes'
  | 'ThirtyMinutes'
  | 'SixtyMinutes'
  | 'NinetyMinutes'
  | 'OneHour'
  | 'OneDay'
  | 'FiveDays'
  | 'OneWeek'
  | 'OneMonth'
  | 'ThreeMonths'

// String values match TradingViewReplica.MarketData.Models.MarketDataRange exactly.
export type MarketDataRange =
  | 'OneDay'
  | 'FiveDays'
  | 'OneMonth'
  | 'ThreeMonths'
  | 'SixMonths'
  | 'YearToDate'
  | 'OneYear'
  | 'TwoYears'
  | 'FiveYears'
  | 'TenYears'
  | 'Max'

export interface ScreenerQuote {
  symbol: string
  displayName: string
  price: number
  changePercent: number
  volume: number | null
  marketCap: number | null
  trailingPE: number | null
  dividendYield: number | null
  analystRating: string | null
  exchange: string | null
  fiftyTwoWeekHigh: number | null
  fiftyTwoWeekLow: number | null
  dayHigh: number | null
  dayLow: number | null
  averageVolume3Month: number | null
  forwardPE: number | null
  eps: number | null
}

export interface ScreenerResult {
  title: string
  total: number
  quotes: ScreenerQuote[]
}

export interface ScreenerDefinition {
  id: string
  label: string
  category: string
}
