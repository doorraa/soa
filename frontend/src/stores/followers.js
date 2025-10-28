import { defineStore } from 'pinia'
import api from '@/services/api'

export const useFollowersStore = defineStore('followers', {
  state: () => ({
    following: [],
    followers: [],
    recommendations: []
  }),

  actions: {
    async followUser(userId) {
      try {
        await api.post(`/followers/follow/${userId}`)
        await this.getFollowing()
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async unfollowUser(userId) {
      try {
        await api.delete(`/followers/unfollow/${userId}`)
        await this.getFollowing()
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getFollowing() {
      try {
        const response = await api.get('/followers/following')
        this.following = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getFollowers() {
      try {
        const response = await api.get('/followers/followers')
        this.followers = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getRecommendations() {
      try {
        const response = await api.get('/followers/recommendations')
        this.recommendations = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    isFollowing(userId) {
      return this.following.some(u => u.id === userId)
    }
  }
})