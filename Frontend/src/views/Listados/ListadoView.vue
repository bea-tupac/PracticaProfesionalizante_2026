<template>
  <main class="section">
    <div class="card card-center card-lg">

      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Panel de administración"
      />

      <section class="text-center">
        <h1>Listas de Carreras con alumnos inscriptos</h1>
        <p class="subtitle">
          Revisá los alumnos inscriptos por carrera
        </p>
      </section>

      <div class="table">

        <div class="table-header table-cols-default">
          <span>Alumno</span>
          <span>DNI</span>
          <span>Edad</span>
          <span>Carrera</span>
          <span>Turno</span>
          <span></span>
          <span></span>
        </div>

        <div
          v-for="item in listado"
          :key="item.alumnoId"
          class="table-row table-cols-default"
        >
          <span>{{ item.nombreCompleto }}</span>
          <span>{{ item.dni }}</span>
          <span>{{ item.edad }}</span>
          <span>{{ item.carrera }}</span>
          <span>{{ item.turno }}</span>
          <span></span>
          <span></span>
        </div>

        <div v-if="listado.length === 0" class="table-row table-cols-default">
          <span class="text-center" style="grid-column: 1 / -1;">No hay alumnos inscriptos</span>
        </div>

      </div>

      <p v-if="error" class="error-msg text-center">
        {{ error }}
      </p>

    </div>
  </main>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { apiGet } from '@/composables/useApiFetch'

const listado = ref([])
const loading = ref(true)
const error   = ref(null)

const cargarListado = async () => {
  try {
    const data = await apiGet('/api/listado')
    listado.value = data
  } catch (err) {
    console.error(err)
    error.value = 'No se pudo cargar el listado'
  } finally {
    loading.value = false
  }
}

onMounted(cargarListado)
</script>