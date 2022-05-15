<template>
	<div class="project-header">
		<h2>{{project?.name}}</h2>
		<button @click="createTaskModal.show()">Add task</button>
	</div>
	<div class="row gx-3 kanban-board">
		<div class="col-4" v-for="status in ['planned', 'ongoing', 'completed']">
			<h3 class="task-status-header">{{camelCaseToSentence(status)}}</h3>
			<div class="tasks">
				<div v-for="task in tasks[status]" :key="task.id"
					 class="task" draggable="true"
					 @dragstart="dragStart($event, {status, id: task.id})"
					 @dragend="dragEnd"
					 @click="openTaskModal(task)">
					 <div>
						<p class="task-name">{{task.name}}</p>
						<p class="task-date">{{task.creation_date}}</p>
					</div>
					<svg class="icon" @click.stop="openTaskEditModal(task)"><use href="#icon-edit" /></svg>
					<svg class="icon ms-1" @click.stop="deleteTask({status, id: task.id})"><use href="#icon-x" /></svg>
				</div>
				<div class="task-drop-area"
					 v-show="draggingFrom !== null && draggingFrom !== status"
					 @dragover.prevent="dragOver" @drop.prevent="drop($event, status)"/>
			</div>
		</div>
	</div>

	<Modal ref="createTaskModal" size="lg">
        <template v-slot:header>
            <h5 class="modal-title">Create task</h5>
        </template>

		<template v-slot:body>
			<div class="dform">
				<input type="text" placeholder="Name" v-model="createTaskForm.name">
				<textarea class="mt-2" placeholder="Text" v-model="createTaskForm.text" v-autosize ></textarea>

				<p class="error-message">{{createTaskForm.error}}</p>
			</div>
		</template>

        <template v-slot:footer>
            <div class="d-flex align-items-center justify-content-between">
                <button type="button" class="btn btn-outline-secondary" @click="createTaskModal.hide()">Close</button>
                <button type="button" class="btn btn-outline-primary ms-2" @click="createTask">Create</button>
            </div>
        </template>
    </Modal>

	<Modal ref="editTaskModal" size="lg">
        <template v-slot:header>
            <h5 class="modal-title">Edit task</h5>
        </template>

		<template v-slot:body>
			<div class="dform">
				<input type="text" placeholder="Name" v-model="editTaskForm.name">
				<textarea class="mt-2" placeholder="Text" v-model="editTaskForm.text" v-autosize ></textarea>

				<p class="error-message">{{editTaskForm.error}}</p>
			</div>
		</template>

        <template v-slot:footer>
            <div class="d-flex align-items-center justify-content-between">
                <button type="button" class="btn btn-outline-secondary" @click="editTaskModal.hide()">Close</button>
                <button type="button" class="btn btn-outline-primary ms-2" @click="editTask">Edit</button>
            </div>
        </template>
    </Modal>

	<Modal ref="taskModal" size="lg">
        <template v-slot:header>
            <h5 class="modal-title">{{selectedTask?.task?.name}}</h5>
        </template>

		<template v-slot:body>
			<div class="task-info">
				<p class="task-text">{{selectedTask?.task?.text}}</p>
			</div>

			<p class="task-comments-header">Comments ({{selectedTask?.comments?.length ?? 0}})</p>
			<div class="py-2 px-2 border-top px-0">
				<div>
					<div class="task-comment" v-for="comment in selectedTask?.comments" :key="comment.id">
						<p class="comment-info">
							<span class="comment-author-name">{{comment.author.name}}</span>
							<span class="comment-date">{{comment.creation_date}}</span>
						</p>
						<p class="comment-text">{{comment.text}}</p>
					</div>
				</div>
				<div class="write-comment">
					<textarea
						ref="commentTextInput"
						v-autosize
						v-model="commentText"
						placeholder="Text"
						class="form-control"
						rows="1"
						/>
					<svg class="icon send-comment-btn" @click="sendComment"><use href="#icon-send" /></svg>
				</div>
				<p class="error-message">{{sendCommentError}}</p>
			</div>
		</template>

        <template v-slot:footer></template>
    </Modal>

	<svg xmlns="http://www.w3.org/2000/svg" class="hidden">
		<defs>
			<symbol id="icon-edit" viewBox="0 0 16 16">
				<path d="M12.146.146a.5.5 0 0 1 .708 0l3 3a.5.5 0 0 1 0 .708l-10 10a.5.5 0 0 1-.168.11l-5 2a.5.5 0 0 1-.65-.65l2-5a.5.5 0 0 1 .11-.168l10-10zM11.207 2.5 13.5 4.793 14.793 3.5 12.5 1.207 11.207 2.5zm1.586 3L10.5 3.207 4 9.707V10h.5a.5.5 0 0 1 .5.5v.5h.5a.5.5 0 0 1 .5.5v.5h.293l6.5-6.5zm-9.761 5.175-.106.106-1.528 3.821 3.821-1.528.106-.106A.5.5 0 0 1 5 12.5V12h-.5a.5.5 0 0 1-.5-.5V11h-.5a.5.5 0 0 1-.468-.325z"/>
			</symbol>
			<symbol id="icon-send" viewBox="0 0 16 16">
				<path d="M15.854.146a.5.5 0 0 1 .11.54l-5.819 14.547a.75.75 0 0 1-1.329.124l-3.178-4.995L.643 7.184a.75.75 0 0 1 .124-1.33L15.314.037a.5.5 0 0 1 .54.11ZM6.636 10.07l2.761 4.338L14.13 2.576 6.636 10.07Zm6.787-8.201L1.591 6.602l4.339 2.76 7.494-7.493Z"/>
			</symbol>
			<symbol id="icon-x" viewBox="0 0 16 16">
				<path d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z"/>
			</symbol>
		</defs>
	</svg>
</template>

<script setup>
import { reactive, ref, watch } from "vue";
import { useStore } from "vuex";
import { binarySearch } from "@/utils/arrayUtils";
import { camelCaseToSentence } from "@/utils/stringUtils";
import { call_get, call_post, call_delete, call_patch} from "@/utils/api";
import Modal from '@/components/Modal.vue';
import { RequestError } from "@/exceptions";

const store = useStore();

const props = defineProps({
    id: { type: Number }
})

const project = ref()

const tasks = ref({
	"planned": [],
	"ongoing": [],
	"completed": []
});

const createTaskModal = ref(null);
const editTaskModal = ref(null);
const taskModal = ref(null);

const createTaskForm = reactive({
	name: '',
	text: '',
    error: ''
})
const editTaskForm = reactive({
	source: null,
	name: '',
	text: '',
    error: ''
})
const selectedTask = reactive({
	task: null,
	comments: null
});
const commentText = ref('');
const sendCommentError = ref('');

function mapAuthors({items, users}) {
	return items.map(item => {
		let {user_id, ...obj} = item;
		let user = users.find(u => u.id == user_id);
		return {
			author: user,
			...obj
		};
	});
}

function openTaskModal(task) {
	selectedTask.task = task;
	call_get(`/api/tasks/${task.id}/comments`)
	.then(res => selectedTask.comments = mapAuthors({items: res.comments, users: res.users}))
	.catch(err => console.log('Comments loading error: ', err.message));
	taskModal.value.show();
}

function openTaskEditModal(task) {
	editTaskForm.source = task;
	editTaskForm.name = task.name;
	editTaskForm.text = task.text;
	editTaskModal.value.show()
}

async function createTask() {
    try {
        let response = await call_post('/api/tasks', {
			name: createTaskForm.name,
			text: createTaskForm.text,
			project_id: project.value.id
		});
        tasks.value.planned.push(response);
        console.log('tasks: ', tasks.value);
        createTaskModal.value.hide();
		createTaskForm.name = createTaskForm.text = createTaskForm.error = '';
    } catch (err) {
        console.log('Task creating error: ', err.message)
        createTaskForm.error = err instanceof RequestError ? err.message : 'Request error';
    }
}

async function editTask() {
    try {
        await call_patch(`/api/tasks/${editTaskForm.source.id}`, {
			name: editTaskForm.name,
			text: editTaskForm.text
		});
        editTaskForm.source.name = editTaskForm.name;
        editTaskForm.source.text = editTaskForm.text;
        editTaskModal.value.hide();
		editTaskForm.error = '';
    } catch (err) {
        console.error('Task editing error: ', err.message)
        editTaskForm.error = err instanceof RequestError ? err.message : 'Request error';
    }
}

async function deleteTask({status, id}) {
    try {
        await call_delete(`/api/tasks/${id}`);

		let from = tasks.value[status];
		let taskInd = from.findIndex(t => t.id === id);
		from.splice(taskInd, 1);
    } catch (err) {
        console.error('Task deleting error: ', err.message)
        // ... = err instanceof RequestError ? err.message : 'Request error';
    }
}

async function sendComment() {
	 try {
        let response = await call_post('/api/comments', {
			text: commentText.value,
			task_id: selectedTask.task.id
		});
		let {user_id, ...obj} = response;
        selectedTask.comments.push({author: {id: store.state.auth.user.id, name: store.state.auth.user.name }, ...obj});
        console.log('comments: ', selectedTask.comments);
        commentText.value = sendCommentError.value = '';
    } catch (err) {
        console.log('Comment sending error: ', err.message)
        sendCommentError.value = err instanceof RequestError ? err.message : 'Request error';
    }
}

const draggingFrom = ref(null);

function dragStart(event, task) {
	event.dataTransfer.setData("text", JSON.stringify(task));
	event.dataTransfer.dropEffect = "move";
	draggingFrom.value = task.status;
}
function dragEnd(event) {
	draggingFrom.value = null;
}

function dragOver(event) {
	event.dataTransfer.dropEffect = "move";
}
async function drop(event, target) {
	const data = JSON.parse(event.dataTransfer.getData("text"));
	let from = tasks.value[data.status];
	let to = tasks.value[target];
	let taskInd = from.findIndex(t => t.id === data.id);
	let task = from[taskInd];
	try {
		await call_patch(`/api/tasks/${task.id}`, { status: target });
		to.splice(binarySearch(to, t => t.id > task.id), 0, task);
		from.splice(taskInd, 1);
	} catch (e) {
		console.log('An error occurred when changing the status of the task');
		// TODO: display it
	}
}

watch(() => props.id, async (id, oldId) => {
	//project.value = store.state.projects.find(p => p.id === id);
	try {
		let projectCall = call_get(`/api/projects/${id}`);
		let tasksCall = call_get(`/api/projects/${id}/tasks`);

		project.value = await projectCall;
		tasks.value = (await tasksCall).tasks;
	} catch(err) {
		console.log('Loading tasks error:', err);
		 alert('error')
	}
}, { immediate: true })

const vAutosize = {
	mounted: (el) => {
		el.setAttribute("style", "height:" + (el.scrollHeight) + "px;overflow-y:hidden;");
		function OnInput() {
			this.style.height = "auto";
			this.style.height = (this.scrollHeight) + "px";
		}
		el.addEventListener("input", OnInput, false);
	}
}
</script>

<style>
.project-header {
	display: inline-flex;
	margin-bottom: 18px;
	width: 100%;
}
.project-header > h2 {
	font-weight: 400;
	font-size: calc(1.325rem + .9vw);
	flex-grow: 1;
	border-left: 2px solid #aaa;
    padding-left: 12px;
}
.project-header > button {
	height: 34px;
	padding: 0 18px;
	align-self: center;
	border: 1px solid #555;
}

.kanban-board {}

.task-status-header {
	text-align: center;
	font-size: 1.5rem;
	font-weight: 400;
	border: 1px solid #aaa;
	border-bottom: none;
	padding: 4px 4px 6px;
	margin: 0;
}

.tasks {
	border: 1px solid #aaa;
	padding: 6px 8px 100px;
	position: relative;
}
.tasks .task-drop-area {
	width: 100%;
	height: 100%;
	position: absolute;
	top: 0;
	left: 0;
	background-color: rgba(0,0,0,0.1);
	border: 10px dashed rgba(0,0,0,0.1);
}

.task {
	border: 1px solid #888;
	border-top: 3px solid #888;
	background-color: #fff;
	padding: 4px;
	margin-bottom: 8px;
	display: flex;
}
.task:hover {
	background-color: #eee;
}

.task > div {
	flex-grow: 1;
}

.task .task-name {
	margin: 0;
	font-size: 1.15rem;

}
.task .task-date {
	color: #666;
	font-size: 0.9rem;
	margin: 0;
}

.task-text {
	white-space: pre-wrap;
	word-break: break-word;
}

.task-comments-heade {
	margin: 0 0 8px;
}

.task-comment:not(:last-child) {
   margin-bottom: 8px;
}

.task-comment .comment-info {
	margin: 0 0 4px;
}

.task-comment .comment-author-name {
	font-size: 1.15rem;
	margin: 0 8px 0 0;
}
.task-comment .comment-date {
	font-size: 0.9rem;
    color: #444;
}

.task-comment .comment-text {
	margin: 0;
	border-left: 1px solid #aaa;
	padding-left: 8px;
	white-space: pre-wrap;
	word-break: break-word;
}

.write-comment {
	display: flex;
	margin-top: 16px;
}
.write-comment textarea {
	flex-grow: 1;
}
.write-comment .send-comment-btn {
	fill: #555;
    width: 22px;
    height: 22px;
    margin: 6px 0 0 8px;
}

.icon {
	width: 16px;
	height: 16px;
	cursor: pointer;
}

</style>
