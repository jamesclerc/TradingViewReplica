import { apiGet } from './httpClient'
import type { Candle, Interval, MarketDataRange } from '../types/market'

export function fetchCandles(
  symbol: string,
  interval: Interval,
  range: MarketDataRange,
): Promise<Candle[]> {
  const params = new URLSearchParams({ interval, range })
  return apiGet<Candle[]>(`/api/candles/${encodeURIComponent(symbol)}?${params.toString()}`)
}

export function fetchLatestCandle(symbol: string, interval: Interval): Promise<Candle> {
  const params = new URLSearchParams({ interval })
  return apiGet<Candle>(`/api/candles/${encodeURIComponent(symbol)}/latest?${params.toString()}`)
}
