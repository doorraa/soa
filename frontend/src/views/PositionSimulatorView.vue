<template>
  <div class="container">
    <div class="card">
      <h2>Position Simulator</h2>
      <p>Click on the map to set your current position</p>
      
      <div v-if="currentPosition" class="alert alert-info mt-20">
        <strong>Current Position:</strong><br>
        Latitude: {{ currentPosition.latitude }}<br>
        Longitude: {{ currentPosition.longitude }}<br>
        Last updated: {{ new Date(currentPosition.updatedAt).toLocaleString() }}
      </div>
      
      <MapComponent 
        :center="mapCenter"
        :markers="markers"
        :clickable="true"
        @map-click="handleMapClick"
      />
      
      <button 
        v-if="pendingPosition" 
        @click="savePosition" 
        class="btn btn-primary mt-20"
        :disabled="saving"
      >
        {{ saving ? 'Saving...' : 'Save Position' }}
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { usePositionStore } from '@/stores/position'
import MapComponent from '@/components/MapComponent.vue'

const positionStore = usePositionStore()

const currentPosition = ref(null)
const pendingPosition = ref(null)
const saving = ref(false)

const mapCenter = computed(() => {
  if (currentPosition.value) {
    return [currentPosition.value.latitude, currentPosition.value.longitude]
  }
  return [45.2517, 19.8659] // Default: Novi Sad
})

const markers = computed(() => {
  const m = []
  if (currentPosition.value) {
    m.push({
      lat: currentPosition.value.latitude,
      lng: currentPosition.value.longitude,
      popup: 'Current Position'
    })
  }
  if (pendingPosition.value) {
    m.push({
      lat: pendingPosition.value.latitude,
      lng: pendingPosition.value.longitude,
      popup: 'New Position (click Save)'
    })
  }
  return m
})

onMounted(async () => {
  try {
    currentPosition.value = await positionStore.getPosition()
  } catch (err) {
    console.log('No position set yet')
  }
})

const handleMapClick = ({ lat, lng }) => {
  pendingPosition.value = { latitude: lat, longitude: lng }
}

const savePosition = async () => {
  saving.value = true
  try {
    currentPosition.value = await positionStore.updatePosition(
      pendingPosition.value.latitude,
      pendingPosition.value.longitude
    )
    pendingPosition.value = null
    alert('Position saved!')
  } catch (err) {
    alert('Failed to save position')
  } finally {
    saving.value = false
  }
}
</script>