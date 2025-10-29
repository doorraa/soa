<template>
  <div class="container">
    <div class="card">
      <h2>Shopping Cart</h2>
      
      <div v-if="loading" class="loading">Loading...</div>
      
      <div v-else-if="cart.items.length === 0" class="empty-state">
        Your cart is empty. <router-link to="/tours">Browse tours</router-link>
      </div>
      
      <div v-else>
        <div v-for="item in cart.items" :key="item.tourId" class="cart-item">
          <div>
            <h3>{{ item.tourName }}</h3>
            <p>${{ item.price }}</p>
          </div>
          <button 
            @click="removeFromCart(item.tourId)" 
            class="btn btn-danger"
            :disabled="removing === item.tourId"
          >
            {{ removing === item.tourId ? 'Removing...' : 'Remove' }}
          </button>
        </div>
        
        <div class="cart-total">
          Total: ${{ cart.totalPrice.toFixed(2) }}
        </div>
        
        <button 
          @click="checkout" 
          class="btn btn-success mt-20"
          :disabled="checkingOut"
        >
          {{ checkingOut ? 'Processing...' : 'Checkout' }}
        </button>
      </div>
    </div>
    
    <!-- Purchased Tours -->
    <div class="card mt-20">
      <h2>My Purchased Tours</h2>
      
      <div v-if="purchasedTours.length === 0" class="empty-state">
        You haven't purchased any tours yet
      </div>
      
      <div v-else>
        <div v-for="token in purchasedTours" :key="token.id" class="list-item">
          <div>
            <h4>{{ token.tourName }}</h4>
            <p>Purchased: {{ new Date(token.purchasedAt).toLocaleDateString() }}</p>
            <p>Price paid: ${{ token.pricePaid }}</p>
          </div>
          <router-link :to="`/tour/${token.tourId}`">
            <button class="btn btn-primary">View Tour</button>
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useCartStore } from '@/stores/cart'

const cartStore = useCartStore()

const cart = ref({ items: [], totalPrice: 0 })
const purchasedTours = ref([])
const loading = ref(true)
const removing = ref(null)
const checkingOut = ref(false)

onMounted(async () => {
  await loadData()
})

const loadData = async () => {
  loading.value = true
  try {
    cart.value = await cartStore.getCart()
    purchasedTours.value = await cartStore.getPurchasedTours()
  } catch (err) {
    console.error(err)
  } finally {
    loading.value = false
  }
}

const removeFromCart = async (tourId) => {
  removing.value = tourId
  try {
    cart.value = await cartStore.removeFromCart(tourId)
  } catch (err) {
    alert('Failed to remove from cart')
  } finally {
    removing.value = null
  }
}

const checkout = async () => {
  if (!confirm(`Proceed with checkout? Total: $${cart.value.totalPrice.toFixed(2)}`)) {
    return
  }
  
  checkingOut.value = true
  try {
    const tokens = await cartStore.checkout()
    alert(`Successfully purchased ${tokens.length} tour(s)!`)
    await loadData()
  } catch (err) {
    alert('Checkout failed: ' + (err.message || 'Unknown error'))
  } finally {
    checkingOut.value = false
  }
}
</script>