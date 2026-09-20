import { CheckboxDropdown } from '../common/CheckboxDropdown'

// klinecharts' full built-in indicator set, confirmed live via getSupportedIndicators() - it
// ships with more than we'd assumed, we just hadn't exposed a UI for any of them yet.
const INDICATOR_OPTIONS = [
  { key: 'MA', label: 'Moving Average (MA)' },
  { key: 'EMA', label: 'Exponential MA (EMA)' },
  { key: 'SMA', label: 'Smoothed MA (SMA)' },
  { key: 'BOLL', label: 'Bollinger Bands' },
  { key: 'BBI', label: 'Bull/Bear Index (BBI)' },
  { key: 'SAR', label: 'Parabolic SAR' },
  { key: 'VOL', label: 'Volume' },
  { key: 'MACD', label: 'MACD' },
  { key: 'RSI', label: 'RSI' },
  { key: 'KDJ', label: 'KDJ' },
  { key: 'CCI', label: 'CCI' },
  { key: 'WR', label: 'Williams %R' },
  { key: 'BIAS', label: 'BIAS' },
  { key: 'BRAR', label: 'BRAR' },
  { key: 'CR', label: 'CR' },
  { key: 'DMA', label: 'DMA' },
  { key: 'DMI', label: 'DMI' },
  { key: 'EMV', label: 'EMV' },
  { key: 'MTM', label: 'Momentum (MTM)' },
  { key: 'OBV', label: 'On Balance Volume' },
  { key: 'PVT', label: 'Price Volume Trend' },
  { key: 'PSY', label: 'Psychological Line' },
  { key: 'ROC', label: 'Rate of Change' },
  { key: 'TRIX', label: 'TRIX' },
  { key: 'VR', label: 'Volume Ratio' },
  { key: 'AO', label: 'Awesome Oscillator' },
  { key: 'AVP', label: 'Average Price' },
]

interface IndicatorToolbarProps {
  active: string[]
  onToggle: (name: string) => void
}

export function IndicatorToolbar({ active, onToggle }: IndicatorToolbarProps) {
  return <CheckboxDropdown label="Indicators" options={INDICATOR_OPTIONS} selected={active} onToggle={onToggle} />
}
