
import axios from 'axios';

let BaseUrl = 'http://localhost:5002/';



// 设置请求拦截器
axios.interceptors.request.use(
	(config) => {
		const token = localStorage.getItem("token");
		if (token) {
			config.headers.Authorization = 'Bearer ' + token;
		}
		return config;
	},
	(error) => {
		return Promise.reject(error);
	}
);

export const Login = (params) => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'Auth',
		params: params,
	})
		.then((res) => res)
		.catch((error) => console.log(error));
};

//以下代码由T4模板生成
export const SelectCamera = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'Camera/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLCamera = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'Camera/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectCameraById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'Camera',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddCamera = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'Camera',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditCamera = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'Camera',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeleteCamera = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'Camera',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectCameraRecord = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'CameraRecord/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLCameraRecord = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'CameraRecord/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectCameraRecordById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'CameraRecord',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddCameraRecord = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'CameraRecord',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditCameraRecord = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'CameraRecord',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeleteCameraRecord = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'CameraRecord',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectDevice = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'Device/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLDevice = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'Device/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectDeviceById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'Device',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddDevice = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'Device',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditDevice = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'Device',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeleteDevice = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'Device',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectRight = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'Right/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLRight = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'Right/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectRightById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'Right',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddRight = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'Right',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditRight = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'Right',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeleteRight = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'Right',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectRole = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'Role/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLRole = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'Role/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectRoleById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'Role',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddRole = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'Role',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditRole = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'Role',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeleteRole = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'Role',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectRoleRight = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'RoleRight/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLRoleRight = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'RoleRight/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectRoleRightById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'RoleRight',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddRoleRight = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'RoleRight',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditRoleRight = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'RoleRight',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeleteRoleRight = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'RoleRight',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectUser = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'User/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLUser = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'User/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectUserById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'User',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddUser = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'User',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditUser = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'User',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeleteUser = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'User',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectWorkOrder = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'WorkOrder/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLWorkOrder = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'WorkOrder/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectWorkOrderById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'WorkOrder',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddWorkOrder = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'WorkOrder',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditWorkOrder = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'WorkOrder',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeleteWorkOrder = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'WorkOrder',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

// 人员管理相关API
export const SelectPersonnel = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'Personnel/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLPersonnel = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'Personnel/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectPersonnelById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'Personnel',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddPersonnel = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'Personnel',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditPersonnel = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'Personnel',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeletePersonnel = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'Personnel',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

// 手环管理相关API
export const SelectBracelet = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'Bracelet/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLBracelet = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'Bracelet/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectBraceletById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'Bracelet',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddBracelet = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'Bracelet',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditBracelet = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'Bracelet',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeleteBracelet = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'Bracelet',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

// 作业手环管理相关API
export const SelectWorkBracelet = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'WorkBracelet/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLWorkBracelet = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'WorkBracelet/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectWorkBraceletById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'WorkBracelet',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddWorkBracelet = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'WorkBracelet',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditWorkBracelet = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'WorkBracelet',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeleteWorkBracelet = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'WorkBracelet',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

// 作业记录管理相关API
export const SelectWorkRecord = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'WorkRecord/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLWorkRecord = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'WorkRecord/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectWorkRecordById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'WorkRecord',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddWorkRecord = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'WorkRecord',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditWorkRecord = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'WorkRecord',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeleteWorkRecord = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'WorkRecord',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

// 气体报警记录相关API
export const SelectGasAlarmRecord = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'GasAlarmRecord/Pages',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}
export const SelectALLGasAlarmRecord = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'GasAlarmRecord/All',
	})
		.then((res) => res.data)
		.catch((error) => console.log(error));
};
export const SelectGasAlarmRecordById = (params) => {
	console.log(params)
	return axios({
		method: 'Get',
		url: BaseUrl + 'GasAlarmRecord',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const AddGasAlarmRecord = (params) => {
	console.log(params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'GasAlarmRecord',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const EditGasAlarmRecord = (params) => {
	console.log(params)
	return axios({
		method: 'Put',
		url: BaseUrl + 'GasAlarmRecord',
		data: params
	}).then(res => res.data).catch(error => console.log(error));
}

export const DeleteGasAlarmRecord = (params) => {
	console.log(params)
	return axios({
		method: 'Delete',
		url: BaseUrl + 'GasAlarmRecord',
		params: params
	}).then(res => res.data).catch(error => console.log(error));
}

// 获取在线设备的最新气体监测实时数据
export const GetRealtimeGasData = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'GasAlarmRecord/GetRealtimeGasData',
	})
		.then((res) => res.data)
		.catch((error) => {
			console.error('获取气体监测数据失败:', error);
			return [];
		});
}

// 获取实时手环信息（在线设备的工单开始状态的作业手环）
export const GetRealtimeBraceletInfo = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'WorkBracelet/GetRealtimeBraceletInfo',
	})
		.then((res) => res.data)
		.catch((error) => {
			console.error('获取手环信息失败:', error);
			return [];
		});
}

// 获取在线设备的在线工单
export const GetRealtimeWorkOrders = () => {
	return axios({
		method: 'Get',
		url: BaseUrl + 'WorkOrder/GetRealtimeWorkOrders',
	})
		.then((res) => res.data)
		.catch((error) => {
			console.error('获取实时工单失败:', error);
			return [];
		});
}
//T4模板生成结束

// 云台控制相关API
export const PTZControl = (params) => {
	console.log('PTZ控制:', params)
	return axios({
		method: 'Post',
		url: BaseUrl + 'api/HK/ptz-control',
		data: params
	}).then(res => res.data).catch(error => {
		console.error('PTZ控制失败:', error);
		return { success: false, message: error.message || '控制失败' };
	});
}

//工人进出状态记录
export const SelectWorkerStatusRecord = params => { return axios.post(`${BaseUrl}WorkerStatusRecord/Pages`, params).then(res => res.data); };
export const AddWorkerStatusRecord = params => { return axios.post(`${BaseUrl}WorkerStatusRecord`, params).then(res => res.data); };
export const EditWorkerStatusRecord = params => { return axios.put(`${BaseUrl}WorkerStatusRecord`, params).then(res => res.data); };
export const DeleteWorkerStatusRecord = params => { return axios.delete(`${BaseUrl}WorkerStatusRecord?Id=` + params.Id).then(res => res.data); };

export const SelectBraceletAbnormal = params => { return axios.post(`${BaseUrl}BraceletAbnormal/Pages`, params).then(res => res.data); };
export const AddBraceletAbnormal = params => { return axios.post(`${BaseUrl}BraceletAbnormal`, params).then(res => res.data); };
export const EditBraceletAbnormal = params => { return axios.put(`${BaseUrl}BraceletAbnormal`, params).then(res => res.data); };
export const DeleteBraceletAbnormal = params => { return axios.delete(`${BaseUrl}BraceletAbnormal?Id=` + params.Id).then(res => res.data); };

export const SelectGasAbnormal = params => { return axios.post(`${BaseUrl}GasAbnormal/Pages`, params).then(res => res.data); };
export const AddGasAbnormal = params => { return axios.post(`${BaseUrl}GasAbnormal`, params).then(res => res.data); };
export const EditGasAbnormal = params => { return axios.put(`${BaseUrl}GasAbnormal`, params).then(res => res.data); };
export const DeleteGasAbnormal = params => { return axios.delete(`${BaseUrl}GasAbnormal?Id=` + params.Id).then(res => res.data); };

export const GetAbnormalConfig = params => { return axios.get(`${BaseUrl}AbnormalConfig/GetByConfigName?configName=` + params).then(res => res.data); };
export const SaveAbnormalConfig = params => { return axios.post(`${BaseUrl}AbnormalConfig`, params).then(res => res.data); };