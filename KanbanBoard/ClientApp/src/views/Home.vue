<template>
<div class="sidebar">
    <div class="user">{{store.state.auth.user?.name}}</div>
    <div class="account-actions">
        <router-link to="/profile" class="profile">Profile</router-link>
        <button @click="logout" class="logout">Logout</button>
    </div>
    <hr />
    <button @click="openCreateProjectDialog" class="create-project-btn">Create project</button>
    <div class="projects">
        <router-link :to="'/project/'+project.id" v-for="project in projects">{{project.name}}</router-link>
    </div>
</div>
    <Modal ref="createProjectModal">
        <template v-slot:header>
            <h5 class="modal-title" id="exampleModalLabel">Create project</h5>
        </template>

            <template v-slot:body>
                <div class="dform">
                    <input type="text" placeholder="Name" v-model="createProjectForm.name">

                    <p class="error-message">{{createProjectForm.error}}</p>
                </div>
            </template>

        <template v-slot:footer>
            <div class="d-flex align-items-center justify-content-between">
                <button type="button" class="btn btn-outline-secondary" @click="createProjectModal.hide()">Close</button>
                <button type="button" class="btn btn-outline-primary ms-2" @click="createProject">Create</button>
            </div>
        </template>
    </Modal>
<main class="mt-3">
	<router-view />
</main>
</template>

<script setup>
import { reactive, ref, watch, computed, toRaw, unref } from 'vue'
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'
import { call_get, call_post } from '@/utils/api'
import Modal from '@/components/Modal.vue';
import { RequestError } from '@/exceptions';

const store = useStore();
const router = useRouter();

const projects = computed(() => store.state.projects)

const createProjectModal = ref(null);

const createProjectForm = reactive({
	name: '',
    error: ''
})

async function logout() {
    try { await store.dispatch('logout'); }
    finally { router.push('/login'); }
}

function openCreateProjectDialog() {
    createProjectModal.value.show();
}

async function test() {
    console.log(await call_get('/i'));
}

async function createProject() {
    try {
        let response = await call_post('/api/projects', { name: createProjectForm.name });
        store.commit('addProject', response);
        console.log('projects: ', store.state.projects);
        createProjectModal.value.hide();
    } catch (err) {
        console.log('error: ', err.message)
        createProjectForm.error = err instanceof RequestError ? err.message : 'Request error';
    }
}

store.dispatch('loadProjects');

</script>

<style>
main {
    margin-left: 250px;
    padding-right: var(--bs-gutter-x,.75rem);
    padding-left: var(--bs-gutter-x,.75rem);
}

.sidebar {
    width: 250px;
    height: 100%;
    position: fixed;
    top: 0;
    left: 0;
    background-color: #eee;
    padding-top: 1rem;
}

.sidebar .user {
    padding: 0px;
    font-size: 20px;
    text-align: center;
    display: block;
}

.sidebar .account-actions {
    margin: 8px 0px 0px;
    font-size: 18px;
}

.sidebar .account-actions * {
    width: calc(50% - 8px);
    font-size: 18px;
    text-align: center;
    display: inline-block;
    text-decoration: none;
    color: rgb(0, 53, 83);
    border: 1px solid #555555;
    margin: 0 4px;
}

.sidebar .projects > a {
    margin: 2px 8px;
    font-size: 18px;
    display: block;
    text-decoration: none;
    color: rgb(0, 28, 44);
    border-bottom: 1px solid rgb(88, 88, 88);
}
.sidebar .projects > a:not(a ~ a) {
    border-top: 1px solid rgb(88, 88, 88);
}
.sidebar .projects > a:hover {
    color: rgb(103, 130, 133);
}
.sidebar .create-project-btn {
    border-width: 1px;
    width: 90%;
    display: block;
    margin: 0 auto 14px;
}

/* ----- */


.dform input {
    border: 1px solid #d2d8d8;
    appearance: none;
    padding: 12px 20px 12px 20px;
    margin: 0;
    border-radius: 0.25em;
    width: 100%;
	box-sizing: border-box;
}

.dform input:focus {
    border-color: #343642;
    box-shadow: 0 0 5px rgb(52 54 66 / 10%);
    outline: none;
}

.dform input[type=submit] {
    padding: 16px 0;
    cursor: pointer;
    background: #2f889a;
    color: #FFF;
    font-weight: bold;
    border: none;
    appearance: none;
}

.dform .error-message {
	margin: 1rem 0 0 0;
	color: #c00;
	text-align: center;
	word-break: break-word;
}
</style>
