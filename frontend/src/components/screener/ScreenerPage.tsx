import { useMemo, useState } from 'react'
import { useScreener, useScreenerDefinitions } from '../../hooks/useScreener'
import { ScreenerTable } from './ScreenerTable'
import { ColumnPicker } from './ColumnPicker'
import { CategoryTabs } from './CategoryTabs'

interface ScreenerPageProps {
  onSelectSymbol: (symbol: string) => void
}

// Preferred display order; any category the backend adds later that isn't in this list still
// shows up, just appended after these.
const CATEGORY_ORDER = ['Stocks', 'ETFs', 'Crypto', 'Funds']

export function ScreenerPage({ onSelectSymbol }: ScreenerPageProps) {
  const { data: definitions } = useScreenerDefinitions()
  const [category, setCategory] = useState('Stocks')
  const [screenerId, setScreenerId] = useState('most_actives')

  const categories = useMemo(() => {
    const found = Array.from(new Set((definitions ?? []).map((d) => d.category)))
    return [...CATEGORY_ORDER.filter((c) => found.includes(c)), ...found.filter((c) => !CATEGORY_ORDER.includes(c))]
  }, [definitions])

  const screensInCategory = useMemo(
    () => (definitions ?? []).filter((d) => d.category === category),
    [definitions, category],
  )

  function handleCategoryChange(nextCategory: string) {
    setCategory(nextCategory)
    const first = (definitions ?? []).find((d) => d.category === nextCategory)
    if (first) {
      setScreenerId(first.id)
    }
  }

  const { data: result, isLoading, isError } = useScreener(screenerId)

  return (
    <div className="screener-page">
      <CategoryTabs categories={categories} active={category} onSelect={handleCategoryChange} />
      <div className="screener-toolbar">
        <select value={screenerId} onChange={(e) => setScreenerId(e.target.value)}>
          {screensInCategory.map((d) => (
            <option key={d.id} value={d.id}>
              {d.label}
            </option>
          ))}
        </select>
        <ColumnPicker />
        {result && (
          <span className="screener-count">
            {result.quotes.length} of {result.total}
          </span>
        )}
      </div>
      {isLoading && <div className="screener-hint">Loading…</div>}
      {isError && <div className="screener-hint">Failed to load screener data.</div>}
      {result && <ScreenerTable quotes={result.quotes} onSelectSymbol={onSelectSymbol} />}
    </div>
  )
}
