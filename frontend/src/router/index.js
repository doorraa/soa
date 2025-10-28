import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import HomeView from '@/views/HomeView.vue'
import LoginView from '@/views/LoginView.vue'
import RegisterView from '@/views/RegisterView.vue'
import ProfileView from '@/views/ProfileView.vue'
import BlogListView from '@/views/BlogListView.vue'
import BlogDetailView from '@/views/BlogDetailView.vue'
import CreateBlogView from '@/views/CreateBlogView.vue'
import FollowersView from '@/views/FollowersView.vue'
/*import ToursView from '@/views/ToursView.vue'
import TourDetailView from '@/views/TourDetailView.vue'
import CreateTourView from '@/views/CreateTourView.vue'
import MyToursView from '@/views/MyToursView.vue'
import PositionSimulatorView from '@/views/PositionSimulatorView.vue'
import ShoppingCartView from '@/views/ShoppingCartView.vue'
import TourExecutionView from '@/views/TourExecutionView.vue'
*/
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView
    },
    {
      path: '/profile',
      name: 'profile',
      component: ProfileView,
      meta: { requiresAuth: true }
    },
    {
      path: '/blogs',
      name: 'blogs',
      component: BlogListView,
      meta: { requiresAuth: true }
    },
    {
      path: '/blog/:id',
      name: 'blog-detail',
      component: BlogDetailView,
      meta: { requiresAuth: true }
    },
    {
      path: '/create-blog',
      name: 'create-blog',
      component: CreateBlogView,
      meta: { requiresAuth: true }
    },
    {
      path: '/followers',
      name: 'followers',
      component: FollowersView,
      meta: { requiresAuth: true }
    },
   /* {
      path: '/tours',
      name: 'tours',
      component: ToursView
    },
    {
      path: '/tour/:id',
      name: 'tour-detail',
      component: TourDetailView
    },
    {
      path: '/create-tour',
      name: 'create-tour',
      component: CreateTourView,
      meta: { requiresAuth: true, requiresGuide: true }
    },
    {
      path: '/my-tours',
      name: 'my-tours',
      component: MyToursView,
      meta: { requiresAuth: true, requiresGuide: true }
    },
    {
      path: '/position-simulator',
      name: 'position-simulator',
      component: PositionSimulatorView,
      meta: { requiresAuth: true }
    },
    {
      path: '/cart',
      name: 'cart',
      component: ShoppingCartView,
      meta: { requiresAuth: true }
    },
    {
      path: '/tour-execution',
      name: 'tour-execution',
      component: TourExecutionView,
      meta: { requiresAuth: true }
    }*/
  ]
})

// Navigation guards
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
  } else if (to.meta.requiresGuide && !authStore.isGuide()) {
    next('/')
  } else {
    next()
  }
})

export default router