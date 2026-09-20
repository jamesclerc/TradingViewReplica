import { useState } from 'react'
import { useSymbolSearch } from '../../hooks/useSymbolSearch'
import { SymbolLogo } from '../common/SymbolLogo'

interface SymbolSearchBoxProps {
  onSelect: (symbol: string) => void
}

export function SymbolSearchBox({ onSelect }: SymbolSearchBoxProps) {
  const [query, setQuery] = useState('')
  const [isOpen, setIsOpen] = useState(false)
  const { data: results, isFetching } = useSymbolSearch(query)

  function handleSelect(symbol: string) {
    onSelect(symbol)
    setQuery('')
    setIsOpen(false)
  }

  return (
    <div className="symbol-search">
      <input
        type="text"
        placeholder="Search symbol (e.g. AAPL)"
        value={query}
        onChange={(e) => {
          setQuery(e.target.value)
          setIsOpen(true)
        }}
        onFocus={() => setIsOpen(true)}
        onBlur={() => setTimeout(() => setIsOpen(false), 150)}
      />
      {isOpen && query.trim().length > 0 && (
        <ul className="symbol-search-results">
          {isFetching && <li className="hint">Searching…</li>}
          {!isFetching && results?.length === 0 && <li className="hint">No matches</li>}
          {results?.map((r) => (
            // onMouseDown (not onClick) fires before the input's onBlur closes the list.
            <li key={r.symbol} onMouseDown={() => handleSelect(r.symbol)}>
              <span className="symbol-search-result-left">
                <SymbolLogo symbol={r.symbol} size={18} />
                <span className="symbol">{r.symbol}</span>
              </span>
              <span className="name">{r.displayName}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
