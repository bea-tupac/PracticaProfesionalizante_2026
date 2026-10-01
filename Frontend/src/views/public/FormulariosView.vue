<template>
  <main class="section">
    <div class="card card-center">

      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner formularios"
      />

      <header class="text-center">
        <h1>Lista de Formularios</h1>
        <p class="subtitle">
          Gestión de formularios académicos
        </p>
      </header>

      <div class="table">

        <div class="table-header table-cols-formularios">
          <span>Nombre</span>
          <span>Estado</span>
          <span>Fecha de apertura</span>
          <span>Fecha de cierre</span>
          <span>Acciones</span>
        </div>

        <div
          v-for="formulario in formularios"
          :key="formulario.id"
          class="table-row table-cols-formularios"
        >
          <span>{{ formulario.nombre }}</span>
          <span>
            <span class="estado-badge" :class="getEstadoClass(formulario.estado)">
              {{ formulario.estado }}
            </span>
          </span>
          <span>{{ formulario.fechaApertura }}</span>
          <span>{{ formulario.fechaCierre }}</span>

          <div class="table-actions center">
            <router-link
              class="btn btn-primary"
              :to="`/editarformulario/${formulario.id}`"
            >
              <el-icon><Edit /></el-icon>
            </router-link>

            <button
              class="btn btn-danger"
              @click="eliminar(formulario.id)"
            >
              <el-icon><Delete /></el-icon>
            </button>
          </div>
        </div>

        <div v-if="formularios.length === 0" class="table-row table-cols-formularios">
          <span class="text-center" style="grid-column: 1 / -1;">No hay formularios registrados</span>
        </div>

      </div>

      <p v-if="error" class="text-center" style="color: var(--color-danger);">
        {{ error }}
      </p>

      <footer class="table-actions center">
        <router-link class="btn btn-primary" to="/agregarformulario">
          Agregar formulario
        </router-link>
      </footer>

    </div>
  </main>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { apiGet, apiDelete } from '@/composables/useApiFetch'
import { ElMessage } from 'element-plus'

const formularios = ref([])
const error = ref(null)

const cargarFormularios = async () => {
  try {
    const data = await apiGet('/api/formularios')
    formularios.value = data
  } catch (err) {
    console.error('Error al cargar formularios:', err)
    if (err instanceof Error) {
      error.value = `No se pudieron cargar los formularios: ${err.message}`
    } else {
      error.value = 'No se pudieron cargar los formularios'
    }
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

const eliminar = async (id) => {
  try {
    await apiDelete(`/api/formularios/${id}`)
    ElMessage.success('Formulario eliminado correctamente')
    await cargarFormularios()
  } catch (err) {
    ElMessage.error(err instanceof Error ? err.message : 'Error al eliminar el formulario')
  }
}

onMounted(cargarFormularios)
</script>