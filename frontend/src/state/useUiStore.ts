import { create } from 'zustand'
import type { Interval, MarketDataRange } from '../types/market'

interface UiState {
  activeSymbol: string
  activeInterval: Interval
  activeRange: MarketDataRange
  isChartPanelOpen: boolean
  chartPanelWidth: number
  activeIndicators: string[]
  setActiveInterval: (interval: Interval) => void
  setActiveRange: (range: MarketDataRange) => void
  openChartPanel: (symbol?: string) => void
  closeChartPanel: () => void
  toggleChartPanel: () => void
  setChartPanelWidth: (width: number) => void
  toggleIndicator: (name: string) => void
}

export const CHART_PANEL_MIN_WIDTH = 320
export const CHART_PANEL_MAX_WIDTH = 900

export const useUiStore = create<UiState>((set) => ({
  activeSymbol: 'AAPL',
  activeInterval: 'OneDay',
  activeRange: 'SixMonths',
  isChartPanelOpen: false,
  chartPanelWidth: 480,
  activeIndicators: [],
  setActiveInterval: (interval) => set({ activeInterval: interval }),
  setActiveRange: (range) => set({ activeRange: range }),
  openChartPanel: (symbol) =>
    set((state) => ({ isChartPanelOpen: true, activeSymbol: symbol ?? state.activeSymbol })),
  closeChartPanel: () => set({ isChartPanelOpen: false }),
  toggleChartPanel: () => set((state) => ({ isChartPanelOpen: !state.isChartPanelOpen })),
  setChartPanelWidth: (width) =>
    set({ chartPanelWidth: Math.min(CHART_PANEL_MAX_WIDTH, Math.max(CHART_PANEL_MIN_WIDTH, width)) }),
  toggleIndicator: (name) =>
    set((state) => ({
      activeIndicators: state.activeIndicators.includes(name)
        ? state.activeIndicators.filter((n) => n !== name)
        : [...state.activeIndicators, name],
    })),
}))
