import { useSyncExternalStore } from 'react'

const compactScreen = '(max-width: 768px)'

export function isPlaybackBlocked() {
  return window.matchMedia(compactScreen).matches ||
    /Android|iPhone|iPad|iPod|Mobile/i.test(navigator.userAgent) ||
    (/Macintosh/i.test(navigator.userAgent) && navigator.maxTouchPoints > 1)
}

function subscribe(callback: () => void) {
  const media = window.matchMedia(compactScreen)
  media.addEventListener('change', callback)
  return () => media.removeEventListener('change', callback)
}

export function usePlaybackBlocked() {
  return useSyncExternalStore(subscribe, isPlaybackBlocked, () => true)
}
