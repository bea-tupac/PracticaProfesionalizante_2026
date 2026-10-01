<template>
  <main class="section">
    <div class="card card-center">
      <img
        class="card-media"
        src="@/components/Banner/bannerProfesor.jpg"
        alt="Banner profesor"
      />

      <header class="card-header text-center">
        <h1>Agregar Administrador</h1>
        <p class="subtitle">Completá los datos del nuevo administrador</p>
      </header>

      <form class="form" @submit.prevent="guardarAdministrador">

        <div class="form-row">
          <div class="field">
            <label for="nombre">Nombre</label>
            <input id="nombre" v-model="admin.nombre" type="text" required />
          </div>
          <div class="field">
            <label for="apellido">Apellido</label>
            <input id="apellido" v-model="admin.apellido" type="text" required />
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="email">Email</label>
            <input id="email" v-model="admin.email" type="email" required />
          </div>
          <div class="field">
            <label for="role">Rol</label>
            <select id="role" v-model="admin.role" required>
              <option value="Admin">Admin</option>
              <option value="SuperAdmin">SuperAdmin</option>
            </select>
          </div>
        </div>

        <div class="form-row">
          <div class="field">
            <label for="password">Contraseña</label>
            <div class="password-input-wrapper">
              <input
                id="password"
                v-model="admin.password"
                :type="showPassword ? 'text' : 'password'"
                autocomplete="new-password"
                placeholder="Mínimo 8 caracteres"
                required
                :disabled="loading"
              />
              <button
                type="button"
                class="password-toggle"
                @click="showPassword = !showPassword"
                :aria-label="showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'"
              >
                <el-icon v-if="showPassword"><Hide /></el-icon>
                <el-icon v-else><View /></el-icon>
              </button>
            </div>
          </div>
          <div class="field">
            <label for="confirmPassword">Confirmar Contraseña</label>
            <div class="password-input-wrapper">
              <input
                id="confirmPassword"
                v-model="confirmPassword"
                :type="showConfirm ? 'text' : 'password'"
                autocomplete="new-password"
                placeholder="Repetir contraseña"
                required
                :disabled="loading"
              />
              <button
                type="button"
                class="password-toggle"
                @click="showConfirm = !showConfirm"
                :aria-label="showConfirm ? 'Ocultar contraseña' : 'Mostrar contraseña'"
              >
                <el-icon v-if="showConfirm"><Hide /></el-icon>
                <el-icon v-else><View /></el-icon>
              </button>
            </div>
          </div>
        </div>

        <p v-if="error" class="form-error text-center">{{ error }}</p>

        <div class="form-actions">
          <button class="btn btn-success" type="submit" :disabled="loading">
            {{ loading ? 'Guardando...' : 'Guardar administrador' }}
          </button>
          <router-link to="/administracion" class="btn btn-secondary">Cancelar</router-link>
        </div>

      </form>
    </div>
  </main>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { Hide, View } from '@element-plus/icons-vue'
import { apiPost } from '@/composables/useApiFetch'

const router = useRouter()

const loading = ref(false)
const error = ref(null)
const showPassword = ref(false)
const showConfirm = ref(false)
const confirmPassword = ref('')

const admin = reactive({
  nombre: '',
  apellido: '',
  email: '',
  role: 'Admin',
  password: ''
})

const guardarAdministrador = async () => {
  error.value = null

  if (!admin.nombre.trim() || !admin.apellido.trim()) {
    error.value = 'Nombre y apellido son obligatorios'
    return
  }
  if (!admin.email.trim()) {
    error.value = 'El email es obligatorio'
    return
  }
  if (admin.password.length < 8) {
    error.value = 'La contraseña debe tener al menos 8 caracteres'
    return
  }
  if (admin.password !== confirmPassword.value) {
    error.value = 'Las contraseñas no coinciden'
    return
  }

  loading.value = true

  try {
    await apiPost('/api/administradores/with-password', {
      nombre: admin.nombre,
      apellido: admin.apellido,
      email: admin.email,
      role: admin.role,
      password: admin.password
    })

    ElMessage.success('Administrador creado correctamente')
    router.push('/administracion')
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Error al crear el administrador'
  } finally {
    loading.value = false
  }
}
</script>
