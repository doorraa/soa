import { defineStore } from 'pinia'
import api from '@/services/api'

export const useBlogStore = defineStore('blog', {
  state: () => ({
    blogs: [],
    myBlogs: []
  }),

  actions: {
    async createBlog(blogData) {
      try {
        const response = await api.post('/blog', blogData)
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getMyBlogs() {
      try {
        const response = await api.get('/blog/my')
        this.myBlogs = response.data
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getBlogsByUser(userId) {
      try {
        const response = await api.get(`/blog/user/${userId}`)
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getBlog(id) {
      try {
        const response = await api.get(`/blog/${id}`)
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async updateBlog(id, blogData) {
      try {
        const response = await api.put(`/blog/${id}`, blogData)
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async deleteBlog(id) {
      try {
        await api.delete(`/blog/${id}`)
        this.myBlogs = this.myBlogs.filter(b => b.id !== id)
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getComments(blogId) {
      try {
        const response = await api.get(`/blog/${blogId}/comments`)
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async addComment(blogId, content) {
      try {
        const response = await api.post(`/blog/${blogId}/comments`, { content })
        return response.data
      } catch (error) {
        throw error.response?.data || error.message
      }
    },

    async getFeed() {
    try {
        const response = await api.get('/blog/feed')
        return response.data
    } catch (error) {
        throw error.response?.data || error.message
    }
    }
  }
})