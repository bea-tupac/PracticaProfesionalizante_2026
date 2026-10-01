<template>
  <main class="section">
    <div class="card card-center card-lg">

      <img
        class="card-media"
        src="/src/components/Banner/bannerEstudiante.jpg"
        alt="Banner inscripción"
      />

      <section class="text-center">
        <h1>
          Formulario de Inscripción Alumnos Tupac Amaru <br />
          2026
        </h1>
        <p class="subtitle">
          Completá todos los campos solicitados
        </p>
      </section>

      <form class="form" @submit.prevent="guardarAlumno">

        <section>
          <h2>Datos personales</h2>
          <hr />

          <div class="form-row">
            <div class="field">
              <label>Nombre</label>
              <input type="text" v-model="alumno.Nombre" />
            </div>

            <div class="field">
              <label>Apellido</label>
              <input type="text" v-model="alumno.Apellido" />
            </div>
          </div>

          <div class="form-row">
            <div class="field">
              <label>DNI</label>
              <input type="text" v-model="alumno.DNI" />
            </div>

            <div class="field">
              <label>Nacionalidad</label>
              <input type="text" v-model="alumno.Nacionalidad" />
            </div>
          </div>

          <div class="form-row">
            <div class="field">
              <label>Fecha de nacimiento</label>
              <input type="date" v-model="alumno.FechaNacimiento" />
            </div>

            <div class="field">
              <label>Edad al 30/06/2026</label>
              <input type="text" :value="edad ?? ''" disabled />
            </div>
          </div>
        </section>

        <section>
          <h2>Contacto</h2>
          <hr />

          <div class="field">
            <label>Dirección</label>
            <input type="text" v-model="alumno.Direccion" />
          </div>

          <div class="form-row">
            <div class="field">
              <label>Teléfono</label>
              <input type="tel" v-model="alumno.Telefono" />
            </div>

            <div class="field">
              <label>Correo electrónico</label>
              <input type="email" v-model="alumno.Email" />
            </div>
          </div>
        </section>

        <section>
          <h2>Información académica</h2>
          <hr />

          <div class="form-checklist mt-2">
            <p class="checklist-title">
              <strong>Indicar si posee:</strong>
            </p>

            <label class="check-item">
              <input type="checkbox" v-model="documentacion.titulo" />
              <span class="check-custom"></span>
              <span class="check-text">Título</span>
            </label>

            <label class="check-item">
              <input type="checkbox" v-model="documentacion.tituloEnTramite" />
              <span class="check-custom"></span>
              <span class="check-text">Título en trámite</span>
            </label>

            <label class="check-item">
              <input type="checkbox" v-model="documentacion.materiasAdeudadas" />
              <span class="check-custom"></span>
              <span class="check-text">Constancia de materias adeudadas</span>
            </label>

            <label class="check-item">
              <input type="checkbox" v-model="documentacion.alumnoRegular" />
              <span class="check-custom"></span>
              <span class="check-text">Constancia de alumno regular</span>
            </label>
          </div>

          <div v-if="documentacion.titulo" class="form-row mt-3">
            <div class="field">
              <label>Fecha de egreso del secundario</label>
              <input type="date" v-model="alumno.FechaInscripcion" required />
            </div>

            <div class="field">
              <label>Título secundario</label>
              <input type="text" v-model="alumno.TituloSecundario" required />
            </div>
          </div>

          <div v-if="documentacion.tituloEnTramite" class="form-row mt-3">
            <div class="field">
              <label>Fecha estimada de obtención del título</label>
              <input type="date" v-model="alumno.FechaEstimadaTitulo" required />
            </div>

            <div class="field">
              <label>Institución donde cursa</label>
              <input type="text" v-model="alumno.InstitucionTitulo" required />
            </div>
          </div>

        </section>

        <section>
          <h2>Inscripción</h2>
          <hr />

          <div class="form-row">
            <div class="field">
              <label>Turno</label>
              <select v-model="alumno.Turno">
                <option disabled value="">Seleccionar turno</option>
                <option>Mañana</option>
                <option>Tarde</option>
                <option>Noche</option>
              </select>
            </div>

            <div class="field">
              <label>Carrera</label>
              <select v-model="alumno.CarreraId">
                <option disabled value="">Seleccionar carrera</option>
                <option
                  v-for="carrera in carreras"
                  :key="carrera.id"
                  :value="carrera.id"
                >
                  {{ carrera.nombre }}
                </option>
              </select>
            </div>
          </div>
        </section>

        <p v-if="error" class="text-center mt-2" style="color: var(--color-danger)">
          {{ error }}
        </p>

        <button
          type="submit"
          class="btn btn-success btn-block"
          :disabled="loading"
        >
          {{ loading ? 'Guardando...' : 'Enviar inscripción' }}
        </button>

        <footer class="form-footer">
          <p>
            Revisá que los datos ingresados sean correctos antes de enviar el formulario.
          </p>
          <p class="footer-copy">
            Formulario desarrollado por estudiantes de 3.º año de la TSAS — 2026
          </p>
        </footer>

      </form>
    </div>
  </main>
</template>

<script setup>
import { reactive, ref, watch, onMounted } from 'vue'
import { computed } from 'vue'

const edad = computed(() => {
  if (!alumno.FechaNacimiento) return null
  const nacimiento = new Date(alumno.FechaNacimiento)
  const diff = new Date().getTime() - nacimiento.getTime()
  return Math.floor(diff / (1000 * 60 * 60 * 24 * 365.25))
})


const alumno = reactive({
  Nombre: "",
  Apellido: "",
  DNI: "",
  Email: "",
  FechaNacimiento: "",
  Direccion: "",
  Nacionalidad: "",
  Telefono: "",
  TituloSecundario: "",
  Turno: "",
  CarreraId: 0,
  FechaInscripcion: "",
  FechaEstimadaTitulo: "",
  InstitucionTitulo: ""
})

const documentacion = reactive({
  titulo: false,
  tituloEnTramite: false,
  materiasAdeudadas: false,
  alumnoRegular: false
})

watch(() => documentacion.titulo, (v) => { 
  if (v) {
    documentacion.tituloEnTramite = false
  } else {
    alumno.FechaInscripcion = ""
    alumno.TituloSecundario = ""
  }
})
watch(() => documentacion.tituloEnTramite, (v) => { 
  if (v) {
    documentacion.titulo = false
  } else {
    alumno.FechaEstimadaTitulo = ""
    alumno.InstitucionTitulo = ""
  }
})

const loading = ref(false)
const error = ref(null)

const carreras = ref([])
const API = import.meta.env.VITE_API_URL

const cargarCarreras = async () => {
  try {
    const res = await fetch(`${API}/api/carreras`)
    if (!res.ok) throw new Error(`Error HTTP ${res.status}`)
    carreras.value = await res.json()
  } catch (err) {
    console.error(err)
    error.value = "No se pudieron cargar las carreras"
  }
}

onMounted(cargarCarreras)

const resetAlumno = () => {
  alumno.Nombre = ""
  alumno.Apellido = ""
  alumno.DNI = ""
  alumno.Email = ""
  alumno.FechaNacimiento = ""
  alumno.Direccion = ""
  alumno.Nacionalidad = ""
  alumno.Telefono = ""
  alumno.TituloSecundario = ""
  alumno.Turno = ""
  alumno.CarreraId = 0
  alumno.FechaInscripcion = ""
  alumno.FechaEstimadaTitulo = ""
  alumno.InstitucionTitulo = ""
  documentacion.titulo = false
  documentacion.tituloEnTramite = false
  documentacion.materiasAdeudadas = false
  documentacion.alumnoRegular = false
}

const guardarAlumno = async () => {
  error.value = null
  loading.value = true

  if (!alumno.Nombre || !alumno.Apellido || !alumno.DNI || !alumno.CarreraId) {
    error.value = "Nombre, Apellido, DNI y Carrera son obligatorios"
    loading.value = false
    return
  }

  if (documentacion.titulo && (!alumno.FechaInscripcion || !alumno.TituloSecundario)) {
    error.value = "Si posee título, debe completar fecha de egreso y título secundario"
    loading.value = false
    return
  }

  if (documentacion.tituloEnTramite && (!alumno.FechaEstimadaTitulo || !alumno.InstitucionTitulo)) {
    error.value = "Si tiene título en trámite, debe completar fecha estimada e institución"
    loading.value = false
    return
  }

  try {
    const res = await fetch(`${API}/api/alumnos`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(alumno)
    })
    if (!res.ok) throw new Error(`Error HTTP ${res.status}`)

    alert("Alumno guardado correctamente")
    resetAlumno()
  } catch (err) {
    console.error(err)
    if (err instanceof Error) error.value = err.message
    else error.value = "Hubo un error al guardar el alumno"
  } finally {
    loading.value = false
  }
}
</script>