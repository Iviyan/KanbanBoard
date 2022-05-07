import { RequestError } from '@/exceptions';
import store from '../store'

async function refreshToken() {
	const response = await fetch('/refresh-token', {
		method: 'POST'
	});
	const json = await response.json();

	console.log('Refresh token update: ', response, 'json: ', json)
	if (response.ok) {
		store.commit('auth', json.access_token);
		return true;
	}
	console.log('Refresh token update error.\n' + json);
	store.dispatch('logout');
	return false;
};

// const isString = s => typeof s === 'string' || s instanceof String;

async function post(url = '', data = {}) {
	return await fetch(url, {
		method: 'POST',
		headers: {
			'Content-Type': 'application/json',
			'Authorization': `Bearer ${store.state.auth.jwt}`,
		},
		body: JSON.stringify(data)
	});
}

async function get(url = '', data = {}) {
	console.log('get | ', url, ' | ', data)
	return await fetch(url + '?' + new URLSearchParams(data), {
		method: 'GET',
		headers: {
			'Authorization': `Bearer ${store.state.auth.jwt}`,
		}
	});
}

let methods = {
	'get': get,
	'post': post
}

export async function call(url = '', method = 'GET', data = {}) {
	if (store.getters.jwtData.exp < Date.now() / 1000) {
		let res = await refreshToken();
		if (!res) throw new Error('Refresh token update error');
	}

	let func = methods[method.toLowerCase()];
	if (!func) throw new Error('Refresh token update error');

	let response = await func(url, data);

	if (response.status === 401) {
		let res = await refreshToken();
		if (!res) throw new Error('Refresh token update error');
		response = await func(url, data);
	}
	if (!response.ok) {
		console.log('Request execution error\n', response);
		if (response.headers.get('content-type')?.includes('application/problem+json') || false)
			throw new RequestError(await response.json());
		throw new Error('Request execution error');
	} 

	let text = await response.text();
	return text.length > 0 ? JSON.parse(text) : text;
}

export async function call_get(url = '', data = {}) { return await call(url, 'get', data); }
export async function call_post(url = '', data = {}) { return await call(url, 'post', data); }
