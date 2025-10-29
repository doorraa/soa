<template>
  <div class="container">
    <div class="card">
      <h2>Published Tours</h2>
      
      <div v-if="loading" class="loading">Loading...</div>
      
      <div v-else-if="tours.length === 0" class="empty-state">
        No published tours available
      </div>
      
      <div v-else class="card-grid">
        <div v-for="tour in tours" :key="tour.id" class="card">
          <h3>{{ tour.name }}</h3>
          <p>{{ tour.description }}</p>
          
          <div class="mt-20">
            <span class="badge badge-published">{{ tour.difficulty === 0 ? 'Easy' : tour.difficulty === 1 ? 'Medium' : 'Hard' }}</span>
            <p><strong>Price:</strong> ${{ tour.price }}</p>
            <p><strong>Duration:</strong> {{ tour.durationHours }}h</p>
            <p><strong>Key Points:</strong> {{ tour.keyPointsCount }}</p>
          </div>
          
          <div class="tags" v-if="tour.tags && tour.tags.length > 0">
            <span v-for="tag in tour.tags" :key="tag" class="tag">{{ tag }}</span>
          </div>
          
          <div class="mt-20 flex gap-10">
            <router-link :to="`/tour/${tour.id}`">
              <button class="btn btn-primary">View Details</button>
            </router-link>
            
            <button 
              v-if="authStore.isTourist()" 
              @click="addToCart(tour.id)" 
              class="btn btn-success"
              :disabled="addingToCart === tour.id"
            >
              {{ addingToCart === tour.id ? 'Adding...' : 'Add to Cart' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useToursStore } from '@/stores/tours'
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'

const toursStore = useToursStore()
const cartStore = useCartStore()
const authStore = useAuthStore()

const tours = ref([])
const loading = ref(true)
const addingToCart = ref(null)

onMounted(async () => {
  try {
    tours.value = await toursStore.getPublishedTours()
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
})

const addToCart = async (tourId) => {
  addingToCart.value = tourId
  try {
    await cartStore.addToCart(tourId)
    alert('Tour added to cart!')
  } catch (err) {
    alert(err.message || 'Failed to add to cart')
  } finally {
    addingToCart.value = null
  }
}
</script>