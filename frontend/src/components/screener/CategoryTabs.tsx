interface CategoryTabsProps {
  categories: string[]
  active: string
  onSelect: (category: string) => void
}

export function CategoryTabs({ categories, active, onSelect }: CategoryTabsProps) {
  return (
    <div className="category-tabs">
      {categories.map((c) => (
        <button key={c} type="button" className={c === active ? 'active' : ''} onClick={() => onSelect(c)}>
          {c}
        </button>
      ))}
    </div>
  )
}
