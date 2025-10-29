<template>
  <div class="container">
    <div class="card">
      <h2>Tour Execution</h2>
      
      <div v-if="loading" class="loading">Loading...</div>
      
      <!-- No Active Tour -->
      <div v-else-if="!activeExecution" class="empty-state">
        <p>No active tour session</p>
        <router-link to="/cart">
          <button class="btn btn-primary mt-20">View Purchased Tours</button>
        </router-link>
      </div>
      
      <!-- Active Tour -->
      <div v-else>
        <h3>{{ activeExecution.tourName }}</h3>
        
        <div class="execution-progress">
          <p><strong>Status:</strong> {{ activeExecution.status === 0 ? 'Active' : activeExecution.status === 1 ? 'Completed' : 'Abandoned' }}</p>
          <p><strong>Started:</strong> {{ new Date(activeExecution.startedAt).toLocaleString() }}</p>
          <p><strong>Progress:</strong> {{ activeExecution.completedKeyPointsCount }} / {{ activeExecution.totalKeyPointsCount }} key points</p>
          
          <div class="progress-bar">
            <div 
              class="progress-fill" 
              :style="{ width: progressPercent + '%' }"
            ></div>
          </div>
          <p class="text-center mt-20">{{ progressPercent }}% Complete</p>
        </div>
        
        <!-- Current Position -->
        <div v-if="currentPosition" class="alert alert-info mt-20">
          <strong>Your Position:</strong><br>
          Lat: {{ currentPosition.latitude.toFixed(6) }}, 
          Lng: {{ currentPosition.longitude.toFixed(6) }}
        </div>
        
        <!-- Check Key Point Status -->
        <div v-if="checkResult" class="alert mt-20" :class="checkResult.found ? 'alert-success' : 'alert-info'">
          {{ checkResult.message }}
          <p v-if="checkResult.found">
            <strong>Completed:</strong> {{ activeExecution.completedKeyPointsCount }} / {{ activeExecution.totalKeyPointsCount }}
          </p>
        </div>
        
        <!-- Map -->
        <div class="mt-20">
          <h4>Your Location</h4>
          <MapComponent 
            v-if="currentPosition"
            :center="[currentPosition.latitude, currentPosition.longitude]"
            :markers="[{
              lat: currentPosition.latitude,
              lng: currentPosition.longitude,
              popup: 'You are here'
            }]"
          />
        </div>
        
        <!-- Actions -->
        <div class="mt-20 flex gap-10">
          <button 
            @click="checkKeyPoint" 
            class="btn btn-primary"
            :disabled="checking"
          >
            {{ checking ? 'Checking...' : 'Check Key Point' }}
          </button>
          
          <button 
            @click="completeTour" 
            class="btn btn-success"
            :disabled="completing"
          >
            {{ completing ? 'Completing...' : 'Complete Tour' }}
          </button>
          
          <button 
            @click="abandonTour" 
            class="btn btn-danger"
            :disabled="abandoning"
          >
            {{ abandoning ? 'Abandoning...' : 'Abandon Tour' }}
          </button>
        </div>
        
        <!-- Auto-check every 10 seconds -->
        <div class="mt-20">
          <label>
            <input type="checkbox" v-model="autoCheck" />
            Auto-check every 10 seconds
          </label>
        </div>
      </div>
    </div>
    
    <!-- Start New Tour -->
    <div v-if="!activeExecution && purchasedTours.length > 0" class="card mt-20">
      <h3>Start a Tour</h3>
      <p>Select a purchased tour to start:</p>
      
      <div v-for="token in purchasedTours" :key="token.id" class="list-item">
        <span>{{ token.tourName }}</span>
        <button 
          @click="startTour(token.tourId)" 
          class="btn btn-success"
          :disabled="starting === token.tourId"
        >
          {{ starting === token.tourId ? 'Starting...' : 'Start Tour' }}
        </button>
      </div>
    </div>
    
    <!-- Execution History -->
    <div class="card mt-20">
      <h3>Tour History</h3>
      
      <div v-if="history.length === 0" class="empty-state">
        No tour history yet
      </div>
      
      <div v-else>
        <div v-for="exec in history" :key="exec.id" class="card mb-20">
          <h4>{{ exec.tourName }}</h4>
          <p><strong>Status:</strong> 
            <span class="badge" :class="`badge-${exec.status}`">{{ exec.status === 0 ? 'Active' : exec.status === 1 ? 'Completed' : 'Abandoned' }}</span>
          </p>
          <p><strong>Started:</strong> {{ new Date(exec.startedAt).toLocaleString() }}</p>
          <p v-if="exec.completedAt"><strong>Completed:</strong> {{ new Date(exec.completedAt).toLocaleString() }}</p>
          <p v-if="exec.lastActivity && !exec.completedAt"><strong>Abandoned:</strong> {{ new Date(exec.lastActivity).toLocaleString() }}</p>
          <p><strong>Progress:</strong> {{ exec.completedKeyPointsCount }} / {{ exec.totalKeyPointsCount }} key points</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed, watch } from 'vue'
import { useExecutionStore } from '@/stores/execution'
import { useCartStore } from '@/stores/cart'
import { usePositionStore } from '@/stores/position'
import MapComponent from '@/components/MapComponent.vue'

const executionStore = useExecutionStore()
const cartStore = useCartStore()
const positionStore = usePositionStore()

const activeExecution = ref(null)
const purchasedTours = ref([])
const history = ref([])
const currentPosition = ref(null)
const loading = ref(true)
const starting = ref(null)
const checking = ref(false)
const completing = ref(false)
const abandoning = ref(false)
const checkResult = ref(null)
const autoCheck = ref(false)

let autoCheckInterval = null

const progressPercent = computed(() => {
  if (!activeExecution.value || activeExecution.value.totalKeyPointsCount === 0) {
    return 0
  }
  return Math.round(
    (activeExecution.value.completedKeyPointsCount / activeExecution.value.totalKeyPointsCount) * 100
  )
})

onMounted(async () => {
  await loadData()
})

onUnmounted(() => {
  if (autoCheckInterval) {
    clearInterval(autoCheckInterval)
  }
})

watch(autoCheck, (newValue) => {
  if (newValue) {
    autoCheckInterval = setInterval(async () => {
      if (activeExecution.value && currentPosition.value) {
        await checkKeyPoint()
      }
    }, 10000) // 10 seconds
  } else {
    if (autoCheckInterval) {
      clearInterval(autoCheckInterval)
      autoCheckInterval = null
    }
  }
})

const loadData = async () => {
  loading.value = true
  try {
    // Get active tour
    try {
      activeExecution.value = await executionStore.getActiveTour()
    } catch (err) {
      activeExecution.value = null
    }
    
    // Get purchased tours
    purchasedTours.value = await cartStore.getPurchasedTours()
    
    // Get history
    history.value = await executionStore.getHistory()
    
    // Get current position
    try {
      currentPosition.value = await positionStore.getPosition()
    } catch (err) {
      console.log('Position not set')
    }
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

const startTour = async (tourId) => {
  // Check if position is set
  if (!currentPosition.value) {
    alert('Please set your position first in Position Simulator')
    return
  }
  
  starting.value = tourId
  try {
    activeExecution.value = await executionStore.startTour(tourId)
    alert('Tour started! Good luck!')
  } catch (err) {
    alert(err.message || 'Failed to start tour')
  } finally {
    starting.value = null
  }
}

const checkKeyPoint = async () => {
  if (!currentPosition.value) {
    alert('Position not set')
    return
  }
  
  checking.value = true
  checkResult.value = null
  
  try {
    // First, get updated position
    currentPosition.value = await positionStore.getPosition()
    
    // Then check key point
    const result = await executionStore.checkKeyPoint(
      currentPosition.value.latitude,
      currentPosition.value.longitude
    )
    
    checkResult.value = {
      message: result.message || 'Not near any key point',
      found: !!result.keyPointName
    }
    
    if (result.keyPointName) {
      // Update active execution
      activeExecution.value = await executionStore.getActiveTour()
    }
    
    // Clear result after 5 seconds
    setTimeout(() => {
      checkResult.value = null
    }, 5000)
  } catch (err) {
    checkResult.value = {
      message: err.message || 'Error checking key point',
      found: false
    }
  } finally {
    checking.value = false
  }
}

const completeTour = async () => {
  if (!confirm('Complete this tour?')) return
  
  completing.value = true
  try {
    await executionStore.completeTour()
    alert('Tour completed! Well done!')
    await loadData()
  } catch (err) {
    alert('Failed to complete tour')
  } finally {
    completing.value = false
  }
}

const abandonTour = async () => {
  if (!confirm('Abandon this tour? Progress will be saved.')) return
  
  abandoning.value = true
  try {
    await executionStore.abandonTour()
    alert('Tour abandoned')
    await loadData()
  } catch (err) {
    alert('Failed to abandon tour')
  } finally {
    abandoning.value = false
  }
}
</script>