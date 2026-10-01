<script setup>
import NavBar from './components/NavBar.vue';
import { RouterView } from 'vue-router';
</script>

<template>
    <NavBar />
  <RouterView />
</template>

<style scoped>
#app {
  display: flex;
  place-items: center;
}
body {
  margin: 0;
}

main {
  padding-top: 60px;
}

</style>
