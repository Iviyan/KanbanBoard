// import { shallowRef, ref, reactive, shallowReactive } from 'vue'
import { call_get } from '../../utils/api';

export default {
	state: {
		projects: [],
		
		tempData: [],
		humidityData: [],
		number: 0
	},
	getters: {
		lastTemp: state => state.tempData.at(-1)?.y ?? 0,
		lastHumidity: state => state.humidityData.at(-1)?.y ?? 0,
	},
	mutations: {
		setTempData(state, value) { state.tempData = value },
		setHumidityData(state, value) { state.humidityData = value },
		tempMeasurement(state, value) {
			//let element = { x: DateTime.now().toFormat("dd.MM.yyyy HH:mm:ss"), y: Number(value) };
			let element = { x: DateTime.now().toMillis(), y: Number(value) };
			if (state.tempData.length >= 20)
				state.tempData.shift();
			state.tempData.push(element);
		},
		humidityMeasurement(state, value) {
			let element = { x: DateTime.now().toMillis(), y: Number(value) };
			if (state.humidityData.length >= 20)
				state.humidityData.shift();
			state.humidityData.push(element);
		},
		numberChange(state, value) {
			state.number = value;
		},
	},
	actions: {
		async loadLastData({ commit, state }) {
			let res = await call_get('/api/last-data');
			commit('setTempData', res.temp_measurements.map(({ x, y}) => ({ x: DateTime.fromISO(x).toMillis(), y })));
			commit('setHumidityData', res.humidity_measurements.map(({ x, y}) => ({ x: DateTime.fromISO(x).toMillis(), y })));
			commit('numberChange', res.number);
			console.log(state)
		}
	}
}