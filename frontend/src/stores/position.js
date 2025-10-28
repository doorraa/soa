import { defineStore } from 'pinia'
import api from '@/services/api'

export const usePositionStore = defineStore('position', {
  state: () => ({
    currentPosition: null
  }),

  actions: {
    async updatePosition(latitude, longitude) {
      try {
        const response = await api.put('/position', { latitude, longitude })
        this.currentPosition = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getPosition() {
      try {
        const response = await api.get('/position')
        this.currentPosition = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    }
  }
})