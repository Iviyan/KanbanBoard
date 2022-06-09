import { createStore } from 'vuex'
import auth from './modules/auth'
import createMultiTabState from 'vuex-multi-tab-state';
import { call_get } from '@/utils/api';

const store = createStore({
	strict: true,
	state: {
		projects: []
	},
	mutations: {
		setProjects(state, value) { state.projects = value },
		addProject(state, value) { state.projects.push(value) },
		deleteProject(state, value) { state.projects.splice(state.projects.findIndex(t => t.id === value), 1) },
		updateProject(state, {id, value}) {
			let ind = state.projects.findIndex(t => t.id === id);
			console.log(`projects (${id}) -> ${ind}`, state.projects, '\n>', { ...state.projects[ind], value });
			state.projects[ind] = { ...state.projects[ind], ...value };
		},
	},
	modules: {
		auth
	},
	plugins: [createMultiTabState({
		statesPaths: ['auth.jwt'],
	})],
	actions: {
		async loadProjects({ commit, state }) {
			let res = await call_get('/api/projects');
			commit('setProjects', res);
		}
	}
});

store.dispatch('initAuth');
console.log('store init');

export default store;
