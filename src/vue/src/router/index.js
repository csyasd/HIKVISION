import Vue from 'vue'
import VueRouter from 'vue-router'
import { isMobileDevice } from '../utils/device'

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
  // ========== 移动端路由（/m/ 前缀）==========
  {
    path: '/m/login',
    name: 'MobileLogin',
    component: () => import('../views/mobile/MobileLogin.vue'),
    meta: { mobile: true }
  },
  {
    path: '/m',
    component: () => import('../views/mobile/MobileLayout.vue'),
    meta: { mobile: true, requiresAuth: true },
    redirect: '/m/main',
    children: [
      { path: 'main', component: () => import('../views/mobile/MobileMain.vue'), name: 'MobileMain' },
      { path: 'realtime', component: () => import('../views/mobile/MobileRealtime.vue'), name: 'MobileRealtime' },
      { path: 'menu', component: () => import('../views/mobile/MobileMenu.vue'), name: 'MobileMenu' },
      { path: 'work-bracelet', component: () => import('../views/mobile/MobileWorkBracelet.vue'), name: 'work-bracelet' },
      { path: 'worker-status', component: () => import('../views/mobile/MobileWorkerStatus.vue'), name: 'worker-status' },
      { path: 'gas-alarm', component: () => import('../views/mobile/MobileGasAlarm.vue'), name: 'gas-alarm' },
      { path: 'bracelet-abnormal', component: () => import('../views/mobile/MobileBraceletAbnormal.vue'), name: 'bracelet-abnormal' },
      { path: 'gas-abnormal', component: () => import('../views/mobile/MobileGasAbnormal.vue'), name: 'gas-abnormal' },
      { path: 'work-order', component: () => import('../views/mobile/MobileWorkOrder.vue'), name: 'work-order' },
      { path: 'device', component: () => import('../views/mobile/MobileDevice.vue'), name: 'device' },
      { path: 'camera', component: () => import('../views/mobile/MobileCamera.vue'), name: 'camera' }
    ]
  },

  // ========== PC 端路由 ==========
  {
    path: '/home',
    name: 'Home',
    component: Home,
    children: [
      { path: 'main', component: Main, name: '首页' },
      { path: 'CameraDetail', component: () => import('../views/RealtimeData.vue'), name: '实时数据' },
      { path: 'CameraPlayer', component: () => import('../views/CameraPlayer.vue'), name: '摄像头播放器' },
      { path: 'DeviceManagement', component: DeviceManagement, name: '设备管理' },
      { path: 'CameraManagement', component: CameraManagement, name: '摄像头管理' },
      { path: 'WorkOrderManagement', component: WorkOrderManagement, name: '施工工单' },
      { path: 'CameraRecordManagement', component: CameraRecordManagement, name: '摄像头记录' },
      { path: 'WorkBraceletManagement', component: WorkBraceletManagement, name: '作业手环管理' },
      { path: 'WorkRecordManagement', component: WorkRecordManagement, name: '作业记录管理' },
      { path: 'GasAlarmRecordManagement', component: GasAlarmRecordManagement, name: '气体报警记录' },
      { path: 'WorkerStatusRecordManagement', component: () => import('../views/WorkerStatusRecordManagement.vue'), name: '工人进出状态记录' },
      { path: 'BraceletAbnormalManagement', component: () => import('../views/BraceletAbnormalManagement.vue'), name: '心率异常记录' },
      { path: 'GasAbnormalManagement', component: () => import('../views/GasAbnormalManagement.vue'), name: '气体异常记录' },
      { path: 'UserManagement', component: UserManagement, name: '用户管理' },
      { path: 'RoleManagement', component: RoleManagement, name: '角色管理' },
      { path: 'RightManagement', component: RightManagement, name: '功能管理' },
      { path: 'CameraDetail', component: CameraDetail, name: '摄像头详情' }
    ]
  },
  {
    path: '/about',
    name: 'About',
    component: () => import(/* webpackChunkName: "about" */ '../views/About.vue')
  },
  {
    path: '/login',
    name: 'Login',
    component: Login
  },
  {
    path: '/',
    redirect: to => (isMobileDevice() ? '/m/login' : '/login')
  }
]

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

// 设备路由跳转：手机访问 PC 路由跳转手机端，电脑访问手机路由跳转 PC 端
router.beforeEach((to, from, next) => {
  const mobile = isMobileDevice()
  const isMobileRoute = to.path.startsWith('/m')
  const forceDesktop = to.query.desktop === '1'

  // 手机访问 PC 路由（且非强制桌面版）→ 跳转手机端
  if (mobile && !isMobileRoute && !forceDesktop) {
    if (to.path === '/login' || to.path === '/') {
      return next('/m/login')
    }
    if (to.path.startsWith('/home')) {
      return next('/m/main')
    }
  }

  // 电脑访问手机路由 → 跳转 PC 端
  if (!mobile && isMobileRoute) {
    if (to.path === '/m/login') return next('/login')
    if (to.path.startsWith('/m')) return next('/home/main')
  }

  // 移动端需登录
  if (to.meta?.requiresAuth && isMobileRoute) {
    const token = localStorage.getItem('token')
    if (!token && to.path !== '/m/login') {
      return next('/m/login')
    }
  }

  next()
})

export default router
