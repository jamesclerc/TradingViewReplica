import { useEffect, useRef, useState } from 'react'

interface CheckboxDropdownOption {
  key: string
  label: string
}

interface CheckboxDropdownProps {
  label: string
  options: CheckboxDropdownOption[]
  selected: string[]
  onToggle: (key: string) => void
}

export function CheckboxDropdown({ label, options, selected, onToggle }: CheckboxDropdownProps) {
  const [isOpen, setIsOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!isOpen) {
      return
    }
    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [isOpen])

  return (
    <div className="checkbox-dropdown" ref={containerRef}>
      <button type="button" className="checkbox-dropdown-trigger" onClick={() => setIsOpen((o) => !o)}>
        {label}
        {selected.length > 0 ? ` (${selected.length})` : ''} ▾
      </button>
      {isOpen && (
        <div className="checkbox-dropdown-menu">
          {options.map((opt) => (
            <label key={opt.key} className="checkbox-dropdown-item">
              <input type="checkbox" checked={selected.includes(opt.key)} onChange={() => onToggle(opt.key)} />
              {opt.label}
            </label>
          ))}
        </div>
      )}
    </div>
  )
}
