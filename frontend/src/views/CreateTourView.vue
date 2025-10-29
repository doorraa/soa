<template>
  <div class="container">
    <div class="card">
      <h2>Create New Tour</h2>
      
      <form @submit.prevent="handleCreate">
        <div class="form-group">
          <label>Tour Name</label>
          <input v-model="form.name" type="text" required minlength="3" maxlength="100" />
        </div>
        
        <div class="form-group">
          <label>Description</label>
          <textarea v-model="form.description" required minlength="10"></textarea>
        </div>
        
        <div class="form-group">
          <label>Difficulty</label>
          <select v-model="form.difficulty" required>
            <option value="0">Easy</option>
            <option value="1">Medium</option>
            <option value="2">Hard</option>
          </select>
        </div>
        
        <div class="form-group">
          <label>Duration (hours)</label>
          <input v-model.number="form.durationHours" type="number" step="0.5" min="0.5" required />
        </div>
        
        <div class="form-group">
          <label>Tags (comma separated)</label>
          <input v-model="tags" type="text" placeholder="adventure, nature, history" />
        </div>
        
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Creating...' : 'Create Tour (Draft)' }}
        </button>
        <router-link to="/my-tours" class="ml-10">
          <button type="button" class="btn btn-secondary">Cancel</button>
        </router-link>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useToursStore } from '@/stores/tours'
import { useRouter } from 'vue-router'

const toursStore = useToursStore()
const router = useRouter()

const form = ref({
  name: '',
  description: '',
  difficulty: 0,
  durationHours: 1.0,
  tags: []
})

const tags = ref('')
const loading = ref(false)

const handleCreate = async () => {
  loading.value = true
  
  try {
    form.value.difficulty = parseInt(form.value.difficulty)
    form.value.tags = tags.value
      .split(',')
      .map(tag => tag.trim())
      .filter(tag => tag.length > 0)
    
    const tour = await toursStore.createTour(form.value)
    router.push(`/tour/${tour.id}`)
  } catch (err) {
    alert('Failed to create tour')
  } finally {
    loading.value = false
  }
}
</script>