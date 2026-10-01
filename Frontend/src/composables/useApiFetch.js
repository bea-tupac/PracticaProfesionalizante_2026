/**
 * Helper global para fetch con:
 * - Headers de autenticación automáticos
 * - Manejo centralizado de 401/403
 * - Auto-unwrap de respuestas { isSuccess, message, data }
 */

import { useRouter } from 'vue-router'

let router = null

export function setRouter(r) {
  router = r
}

/**
 * Obtiene el token de sessionStorage.
 * Filtra tokens inválidos ("undefined", "null", vacíos o demasiado cortos).
 */
function getToken() {
  const token = sessionStorage.getItem('auth_token')
  if (!token || token === 'undefined' || token === 'null' || token.length < 20) {
    return null
  }
  return token
}

function getApiBaseUrl() {
  return import.meta.env.VITE_API_URL ?? 'http://localhost:5127'
}

export async function apiFetch(endpoint, options = {}) {
  const token = getToken()

  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers || {}),
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  const response = await fetch(`${getApiBaseUrl()}${endpoint}`, {
    ...options,
    headers,
  })

  // Manejo de 401/403: solo limpiar sesión si HABÍA un token y NO estamos en login
  if (response.status === 401 || response.status === 403) {
    console.warn(`[apiFetch] ${response.status} en ${endpoint}`)

    const estabaAutenticado = !!token
    const enLogin = router?.currentRoute.value.name === 'login'

    if (estabaAutenticado && !enLogin) {
      sessionStorage.removeItem('auth_token')
      sessionStorage.removeItem('auth_admin')

      try {
        const { ElMessage } = await import('element-plus')
        ElMessage.warning('Sesión expirada. Iniciá sesión nuevamente.')
      } catch { /* noop */ }

      router?.push({ name: 'login', query: { redirect: router.currentRoute.value.fullPath } })
    }
  }

  return response
}

/**
 * Desenvuelve una respuesta del backend.
 * Si viene envuelta como { isSuccess, message, data }, devuelve solo .data.
 */
function unwrap(json) {
  if (
    json !== null &&
    typeof json === 'object' &&
    'isSuccess' in json &&
    'data' in json
  ) {
    return json.data
  }
  return json
}

/**
 * Extrae el mensaje de error de una respuesta fallida.
 */
async function extractError(response) {
  try {
    const err = await response.json()
    return err.error ?? err.message ?? `Error HTTP ${response.status}`
  } catch {
    return `Error HTTP ${response.status}`
  }
}

export async function apiGet(endpoint) {
  const response = await apiFetch(endpoint)
  if (!response.ok) {
    throw new Error(await extractError(response))
  }
  const json = await response.json()
  return unwrap(json)
}

export async function apiPost(endpoint, body) {
  const response = await apiFetch(endpoint, {
    method: 'POST',
    body: JSON.stringify(body),
  })
  if (!response.ok) {
    throw new Error(await extractError(response))
  }
  const json = await response.json()
  return unwrap(json)
}

export async function apiPut(endpoint, body) {
  const response = await apiFetch(endpoint, {
    method: 'PUT',
    body: JSON.stringify(body),
  })
  if (!response.ok) {
    throw new Error(await extractError(response))
  }
  const json = await response.json()
  return unwrap(json)
}

export async function apiDelete(endpoint) {
  const response = await apiFetch(endpoint, {
    method: 'DELETE',
  })
  if (!response.ok) {
    throw new Error(await extractError(response))
  }
}