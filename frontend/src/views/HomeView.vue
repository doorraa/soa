<template>
  <div class="container">
    <div class="card text-center">
      <h1>Welcome to TouristApp</h1>
      <p v-if="!authStore.isAuthenticated">Please login or register to continue</p>
      <p v-else>Hello, {{ authStore.user?.username }}!</p>
      
      <div class="mt-20" v-if="authStore.isAuthenticated">
        <router-link to="/tours">
          <button class="btn btn-primary">Explore Tours</button>
        </router-link>
        <router-link to="/blogs" class="ml-10">
          <button class="btn btn-secondary">Read Blogs</button>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
</script>