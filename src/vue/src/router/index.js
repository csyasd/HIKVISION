import Vue from 'vue'
import VueRouter from 'vue-router'
import Home from '../views/Home.vue'
import Login from '../views/Login.vue'
import Main from '../views/Main.vue'

import RightManagement from '../views/User/RightManagement.vue'
import RoleManagement from '../views/User/RoleManagement.vue'
import UserManagement from '../views/User/UserManagement.vue'

import CameraManagement from '../views/CameraManagement.vue'
import CameraRecordManagement from '../views/CameraRecordManagement.vue'
import DeviceManagement from '../views/DeviceManagement.vue'
import WorkOrderManagement from '../views/WorkOrderManagement.vue'

import CameraDetail from '../views/CameraDetail.vue'
import WorkBraceletManagement from '../views/WorkBraceletManagement.vue'
import WorkRecordManagement from '../views/WorkRecordManagement.vue'
import GasAlarmRecordManagement from '../views/GasAlarmRecordManagement.vue'
Vue.use(VueRouter)

const routes = [
    {
        path: '/home',
        name: 'Home',
        component: Home,
        children: [
            {
                // 首页
                path: 'main',
                component: Main,
                name: '首页',
            },
            {
                // 实时数据
                path: 'CameraDetail',
                component: () => import('../views/RealtimeData.vue'),
                name: '实时数据',
            },
            {
                // 摄像头播放器（iframe页面）
                path: 'CameraPlayer',
                component: () => import('../views/CameraPlayer.vue'),
                name: '摄像头播放器',
            },
            {
                // 设备管理
                path: 'DeviceManagement',
                component: DeviceManagement,
                name: '设备管理'
            },
            {
                // 摄像头管理
                path: 'CameraManagement',
                component: CameraManagement,
                name: '摄像头管理'
            },
            {
                // 作业记录
                path: 'WorkOrderManagement',
                component: WorkOrderManagement,
                name: '施工工单'
            },
            {
                // 摄像头记录
                path: 'CameraRecordManagement',
                component: CameraRecordManagement,
                name: '摄像头记录'
            },
            {
                // 作业手环管理
                path: 'WorkBraceletManagement',
                component: WorkBraceletManagement,
                name: '作业手环管理'
            },
            {
                // 作业记录管理
                path: 'WorkRecordManagement',
                component: WorkRecordManagement,
                name: '作业记录管理'
            },
            {
                // 气体报警记录
                path: 'GasAlarmRecordManagement',
                component: GasAlarmRecordManagement,
                name: '气体报警记录'
            },
            {
                // 工人进出状态记录
                path: 'WorkerStatusRecordManagement',
                component: () => import('../views/WorkerStatusRecordManagement.vue'),
                name: '工人进出状态记录'
            },
            {
                // 用户管理
                path: 'UserManagement',
                component: UserManagement,
                name: '用户管理',
            },
            {
                // 角色管理
                path: 'RoleManagement',
                component: RoleManagement,
                name: '角色管理',
            },
            {
                // 功能管理
                path: 'RightManagement',
                component: RightManagement,
                name: '功能管理',
            },
            {
                path: 'CameraDetail',
                component: CameraDetail,
                name: '摄像头详情',
            }
        ],
    },
    {
        path: '/about',
        name: 'About',
        // route level code-splitting
        // this generates a separate chunk (about.[hash].js) for this route
        // which is lazy-loaded when the route is visited.
        component: () =>
            import(/* webpackChunkName: "about" */ '../views/About.vue'),
    },
    {
        path: '/login',
        name: 'Login',
        component: Login,
    },
    {
        path: '/',
        redirect: '/login',
    },
]

const router = new VueRouter({
    mode: 'history',
    base: process.env.BASE_URL,
    routes,
})

export default router
