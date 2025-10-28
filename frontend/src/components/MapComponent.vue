<template>
  <div class="map-container" ref="mapContainer"></div>
</template>

<script setup>
import { onMounted, ref, watch } from 'vue'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'

const props = defineProps({
  center: {
    type: Array,
    default: () => [45.2517, 19.8659] // Novi Sad
  },
  zoom: {
    type: Number,
    default: 13
  },
  markers: {
    type: Array,
    default: () => []
  },
  clickable: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['map-click', 'marker-click'])

const mapContainer = ref(null)
let map = null
let markersLayer = null

onMounted(() => {
  // Initialize map
  map = L.map(mapContainer.value).setView(props.center, props.zoom)

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    attribution: '© OpenStreetMap contributors'
  }).addTo(map)

  // Markers layer
  markersLayer = L.layerGroup().addTo(map)

  // Add markers
  updateMarkers()

  // Click handler
  if (props.clickable) {
    map.on('click', (e) => {
      emit('map-click', { lat: e.latlng.lat, lng: e.latlng.lng })
    })
  }
})

const updateMarkers = () => {
  if (!markersLayer) return

  markersLayer.clearLayers()

  props.markers.forEach((marker) => {
    const m = L.marker([marker.lat, marker.lng])
      .bindPopup(marker.popup || '')
      .addTo(markersLayer)

    if (marker.clickable) {
      m.on('click', () => {
        emit('marker-click', marker)
      })
    }
  })
}

watch(() => props.markers, updateMarkers, { deep: true })
watch(() => props.center, (newCenter) => {
  if (map) {
    map.setView(newCenter, props.zoom)
  }
})
</script>