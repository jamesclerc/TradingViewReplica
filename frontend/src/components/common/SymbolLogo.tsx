import { useEffect, useState } from 'react'

interface SymbolLogoProps {
  symbol: string
  size?: number
}

// Financial Modeling Prep serves stock logos directly by ticker, free, no API key required -
// verified live (September 2026). 404s cleanly for symbols it doesn't have (futures, some ETFs),
// which the onError fallback below handles. No backend involvement needed: plain <img> tags
// don't require CORS to render, just to read pixel data, which we're not doing.
function logoUrl(symbol: string): string {
  return `https://financialmodelingprep.com/image-stock/${encodeURIComponent(symbol)}.png`
}

export function SymbolLogo({ symbol, size = 20 }: SymbolLogoProps) {
  const [failed, setFailed] = useState(false)

  // Resets the fallback state when reused for a different symbol instead of remounting (e.g.
  // the chart toolbar keeps one SymbolLogo instance alive across symbol switches).
  useEffect(() => {
    setFailed(false)
  }, [symbol])

  if (failed) {
    return (
      <span
        className="symbol-logo-fallback"
        style={{ width: size, height: size, fontSize: size * 0.42 }}
      >
        {symbol.slice(0, 2)}
      </span>
    )
  }

  return (
    <img
      className="symbol-logo"
      src={logoUrl(symbol)}
      alt=""
      width={size}
      height={size}
      loading="lazy"
      onError={() => setFailed(true)}
    />
  )
}
