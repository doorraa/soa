<template>
  <div class="container">
    <div class="card">
      <h2>Followers & Following</h2>
      
      <!-- Search Users -->
      <div class="card mt-20">
        <h3>Search Users</h3>
        <form @submit.prevent="handleSearch" class="flex gap-10">
          <input 
            v-model="searchQuery" 
            type="text" 
            placeholder="Search by username..." 
            class="form-control"
            style="flex: 1;"
          />
          <button type="submit" class="btn btn-primary" :disabled="searching">
            {{ searching ? 'Searching...' : 'Search' }}
          </button>
        </form>
        
        <div v-if="searchResults.length > 0" class="mt-20">
          <h4>Search Results:</h4>
          <ul class="list">
            <li v-for="user in searchResults" :key="user.userId" class="list-item">
              <div>
                <strong>{{ user.username }}</strong>
                <p v-if="user.firstName || user.lastName">
                  {{ user.firstName }} {{ user.lastName }}
                </p>
              </div>
              <button 
                v-if="!followersStore.isFollowing(user.userId) && user.userId !== authStore.user?.id"
                @click="handleFollow(user.userId)" 
                class="btn btn-primary"
              >
                Follow
              </button>
              <span v-else-if="user.userId === authStore.user?.id" class="badge badge-draft">
                You
              </span>
              <span v-else class="badge badge-published">
                Following
              </span>
            </li>
          </ul>
        </div>
        
        <div v-else-if="searched && searchResults.length === 0" class="empty-state">
          No users found
        </div>
      </div>
      
      <div class="mt-20">
        <h3>Following ({{ following.length }})</h3>
        <div v-if="following.length === 0" class="empty-state">
          You're not following anyone yet
        </div>
        <ul v-else class="list">
          <li v-for="user in profileFollowing" :key="user.id" class="list-item">
            <span>@{{ user.username }}</span>
            <button @click="handleUnfollow(user.userId)" class="btn btn-danger">
              Unfollow
            </button>
          </li>
        </ul>
      </div>
      
      <div class="mt-20">
        <h3>Followers ({{ followers.length }})</h3>
        <div v-if="followers.length === 0" class="empty-state">
          No followers yet
        </div>
        <ul v-else class="list">
          <li v-for="user in profileFollowers" :key="user.id" class="list-item">
            <span>@{{ user.username }}</span>
          </li>
        </ul>
      </div>
      
      <div class="mt-20">
        <h3>Recommendations</h3>
        <div v-if="recommendations.length === 0" class="empty-state">
          No recommendations available
        </div>
        <ul v-else class="list">
          <li v-for="user in profileRecommendations" :key="user.id" class="list-item">
            <span>@{{ user.username }}</span>
            <button @click="handleFollow(user.userId)" class="btn btn-primary">
              Follow
            </button>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useFollowersStore } from '@/stores/followers'
import { useProfileStore } from '@/stores/profile'
import { useAuthStore } from '@/stores/auth'

const followersStore = useFollowersStore()
const profileStore = useProfileStore()
const authStore = useAuthStore()

const following = ref([])
const followers = ref([])
const recommendations = ref([])
const searchQuery = ref('')
const searchResults = ref([])
const searching = ref(false)
const searched = ref(false)
const profileFollowing = ref([])
const profileFollowers = ref([])
const profileRecommendations = ref([])

onMounted(async () => {
  await loadData()
})

const loadData = async () => {
  try {
    following.value = await followersStore.getFollowing()
    profileFollowing.value = await Promise.all(
      following.value.map(async (user) => {
        const userId = user.id 
        return await profileStore.getUserProfile(userId)
      })
    )

    followers.value = await followersStore.getFollowers()
    profileFollowers.value = await Promise.all(
      followers.value.map(async (user) => {
        const userId = user.id 
        return await profileStore.getUserProfile(userId)
      })
    )

    recommendations.value = await followersStore.getRecommendations()
    profileRecommendations.value = await Promise.all(
      recommendations.value.map(async (user) => {
        const userId = user.id 
        return await profileStore.getUserProfile(userId)
      })
    )

  } catch (err) {
    console.error(err)
  }
}

const handleSearch = async () => {
  if (searchQuery.value.trim().length < 2) {
    alert('Please enter at least 2 characters')
    return
  }
  
  searching.value = true
  searched.value = false
  try {
    searchResults.value = await profileStore.searchUsers(searchQuery.value)
    searched.value = true
  } catch (err) {
    alert('Search failed')
  } finally {
    searching.value = false
  }
}

const handleFollow = async (userId) => {
  try {
    await followersStore.followUser(userId)
    await loadData()
    // Remove from search results or refresh search
    if (searchQuery.value) {
      await handleSearch()
    }
  } catch (err) {
    alert('Failed to follow user')
  }
}

const handleUnfollow = async (userId) => {
  try {
    await followersStore.unfollowUser(userId)
    await loadData()
  } catch (err) {
    alert('Failed to unfollow user')
  }
}
</script>

<style scoped>
.form-control {
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}
</style>