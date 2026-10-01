import { createRouter, createWebHistory } from 'vue-router'
import { useAuth } from '@/composables/useAuth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    //  Públicas
    {
      path: '/',
      name: 'home',
      component: () => import('../views/public/HomeView.vue'),
      meta: { requiereAuth: true }
    },
    {
      path: '/contacto',
      name: 'contacto',
      component: () => import('../views/public/ContactoView.vue')
    },
    {
      path: '/inscripcion',
      name: 'inscripcion',
      component: () => import('../views/Listados/InscripciónView.vue')
    },

    //  Auth
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/auth/LoginView.vue'),
      meta: { soloInvitado: true }
    },

    //  Administración
    {
      path: '/administracion',
      name: 'administracion',
      component: () => import('../views/administradores/AdministradorView.vue'),
      meta: { requiereAuth: true }
    },
    {
      path: '/agregaradministracion',
      name: 'agregaradministracion',
      component: () => import('../views/administradores/AgregarAdministradorView.vue'),
      meta: { requiereAuth: true }
    },
    {
      path: '/editaradministrador/:id',
      name: 'editaradministrador',
      component: () => import('../views/administradores/EditarAdministradorView.vue'),
      meta: { requiereAuth: true },
      props: true
    },
    {
      path: '/eliminaradministrador/:id',
      name: 'eliminaradministrador',
      component: () => import('../views/administradores/EliminarAdministradorView.vue'),
      meta: { requiereAuth: true },
      props: true
    },

    //  Carreras
    {
      path: '/carreras',
      name: 'carreras',
      component: () => import('../views/carrera/CarreraView.vue'),
      meta: { requiereAuth: true }
    },
    {
      path: '/agregarcarreras',
      name: 'agregarcarreras',
      component: () => import('../views/carrera/AgregarCarreraView.vue'),
      meta: { requiereAuth: true }
    },
    {
      path: '/editarcarrera/:id',
      name: 'editarcarrera',
      component: () => import('../views/carrera/EditarCarreraView.vue'),
      meta: { requiereAuth: true },
      props: true
    },
    {
      path: '/eliminarcarreras/:id',
      name: 'eliminarcarreras',
      component: () => import('../views/carrera/EliminarCarreraView.vue'),
      meta: { requiereAuth: true },
      props: true
    },

    //  Formularios
    {
      path: '/formularios',
      name: 'formularios',
      component: () => import('../views/public/FormulariosView.vue'),
      meta: { requiereAuth: true }
    },
    {
      path: '/agregarformulario',
      name: 'agregarformulario',
      component: () => import('../views/public/AgregarFormularioView.vue'),
      meta: { requiereAuth: true }
    },
    {
      path: '/editarformulario/:id',
      name: 'editarformulario',
      component: () => import('../views/public/EditarFormularioView.vue'),
      meta: { requiereAuth: true },
      props: true
    },
    {
      path: '/eliminarformulario/:id',
      name: 'eliminarformulario',
      component: () => import('../views/public/EliminarFormularioView.vue'),
      meta: { requiereAuth: true },
      props: true
    },

    //  Listados
    {
      path: '/listados',
      name: 'listados',
      component: () => import('../views/Listados/ListadoView.vue'),
      meta: { requiereAuth: true }
    },

    //  404
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('../views/NotFound.vue')
    }
  ]
})

//  Navigation guard global
router.beforeEach((to) => {
  const { isAuthenticated } = useAuth()
  const autenticado = isAuthenticated()

  // Ruta protegida y no hay sesión  redirigir al login
  if (to.meta.requiereAuth && !autenticado)
    return { name: 'login' }

  // Ya está logueado e intenta entrar al login  redirigir al inicio
  if (to.meta.soloInvitado && autenticado)
    return { name: 'home' }
})

export default router