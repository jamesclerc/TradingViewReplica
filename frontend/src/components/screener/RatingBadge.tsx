interface RatingBadgeProps {
  rating: string | null
}

// Yahoo formats this as "{score} - {label}", e.g. "1.8 - Buy". The label is one of a fixed set
// of five tiers; group them into buy/hold/sell tones for color.
const RATING_TONE: Record<string, 'buy' | 'hold' | 'sell'> = {
  'Strong Buy': 'buy',
  Buy: 'buy',
  Hold: 'hold',
  Underperform: 'sell',
  Sell: 'sell',
  'Strong Sell': 'sell',
}

export function RatingBadge({ rating }: RatingBadgeProps) {
  if (!rating) {
    return <span className="rating-badge none">—</span>
  }

  const label = rating.split('-').pop()?.trim() ?? rating
  const tone = RATING_TONE[label] ?? 'hold'

  return <span className={`rating-badge ${tone}`}>{label}</span>
}
