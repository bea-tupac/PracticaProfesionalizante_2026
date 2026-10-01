<template>
  <main class="section">
    <div class="card card-center">

      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner formulario"
      />

      <header class="text-center">
        <h1>Eliminar Formulario</h1>
        <p class="subtitle">
          ¿Estás seguro de que deseas eliminar este formulario?
        </p>
      </header>

      <div v-if="formulario" class="table">

        <div class="table-header table-cols-formularios">
          <span>Nombre</span>
          <span>Estado</span>
          <span>Fecha de apertura</span>
          <span>Fecha de cierre</span>
          <span>Acciones</span>
        </div>

        <div class="table-row table-cols-formularios">
          <span>{{ formulario.nombre }}</span>
          <span>
            <span class="estado-badge" :class="getEstadoClass(formulario.estado)">
              {{ formulario.estado }}
            </span>
          </span>
          <span>{{ formulario.fechaApertura }}</span>
          <span>{{ formulario.fechaCierre }}</span>

          <div class="table-actions center">
            <button
              class="btn btn-danger"
              @click="eliminarFormulario"
              :disabled="loading"
            >
              {{ loading ? 'Eliminando...' : 'Eliminar' }}
            </button>

            <router-link to="/formularios" class="btn btn-secondary">
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
import { useRouter } from 'vue-router'
import { apiGet, apiDelete } from '@/composables/useApiFetch'
import { ElMessage } from 'element-plus'

const router = useRouter()

const props = defineProps(['id'])

const loading = ref(false)
const error = ref(null)
const formulario = ref(null)

onMounted(async () => {
  try {
    const data = await apiGet(`/api/formularios/${Number(props.id)}`)
    formulario.value = data
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'No se pudo cargar el formulario'
  }
})

const eliminarFormulario = async () => {
  loading.value = true
  error.value = null

  try {
    await apiDelete(`/api/formularios/${Number(props.id)}`)

    ElMessage.success('Formulario eliminado correctamente')
    router.push('/formularios')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al eliminar el formulario'
  } finally {
    loading.value = false
  }
}

const getEstadoClass = (estado) => {
  switch (estado) {
    case 'Abierto': return 'estado-abierto'
    case 'Cerrado': return 'estado-cerrado'
    case 'Borrador': return 'estado-borrador'
    default: return 'estado-cerrado'
  }
}
</script>