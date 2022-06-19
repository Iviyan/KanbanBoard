import { createRouter, createWebHistory } from 'vue-router'
import { useStore } from 'vuex'
import store from '../store'

import PassThrough from '../components/PassThrough.vue'
import LogRegView from '../views/LogReg.vue'

//const store = useStore();

const HomeView = () => import(/* webpackChunkName: "home" */ '../views/Home.vue')
const ProfileView = () => import(/* webpackChunkName: "home" */ '../views/Profile.vue')
const ProjectView = () => import(/* webpackChunkName: "home" */ '../views/Project.vue')

const ifNotAuthenticated = (to, from) => {
	if (store.getters.isAuth) {
    	console.log(`"${to}" page access denial`)
		return '/';
  }
}

const ifAuthenticated = (to, from) => {
	if (!store.getters.isAuth && to.name !== 'Login') {
		return {
			name: 'login'
		}
  }
}

const routes = [
  {
    path: '/login',
    name: 'login',
    component: LogRegView,
    props: { action: 'login' },
    beforeEnter: ifNotAuthenticated
  },
  {
    path: '/register',
    name: 'register',
    component: LogRegView,
    props: { action: 'register' },
    beforeEnter: ifNotAuthenticated,
  },
  {
    path: '/reset-password',
    name: 'reset-password',
    component: LogRegView,
    props: { action: 'reset-password' },
    beforeEnter: ifNotAuthenticated,
  },
  {
    path: '/',
    name: 'home',
    component: HomeView,
    beforeEnter: ifAuthenticated,
    children: [
      { path: 'profile', component: ProfileView, beforeEnter: () => console.log('profile') },
      { path: 'project/:id(\\d+)', component: ProjectView, props: route => ({ id: Number(route.params.id) }) },
    ]
  },
  { path: '/:catchAll(.*)*', redirect: '/' }
]

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
})

export default router
