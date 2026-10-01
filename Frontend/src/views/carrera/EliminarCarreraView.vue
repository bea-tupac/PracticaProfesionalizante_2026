<template>
  <main class="section">
    <div class="card card-center">

      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner profesor"
      />

      <header class="text-center">
        <h1>Eliminar Carrera</h1>
        <p class="subtitle">
          ¿Estás seguro de que deseas eliminar esta carrera?
        </p>
      </header>

      <div v-if="carrera" class="table">

        <div class="table-header table-cols-default">
          <span>Nombre</span>
          <span>Duración</span>
          <span>Turno</span>
          <span>Modalidad</span>
          <span>Horario</span>
          <span>Estado</span>
          <span>Acciones</span>
        </div>

        <div class="table-row table-cols-default">
          <span>{{ carrera.nombre }}</span>
          <span>{{ carrera.duracionAnios }} años</span>
          <span>{{ carrera.turno }}</span>
          <span>{{ carrera.modalidad }}</span>
          <span>{{ carrera.horario }}</span>
          <span>{{ carrera.estado }}</span>

          <div class="table-actions center">
            <button
              class="btn btn-danger"
              @click="eliminarCarrera"
              :disabled="loading"
            >
              {{ loading ? 'Eliminando...' : 'Eliminar' }}
            </button>

            <router-link to="/carreras" class="btn btn-secondary">
              Cancelar
            </router-link>
          </div>
        </div>

      </div>

      <p v-if="error" class="text-center text-danger">
        {{ error }}
      </p>

    </div>
  </main>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { apiGet, apiDelete } from '@/composables/useApiFetch'
import { ElMessage } from 'element-plus'

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const error = ref(null)
const carrera = ref(null)

const id = Number(route.params.id)

if (isNaN(id) || id <= 0) {
  ElMessage.error('ID de carrera inválido')
  router.push('/carreras')
}

onMounted(async () => {
  if (isNaN(id) || id <= 0) return

  try {
    carrera.value = await apiGet(`/api/carreras/${id}`)
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'No se pudo cargar la carrera'
  }
})

const eliminarCarrera = async () => {
  if (isNaN(id) || id <= 0) return

  loading.value = true
  error.value = null

  try {
    await apiDelete(`/api/carreras/${id}`)
    ElMessage.success('Carrera eliminada correctamente')
    router.push('/carreras')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al eliminar la carrera'
  } finally {
    loading.value = false
  }
}
</script>
