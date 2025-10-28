<template>
  <nav class="navbar">
    <div class="container">
      <h1 @click="$router.push('/')">TouristApp</h1>
      
      <nav v-if="authStore.isAuthenticated">
        <router-link to="/">Home</router-link>
        <router-link to="/profile">Profile</router-link>
        <router-link to="/blogs">Blogs</router-link>
        <router-link to="/followers">Followers</router-link>
        <router-link to="/tours">Tours</router-link>
        
        <!-- Guide only -->
        <router-link v-if="authStore.isGuide()" to="/my-tours">My Tours</router-link>
        <router-link v-if="authStore.isGuide()" to="/create-tour">Create Tour</router-link>
        
        <!-- Tourist only -->
        <router-link v-if="authStore.isTourist()" to="/position-simulator">Simulator</router-link>
        <router-link v-if="authStore.isTourist()" to="/cart">Cart</router-link>
        <router-link v-if="authStore.isTourist()" to="/tour-execution">Active Tour</router-link>
        
        <button @click="logout">Logout</button>
      </nav>
      
      <nav v-else>
        <router-link to="/login">Login</router-link>
        <router-link to="/register">Register</router-link>
      </nav>
    </div>
  </nav>
</template>

<script setup>
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

const logout = () => {
  authStore.logout()
  router.push('/login')
}
</script>