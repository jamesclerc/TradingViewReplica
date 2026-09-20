import { useCallback } from 'react'

interface ColumnResizeHandleProps {
  currentWidth: number
  onResize: (width: number) => void
}

export function ColumnResizeHandle({ currentWidth, onResize }: ColumnResizeHandleProps) {
  const handlePointerDown = useCallback((e: React.PointerEvent<HTMLDivElement>) => {
    e.currentTarget.setPointerCapture(e.pointerId)
  }, [])

  const handlePointerMove = useCallback(
    (e: React.PointerEvent<HTMLDivElement>) => {
      if (!e.currentTarget.hasPointerCapture(e.pointerId)) {
        return
      }
      onResize(currentWidth + e.movementX)
    },
    [currentWidth, onResize],
  )

  const handlePointerUp = useCallback((e: React.PointerEvent<HTMLDivElement>) => {
    e.currentTarget.releasePointerCapture(e.pointerId)
  }, [])

  return (
    <div
      className="column-resize-handle"
      // Stop a plain click from reaching the <th>'s sort handler.
      onClick={(e) => e.stopPropagation()}
      onPointerDown={handlePointerDown}
      onPointerMove={handlePointerMove}
      onPointerUp={handlePointerUp}
    />
  )
}
