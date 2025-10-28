<template>
  <div id="app">
    <Navbar />
    <RouterView />
  </div>
</template>

<script setup>
import Navbar from './components/Navbar.vue'
</script>

<style>
#app {
  min-height: 100vh;
}
</style>