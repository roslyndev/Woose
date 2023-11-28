import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import { HomeView } from '../views';

const routes: Array<RouteRecordRaw> = [
  { path: '/',name: 'home',component: HomeView },
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
