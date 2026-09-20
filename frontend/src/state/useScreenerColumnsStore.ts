import { create } from 'zustand'
import type { ScreenerQuote } from '../types/market'

export type ScreenerColumnKey = Exclude<keyof ScreenerQuote, 'symbol' | 'displayName'>

interface ColumnDef {
  key: ScreenerColumnKey
  label: string
}

// The full set of columns the screener can show - a superset of what's visible by default.
// "Add columns" (from the user's request) means picking from here, not new backend fields.
export const ALL_SCREENER_COLUMNS: ColumnDef[] = [
  { key: 'price', label: 'Price' },
  { key: 'changePercent', label: 'Chg %' },
  { key: 'volume', label: 'Volume' },
  { key: 'marketCap', label: 'Market Cap' },
  { key: 'trailingPE', label: 'P/E' },
  { key: 'dividendYield', label: 'Div Yield' },
  { key: 'analystRating', label: 'Rating' },
  { key: 'exchange', label: 'Exchange' },
  { key: 'fiftyTwoWeekHigh', label: '52W High' },
  { key: 'fiftyTwoWeekLow', label: '52W Low' },
  { key: 'dayHigh', label: 'Day High' },
  { key: 'dayLow', label: 'Day Low' },
  { key: 'averageVolume3Month', label: 'Avg Vol (3M)' },
  { key: 'forwardPE', label: 'Fwd P/E' },
  { key: 'eps', label: 'EPS' },
]

const DEFAULT_VISIBLE: ScreenerColumnKey[] = [
  'price',
  'changePercent',
  'volume',
  'marketCap',
  'trailingPE',
  'dividendYield',
  'analystRating',
]

export const DEFAULT_COLUMN_WIDTH = 110
export const SYMBOL_COLUMN_KEY = 'symbol'
const MIN_COLUMN_WIDTH = 60

// Tracks the display order of ALL columns (visible or not), so hiding then re-showing a column
// restores it to its last position instead of snapping back to ALL_SCREENER_COLUMNS' order.
const DEFAULT_ORDER: ScreenerColumnKey[] = ALL_SCREENER_COLUMNS.map((c) => c.key)

interface ScreenerColumnsState {
  visibleColumns: ScreenerColumnKey[]
  columnOrder: ScreenerColumnKey[]
  columnWidths: Partial<Record<string, number>>
  toggleColumn: (key: ScreenerColumnKey) => void
  setColumnWidth: (key: string, width: number) => void
  moveColumnBefore: (key: ScreenerColumnKey, beforeKey: ScreenerColumnKey | null) => void
}

export const useScreenerColumnsStore = create<ScreenerColumnsState>((set) => ({
  visibleColumns: DEFAULT_VISIBLE,
  columnOrder: DEFAULT_ORDER,
  columnWidths: {},
  toggleColumn: (key) =>
    set((state) => ({
      visibleColumns: state.visibleColumns.includes(key)
        ? state.visibleColumns.filter((k) => k !== key)
        : [...state.visibleColumns, key],
    })),
  setColumnWidth: (key, width) =>
    set((state) => ({
      columnWidths: { ...state.columnWidths, [key]: Math.max(MIN_COLUMN_WIDTH, width) },
    })),
  moveColumnBefore: (key, beforeKey) =>
    set((state) => {
      const withoutKey = state.columnOrder.filter((k) => k !== key)
      if (beforeKey === null) {
        return { columnOrder: [...withoutKey, key] }
      }
      const targetIndex = withoutKey.indexOf(beforeKey)
      const next = [...withoutKey]
      next.splice(targetIndex, 0, key)
      return { columnOrder: next }
    }),
}))
