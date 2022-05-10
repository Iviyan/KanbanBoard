import { createRouter, createWebHistory } from 'vue-router'
import { useStore } from 'vuex'
import store from '../store'

import PassThrough from '../components/PassThrough.vue'
import LoginView from '../views/Login.vue'
import RegisterView from '../views/Register.vue'

//const store = useStore();

const HomeView = () => import(/* webpackChunkName: "home" */ '../views/Home.vue')
const ProjectView = () => import(/* webpackChunkName: "home" */ '../views/Project.vue')

const ifNotAuthenticated = (to, from) => { console.log('login page redirect')
	if (store.getters.isAuth) {
    console.log('login page access denial')
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
    component: RegisterView,
    props: { action: 'login' },
    beforeEnter: ifNotAuthenticated
  },
  {
    path: '/register',
    name: 'register',
    component: RegisterView,
    props: { action: 'register' },
    beforeEnter: ifNotAuthenticated,
  },
  {
    path: '/reset-password',
    name: 'reset-password',
    component: RegisterView,
    props: { action: 'reset-password' },
    beforeEnter: ifNotAuthenticated,
  },
  {
    path: '/',
    name: 'home',
    component: HomeView,
    beforeEnter: ifAuthenticated,
    children: [
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
