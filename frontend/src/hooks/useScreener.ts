import { useQuery } from '@tanstack/react-query'
import { fetchScreener, fetchScreenerDefinitions } from '../api/screener'

export function useScreenerDefinitions() {
  return useQuery({
    queryKey: ['screener-definitions'],
    queryFn: fetchScreenerDefinitions,
    staleTime: Infinity,
  })
}

export function useScreener(screenerId: string) {
  return useQuery({
    queryKey: ['screener', screenerId],
    queryFn: () => fetchScreener(screenerId),
    refetchInterval: 60_000,
  })
}
