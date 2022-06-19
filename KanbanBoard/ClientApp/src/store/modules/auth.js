import { postj } from '../../utils/fetch'
import { call_post } from '@/utils/api'
import jwt_decode from "jwt-decode"
import { RequestError } from '../../exceptions'

function localStorageSet(key, value) {
	if (!!value)
		localStorage.setItem(key, typeof value === 'object' ? JSON.stringify(value) : value);
	else
		localStorage.removeItem(key);
}

export default {
	state: {
		jwt: null,
		jwtData: null,
		user: {}
	},
	getters: {
		isAuth: state => !!state.jwt,
		jwtData: state => state.jwtData,
	},
	mutations: {
		auth(state, value) {
			console.log('auth, ', value)
			state.jwt = value;
			if (!!value) {
				state.jwtData = jwt_decode(value);
				localStorage.setItem('jwt', value);
			} else {
				localStorage.removeItem('jwt');
				localStorage.removeItem('user')
			}
		},
		logout(state, value) {
			state.jwt = state.jwtData = null;
			localStorage.removeItem('jwt');
		},
		setUser(state, value) {
			 state.user = value;
			 localStorageSet('user', value);
		},
		patchUser(state, value) {
			 state.user = {...state.user, ...value};
			 localStorageSet('user', value);
		}
	},
	actions: {
		initAuth(ctx) {
			let jwt = localStorage.getItem('jwt');
			if (jwt) {
				ctx.commit('auth', jwt);
				ctx.commit('setUser', JSON.parse(localStorage.getItem('user')));
			}
		},

		async login({ commit }, { login, password }) {
			let response;
			try {
				response = await postj('/login', {
					email: login,
					password: password
				});
			} catch (e) {
				console.error('Auth request error: ', e);
				throw e;
			}
			//console.log('login response: ', response);
			let jwt = response.access_token;
			if (jwt) commit('auth', jwt);
			else throw new Error('Token receiving error');
			commit('setUser', response.user)
		},
		async logout({ commit }) {
			try { await call_post('/logout'); }
			finally { commit('logout'); }
		}
	}
}
