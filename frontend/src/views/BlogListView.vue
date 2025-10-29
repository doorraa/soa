<template>
  <div class="container">
    <div class="card">
      <div class="flex justify-between items-center">
        <h2>Blogs</h2>
        <router-link to="/create-blog">
          <button class="btn btn-primary">Create Blog</button>
        </router-link>
      </div>
      
      <!-- Tabs -->
      <div class="flex gap-10 mt-20" style="border-bottom: 2px solid #ddd; padding-bottom: 10px;">
        <button 
          @click="activeTab = 'feed'" 
          class="btn"
          :class="activeTab === 'feed' ? 'btn-primary' : 'btn-secondary'"
        >
          Feed
        </button>
        <button 
          @click="activeTab = 'my'" 
          class="btn"
          :class="activeTab === 'my' ? 'btn-primary' : 'btn-secondary'"
        >
          My Blogs
        </button>
      </div>
      
      <div v-if="loading" class="loading">Loading...</div>
      
      <div v-else-if="displayedBlogs.length === 0" class="empty-state">
        {{ activeTab === 'feed' ? 'No blogs in your feed yet' : 'No blogs yet. Create your first blog!' }}
      </div>
      
      <div v-else class="card-grid">
        <div v-for="blog in displayedBlogs" :key="blog.id" class="card">
          <h3>{{ blog.title }}</h3>
          <p>By {{ blog.username }}</p>
          <p>{{ blog.description.substring(0, 100) }}...</p>
          <p style="color: #7f8c8d; font-size: 14px;">
            {{ new Date(blog.createdAt).toLocaleDateString() }} • {{ blog.commentCount }} comments
          </p>
          
          <router-link :to="`/blog/${blog.id}`">
            <button class="btn btn-primary mt-20">Read More</button>
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useBlogStore } from '@/stores/blog'

const blogStore = useBlogStore()

const feedBlogs = ref([])
const myBlogs = ref([])
const loading = ref(true)
const activeTab = ref('feed')

const displayedBlogs = computed(() => {
  return activeTab.value === 'feed' ? feedBlogs.value : myBlogs.value
})

onMounted(async () => {
  await loadBlogs()
})

const loadBlogs = async () => {
  loading.value = true
  try {
    feedBlogs.value = await blogStore.getFeed()
    myBlogs.value = await blogStore.getMyBlogs()
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}
</script>