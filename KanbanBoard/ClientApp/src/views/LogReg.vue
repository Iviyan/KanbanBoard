<template>
		<teleport to="body">
				<div class="cd-user-modal is-visible"> <!-- this is the entire modal form, including the background -->
						<div class="cd-user-modal-container"> <!-- this is the container wrapper -->
								<ul class="cd-switcher">
										<li><router-link to="/login" :class="{ 'selected': action === 'login' }">Sign in</router-link></li>
										<li><router-link to="/register" :class="{ 'selected': action === 'register' }">New account</router-link></li>
								</ul>

								<div id="cd-login" :class="{ 'is-selected': action === 'login' }"> <!-- log in form -->
										<form class="cd-form" @submit.prevent="tryLogin">
												<p class="fieldset">
														<input v-model="loginForm.login" class="full-width has-padding has-border" id="signin-email" type="email" placeholder="E-mail">
														<span class="cd-error-message">Error message here!</span>
												</p>

												<p class="fieldset">
														<input v-model="loginForm.password" class="full-width has-padding has-border" id="signin-password" :type="showPassword.signin ? 'text' : 'password'"  placeholder="Password">
														<a class="hide-password" @click="showPassword.signin = !showPassword.signin">{{showPassword.signin ? 'Hide' : 'Show'}}</a>
														<span class="cd-error-message">Error message here!</span>
												</p>

												<p class="error-message">{{loginForm.error}}</p>

												<p class="fieldset">
														<input class="full-width" type="submit" value="Login">
												</p>
										</form>

										<!-- <p class="cd-form-bottom-message"><router-link to="/reset-password">Forgot your password?</router-link></p> -->
										<!-- <a href="#0" class="cd-close-form">Close</a> -->
								</div> <!-- cd-login -->

								<div id="cd-signup" :class="{ 'is-selected': action === 'register' }"> <!-- sign up form -->
										<form class="cd-form" @submit.prevent="tryRegister">
												<p class="fieldset">
														<input v-model="regForm.name" class="full-width has-padding has-border" id="signup-username" type="text" placeholder="Username">
														<span class="cd-error-message">Error message here!</span>
												</p>

												<p class="fieldset">
														<input v-model="regForm.login" class="full-width has-padding has-border" id="signup-email" type="email" placeholder="E-mail">
														<span class="cd-error-message">Error message here!</span>
												</p>

												<p class="fieldset">
														<input v-model="regForm.password" class="full-width has-padding has-border" id="signup-password" :type="showPassword.signup ? 'text' : 'password'"  placeholder="Password">
														<a class="hide-password" @click="showPassword.signup = !showPassword.signup">{{showPassword.signup ? 'Hide' : 'Show'}}</a>
														<span class="cd-error-message">Error message here!</span>
												</p>

												<p class="error-message">{{regForm.error}}</p>

												<p class="fieldset">
														<input class="full-width has-padding" type="submit" value="Create account">
												</p>
										</form>

										<!-- <a href="#0" class="cd-close-form">Close</a> -->
								</div> <!-- cd-signup -->

								<div id="cd-reset-password" :class="{ 'is-selected': action === 'reset-password' }"> <!-- reset password form -->
										<p class="cd-form-message">Lost your password? Please enter your email address. You will receive a link to create a new password.</p>

										<form class="cd-form">
												<p class="fieldset">
														<input class="full-width has-padding has-border" id="reset-email" type="email" placeholder="E-mail">
														<span class="cd-error-message">Error message here!</span>
												</p>

												<p class="fieldset">
														<input class="full-width has-padding" type="submit" value="Reset password">
												</p>
										</form>

										<p class="cd-form-bottom-message"><router-link to="/login">Back to log-in</router-link></p>
								</div> <!-- cd-reset-password -->
								<a href="#0" class="cd-close-form">Close</a>
						</div> <!-- cd-user-modal-container -->
				</div> <!-- cd-user-modal -->
		</teleport>
</template>


<script>
// use normal <script> to declare options
export default {
	inheritAttrs: false
}
</script>

<script setup>
import { reactive, ref, watch } from 'vue'
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'
import { postj } from '@/utils/fetch'
import { RequestError } from '@/exceptions';

const store = useStore();
const router = useRouter();

const props = defineProps({
	action:  {
		type: String,
		default: 'login'
	}
})

const loginForm = reactive({
	login: '',
	password: '',
	error: ''
})

const regForm = reactive({
	login: '',
	password: '',
	name: '',
	error: ''
})

const showPassword = reactive({
	signin: false,
	signup: false,
});

function tryLogin() {
	//console.log(loginForm, ' | ', loginForm.login);return;
	store.dispatch('login', { login: loginForm.login, password: loginForm.password })
	.then(() => {
		loginForm.error = '';
		router.push("/");
	})
	.catch(err => {
		console.log('login err')
		if (err instanceof RequestError)
			loginForm.error = err.message;
		else
			loginForm.error = 'Request error';
	});
}

async function tryRegister() {
	let res;
	try {
		res = await postj('/register', {
			email: regForm.login,
			password: regForm.password,
			name: regForm.name
		});
	} catch (e) {
		if (err instanceof RequestError)
			regForm.error = err.message;
		else
			regForm.error = 'Request error';
	}
	console.log('reg response: ', res);

	let jwt = res.access_token;
	console.log('jwt: ', jwt);
	if (jwt) {
		store.commit('auth', jwt);
		regForm.error = '';
		router.push("/");
	}
	else regForm.error = "Ошибка получения токена";
}
</script>

<style>
/* --------------------------------

Primary style

-------------------------------- */

.cd-user-modal {
	font-size: 100%;
	font-family: "PT Sans", sans-serif;
	color: #505260;
	background-color: #fff;
}

.cd-user-modal a {
	color: #2f889a;
	text-decoration: none;
}

.cd-user-modal input, textarea {
	font-family: "PT Sans", sans-serif;
	font-size: 16px;
	font-size: 1rem;
}
input::-ms-clear, textarea::-ms-clear {
	display: none;
}

/* --------------------------------

xsigin/signup popup

-------------------------------- */
.cd-user-modal {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background: rgba(52, 54, 66, 0.9);
	z-index: 3;
	overflow-y: auto;
	cursor: pointer;
	visibility: hidden;
	opacity: 0;
	-webkit-transition: opacity 0.3s 0, visibility 0 0.3s;
	-moz-transition: opacity 0.3s 0, visibility 0 0.3s;
	transition: opacity 0.3s 0, visibility 0 0.3s;
}
.cd-user-modal.is-visible {
	visibility: visible;
	opacity: 1;
	-webkit-transition: opacity 0.3s 0, visibility 0 0;
	-moz-transition: opacity 0.3s 0, visibility 0 0;
	transition: opacity 0.3s 0, visibility 0 0;
}
.cd-user-modal.is-visible .cd-user-modal-container {
	-webkit-transform: translateY(0);
	-moz-transform: translateY(0);
	-ms-transform: translateY(0);
	-o-transform: translateY(0);
	transform: translateY(0);
}

.cd-user-modal-container {
	position: relative;
	width: 90%;
	max-width: 600px;
	background: #FFF;
	margin: 3em auto 4em;
	cursor: auto;
	border-radius: 0.25em;
	-webkit-transform: translateY(-30px);
	-moz-transform: translateY(-30px);
	-ms-transform: translateY(-30px);
	-o-transform: translateY(-30px);
	transform: translateY(-30px);
	-webkit-transition-property: -webkit-transform;
	-moz-transition-property: -moz-transform;
	transition-property: transform;
	-webkit-transition-duration: 0.3s;
	-moz-transition-duration: 0.3s;
	transition-duration: 0.3s;
}
.cd-user-modal-container .cd-switcher {
	list-style: none;
	padding: 0;
}
.cd-user-modal-container .cd-switcher::after {
	clear: both;
	content: "";
	display: table;
}
.cd-user-modal-container .cd-switcher li {
	width: 50%;
	float: left;
	text-align: center;
}
.cd-user-modal-container .cd-switcher li:first-child a {
	border-radius: 0.25em 0 0 0;
}
.cd-user-modal-container .cd-switcher li:last-child a {
	border-radius: 0 0.25em 0 0;
}
.cd-user-modal-container .cd-switcher a {
	display: block;
	width: 100%;
	height: 50px;
	line-height: 50px;
	background: #d2d8d8;
	color: #809191;
}
.cd-user-modal-container .cd-switcher a.selected {
	background: #FFF;
	color: #505260;
}
@media only screen and (min-width: 600px) {
	.cd-user-modal-container {
		margin: 4em auto;
	}
	.cd-user-modal-container .cd-switcher a {
		height: 70px;
		line-height: 70px;
	}
}

.cd-form {
	padding: 1.4em;
}
.cd-form input {
	box-sizing: border-box;
}
.cd-form .fieldset {
	position: relative;
	margin: 1.4em 0;
}
.cd-form .fieldset:first-child {
	margin-top: 0;
}
.cd-form .fieldset:last-child {
	margin-bottom: 0;
}
.cd-form label {
	font-size: 14px;
	font-size: 0.875rem;
}

.cd-form input {
	margin: 0;
	padding: 0;
	border-radius: 0.25em;
}
.cd-form input.full-width {
	width: 100%;
}
.cd-form input.has-padding {
	padding: 12px 20px 12px 20px;
}
.cd-form input.has-border {
	border: 1px solid #d2d8d8;
	-webkit-appearance: none;
	-moz-appearance: none;
	-ms-appearance: none;
	-o-appearance: none;
	appearance: none;
}
.cd-form input.has-border:focus {
	border-color: #343642;
	box-shadow: 0 0 5px rgba(52, 54, 66, 0.1);
	outline: none;
}
.cd-form input.has-error {
	border: 1px solid #d76666;
}
.cd-form input[type=password] {
	/* space left for the HIDE button */
	padding-right: 65px;
}
.cd-form input[type=submit] {
	padding: 16px 0;
	cursor: pointer;
	background: #2f889a;
	color: #FFF;
	font-weight: bold;
	border: none;
	-webkit-appearance: none;
	-moz-appearance: none;
	-ms-appearance: none;
	-o-appearance: none;
	appearance: none;
}
.no-touch .cd-form input[type=submit]:hover, .no-touch .cd-form input[type=submit]:focus {
	background: #3599ae;
	outline: none;
}
.cd-form .hide-password {
	display: inline-block;
	position: absolute;
	right: 0;
	top: 0;
	padding: 6px 15px;
	border-left: 1px solid #d2d8d8;
	top: 50%;
	bottom: auto;
	cursor: pointer;
	-webkit-transform: translateY(-50%);
	-moz-transform: translateY(-50%);
	-ms-transform: translateY(-50%);
	-o-transform: translateY(-50%);
	transform: translateY(-50%);
	font-size: 14px;
	font-size: 0.875rem;
	color: #343642;
}
.cd-form .error-message {
	margin: -1rem 0;
	color: #c00;
	text-align: center;
	word-break: break-word;
}
.cd-form .cd-error-message {
	display: inline-block;
	position: absolute;
	left: -5px;
	bottom: -35px;
	background: rgba(215, 102, 102, 0.9);
	padding: 0.8em;
	z-index: 2;
	color: #FFF;
	font-size: 13px;
	font-size: 0.8125rem;
	border-radius: 0.25em;
	/* prevent click and touch events */
	pointer-events: none;
	visibility: hidden;
	opacity: 0;
	-webkit-transition: opacity 0.2s 0, visibility 0 0.2s;
	-moz-transition: opacity 0.2s 0, visibility 0 0.2s;
	transition: opacity 0.2s 0, visibility 0 0.2s;
}
.cd-form .cd-error-message::after {
	/* triangle */
	content: "";
	position: absolute;
	left: 22px;
	bottom: 100%;
	height: 0;
	width: 0;
	border-bottom: 8px solid rgba(215, 102, 102, 0.9);
	border-left: 8px solid transparent;
	border-right: 8px solid transparent;
}
.cd-form .cd-error-message.is-visible {
	opacity: 1;
	visibility: visible;
	-webkit-transition: opacity 0.2s 0, visibility 0 0;
	-moz-transition: opacity 0.2s 0, visibility 0 0;
	transition: opacity 0.2s 0, visibility 0 0;
}
@media only screen and (min-width: 600px) {
	.cd-form {
		padding: 2em;
	}
	.cd-form .fieldset {
		margin: 2em 0;
	}
	.cd-form .fieldset:first-child {
		margin-top: 0;
	}
	.cd-form .fieldset:last-child {
		margin-bottom: 0;
	}
	.cd-form input.has-padding {
		padding: 16px 20px 16px 20px;
	}
	.cd-form input[type=submit] {
		padding: 16px 0;
	}
}

.cd-form-message {
	padding: 1.4em 1.4em 0;
	font-size: 1rem;
	line-height: 1.4;
	text-align: center;
}
@media only screen and (min-width: 600px) {
	.cd-form-message {
		padding: 2em 2em 0;
	}
}

.cd-form-bottom-message {
		margin-top: -1rem;
		padding-bottom: 0.5rem;
		text-align: center;
}

.cd-close-form {
	/* form X button on top right */
	display: block;
	position: absolute;
	width: 40px;
	height: 40px;
	right: 0;
	top: -40px;
	background: url("https://s3-us-west-2.amazonaws.com/s.cdpn.io/148866/cd-icon-close.svg") no-repeat center center;
	text-indent: 100%;
	white-space: nowrap;
	overflow: hidden;
}
@media only screen and (min-width: 1170px) {
	.cd-close-form {
		display: none;
	}
}

#cd-login, #cd-signup, #cd-reset-password {
	display: none;
}

#cd-login.is-selected, #cd-signup.is-selected, #cd-reset-password.is-selected {
	display: block;
}
</style>
