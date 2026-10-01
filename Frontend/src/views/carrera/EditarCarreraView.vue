<template>
  <main class="section">
    <div class="card card-center">
      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner profesor"
      />
      <header class="text-center">
        <h1>Editar Carrera</h1>
        <p class="subtitle">Modificá los datos de la carrera</p>
      </header>

      <form class="form" @submit.prevent="guardarCarrera">

        <div class="form-row">
          <div class="field">
            <label for="nombre">Nombre</label>
            <input
              id="nombre"
              v-model="carrera.nombre"
              type="text"
              required
            />
          </div>

          <div class="field">
            <label for="duracion">Duración (años)</label>
            <input
              id="duracion"
              v-model.number="carrera.duracionAnios"
              type="number"
              min="1"
              required
            />
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="turno">Turno</label>
            <select id="turno" v-model="carrera.turno" required>
              <option>Mañana</option>
              <option>Tarde</option>
              <option>Noche</option>
            </select>
          </div>

          <div class="field">
            <label for="modalidad">Modalidad</label>
            <select id="modalidad" v-model="carrera.modalidad" required>
              <option>Presencial</option>
              <option>Virtual</option>
              <option>Mixta</option>
            </select>
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="horario">Horario</label>
            <input
              id="horario"
              v-model="carrera.horario"
              type="text"
              placeholder="Ej: 18:00 a 22:00"
            />
          </div>

          <div class="field">
            <label for="estado">Estado</label>
            <select id="estado" v-model="carrera.estado" required>
              <option>Activa</option>
              <option>Inactiva</option>
            </select>
          </div>
        </div>

        <div class="form-actions">
          <button
            type="submit"
            class="btn btn-success"
            :disabled="loading"
          >
            {{ loading ? 'Guardando...' : 'Guardar cambios' }}
          </button>

          <router-link to="/carreras" class="btn btn-secondary">
            Cancelar
          </router-link>
        </div>

        <p v-if="error" class="form-error text-center">
          {{ error }}
        </p>

      </form>
    </div>
  </main>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { apiGet, apiPut } from '@/composables/useApiFetch'
import { ElMessage } from 'element-plus'

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const error = ref(null)

const carrera = reactive({
  id: 0,
  nombre: '',
  duracionAnios: 1,
  turno: 'Mañana',
  modalidad: 'Presencial',
  horario: '',
  estado: 'Activa'
})

const id = Number(route.params.id)

if (isNaN(id) || id <= 0) {
  ElMessage.error('ID de carrera inválido')
  router.push('/carreras')
}

onMounted(async () => {
  if (isNaN(id) || id <= 0) return

  try {
    const data = await apiGet(`/api/carreras/${id}`)
    Object.assign(carrera, data)
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'No se pudieron cargar los datos de la carrera'
  }
})

const guardarCarrera = async () => {
  if (isNaN(id) || id <= 0) return

  loading.value = true
  error.value = null

  try {
    await apiPut(`/api/carreras/${id}`, { ...carrera, id })
    ElMessage.success('Carrera actualizada correctamente')
    router.push('/carreras')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al actualizar la carrera'
  } finally {
    loading.value = false
  }
}
</script>
