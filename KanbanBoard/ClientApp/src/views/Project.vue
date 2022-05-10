<template>
	<div class="row gx-3 kanban-board">
		<div class="col-4" v-for="status in ['planned', 'ongoing', 'completed']">
			<h3 class="task-status-header">{{camelCaseToSentence(status)}}</h3>
			<div class="tasks">
				<div v-for="task in tasks[status]" :key="task.id"
					 class="task" draggable="true"
					 @dragstart="dragStart($event, {status, id: task.id})"
					 @dragend="dragEnd">
					<p class="task-name">{{task.name}}</p>
					<p class="task-date">{{task.creation_date}}</p>
				</div>
				<div class="task-drop-area"
					 v-show="draggingFrom !== null && draggingFrom !== status"
					 @dragover.prevent="dragOver" @drop.prevent="drop($event, status)"/>
			</div>
		</div>
	</div>
</template>

<script setup>
import { ref } from "vue";
import { binarySearch } from "@/utils/arrayUtils";
import { camelCaseToSentence } from "@/utils/stringUtils";

const props = defineProps({
    id: { type: Number }
})

const draggingFrom = ref(null);

const tasks = ref({
	"planned": [
		{ id: 1, name: 'Clean room', creation_date: '01.01.2022 12:13:54' },
		{ id: 2, name: 'Wash dishes', creation_date: '01.01.2022 12:13:54' },
	],
	"ongoing": [
		{ id: 3, name: 'Clean room ongoing', creation_date: '01.01.2022 12:13:54' },
		{ id: 4, name: 'Wash dishes ongoing', creation_date: '01.01.2022 12:13:54' },
	],
	"completed": [
		{ id: 5, name: 'Clean room completed', creation_date: '01.01.2022 12:13:54' },
		{ id: 6, name: 'Wash dishes completed', creation_date: '01.01.2022 12:13:54' },
	]
});

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
function drop(event, target) {
	const data = JSON.parse(event.dataTransfer.getData("text"));
	let from = tasks.value[data.status];
	let to = tasks.value[target];
	let taskInd = from.findIndex(t => t.id === data.id);
	let task = from[taskInd];
	to.splice(binarySearch(to, t => t.id > task.id), 0, task);
	from.splice(taskInd, 1);
}
</script>

<style>
.kanban-board {

}

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
}

.task > .task-name {
	margin: 0;
	font-size: 1.15rem;

}
.task > .task-date {
	color: #666;
	font-size: 0.9rem;
	margin: 0;
}
</style>
