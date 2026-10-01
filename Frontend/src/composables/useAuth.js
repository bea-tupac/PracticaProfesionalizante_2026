const TOKEN_KEY = 'auth_token'
const ADMIN_KEY = 'auth_admin'

export function useAuth() {
  /**
   * Obtiene el token de sessionStorage.
   * Filtra tokens inválidos ("undefined", "null", vacíos o demasiado cortos).
   */
  const getToken = () => {
    const token = sessionStorage.getItem(TOKEN_KEY)
    if (!token || token === 'undefined' || token === 'null' || token.length < 20) {
      return null
    }
    return token
  }

  const getAdmin = () => {
    const raw = sessionStorage.getItem(ADMIN_KEY)
    if (!raw || raw === 'undefined' || raw === 'null') return null
    try {
      return JSON.parse(raw)
    } catch {
      return null
    }
  }

  const isAuthenticated = () => !!getToken()

  const guardarSesion = (token, admin) => {
    // Validación defensiva: no guardar si el token es inválido
    if (!token || token === 'undefined' || token === 'null' || token.length < 20) {
      console.warn('[useAuth] Intentando guardar un token inválido:', token)
      return
    }
    sessionStorage.setItem(TOKEN_KEY, token)
    sessionStorage.setItem(ADMIN_KEY, JSON.stringify(admin))
  }

  const cerrarSesion = () => {
    sessionStorage.removeItem(TOKEN_KEY)
    sessionStorage.removeItem(ADMIN_KEY)
  }

  /** Devuelve los headers necesarios para llamadas autenticadas. */
  const authHeaders = () => {
    const token = getToken()
    return token
      ? { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` }
      : { 'Content-Type': 'application/json' }
  }

  return { getToken, getAdmin, isAuthenticated, guardarSesion, cerrarSesion, authHeaders }
}