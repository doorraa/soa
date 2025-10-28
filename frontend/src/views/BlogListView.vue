<template>
  <div class="container">
    <div class="card">
      <div class="flex justify-between items-center">
        <h2>Blogs</h2>
        <router-link to="/create-blog">
          <button class="btn btn-primary">Create Blog</button>
        </router-link>
      </div>
      
      <div v-if="loading" class="loading">Loading...</div>
      
      <div v-else-if="blogs.length === 0" class="empty-state">
        No blogs yet. Create your first blog!
      </div>
      
      <div v-else class="card-grid">
        <div v-for="blog in blogs" :key="blog.id" class="card">
          <h3>{{ blog.title }}</h3>
          <p>By {{ blog.username }}</p>
          <p>{{ blog.description.substring(0, 100) }}...</p>
          <p class="text-muted">{{ new Date(blog.createdAt).toLocaleDateString() }}</p>
          
          <router-link :to="`/blog/${blog.id}`">
            <button class="btn btn-primary mt-20">Read More</button>
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useBlogStore } from '@/stores/blog'

const blogStore = useBlogStore()

const blogs = ref([])
const loading = ref(true)

onMounted(async () => {
  try {
    blogs.value = await blogStore.getMyBlogs()
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
})
</script>