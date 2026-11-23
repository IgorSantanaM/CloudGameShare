import { useEffect, useRef, useState } from 'react'
import { requestApi } from './catalogApi'
import type { CatalogGame } from './catalogApi'
import { isPlaybackBlocked, usePlaybackBlocked } from './playback'

type Session = {
  id: string
  status: 'Pending' | 'Provisioning' | 'Ready' | 'Failed' | 'Ended'
  expiresAtUtc: string
  failureReason: string | null
}

export default function SessionPanel({ game, onDismiss }: { game: CatalogGame; onDismiss: () => void }) {
  const blocked = usePlaybackBlocked()
  const [session, setSession] = useState<Session | null>(null)
  const [error, setError] = useState('')
  const [creating, setCreating] = useState(false)
  const [pollVersion, setPollVersion] = useState(0)
  const requestKey = useRef(crypto.randomUUID())
  const pendingRequest = useRef(false)
  const started = useRef(false)
  const panel = useRef<HTMLElement>(null)

  async function start() {
    if (isPlaybackBlocked() || pendingRequest.current || session) return
    pendingRequest.current = true
    setCreating(true)
    setError('')
    try {
      const response = await requestApi('/api/gaming/sessions/', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Idempotency-Key': requestKey.current },
        body: JSON.stringify({ gameId: game.id }),
      })
      setSession(await response.json())
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Unable to start the session.')
    } finally {
      pendingRequest.current = false
      setCreating(false)
    }
  }

  useEffect(() => {
    if (!started.current) {
      started.current = true
      panel.current?.focus()
      void start()
    }
    // Start once per mounted request. Retries reuse its idempotency key.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const status = session?.status
  const sessionId = session?.id
  useEffect(() => {
    if (!sessionId || status === 'Failed' || status === 'Ended') return
    const controller = new AbortController()
    let timer: number
    async function refresh() {
      try {
        const response = await requestApi(`/api/gaming/sessions/${encodeURIComponent(sessionId!)}`, { signal: controller.signal })
        const updated: Session = await response.json()
        if (controller.signal.aborted) return
        setSession(updated)
        setError('')
        if (updated.status !== 'Failed' && updated.status !== 'Ended') timer = window.setTimeout(refresh, 2000)
      } catch (cause) {
        if (!controller.signal.aborted) setError(cause instanceof Error ? cause.message : 'Unable to refresh session status.')
      }
    }
    timer = window.setTimeout(refresh, 2000)
    return () => { controller.abort(); window.clearTimeout(timer) }
  }, [sessionId, status, pollVersion])

  const canDismiss = session?.status === 'Failed' || session?.status === 'Ended'
  return (
    <section ref={panel} tabIndex={-1} className="session-panel" aria-labelledby="session-title">
      <h2 id="session-title">Session · {game.name}</h2>
      <div role="status" aria-live="polite">
        {creating && <p>Requesting your gaming session…</p>}
        {session && <p>Status · <strong>{session.status}</strong></p>}
        {session?.status === 'Ready' && <p>Your session is ready. The server does not yet provide a streaming connection.</p>}
        {session?.failureReason && <p>{session.failureReason}</p>}
        {session && <p>Expires {new Date(session.expiresAtUtc).toLocaleString()}</p>}
      </div>
      {blocked && <p>Mobile gameplay is currently unsupported. Browse and manage games here, then play on a desktop.</p>}
      {error && <p role="alert">{error}</p>}
      <div className="card-actions">
        {error && <button className="button button-cream" disabled={creating || (!session && blocked)} onClick={() => session ? setPollVersion((value) => value + 1) : void start()}>Retry {session ? 'status' : 'session request'}</button>}
        {canDismiss && <button className="button button-cream" onClick={onDismiss}>Dismiss session</button>}
      </div>
    </section>
  )
}
