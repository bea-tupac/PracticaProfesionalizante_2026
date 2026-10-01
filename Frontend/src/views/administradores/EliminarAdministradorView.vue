<template>
  <main class="section">
    <div class="card card-center">

      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner profesor"
      />

      <header class="text-center">
        <h1>Eliminar Administrador</h1>
        <p class="subtitle">
          ¿Estás seguro de que deseas eliminar este administrador?
        </p>
      </header>

      <div v-if="admin" class="table">

        <div class="table-header table-cols-admin">
          <span>Nombre</span>
          <span>Apellido</span>
          <span>Email</span>
          <span>Rol</span>
          <span>Acciones</span>
        </div>

        <div class="table-row table-cols-admin">
          <span>{{ admin.nombre }}</span>
          <span>{{ admin.apellido }}</span>
          <span>{{ admin.email }}</span>
          <span>{{ admin.role }}</span>

          <div class="table-actions center">
            <button
              class="btn btn-danger"
              @click="eliminarAdministrador"
              :disabled="loading"
            >
              {{ loading ? 'Eliminando...' : 'Eliminar' }}
            </button>

            <router-link to="/administracion" class="btn btn-secondary">
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
const admin = ref(null)

onMounted(async () => {
  try {
    const data = await apiGet(`/api/administradores/${Number(props.id)}`)
    admin.value = data
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'No se pudo cargar el administrador'
  }
})

const eliminarAdministrador = async () => {
  loading.value = true
  error.value = null

  try {
    await apiDelete(`/api/administradores/${Number(props.id)}`)
    ElMessage.success('Administrador eliminado correctamente')
    router.push('/administracion')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al eliminar el administrador'
  } finally {
    loading.value = false
  }
}
</script>
