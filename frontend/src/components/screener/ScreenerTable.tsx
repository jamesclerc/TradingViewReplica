import { useState } from 'react'
import type { ScreenerQuote } from '../../types/market'
import { formatCompactNumber, formatPercent, formatPrice } from '../../utils/format'
import { RatingBadge } from './RatingBadge'
import { ColumnResizeHandle } from './ColumnResizeHandle'
import { SymbolLogo } from '../common/SymbolLogo'
import {
  ALL_SCREENER_COLUMNS,
  DEFAULT_COLUMN_WIDTH,
  useScreenerColumnsStore,
  type ScreenerColumnKey,
} from '../../state/useScreenerColumnsStore'

type SortKey = 'symbol' | ScreenerColumnKey

interface ScreenerTableProps {
  quotes: ScreenerQuote[]
  onSelectSymbol: (symbol: string) => void
}

const SYMBOL_COLUMN_KEY = 'symbol'
const DEFAULT_SYMBOL_WIDTH = 170
const COLUMN_DRAG_MIME = 'application/x-screener-column'

function getSortValue(quote: ScreenerQuote, key: SortKey): number | string | null {
  if (key === 'symbol') {
    return quote.symbol
  }
  return quote[key]
}

function renderCell(quote: ScreenerQuote, key: ScreenerColumnKey) {
  switch (key) {
    case 'price':
      return formatPrice(quote.price)
    case 'changePercent':
      return (
        <span className={quote.changePercent >= 0 ? 'positive' : 'negative'}>
          {formatPercent(quote.changePercent)}
        </span>
      )
    case 'volume':
      return formatCompactNumber(quote.volume)
    case 'marketCap':
      return formatCompactNumber(quote.marketCap)
    case 'trailingPE':
      return quote.trailingPE !== null ? quote.trailingPE.toFixed(1) : '—'
    case 'forwardPE':
      return quote.forwardPE !== null ? quote.forwardPE.toFixed(1) : '—'
    case 'dividendYield':
      return quote.dividendYield !== null ? `${quote.dividendYield.toFixed(2)}%` : '—'
    case 'analystRating':
      return <RatingBadge rating={quote.analystRating} />
    case 'exchange':
      return quote.exchange ?? '—'
    case 'fiftyTwoWeekHigh':
      return quote.fiftyTwoWeekHigh !== null ? formatPrice(quote.fiftyTwoWeekHigh) : '—'
    case 'fiftyTwoWeekLow':
      return quote.fiftyTwoWeekLow !== null ? formatPrice(quote.fiftyTwoWeekLow) : '—'
    case 'dayHigh':
      return quote.dayHigh !== null ? formatPrice(quote.dayHigh) : '—'
    case 'dayLow':
      return quote.dayLow !== null ? formatPrice(quote.dayLow) : '—'
    case 'averageVolume3Month':
      return formatCompactNumber(quote.averageVolume3Month)
    case 'eps':
      return quote.eps !== null ? quote.eps.toFixed(2) : '—'
    default:
      return '—'
  }
}

export function ScreenerTable({ quotes, onSelectSymbol }: ScreenerTableProps) {
  const visibleColumns = useScreenerColumnsStore((s) => s.visibleColumns)
  const columnOrder = useScreenerColumnsStore((s) => s.columnOrder)
  const columnWidths = useScreenerColumnsStore((s) => s.columnWidths)
  const setColumnWidth = useScreenerColumnsStore((s) => s.setColumnWidth)
  const moveColumnBefore = useScreenerColumnsStore((s) => s.moveColumnBefore)

  const [sortKey, setSortKey] = useState<SortKey>('marketCap')
  const [sortDesc, setSortDesc] = useState(true)
  const [draggedKey, setDraggedKey] = useState<ScreenerColumnKey | null>(null)
  const [dragOverKey, setDragOverKey] = useState<ScreenerColumnKey | null>(null)

  function toggleSort(key: SortKey) {
    if (key === sortKey) {
      setSortDesc((prev) => !prev)
    } else {
      setSortKey(key)
      setSortDesc(true)
    }
  }

  function handleDragStart(e: React.DragEvent<HTMLSpanElement>, key: ScreenerColumnKey) {
    setDraggedKey(key)
    e.dataTransfer.effectAllowed = 'move'
    e.dataTransfer.setData(COLUMN_DRAG_MIME, key)
  }

  function handleDragOver(e: React.DragEvent<HTMLTableCellElement>, key: ScreenerColumnKey) {
    e.preventDefault()
    setDragOverKey(key)
  }

  function handleDrop(e: React.DragEvent<HTMLTableCellElement>, key: ScreenerColumnKey) {
    e.preventDefault()
    // Read the source from dataTransfer, not the draggedKey state: dragstart's setDraggedKey
    // hasn't necessarily flushed and re-rendered by the time drop fires, but dataTransfer is
    // set synchronously on the native event and is available immediately either way.
    const sourceKey = e.dataTransfer.getData(COLUMN_DRAG_MIME) as ScreenerColumnKey | ''
    if (sourceKey && sourceKey !== key) {
      moveColumnBefore(sourceKey, key)
    }
    setDraggedKey(null)
    setDragOverKey(null)
  }

  function handleDragEnd() {
    setDraggedKey(null)
    setDragOverKey(null)
  }

  const columns = columnOrder
    .filter((key) => visibleColumns.includes(key))
    .map((key) => ALL_SCREENER_COLUMNS.find((col) => col.key === key)!)

  const sorted = [...quotes].sort((a, b) => {
    const av = getSortValue(a, sortKey)
    const bv = getSortValue(b, sortKey)
    if (av === null) return 1
    if (bv === null) return -1
    if (typeof av === 'string' || typeof bv === 'string') {
      return sortDesc ? String(bv).localeCompare(String(av)) : String(av).localeCompare(String(bv))
    }
    return sortDesc ? bv - av : av - bv
  })

  return (
    <div className="screener-table-wrap">
      <table className="screener-table screener-table-fixed">
        <colgroup>
          <col style={{ width: columnWidths[SYMBOL_COLUMN_KEY] ?? DEFAULT_SYMBOL_WIDTH }} />
          {columns.map((col) => (
            <col key={col.key} style={{ width: columnWidths[col.key] ?? DEFAULT_COLUMN_WIDTH }} />
          ))}
        </colgroup>
        <thead>
          <tr>
            <th onClick={() => toggleSort('symbol')}>
              Symbol
              {sortKey === 'symbol' ? (sortDesc ? ' ▼' : ' ▲') : ''}
              <ColumnResizeHandle
                currentWidth={columnWidths[SYMBOL_COLUMN_KEY] ?? DEFAULT_SYMBOL_WIDTH}
                onResize={(w) => setColumnWidth(SYMBOL_COLUMN_KEY, w)}
              />
            </th>
            {columns.map((col) => (
              <th
                key={col.key}
                onClick={() => toggleSort(col.key)}
                // Drop is detected over the whole header (bigger, easier target); the drag
                // SOURCE is only the label span below, so the resize handle's pixels never
                // carry a draggable ancestor at all - no override needed, no ambiguity.
                onDragOver={(e) => handleDragOver(e, col.key)}
                onDrop={(e) => handleDrop(e, col.key)}
                className={[
                  draggedKey === col.key ? 'dragging' : '',
                  dragOverKey === col.key ? 'drag-over' : '',
                ]
                  .filter(Boolean)
                  .join(' ')}
              >
                <span
                  className="th-label"
                  draggable
                  onDragStart={(e) => handleDragStart(e, col.key)}
                  onDragEnd={handleDragEnd}
                >
                  {col.label}
                  {sortKey === col.key ? (sortDesc ? ' ▼' : ' ▲') : ''}
                </span>
                <ColumnResizeHandle
                  currentWidth={columnWidths[col.key] ?? DEFAULT_COLUMN_WIDTH}
                  onResize={(w) => setColumnWidth(col.key, w)}
                />
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {sorted.map((q) => (
            <tr key={q.symbol} onClick={() => onSelectSymbol(q.symbol)}>
              <td className="symbol-cell">
                <SymbolLogo symbol={q.symbol} />
                <span className="symbol-cell-text">
                  <span className="symbol">{q.symbol}</span>
                  <span className="name">{q.displayName}</span>
                </span>
              </td>
              {columns.map((col) => (
                <td key={col.key}>{renderCell(q, col.key)}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
