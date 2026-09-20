import { useUiStore } from '../../state/useUiStore'
import { KLineChartWidget } from './KLineChartWidget'
import { TimeframeSelector } from './TimeframeSelector'
import { IndicatorToolbar } from './IndicatorToolbar'
import { SymbolLogo } from '../common/SymbolLogo'

export function ChartWorkspace() {
  const symbol = useUiStore((s) => s.activeSymbol)
  const interval = useUiStore((s) => s.activeInterval)
  const range = useUiStore((s) => s.activeRange)
  const setActiveInterval = useUiStore((s) => s.setActiveInterval)
  const activeIndicators = useUiStore((s) => s.activeIndicators)
  const toggleIndicator = useUiStore((s) => s.toggleIndicator)

  return (
    <div className="chart-workspace">
      <div className="chart-toolbar">
        <span className="active-symbol">
          {symbol && <SymbolLogo symbol={symbol} size={20} />}
          {symbol || 'Select a symbol'}
        </span>
        <TimeframeSelector value={interval} onChange={setActiveInterval} />
        <IndicatorToolbar active={activeIndicators} onToggle={toggleIndicator} />
      </div>
      <div className="chart-container">
        {symbol ? (
          <KLineChartWidget symbol={symbol} interval={interval} range={range} indicators={activeIndicators} />
        ) : (
          <div className="chart-placeholder">Search for a symbol to get started</div>
        )}
      </div>
    </div>
  )
}
