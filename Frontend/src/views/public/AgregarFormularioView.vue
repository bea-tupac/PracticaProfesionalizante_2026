<template>
  <main class="section">
    <div class="card card-center">

      <img
        class="card-media"
        src="@/components/Banner/bannerEstudiante.jpg"
        alt="Banner formulario"
      />

      <header class="card-header text-center">
        <h1>Agregar Formulario</h1>
        <p class="subtitle">
          Completá los datos del nuevo formulario
        </p>
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
            {{ loading ? 'Guardando...' : 'Guardar formulario' }}
          </button>

          <router-link to="/formularios" class="btn btn-secondary">
            Cancelar
          </router-link>
        </div>

        <p v-if="error" class="form-error text-center" style="color: var(--color-danger);">
          {{ error }}
        </p>

      </form>
    </div>
  </main>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { apiPost } from '@/composables/useApiFetch'
import { ElMessage } from 'element-plus'

const router = useRouter()

const loading = ref(false)
const error = ref(null)

const formulario = reactive({
  nombre: '',
  estado: 'Borrador',
  fechaApertura: '',
  fechaCierre: '',
  descripcion: ''
})

const guardarFormulario = async () => {
  error.value = null

  if (!formulario.nombre.trim()) {
    error.value = 'El nombre no puede estar vacío'
    return
  }
  if (!formulario.fechaApertura) {
    error.value = 'La fecha de apertura es obligatoria'
    return
  }
  if (!formulario.fechaCierre) {
    error.value = 'La fecha de cierre es obligatoria'
    return
  }
  if (formulario.fechaCierre <= formulario.fechaApertura) {
    error.value = 'La fecha de cierre debe ser posterior a la de apertura'
    return
  }

  loading.value = true

  try {
    const res = await apiPost('/api/formularios', formulario)

    ElMessage.success('Formulario guardado correctamente')
    router.push('/formularios')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al guardar el formulario'
  } finally {
    loading.value = false
  }
}
</script>