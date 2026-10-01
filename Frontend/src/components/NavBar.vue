<template>
  <header v-if="!ocultarNavbar" class="navbar">
    <div class="logo">
      <RouterLink to="/"><img src=""></RouterLink>
    </div>
    <nav class="menu">
      <RouterLink to="/inscripcion">Inscripción</RouterLink>
      <RouterLink to="/contacto">Contacto</RouterLink>
    </nav>
  </header>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const ocultarNavbar = computed(() =>
  route.path === '/inscripcion'
)
</script>
