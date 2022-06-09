<template>
	<form class="settings-form" @submit.prevent="editProfile">
		<h4>Edit profile </h4>
		<input type="text" placeholder="Name" v-model="editProfileForm.name">
		<input type="submit" value="Edit" >

		<p class="error-message">{{editProfileForm.error}}</p>
	</form>

<form class="settings-form" @submit.prevent="changePassword">
		<h4>Change password</h4>
		<input type="password" placeholder="Old password" v-model="changePasswordForm.oldPassword">
		<input type="password" placeholder="New password" v-model="changePasswordForm.newPassword">
		<input type="password" placeholder="Repeat new password" v-model="changePasswordForm.newPassword2">
		<input type="submit" value="Change" >

		<p class="error-message">{{changePasswordForm.error}}</p>
	</form>
</template>

<script setup>
import { reactive, ref, watch } from "vue";
import { useStore } from "vuex";
import { call_get, call_post, call_delete, call_patch} from "@/utils/api";
import { RequestError } from "@/exceptions";

const store = useStore();

const editProfileForm = reactive({
	name: store.state.auth.user?.name ?? '',
    error: ''
})

const changePasswordForm = reactive({
	oldPassword: '',
	newPassword: '',
	newPassword2: '',
    error: ''
})

async function editProfile() {
    try {
        await call_patch(`/profile`, {
			name: editProfileForm.name
		});
		store.commit('patchUser', {name: editProfileForm.name});
		editProfileForm.error = '';
    } catch (err) {
        console.error('Profile editing error: ', err.message)
        editProfileForm.error = err instanceof RequestError ? err.message : 'Request error';
    }
}

async function changePassword() {
	if (changePasswordForm.newPassword !== changePasswordForm.newPassword2) {
		changePasswordForm.error = 'The values of the fields of the new password are not the same';
		return;
	}

    try {
        await call_post(`/change-password`, {
			old_password: changePasswordForm.oldPassword,
			new_password: changePasswordForm.newPassword
		});
		changePasswordForm.error = '';
    } catch (err) {
        console.error('Password changing error: ', err.message)
        changePasswordForm.error = err instanceof RequestError ? err.message : 'Request error';
    }
}
</script>

<style>

.settings-form {

}

.settings-form:not(:last-child) {
   margin-bottom: 24px;
}

.settings-form h4 {
	font-weight: 400;
	text-align: center;
}

.settings-form input[type=text], .settings-form input[type=password] {
  width: 100%;
  padding: 8px 10px;
  margin: 8px 0;
  box-sizing: border-box;
}
.settings-form input[type=submit] {
  width: 100%
}

</style>
