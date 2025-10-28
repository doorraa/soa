<template>
  <div class="container">
    <div class="card" style="max-width: 500px; margin: 50px auto;">
      <h2>Register</h2>
      
      <div v-if="error" class="alert alert-error">{{ error }}</div>
      
      <form @submit.prevent="handleRegister">
        <div class="form-group">
          <label>Username</label>
          <input v-model="form.username" type="text" required minlength="3" />
        </div>
        
        <div class="form-group">
          <label>Email</label>
          <input v-model="form.email" type="email" required />
        </div>
        
        <div class="form-group">
          <label>Password</label>
          <input v-model="form.password" type="password" required minlength="6" />
        </div>
        
        <div class="form-group">
          <label>Role</label>
          <select v-model="form.role" required>
            <option value="0">Tourist</option>
            <option value="1">Guide</option>
          </select>
        </div>
        
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Loading...' : 'Register' }}
        </button>
      </form>
      
      <p class="mt-20">
        Already have an account? 
        <router-link to="/login">Login here</router-link>
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

const form = ref({
  username: '',
  email: '',
  password: '',
  role: 0
})

const loading = ref(false)
const error = ref('')

const handleRegister = async () => {
  loading.value = true
  error.value = ''
  
  try {
    form.value.role = parseInt(form.value.role)
    await authStore.register(form.value)
    router.push('/')
  } catch (err) {
    error.value = err.message || 'Registration failed'
  } finally {
    loading.value = false
  }
}
</script>