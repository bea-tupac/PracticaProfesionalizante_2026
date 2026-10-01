<template>
  <main class="section">
    <div class="card card-center">

      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Panel de administración"
      />

      <header class="text-center">
        <h1>Panel de Administración</h1>
        <p class="subtitle">
          Seleccioná la opción que deseas gestionar
        </p>
      </header>

      <nav class="admin-menu" aria-label="Menú de administración">
        <ul>
          <li>
            <router-link to="/administracion" class="btn btn-primary btn-menu">
              Gestión de administradores
            </router-link>
          </li>

          <li>
            <router-link to="/carreras" class="btn btn-primary btn-menu">
              Carreras
            </router-link>
          </li>

          <li>
            <router-link to="/formularios" class="btn btn-primary btn-menu">
              Lista de formularios
            </router-link>
          </li>

          <li>
            <router-link to="/inscripcion" class="btn btn-primary btn-menu">
              Formulario de inscripción
            </router-link>
          </li>

          <li>
            <router-link to="/listados" class="btn btn-primary btn-menu">
              Listados
            </router-link>
          </li>

          <li>
            <button class="btn btn-secondary btn-menu" disabled>
              Descargas
            </button>
          </li>

          <li>
            <button class="btn btn-danger btn-menu" @click="logout">
              Cerrar sesión
            </button>
          </li>
        </ul>
      </nav>

      <p v-if="mensaje" class="text-center mt-3" style="color: var(--color-success);">
        {{ mensaje }}
      </p>

    </div>
  </main>
</template>


<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '@/composables/useAuth'

const router = useRouter()
const { cerrarSesion, authHeaders } = useAuth()
const API = import.meta.env.VITE_API_URL

const mensaje = ref('')

const logout = () => {
  cerrarSesion()
  router.push('/login')
}

onMounted(async () => {
  try {
    const res = await fetch(`${API}/`, { headers: authHeaders() })
    if (res.ok) {
      mensaje.value = 'Backend conectado'
    }
  } catch (err) {
    console.error(err)
  }
})
</script>