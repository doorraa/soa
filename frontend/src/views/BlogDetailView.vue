<template>
  <div class="container">
    <div v-if="loading" class="loading">Loading...</div>
    
    <div v-else-if="blog" class="card">
      <h2>{{ blog.title }}</h2>
      <p>By {{ blog.username }} - {{ new Date(blog.createdAt).toLocaleDateString() }}</p>
      
      <div v-if="blog.imageUrls && blog.imageUrls.length > 0" class="mt-20">
        <img 
          v-for="(url, index) in blog.imageUrls" 
          :key="index" 
          :src="url" 
          alt="Blog image"
          style="max-width: 100%; margin-bottom: 10px; border-radius: 8px;"
        />
      </div>
      
      <p class="mt-20">{{ blog.description }}</p>
      
      <!-- Comments Section -->
      <div class="card mt-20">
        <h3>Comments ({{ comments.length }})</h3>
        
        <div v-if="comments.length === 0" class="empty-state">
          No comments yet. Be the first to comment!
        </div>
        
        <div v-else>
          <div v-for="comment in comments" :key="comment.id" class="comment">
            <div class="comment-author">{{ comment.username }}</div>
            <div class="comment-date">{{ new Date(comment.createdAt).toLocaleString() }}</div>
            <div>{{ comment.content }}</div>
          </div>
        </div>
        
        <!-- Add Comment Form -->
        <form @submit.prevent="handleAddComment" class="mt-20">
          <div class="form-group">
            <label>Add a comment</label>
            <textarea v-model="commentContent" required minlength="1"></textarea>
          </div>
          
          <button type="submit" class="btn btn-primary" :disabled="addingComment">
            {{ addingComment ? 'Posting...' : 'Post Comment' }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useBlogStore } from '@/stores/blog'
import { useRoute } from 'vue-router'

const blogStore = useBlogStore()
const route = useRoute()

const blog = ref(null)
const comments = ref([])
const loading = ref(true)
const commentContent = ref('')
const addingComment = ref(false)

onMounted(async () => {
  try {
    const blogId = route.params.id
    blog.value = await blogStore.getBlog(blogId)
    comments.value = await blogStore.getComments(blogId)
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
})

const handleAddComment = async () => {
  addingComment.value = true
  
  try {
    await blogStore.addComment(route.params.id, commentContent.value)
    comments.value = await blogStore.getComments(route.params.id)
    commentContent.value = ''
  } catch (err) {
    alert('Failed to add comment. You must follow this user first.')
  } finally {
    addingComment.value = false
  }
}
</script>