import { useEffect, useRef } from 'react'
import { dispose, init } from 'klinecharts'
import type { Chart, KLineData } from 'klinecharts'
import { fetchCandles, fetchLatestCandle } from '../../api/candles'
import type { Interval, MarketDataRange } from '../../types/market'
import { theme } from '../../styles/theme'
import { toKLinePeriod } from './period'

interface KLineChartWidgetProps {
  symbol: string
  interval: Interval
  range: MarketDataRange
  indicators: string[]
}

// No push infra (SignalR etc.) - just polls the lightweight "latest bar" endpoint and feeds it
// into klinecharts' own live-update callback. Yahoo's free data is itself delayed ~15-20 min
// regardless, so polling much faster than this wouldn't surface meaningfully fresher prices.
const LIVE_POLL_INTERVAL_MS = 10_000

function toKLineData(candle: {
  timestampMs: number
  open: number
  high: number
  low: number
  close: number
  volume: number
}): KLineData {
  return {
    timestamp: candle.timestampMs,
    open: candle.open,
    high: candle.high,
    low: candle.low,
    close: candle.close,
    volume: candle.volume,
  }
}

export function KLineChartWidget({ symbol, interval, range, indicators }: KLineChartWidgetProps) {
  const containerRef = useRef<HTMLDivElement>(null)
  const chartRef = useRef<Chart | null>(null)

  // Re-inits the whole chart on any of [symbol, interval, range] change rather than trying to
  // trigger klinecharts' internal forward/backward pagination for a range switch - klinecharts
  // has no native "range" concept (only period + forward/backward paging by timestamp), so this
  // keeps the REST call's range param authoritative and avoids fighting the library's own
  // pagination state machine. True infinite-scroll pagination is a later enhancement.
  useEffect(() => {
    const container = containerRef.current
    if (!container || !symbol) {
      return
    }

    // Traced live: klinecharts calls subscribeBar asynchronously, after getBars' promise
    // resolves - not synchronously during setDataLoader/setSymbol/setPeriod. dispose() doesn't
    // cancel that in-flight chain, so in React StrictMode (whose dev-only double-invoke tears
    // this effect down and back up immediately) the FIRST instance's getBars can still resolve
    // and call subscribeBar *after* its own cleanup already ran, permanently orphaning an
    // interval - confirmed by tracing actual call order, not assumed. `disposed` is a plain
    // closure variable (not a ref) so each effect invocation gets its own independent flag;
    // every async callback checks it before doing anything, so a stale invocation's callbacks
    // become no-ops instead of creating work nothing will ever clean up.
    let disposed = false
    const liveIntervals = new Set<number>()

    const chart = init(container, {
      styles: {
        candle: {
          bar: {
            upColor: theme.bullish,
            downColor: theme.bearish,
            noChangeColor: theme.textSecondary,
            upBorderColor: theme.bullish,
            downBorderColor: theme.bearish,
            noChangeBorderColor: theme.textSecondary,
            upWickColor: theme.bullish,
            downWickColor: theme.bearish,
            noChangeWickColor: theme.textSecondary,
          },
        },
        grid: {
          horizontal: { color: theme.border },
          vertical: { color: theme.border },
        },
      },
    })
    chartRef.current = chart

    if (chart) {
      chart.setDataLoader({
        getBars: async ({ callback }) => {
          try {
            const candles = await fetchCandles(symbol, interval, range)
            if (disposed) return
            callback(candles.map(toKLineData), false)
          } catch {
            if (!disposed) callback([], false)
          }
        },
        subscribeBar: ({ callback }) => {
          if (disposed) return
          const id = window.setInterval(async () => {
            if (disposed) return
            try {
              const candle = await fetchLatestCandle(symbol, interval)
              if (disposed) return
              callback(toKLineData(candle))
            } catch {
              // Transient failure: just skip this tick, the next poll retries on its own.
            }
          }, LIVE_POLL_INTERVAL_MS)
          liveIntervals.add(id)
        },
        unsubscribeBar: () => {
          liveIntervals.forEach((id) => window.clearInterval(id))
          liveIntervals.clear()
        },
      })

      chart.setSymbol({ ticker: symbol })
      chart.setPeriod(toKLinePeriod(interval))
    }

    const resizeObserver = new ResizeObserver(() => chart?.resize())
    resizeObserver.observe(container)

    return () => {
      disposed = true
      resizeObserver.disconnect()
      dispose(container)
      chartRef.current = null
      // Defensive: dispose() is expected to invoke unsubscribeBar on its own, but don't leak
      // any poll that turns out not to have been covered by that.
      liveIntervals.forEach((id) => window.clearInterval(id))
      liveIntervals.clear()
    }
  }, [symbol, interval, range])

  // Separate effect so toggling an indicator doesn't re-init the chart (and re-fetch candles) -
  // it diffs against whatever's currently applied and only adds/removes what changed. Also
  // re-runs after [symbol, interval, range] changes recreate the chart above, so indicators
  // carry over onto the fresh instance instead of resetting.
  useEffect(() => {
    const chart = chartRef.current
    if (!chart) {
      return
    }

    const applied = new Set(chart.getIndicators().map((i) => i.name))
    for (const name of indicators) {
      if (!applied.has(name)) {
        chart.createIndicator(name)
      }
    }
    for (const name of applied) {
      if (!indicators.includes(name)) {
        chart.removeIndicator({ name })
      }
    }
  }, [indicators, symbol, interval, range])

  return <div ref={containerRef} className="kline-chart-widget" />
}
