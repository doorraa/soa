<template>
  <div class="container">
    <div v-if="loading" class="loading">Loading...</div>
    
    <div v-else-if="tour" class="card">
      <h2>{{ tour.name }}</h2>
      
      <div class="mt-20">
        <span
            class="badge"
            :class="`badge-${tour.status}`"
            >
            {{ tour.status === 0 ? 'Draft' : tour.status === 1 ? 'Published' : 'Archived' }}
        </span>
        <span class="badge badge-published ml-10">{{ tour.difficulty === 0 ? 'Easy' : tour.difficulty === 1 ? 'Medium' : 'Hard' }}</span>
      </div>
      
      <p class="mt-20">{{ tour.description }}</p>
      
      <div class="mt-20">
        <p><strong>Price:</strong> ${{ tour.price }}</p>
        <p><strong>Duration:</strong> {{ tour.durationHours }} hours</p>
        <p><strong>Key Points:</strong> {{ tour.keyPointsCount }}</p>
        <p><strong>Created:</strong> {{ new Date(tour.createdAt).toLocaleDateString() }}</p>
      </div>
      
      <div class="tags mt-20" v-if="tour.tags && tour.tags.length > 0">
        <span v-for="tag in tour.tags" :key="tag" class="tag">{{ tag }}</span>
      </div>
      
      <!-- Start Point -->
      <div v-if="tour.startPoint" class="card mt-20">
        <h3>Start Point</h3>
        <p><strong>{{ tour.startPoint.name }}</strong></p>
        <p>{{ tour.startPoint.description }}</p>
        <p>Location: {{ tour.startPoint.latitude }}, {{ tour.startPoint.longitude }}</p>
      </div>
      
      <!-- Key Points (if purchased) -->
      <div v-if="showKeyPoints" class="card mt-20">
        <h3>All Key Points</h3>
        <button @click="loadKeyPoints" class="btn btn-primary mb-20">
          Load Key Points
        </button>
        
        <div v-if="keyPoints.length > 0">
          <div v-for="kp in keyPoints" :key="kp.order" class="card mb-20">
            <h4>{{ kp.order }}. {{ kp.name }}</h4>
            <p>{{ kp.description }}</p>
            <p>Location: {{ kp.latitude }}, {{ kp.longitude }}</p>
            <img v-if="kp.imageUrl" :src="kp.imageUrl" alt="Key point" style="max-width: 100%; border-radius: 8px; margin-top: 10px;" />
          </div>
          
          <!-- Map with key points -->
          <MapComponent 
            :center="[keyPoints[0].latitude, keyPoints[0].longitude]"
            :markers="keyPoints.map(kp => ({ 
              lat: kp.latitude, 
              lng: kp.longitude, 
              popup: `${kp.order}. ${kp.name}` 
            }))"
          />
        </div>
      </div>
      
      <!-- Actions -->
      <div class="mt-20 flex gap-10" v-if="authStore.isTourist()">
        <button 
          @click="addToCart" 
          class="btn btn-success"
          :disabled="addingToCart"
        >
          {{ addingToCart ? 'Adding...' : 'Add to Cart' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useToursStore } from '@/stores/tours'
import { useCartStore } from '@/stores/cart'
import { useAuthStore } from '@/stores/auth'
import { useRoute } from 'vue-router'
import MapComponent from '@/components/MapComponent.vue'

const toursStore = useToursStore()
const cartStore = useCartStore()
const authStore = useAuthStore()
const route = useRoute()

const tour = ref(null)
const keyPoints = ref([])
const loading = ref(true)
const addingToCart = ref(false)

const showKeyPoints = computed(() => {
  // Show key points if user is the author or has purchased
  return tour.value && (
    tour.value.authorId === authStore.user?.id ||
    cartStore.hasPurchased(tour.value.id)
  )
})

onMounted(async () => {
  try {
    tour.value = await toursStore.getTour(route.params.id)
    
    // Try to load purchased tours for tourist
    if (authStore.isTourist()) {
      await cartStore.getPurchasedTours()
    }
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
})

const loadKeyPoints = async () => {
  try {
    keyPoints.value = await toursStore.getKeyPoints(route.params.id)
  } catch (err) {
    alert('You must purchase this tour to see all key points')
  }
}

const addToCart = async () => {
  addingToCart.value = true
  try {
    await cartStore.addToCart(route.params.id)
    alert('Tour added to cart!')
  } catch (err) {
    alert(err.message || 'Failed to add to cart')
  } finally {
    addingToCart.value = false
  }
}
</script>