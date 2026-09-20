import type { Interval } from '../../types/market'

interface TimeframeOption {
  label: string
  interval: Interval
}

const TIMEFRAMES: TimeframeOption[] = [
  { label: '5m', interval: 'FiveMinutes' },
  { label: '15m', interval: 'FifteenMinutes' },
  { label: '1H', interval: 'OneHour' },
  { label: '1D', interval: 'OneDay' },
  { label: '1W', interval: 'OneWeek' },
  { label: '1M', interval: 'OneMonth' },
]

interface TimeframeSelectorProps {
  value: Interval
  onChange: (interval: Interval) => void
}

export function TimeframeSelector({ value, onChange }: TimeframeSelectorProps) {
  return (
    <div className="timeframe-selector">
      {TIMEFRAMES.map((tf) => (
        <button
          key={tf.interval}
          type="button"
          className={tf.interval === value ? 'active' : ''}
          onClick={() => onChange(tf.interval)}
        >
          {tf.label}
        </button>
      ))}
    </div>
  )
}
