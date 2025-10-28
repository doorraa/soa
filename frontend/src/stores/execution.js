import { defineStore } from 'pinia'
import api from '@/services/api'

export const useExecutionStore = defineStore('execution', {
  state: () => ({
    activeExecution: null,
    history: []
  }),

  actions: {
    async startTour(tourId) {
      try {
        const response = await api.post('/tourexecution/start', { tourId })
        this.activeExecution = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async checkKeyPoint(latitude, longitude) {
      try {
        const response = await api.post('/tourexecution/check-keypoint', { latitude, longitude })
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async completeTour() {
      try {
        const response = await api.put('/tourexecution/complete')
        this.activeExecution = null
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async abandonTour() {
      try {
        const response = await api.put('/tourexecution/abandon')
        this.activeExecution = null
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getActiveTour() {
      try {
        const response = await api.get('/tourexecution/active')
        this.activeExecution = response.data
        return response.data
      } catch (error) {
        this.activeExecution = null
        throw error.response?.data || error.message
      }
    },

    async getHistory() {
      try {
        const response = await api.get('/tourexecution/history')
        this.history = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    }
  }
})