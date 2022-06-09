import { RequestError } from '@/exceptions';
import {Mutex} from 'async-mutex';
import store from '../store'

async function refreshToken() {
	const release = await refreshTokenMutex.acquire();
	try {
		const response = await fetch('/refresh-token', {
			method: 'POST'
		});
		const json = await response.json();

		console.log('Refresh token update: ', response, 'json: ', json)
		if (response.ok) {
			store.commit('auth', json.access_token);
			return true;
		}
		console.error('Refresh token update error.\n', json);
		store.dispatch('logout');
		return false;
	} finally { release(); }
};

// const isString = s => typeof s === 'string' || s instanceof String;

function isEmpty(obj) {
	for (var i in obj) return false;
	return true;
}

async function get(url = '', data = {}) {
	console.log('get | ', url, ' | ', data)
	return await fetch(url + (isEmpty(data) ? '' : '?' + new URLSearchParams(data)), {
		method: 'GET',
		headers: {
			'Authorization': `Bearer ${store.state.auth.jwt}`,
		}
	});
}

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

async function delete_(url = '', data = {}) {
	return await fetch(url, {
		method: 'DELETE',
		headers: {
			'Content-Type': 'application/json',
			'Authorization': `Bearer ${store.state.auth.jwt}`,
		},
		body: JSON.stringify(data)
	});
}

async function patch(url = '', data = {}) {
	return await fetch(url, {
		method: 'PATCH',
		headers: {
			'Content-Type': 'application/json',
			'Authorization': `Bearer ${store.state.auth.jwt}`,
		},
		body: JSON.stringify(data)
	});
}

let methods = {
	'get': get,
	'post': post,
	'delete': delete_,
	'patch': patch,
}

const refreshTokenMutex = new Mutex();

export async function call(url = '', method = 'GET', data = {}) {
	if (store.getters.jwtData.exp < Date.now() / 1000 && !refreshTokenMutex.isLocked()) {
		let res = await refreshToken();
		if (!res) throw new Error('Refresh token update error');
	}
	await refreshTokenMutex.waitForUnlock();

	let func = methods[method.toLowerCase()];
	if (!func) throw new Error('Refresh token update error');

	let response = await func(url, data);

	if (response.status === 401) {
		if (!refreshTokenMutex.isLocked()) {
			let res = await refreshToken();
			if (!res) throw new Error('Refresh token update error');
		} else await refreshTokenMutex.waitForUnlock();
		response = await func(url, data);
	}
	if (!response.ok) {
		console.error('Request execution error\n', response);
		if (response.headers.get('content-type')?.includes('application/problem+json') || false) {
			let json = await response.json();
			console.info('Request response: ', json)
			throw new RequestError(json);
		}
		throw new Error('Request execution error');
	}

	let text = await response.text();
	return text.length > 0 ? JSON.parse(text) : text;
}

export async function call_get(url = '', data = {}) { return await call(url, 'get', data); }
export async function call_post(url = '', data = {}) { return await call(url, 'post', data); }
export async function call_delete(url = '', data = {}) { return await call(url, 'delete', data); }
export async function call_patch(url = '', data = {}) { return await call(url, 'patch', data); }
