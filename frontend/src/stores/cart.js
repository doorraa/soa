import { defineStore } from 'pinia'
import api from '@/services/api'

export const useCartStore = defineStore('cart', {
  state: () => ({
    cart: { items: [], totalPrice: 0 },
    purchasedTours: []
  }),

  actions: {
    async getCart() {
      try {
        const response = await api.get('/shoppingcart')
        this.cart = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async addToCart(tourId) {
      try {
        const response = await api.post('/shoppingcart/add', { tourId })
        this.cart = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async removeFromCart(tourId) {
      try {
        const response = await api.delete(`/shoppingcart/remove/${tourId}`)
        this.cart = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async checkout() {
      try {
        const response = await api.post('/shoppingcart/checkout')
        this.purchasedTours = response.data
        this.cart = { items: [], totalPrice: 0 }
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getPurchasedTours() {
      try {
        const response = await api.get('/shoppingcart/purchased')
        this.purchasedTours = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    hasPurchased(tourId) {
      return this.purchasedTours.some(t => t.tourId === tourId)
    }
  }
})