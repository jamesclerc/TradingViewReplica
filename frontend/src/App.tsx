import { SymbolSearchBox } from './components/search/SymbolSearchBox'
import { ScreenerPage } from './components/screener/ScreenerPage'
import { ChartPanel } from './components/chart/ChartPanel'
import { useUiStore } from './state/useUiStore'
import './App.css'

function App() {
  const openChartPanel = useUiStore((s) => s.openChartPanel)
  const closeChartPanel = useUiStore((s) => s.closeChartPanel)
  const toggleChartPanel = useUiStore((s) => s.toggleChartPanel)
  const isChartPanelOpen = useUiStore((s) => s.isChartPanelOpen)

  return (
    <div className="app-shell">
      <header className="app-header">
        <span className="app-title">TradingView Replica</span>
        <SymbolSearchBox onSelect={openChartPanel} />
        <button type="button" className="chart-toggle-button" onClick={toggleChartPanel}>
          {isChartPanelOpen ? 'Hide Chart' : 'Show Chart'}
        </button>
      </header>
      <div className="app-body">
        <div className="screener-main">
          <ScreenerPage onSelectSymbol={openChartPanel} />
        </div>
        {isChartPanelOpen && <ChartPanel onClose={closeChartPanel} />}
      </div>
    </div>
  )
}

export default App
