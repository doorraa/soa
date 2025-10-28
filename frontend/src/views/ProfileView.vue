<template>
  <div class="container">
    <div class="card">
      <h2>My Profile</h2>
      
      <div v-if="loading" class="loading">Loading...</div>
      
      <div v-else-if="profile" class="profile-header">
        <img 
          v-if="profile.profilePictureUrl" 
          :src="profile.profilePictureUrl" 
          alt="Profile" 
          class="profile-picture"
        />
        
        <h3>{{ profile.firstName }} {{ profile.lastName }}</h3>
        <p>@{{ profile.username }}</p>
        <p><em>"{{ profile.motto }}"</em></p>
        <p>{{ profile.bio }}</p>
        
        <button @click="editing = true" class="btn btn-primary mt-20">
          Edit Profile
        </button>
      </div>
      
      <!-- Edit Form -->
      <div v-if="editing" class="card mt-20">
        <h3>Edit Profile</h3>
        <form @submit.prevent="handleUpdate">
          <div class="form-group">
            <label>First Name</label>
            <input v-model="editForm.firstName" type="text" />
          </div>
          
          <div class="form-group">
            <label>Last Name</label>
            <input v-model="editForm.lastName" type="text" />
          </div>
          
          <div class="form-group">
            <label>Bio</label>
            <textarea v-model="editForm.bio"></textarea>
          </div>
          
          <div class="form-group">
            <label>Motto</label>
            <input v-model="editForm.motto" type="text" />
          </div>
          
          <div class="form-group">
            <label>Profile Picture URL</label>
            <input v-model="editForm.profilePictureUrl" type="text" />
          </div>
          
          <button type="submit" class="btn btn-success" :disabled="updating">
            {{ updating ? 'Saving...' : 'Save Changes' }}
          </button>
          <button type="button" @click="editing = false" class="btn btn-secondary ml-10">
            Cancel
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useProfileStore } from '@/stores/profile'

const profileStore = useProfileStore()

const profile = ref(null)
const loading = ref(true)
const editing = ref(false)
const updating = ref(false)
const editForm = ref({
  firstName: '',
  lastName: '',
  bio: '',
  motto: '',
  profilePictureUrl: ''
})

onMounted(async () => {
  try {
    profile.value = await profileStore.getMyProfile()
    editForm.value = { ...profile.value }
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
})

const handleUpdate = async () => {
  updating.value = true
  try {
    profile.value = await profileStore.updateProfile(editForm.value)
    editing.value = false
  } catch (err) {
    alert('Failed to update profile')
  } finally {
    updating.value = false
  }
}
</script>