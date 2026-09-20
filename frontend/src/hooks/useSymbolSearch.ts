import { useQuery } from '@tanstack/react-query'
import { searchSymbols } from '../api/symbols'

export function useSymbolSearch(query: string) {
  return useQuery({
    queryKey: ['symbol-search', query],
    queryFn: () => searchSymbols(query),
    enabled: query.trim().length > 0,
    staleTime: 30_000,
  })
}
