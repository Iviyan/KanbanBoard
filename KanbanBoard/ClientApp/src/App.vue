<template>
		<router-view />
</template>

<script setup>
import { getCurrentInstance, watch, onErrorCaptured } from 'vue';
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'
import { call_get } from './utils/api';

const app = getCurrentInstance();
const store = useStore();
const router = useRouter();

// onErrorCaptured((err,vm,info) => {
//     console.log(`cat EC: ${err.toString()}\ninfo: ${info}`);
//      return false;
//   });

const globalProperties = app.appContext.config.globalProperties;

let jwtChangeHandler = async (jwt, oldJwt) => {
	if (!jwt) {
		console.log('JWT clear');
		await router.push('/login');
	} else {
		if (!oldJwt) {
			console.log('JWT set');

		} else {
			console.log('JWT update');
		}
	}
};

watch(() => store.state.auth.jwt, jwtChangeHandler);
jwtChangeHandler(store.state.auth.jwt);
</script>
