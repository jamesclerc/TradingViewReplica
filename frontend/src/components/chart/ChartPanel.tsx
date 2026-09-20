import { useCallback } from 'react'
import { useUiStore } from '../../state/useUiStore'
import { ChartWorkspace } from './ChartWorkspace'

interface ChartPanelProps {
  onClose: () => void
}

export function ChartPanel({ onClose }: ChartPanelProps) {
  const width = useUiStore((s) => s.chartPanelWidth)
  const setWidth = useUiStore((s) => s.setChartPanelWidth)

  // Pointer capture routes all subsequent pointer events to the handle until release, even once
  // the cursor leaves it - simpler and more robust than attaching/detaching window listeners.
  const handlePointerDown = useCallback((e: React.PointerEvent<HTMLDivElement>) => {
    e.currentTarget.setPointerCapture(e.pointerId)
  }, [])

  const handlePointerMove = useCallback(
    (e: React.PointerEvent<HTMLDivElement>) => {
      if (!e.currentTarget.hasPointerCapture(e.pointerId)) {
        return
      }
      // The handle sits on the panel's left edge; dragging left (negative movementX) should
      // widen the panel, since it's anchored to the right side of the screen.
      setWidth(width - e.movementX)
    },
    [width, setWidth],
  )

  const handlePointerUp = useCallback((e: React.PointerEvent<HTMLDivElement>) => {
    e.currentTarget.releasePointerCapture(e.pointerId)
  }, [])

  return (
    <div className="chart-panel" style={{ width }}>
      <div
        className="chart-panel-resize-handle"
        onPointerDown={handlePointerDown}
        onPointerMove={handlePointerMove}
        onPointerUp={handlePointerUp}
      />
      <button type="button" className="chart-panel-close" onClick={onClose} aria-label="Close chart">
        ×
      </button>
      <ChartWorkspace />
    </div>
  )
}
