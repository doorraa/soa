<template>
  <div class="container">
    <div class="card">
      <div class="flex justify-between items-center">
        <h2>My Tours</h2>
        <router-link to="/create-tour">
          <button class="btn btn-primary">Create New Tour</button>
        </router-link>
      </div>
      
      <div v-if="loading" class="loading">Loading...</div>
      
      <div v-else-if="tours.length === 0" class="empty-state">
        You haven't created any tours yet
      </div>
      
      <div v-else class="card-grid">
        <div v-for="tour in tours" :key="tour.id" class="card">
          <h3>{{ tour.name }}</h3>
          <span
            class="badge"
            :class="`badge-${tour.status}`"
            >
            {{ tour.status === 0 ? 'Draft' : tour.status === 1 ? 'Published' : 'Archived' }}
          </span>
          
          <p class="mt-20">{{ tour.description.substring(0, 100) }}...</p>
          
          <div class="mt-20">
            <p><strong>Difficulty:</strong> {{ getDifficultyText(tour.difficulty) }}</p>
            <p><strong>Price:</strong> ${{ tour.price }}</p>
            <p><strong>Key Points:</strong> {{ tour.keyPointsCount }}</p>
          </div>
          
          <div class="mt-20 flex gap-10" style="flex-wrap: wrap;">
            <router-link :to="`/tour/${tour.id}`">
              <button class="btn btn-primary">View</button>
            </router-link>
            
            <button 
              v-if="tour.status === 0" 
              @click="showAddKeyPoint(tour)"
              class="btn btn-secondary"
            >
              Add Key Point
            </button>
            
            <button 
              v-if="tour.status === 0 && tour.keyPointsCount >= 2" 
              @click="showPublish(tour)"
              class="btn btn-success"
            >
              Publish
            </button>
            
            <button 
              v-if="tour.status === 1" 
              @click="archiveTour(tour.id)"
              class="btn btn-secondary"
            >
              Archive
            </button>
            
            <button 
              @click="deleteTour(tour.id)"
              class="btn btn-danger"
            >
              Delete
            </button>
          </div>
        </div>
      </div>
    </div>
    
    <!-- Add Key Point Modal -->
    <div v-if="addingKeyPoint" class="modal">
      <div class="modal-content card">
        <h3>Add Key Point to {{ selectedTour?.name }}</h3>
        
        <form @submit.prevent="handleAddKeyPoint">
          <div class="form-group">
            <label>Name</label>
            <input v-model="keyPointForm.name" type="text" required minlength="3" />
          </div>
          
          <div class="form-group">
            <label>Description</label>
            <textarea v-model="keyPointForm.description" required></textarea>
          </div>
          
          <div class="form-group">
            <label>Image URL</label>
            <input v-model="keyPointForm.imageUrl" type="text" />
          </div>
          
          <div class="form-group">
            <label>Latitude</label>
            <input v-model.number="keyPointForm.latitude" type="number" step="0.000001" required />
          </div>
          
          <div class="form-group">
            <label>Longitude</label>
            <input v-model.number="keyPointForm.longitude" type="number" step="0.000001" required />
          </div>
          
          <div class="form-group">
            <label>Order (1 = start)</label>
            <input v-model.number="keyPointForm.order" type="number" min="1" required />
          </div>
          
          <p class="text-muted">Click on map to set location:</p>
          <MapComponent 
            :clickable="true"
            @map-click="handleMapClick"
          />
          
          <button type="submit" class="btn btn-primary mt-20" :disabled="addingKp">
            {{ addingKp ? 'Adding...' : 'Add Key Point' }}
          </button>
          <button type="button" @click="addingKeyPoint = false" class="btn btn-secondary ml-10">
            Cancel
          </button>
        </form>
      </div>
    </div>
    
    <!-- Publish Modal -->
    <div v-if="publishing" class="modal">
      <div class="modal-content card">
        <h3>Publish Tour: {{ selectedTour?.name }}</h3>
        
        <form @submit.prevent="handlePublish">
          <div class="form-group">
            <label>Set Price ($)</label>
            <input v-model.number="publishPrice" type="number" step="0.01" min="0.01" max="10000" required />
          </div>
          
          <button type="submit" class="btn btn-success" :disabled="publishingTour">
            {{ publishingTour ? 'Publishing...' : 'Publish Tour' }}
          </button>
          <button type="button" @click="publishing = false" class="btn btn-secondary ml-10">
            Cancel
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useToursStore } from '@/stores/tours'
import MapComponent from '@/components/MapComponent.vue'

const toursStore = useToursStore()

const tours = ref([])
const loading = ref(true)
const addingKeyPoint = ref(false)
const publishing = ref(false)
const selectedTour = ref(null)
const addingKp = ref(false)
const publishingTour = ref(false)
const publishPrice = ref(0)

const keyPointForm = ref({
  name: '',
  description: '',
  imageUrl: '',
  latitude: 45.2517,
  longitude: 19.8659,
  order: 1
})

onMounted(async () => {
  await loadTours()
})

const loadTours = async () => {
  loading.value = true
  try {
    tours.value = await toursStore.getMyTours()
    console.log(tours.value)
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

const getDifficultyText = (difficulty) => {
  const map = { 0: 'Easy', 1: 'Medium', 2: 'Hard' }
  return map[difficulty] || 'Unknown'
}

const showAddKeyPoint = (tour) => {
  selectedTour.value = tour
  keyPointForm.value.order = tour.keyPointsCount + 1
  addingKeyPoint.value = true
}

const handleMapClick = ({ lat, lng }) => {
  keyPointForm.value.latitude = lat
  keyPointForm.value.longitude = lng
}

const handleAddKeyPoint = async () => {
  addingKp.value = true
  try {
    await toursStore.addKeyPoint(selectedTour.value.id, keyPointForm.value)
    addingKeyPoint.value = false
    await loadTours()
    
    // Reset form
    keyPointForm.value = {
      name: '',
      description: '',
      imageUrl: '',
      latitude: 45.2517,
      longitude: 19.8659,
      order: 1
    }
  } catch (err) {
    alert('Failed to add key point')
  } finally {
    addingKp.value = false
  }
}

const showPublish = (tour) => {
  selectedTour.value = tour
  publishPrice.value = 10
  publishing.value = true
}

const handlePublish = async () => {
  publishingTour.value = true
  try {
    await toursStore.publishTour(selectedTour.value.id, publishPrice.value)
    publishing.value = false
    await loadTours()
  } catch (err) {
    alert('Failed to publish tour')
  } finally {
    publishingTour.value = false
  }
}

const archiveTour = async (tourId) => {
  if (!confirm('Archive this tour?')) return
  
  try {
    await toursStore.archiveTour(tourId)
    await loadTours()
  } catch (err) {
    alert('Failed to archive tour')
  }
}

const deleteTour = async (tourId) => {
  if (!confirm('Delete this tour? This cannot be undone.')) return
  
  try {
    await toursStore.deleteTour(tourId)
    await loadTours()
  } catch (err) {
    alert('Failed to delete tour')
  }
}
</script>

<style scoped>
.modal {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  overflow-y: auto;
}

.modal-content {
  max-width: 600px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
  margin: 20px;
}

.ml-10 {
  margin-left: 10px;
}
</style>