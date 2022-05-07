<template>
    <teleport to="body">
        <div class="fs-modal">
            <form class="form-signin" @submit.prevent="tryLogin">
                <h1 class="h3 mb-3 fw-normal text-center">Вход</h1>

                <div class="form-floating">
                    <input required v-model="login" type="text" class="form-control" id="floatingInput" placeholder="name@example.com">
                    <label for="floatingInput">Логин</label>
                </div>
                <div class="form-floating">
                    <input required v-model="password" type="password" class="form-control" id="floatingPassword" placeholder="Password">
                    <label for="floatingPassword">Пароль</label>
                </div>

                <button class="w-100 btn btn-lg btn-primary" type="submit">Войти</button>
                <p class="mt-3 mb-3 text-danger text-center">{{error}}</p>
                <router-link to="/register" class="d-block mt-3 mb-3 link-secondary text-decoration-none text-center">Нет аккаунта?</router-link>
            </form>
        </div>
    </teleport>
</template>

<script setup>
import { ref } from 'vue'
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'

const store = useStore();
const router = useRouter();

const error = ref('');
const login = ref('');
const password = ref('');

function tryLogin() { 
    store.dispatch('login', { login: login.value, password: password.value })
    .then(() => {
        error.value = '';
        router.push("/");
    })
    .catch(err => {
        console.log(err);
        if (err.name === 'LoginException')
            error.value = err.message;
        else
            error.value = 'Ошибка';
    });
}
</script>

<style>
.fs-modal {
    display: flex;
    align-items: center;
    position: fixed;
    z-index: 1;
    left: 0;
    top: 0;
    width: 100%;
    height: 100%;
    overflow: auto;
    padding-top: 40px;
    padding-bottom: 40px;
    background-color: #f5f5f5;
}

.form-signin {
    width: 100%;
    max-width: 330px;
    padding: 15px;
    margin: auto;
}

.form-signin .checkbox {
    font-weight: 400;
}

.form-floating:focus-within {
    z-index: 2;
}

.form-signin input[type="email"] {
    margin-bottom: -1px;
    border-bottom-right-radius: 0;
    border-bottom-left-radius: 0;
}

.form-signin input[type="password"] {
    margin-bottom: 10px;
    border-top-left-radius: 0;
    border-top-right-radius: 0;
}
/* .form-signin {
    width: 100%;
    max-width: 330px;
    padding: 15px;
    margin: auto;
}

.form-signin .checkbox {
    font-weight: 400;
}

.form-signin .form-floating:focus-within {
    z-index: 2;
}

.form-signin input[type="email"] {
    margin-bottom: -1px;
    border-bottom-right-radius: 0;
    border-bottom-left-radius: 0;
}

.form-signin input[type="password"] {
    margin-bottom: 10px;
    border-top-left-radius: 0;
    border-top-right-radius: 0;
} */
</style>
