import { apiGet } from './httpClient'
import type { SymbolSearchResult } from '../types/market'

export function searchSymbols(query: string): Promise<SymbolSearchResult[]> {
  const params = new URLSearchParams({ q: query })
  return apiGet<SymbolSearchResult[]>(`/api/symbols/search?${params.toString()}`)
}
