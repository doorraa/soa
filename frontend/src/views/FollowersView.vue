<template>
  <div class="container">
    <div class="card">
      <h2>Followers & Following</h2>
      
      <div class="mt-20">
        <h3>Following ({{ following.length }})</h3>
        <div v-if="following.length === 0" class="empty-state">
          You're not following anyone yet
        </div>
        <ul v-else class="list">
          <li v-for="user in following" :key="user.id" class="list-item">
            <span>{{ user.username }}</span>
            <button @click="handleUnfollow(user.id)" class="btn btn-danger">
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
          <li v-for="user in followers" :key="user.id" class="list-item">
            <span>{{ user.username }}</span>
          </li>
        </ul>
      </div>
      
      <div class="mt-20">
        <h3>Recommendations</h3>
        <div v-if="recommendations.length === 0" class="empty-state">
          No recommendations available
        </div>
        <ul v-else class="list">
          <li v-for="user in recommendations" :key="user.id" class="list-item">
            <span>{{ user.username }}</span>
            <button @click="handleFollow(user.id)" class="btn btn-primary">
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

const followersStore = useFollowersStore()

const following = ref([])
const followers = ref([])
const recommendations = ref([])

onMounted(async () => {
  await loadData()
})

const loadData = async () => {
  try {
    following.value = await followersStore.getFollowing()
    followers.value = await followersStore.getFollowers()
    recommendations.value = await followersStore.getRecommendations()
  } catch (err) {
    console.error(err)
  }
}

const handleFollow = async (userId) => {
  try {
    await followersStore.followUser(userId)
    await loadData()
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