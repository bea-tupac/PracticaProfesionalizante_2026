<template>
  <main class="section">
    <div class="card card-center">

      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner profesor"
      />

      <header class="text-center">
        <h1>Gestión de Administradores</h1>
        <p class="subtitle">
          Administrá los administradores del sistema
        </p>
      </header>

      <div class="table">

        <div class="table-header table-cols-admin">
          <span>Nombre</span>
          <span>Apellido</span>
          <span>Email</span>
          <span>Acciones</span>
        </div>

        <div
          v-for="admin in administradores"
          :key="admin.id"
          class="table-row table-cols-admin"
        >
          <span>{{ admin.nombre }}</span>
          <span>{{ admin.apellido }}</span>
          <span>{{ admin.email }}</span>

          <div class="table-actions center">
            <router-link
              class="btn btn-success"
              :to="`/editaradministrador/${admin.id}`"
            >
              <el-icon><Edit /></el-icon>
            </router-link>

            <router-link
              class="btn btn-danger"
              :to="`/eliminaradministrador/${admin.id}`"
            >
              <el-icon><Delete /></el-icon>
            </router-link>
          </div>
        </div>

        <div v-if="!administradores.length && !error" class="table-row table-cols-admin">
          <span class="table-empty-state">No hay administradores registrados</span>
        </div>

        <div v-if="error" class="table-row table-cols-admin">
          <span class="table-empty-state" style="color: var(--color-danger);">{{ error }}</span>
        </div>

      </div>

      <footer class="table-actions center">
        <router-link
          to="/agregaradministracion"
          class="btn btn-primary"
        >
          Agregar administrador
        </router-link>
      </footer>

    </div>
  </main>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { apiGet } from '@/composables/useApiFetch'

const administradores = ref([])
const loading = ref(true)
const error = ref(null)

const cargarAdministradores = async () => {
  try {
    const data = await apiGet(`/api/administradores`)
    administradores.value = data.map(a => ({
      id: a.id,
      nombre: a.nombre,
      apellido: a.apellido,
      email: a.email
    }))
  } catch (err) {
    console.error(err)
    error.value = err instanceof Error ? err.message : 'No se pudieron cargar los administradores'
  } finally {
    loading.value = false
  }
}

onMounted(cargarAdministradores)
</script>