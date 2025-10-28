<template>
  <div class="container">
    <div class="card">
      <h2>Create Blog</h2>
      
      <form @submit.prevent="handleCreate">
        <div class="form-group">
          <label>Title</label>
          <input v-model="form.title" type="text" required minlength="3" />
        </div>
        
        <div class="form-group">
          <label>Description</label>
          <textarea v-model="form.description" required minlength="10"></textarea>
        </div>
        
        <div class="form-group">
          <label>Image URLs (comma separated)</label>
          <input v-model="imageUrls" type="text" placeholder="https://example.com/img1.jpg, https://example.com/img2.jpg" />
        </div>
        
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Creating...' : 'Create Blog' }}
        </button>
        <router-link to="/blogs" class="ml-10">
          <button type="button" class="btn btn-secondary">Cancel</button>
        </router-link>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useBlogStore } from '@/stores/blog'
import { useRouter } from 'vue-router'

const blogStore = useBlogStore()
const router = useRouter()

const form = ref({
  title: '',
  description: '',
  imageUrls: []
})

const imageUrls = ref('')
const loading = ref(false)

const handleCreate = async () => {
  loading.value = true
  
  try {
    form.value.imageUrls = imageUrls.value
      .split(',')
      .map(url => url.trim())
      .filter(url => url.length > 0)
    
    await blogStore.createBlog(form.value)
    router.push('/blogs')
  } catch (err) {
    alert('Failed to create blog')
  } finally {
    loading.value = false
  }
}
</script>