import type { Period } from 'klinecharts'
import type { Interval } from '../../types/market'

// Maps our Interval (matches the backend's C# enum) to klinecharts' { type, span } period shape.
export function toKLinePeriod(interval: Interval): Period {
  switch (interval) {
    case 'OneMinute':
      return { type: 'minute', span: 1 }
    case 'TwoMinutes':
      return { type: 'minute', span: 2 }
    case 'FiveMinutes':
      return { type: 'minute', span: 5 }
    case 'FifteenMinutes':
      return { type: 'minute', span: 15 }
    case 'ThirtyMinutes':
      return { type: 'minute', span: 30 }
    case 'SixtyMinutes':
    case 'OneHour':
      return { type: 'hour', span: 1 }
    case 'NinetyMinutes':
      return { type: 'minute', span: 90 }
    case 'OneDay':
      return { type: 'day', span: 1 }
    case 'FiveDays':
      return { type: 'day', span: 5 }
    case 'OneWeek':
      return { type: 'week', span: 1 }
    case 'OneMonth':
      return { type: 'month', span: 1 }
    case 'ThreeMonths':
      return { type: 'month', span: 3 }
  }
}
