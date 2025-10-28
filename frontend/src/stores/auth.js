import { defineStore } from 'pinia'
import api from '@/services/api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    token: localStorage.getItem('token') || null,
    isAuthenticated: !!localStorage.getItem('token')
  }),

  actions: {
    async register(userData) {
      try {
        const response = await api.post('/auth/register', userData)
        this.token = response.data.token
        this.user = {
          id: response.data.userId,
          username: response.data.username,
          role: response.data.role
        }
        localStorage.setItem('token', this.token)
        this.isAuthenticated = true
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async login(credentials) {
      try {
        const response = await api.post('/auth/login', credentials)
        this.token = response.data.token
        this.user = {
          id: response.data.userId,
          username: response.data.username,
          role: response.data.role
        }
        localStorage.setItem('token', this.token)
        this.isAuthenticated = true
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    logout() {
      this.user = null
      this.token = null
      this.isAuthenticated = false
      localStorage.removeItem('token')
    },

    isGuide() {
      return this.user?.role === 1 // Guide = 1
    },

    isTourist() {
      return this.user?.role === 0 // Tourist = 0
    }
  }
})