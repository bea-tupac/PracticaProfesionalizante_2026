<template>
  <main class="section">
    <div class="card card-center">

      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner formulario"
      />

      <header class="text-center">
        <h1>Editar Formulario</h1>
        <p class="subtitle">Modificá los datos del formulario</p>
      </header>

      <form class="form" @submit.prevent="guardarFormulario">

        <div class="form-row">
          <div class="field">
            <label for="nombre">Nombre</label>
            <input
              id="nombre"
              v-model="formulario.nombre"
              type="text"
              required
            />
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="estado">Estado</label>
            <select id="estado" v-model="formulario.estado" required>
              <option value="Borrador">Borrador</option>
              <option value="Abierto">Abierto</option>
              <option value="Cerrado">Cerrado</option>
            </select>
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="fechaApertura">Fecha de apertura</label>
            <input
              id="fechaApertura"
              v-model="formulario.fechaApertura"
              type="date"
              required
            />
          </div>

          <div class="field">
            <label for="fechaCierre">Fecha de cierre</label>
            <input
              id="fechaCierre"
              v-model="formulario.fechaCierre"
              type="date"
              required
            />
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="descripcion">Descripción</label>
            <textarea
              id="descripcion"
              v-model="formulario.descripcion"
              rows="4"
            ></textarea>
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

          <router-link to="/formularios" class="btn btn-secondary">
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
import { useRouter } from 'vue-router'
import { apiGet, apiPut } from '@/composables/useApiFetch'
import { ElMessage } from 'element-plus'

const router = useRouter()

const props = defineProps(['id'])

const loading = ref(false)
const error = ref(null)

const formulario = reactive({
  id: 0,
  nombre: '',
  estado: 'Borrador',
  fechaApertura: '',
  fechaCierre: '',
  descripcion: ''
})

onMounted(async () => {
  try {
    const data = await apiGet(`/api/formularios/${Number(props.id)}`)
    Object.assign(formulario, data)
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'No se pudieron cargar los datos del formulario'
  }
})

const guardarFormulario = async () => {
  loading.value = true
  error.value = null

  if (!formulario.nombre.trim()) {
    error.value = 'El nombre no puede estar vacío'
    loading.value = false
    return
  }
  if (!formulario.fechaApertura) {
    error.value = 'La fecha de apertura es obligatoria'
    loading.value = false
    return
  }
  if (!formulario.fechaCierre) {
    error.value = 'La fecha de cierre es obligatoria'
    loading.value = false
    return
  }
  if (formulario.fechaCierre <= formulario.fechaApertura) {
    error.value = 'La fecha de cierre debe ser posterior a la de apertura'
    loading.value = false
    return
  }

  try {
    await apiPut(`/api/formularios/${Number(props.id)}`, formulario)

    ElMessage.success('Formulario actualizado correctamente')
    router.push('/formularios')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al actualizar el formulario'
  } finally {
    loading.value = false
  }
}
</script>