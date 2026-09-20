import { CheckboxDropdown } from '../common/CheckboxDropdown'
import { ALL_SCREENER_COLUMNS, useScreenerColumnsStore } from '../../state/useScreenerColumnsStore'

export function ColumnPicker() {
  const visibleColumns = useScreenerColumnsStore((s) => s.visibleColumns)
  const toggleColumn = useScreenerColumnsStore((s) => s.toggleColumn)

  return (
    <CheckboxDropdown
      label="Columns"
      options={ALL_SCREENER_COLUMNS}
      selected={visibleColumns}
      onToggle={(key) => toggleColumn(key as (typeof visibleColumns)[number])}
    />
  )
}
