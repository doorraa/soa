import { defineStore } from 'pinia'
import api from '@/services/api'

export const useToursStore = defineStore('tours', {
  state: () => ({
    tours: [],
    myTours: [],
    publishedTours: []
  }),

  actions: {
    async createTour(tourData) {
      try {
        const response = await api.post('/tours', tourData)
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getMyTours() {
      try {
        const response = await api.get('/tours/my')
        this.myTours = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getPublishedTours() {
      try {
        const response = await api.get('/tours/published')
        this.publishedTours = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getTour(id) {
      try {
        const response = await api.get(`/tours/${id}`)
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async updateTour(id, tourData) {
      try {
        const response = await api.put(`/tours/${id}`, tourData)
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async deleteTour(id) {
      try {
        await api.delete(`/tours/${id}`)
        this.myTours = this.myTours.filter(t => t.id !== id)
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async addKeyPoint(tourId, keyPointData) {
      try {
        const response = await api.post(`/tours/${tourId}/keypoints`, keyPointData)
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getKeyPoints(tourId) {
      try {
        const response = await api.get(`/tours/${tourId}/keypoints`)
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async publishTour(tourId, price) {
      try {
        const response = await api.put(`/tours/${tourId}/publish`, { price })
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async archiveTour(tourId) {
      try {
        const response = await api.put(`/tours/${tourId}/archive`)
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    }
  }
})