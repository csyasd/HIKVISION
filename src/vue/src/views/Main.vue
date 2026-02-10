<template>
    <div class="monitoring-dashboard">
        <!-- 顶部标题栏 -->
        <div class="header">
            <h1>AIS数字化安全监控系统</h1>
            
        </div>

            <!-- 主要内容区域 -->
            <div class="content-area">
                <!-- 左侧数据面板 -->
                <div class="left-panel">
                    <!-- 时间显示 -->
                    <div class="time-display">
                        <div class="current-time">{{ currentTime }}</div>
                        <div class="current-date">{{ currentDate }}</div>
                    </div>

                    <!-- 设备总数卡片 -->
                    <div class="device-summary-card">
                        <div class="card-title">设备总数</div>
                        <div class="device-count">{{ deviceTotal }}</div>
                        <div class="device-types">
                            <div class="device-type-wrapper">
                                <el-popover
                                    placement="right"
                                    width="400"
                                    trigger="hover"
                                    popper-class="dark-popover">
                                    <template slot="reference">
                                        <div class="device-type">
                                            <span class="type-name">在线</span>
                                            <span class="type-count online">{{ onlineCount }}</span>
                                        </div>
                                    </template>
                                    <div class="popover-content">
                                        <div class="popover-header">在线工单信息</div>
                                        <table class="popover-table">
                                            <thead>
                                                <tr>
                                                    <th>编号</th>
                                                    <th>内容</th>
                                                    <th>开始时间</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-if="onlineWorkOrders.length === 0">
                                                    <td colspan="3" style="text-align: center; color: #909399; padding: 10px;">暂无在线工单</td>
                                                </tr>
                                                <tr v-for="wo in onlineWorkOrders" :key="wo.Id">
                                                    <td>{{ wo.Code }}</td>
                                                    <td>{{ wo.Content }}</td>
                                                    <td>{{ wo.StartTime }}</td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                </el-popover>
                            </div>
                            <div class="device-type">
                                <span class="type-name">离线</span>
                                <span class="type-count offline">{{ offlineCount }}</span>
                            </div>
                        </div>
                    </div>

                    <!-- 设备视频区域 -->
                    <div class="video-panel">
                        <div class="panel-header">
                            <span class="panel-title">设备视频</span>
                            <span class="more-videos" @click="$router.push('/home/CameraDetail')">更多视频</span>
                        </div>
                        <div class="video-grid">
                            <div class="video-item" v-for="camera in cameras" :key="camera.id" @click="toRealtime(camera.id)" style="cursor: pointer;">
                                <div class="video-container">
                                    <video 
                                        :id="`video_${camera.id}`"
                                        class="video-stream"
                                        muted
                                        autoplay
                                        playsinline
                                        style="width: 100%; height: 100%; background: #000; object-fit: contain;">
                                    </video>
                                    <div class="video-loading" v-if="loadingStatus[camera.id] && !cameraErrors[camera.id]">
                                        <i class="el-icon-loading"></i>
                                    </div>
                                    <div class="video-error" v-if="cameraErrors[camera.id]">
                                        <i class="el-icon-warning"></i>
                                        <span @click="playCamera(camera)">重试</span>
                                    </div>
                                </div>
                                <div class="video-info">{{ camera.name }}</div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- 右侧地图和数据显示 -->
                <div class="right-panel">
                    <!-- 地图容器 -->
                    <div id="map" class="map-container"></div>

                    <!-- 气体监测数据表格 -->
                    <div class="gas-monitoring-table">
                        <div class="table-header">气体监测实时数据</div>
                        <table class="data-table">
                            <thead>
                                <tr>
                                    <th>设备名称及型号</th>
                                    <th>工单编号</th>
                                    <th>气体名称</th>
                                    <th>检测数值</th>
                                    <th>状态</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-if="gasMonitoringData.length === 0">
                                    <td colspan="5" style="text-align: center; color: #909399; padding: 15px;">
                                        暂无数据
                                    </td>
                                </tr>
                                <tr v-for="(item, index) in gasMonitoringData" :key="index">
                                    <td>{{ item.DeviceName }}</td>
                                    <td>{{ item.WorkOrderCode }}</td>
                                    <td>{{ item.GasName }}</td>
                                    <td>{{ item.GasValue }}</td>
                                    <td class="status-normal">{{ item.Status }}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <!-- 手环信息表格 -->
                    <div class="alarm-table">
                        <div class="table-header">手环信息</div>
                        <table class="data-table">
                            <thead>
                                <tr>
                                    <th>设备名称及型号</th>
                                    <th>工单编号</th>
                                    <th>工人姓名</th>
                                    <th>心率</th>
                                    <th>进离场状态</th>
                                    <th>进场时间</th>
                                    <th>出场时间</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr v-if="braceletInfoData.length === 0">
                                    <td colspan="7" style="text-align: center; color: #909399; padding: 15px;">
                                        暂无数据
                                    </td>
                                </tr>
                                <tr v-for="(item, index) in braceletInfoData" :key="index">
                                    <td>{{ item.DeviceName }}</td>
                                    <td>{{ item.WorkOrderCode }}</td>
                                    <td>{{ item.WorkerName }}</td>
                                    <td>{{ item.HeartRate || '-' }}</td>
                                    <td class="status-normal">{{ item.EntryExitStatus }}</td>
                                    <td>{{ item.EntryTime || '-' }}</td>
                                    <td>{{ item.ExitTime || '-' }}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
</template>

<script>
import { SelectALLDevice, GetRealtimeGasData, GetRealtimeBraceletInfo, SelectALLCamera, GetRealtimeWorkOrders, BaseUrl } from '@/api/api.js';

export default {
    name: 'MonitoringDashboard',
    data() {
        return {
            currentTime: '',
            currentDate: '',
            cameras: [],
            map: null,
            markers: [],
            devices: [],
            deviceTotal: 0,
            onlineCount: 0,
            offlineCount: 0,
            gasMonitoringData: [],
            braceletInfoData: [],
            workOrders: [],
            playerInstances: {}, 
            lastCheckTime: {}, // 记录上一次播放时间
            stuckCount: {},     // 记录卡死次数
            loadingStatus: {},
            cameraErrors: {},
            latencyCheckTimer: null,
            API_BASE: BaseUrl.replace(/\/$/, '')
        }
    },
    mounted() {
        this.updateTime();
        this.initMap();
        (async () => {
            await this.loadDevices();
            await this.loadCameras();
            this.loadGasMonitoringData();
            this.loadBraceletInfo();
            this.loadWorkOrders();
            this.autoPlayAll();
        })();
        setInterval(this.updateTime, 1000);
        setInterval(async () => {
            await this.loadDevices();
            await this.loadCameras();
            this.loadGasMonitoringData();
            this.loadBraceletInfo();
            this.loadWorkOrders();
        }, 5000);

        // 每 2.5 秒执行一次健康检查（卡死监控 + 追帧）
        this.latencyCheckTimer = setInterval(this.checkHealth, 2500);
    },
    beforeDestroy() {
        if (this.latencyCheckTimer) clearInterval(this.latencyCheckTimer);
        this.cleanupAll();
    },
    computed: {
        onlineWorkOrders() {
            return this.workOrders || [];
        }
    },
    methods: {
        updateTime() {
            const now = new Date();
            this.currentTime = now.toLocaleTimeString();
            this.currentDate = now.toLocaleDateString('zh-CN', {
                year: 'numeric',
                month: '2-digit',
                day: '2-digit',
                weekday: 'long'
            });
        },
        
        initMap() {
            if (typeof AMap === 'undefined') {
                const script = document.createElement('script');
                script.src = 'https://webapi.amap.com/maps?v=2.0&key=933b70f0dfaf67b0f950d1682dc27ca1';
                script.onload = () => this.createMap();
                document.head.appendChild(script);
            } else {
                this.createMap();
            }
        },
        
        createMap() {
            this.map = new AMap.Map('map', {
                viewMode: '3D',
                zoom: 12,
                center: [116.4074, 39.9042],
                mapStyle: 'amap://styles/dark',
                pitch: 45,
                rotation: 0
            });
            if (this.devices.length > 0) this.updateMapMarkers();
        },
        
        async loadDevices() {
            try {
                const res = await SelectALLDevice();
                if (res) {
                    this.devices = res;
                    this.deviceTotal = this.devices.length;
                    this.onlineCount = this.devices.filter(d => d.OnlineStatus === '在线').length;
                    this.offlineCount = this.deviceTotal - this.onlineCount;
                    this.updateMapMarkers();
                }
            } catch (error) {
                console.error('加载设备列表失败:', error);
            }
        },
        
        updateMapMarkers() {
            if (!this.map) return;
            this.map.remove(this.markers);
            this.markers = [];
            
            const validDevices = this.devices.filter(d => 
                d.GpsLongitude && d.GpsLatitude && 
                !isNaN(parseFloat(d.GpsLongitude)) && !isNaN(parseFloat(d.GpsLatitude))
            );
            
            validDevices.forEach(device => {
                const lng = parseFloat(device.GpsLongitude);
                const lat = parseFloat(device.GpsLatitude);
                const marker = new AMap.Marker({
                    position: [lng, lat],
                    title: device.Name,
                    icon: new AMap.Icon({
                        size: new AMap.Size(32, 32),
                        image: 'data:image/svg+xml;base64,' + btoa(`
                            <svg width="32" height="32" viewBox="0 0 32 32" xmlns="http://www.w3.org/2000/svg">
                                <circle cx="16" cy="16" r="12" fill="#409eff" stroke="#fff" stroke-width="2"/>
                                <circle cx="16" cy="16" r="6" fill="#fff"/>
                            </svg>
                        `)
                    })
                });
                marker.on('click', () => this.showDeviceInfo(device, lng, lat));
                this.map.add(marker);
                this.markers.push(marker);
            });
            if (this.markers.length > 0) this.map.setFitView();
        },
        
        showDeviceInfo(device, lng, lat) {
            const onlineStatus = device.OnlineStatus || '离线';
            const onlineStatusColor = onlineStatus === '在线' ? '#67c23a' : '#909399';
            const infoWindow = new AMap.InfoWindow({
                content: `
                    <div style="color: #e6edf3; padding: 16px; background: rgba(13, 17, 23, 0.9); backdrop-filter: blur(10px); border: 1px solid rgba(255, 255, 255, 0.1); border-radius: 12px; min-width: 240px; box-shadow: 0 8px 32px rgba(0,0,0,0.5);">
                        <h4 style="margin: 0 0 12px 0; border-bottom: 1px solid rgba(255, 255, 255, 0.1); padding-bottom: 10px; color: #409eff; font-size: 16px; font-weight: 700;">${device.Name}</h4>
                        <p style="margin: 6px 0; font-size: 13px; color: #ccc;"><b>在线状态:</b> <span style="color: ${onlineStatusColor}; font-weight: bold;">${onlineStatus}</span></p>
                        <p style="margin: 6px 0; font-size: 13px; color: #ccc;"><b>IP地址:</b> ${device.IP || '未知'}</p>
                    </div>
                `,
                offset: new AMap.Pixel(0, -30)
            });
            infoWindow.open(this.map, [lng, lat]);
        },
        
        async loadCameras() {
            try {
                const camerasRes = await SelectALLCamera();
                if (!camerasRes || !Array.isArray(camerasRes)) return;
                const onlineDeviceIds = this.devices.filter(d => d.OnlineStatus === '在线').map(d => d.Id);
                this.cameras = camerasRes
                    .filter(camera => camera.DeviceId && onlineDeviceIds.includes(camera.DeviceId))
                    .slice(0, 4)
                    .map(camera => ({
                        id: camera.Id,
                        name: camera.Name || (camera.Device ? camera.Device.Name : '未知摄像头'),
                        ip: camera.IP,
                        deviceId: camera.DeviceId
                    }));
            } catch (error) {
                console.error('加载摄像头列表失败:', error);
            }
        },
        
        async loadGasMonitoringData() {
            try {
                const res = await GetRealtimeGasData();
                if (res && Array.isArray(res)) {
                    const gasNamesMap = {
                        "Gas1": "一氧化碳",
                        "Gas2": "硫化氢",
                        "Gas3": "甲烷",
                        "Gas4": "二氧化硫"
                    };
                    const gasUnitsMap = {
                        "Gas1": "ppm",
                        "Gas2": "ppm",
                        "Gas3": "%LEL",
                        "Gas4": "ppm"
                    };
                    this.gasMonitoringData = res.map(item => {
                        const originalName = item.GasName;
                        if (gasNamesMap[originalName]) {
                            return {
                                ...item,
                                GasName: gasNamesMap[originalName],
                                GasValue: `${item.GasValue} ${gasUnitsMap[originalName]}`
                            };
                        }
                        return item;
                    });
                } else {
                    this.gasMonitoringData = [];
                }
            } catch (error) {
                console.error('加载气体监测数据失败:', error);
            }
        },
        
        async loadBraceletInfo() {
            try {
                const res = await GetRealtimeBraceletInfo();
                this.braceletInfoData = (res && Array.isArray(res)) ? res : [];
            } catch (error) {}
        },
        
        async loadWorkOrders() {
            try {
                const res = await GetRealtimeWorkOrders();
                if (res) this.workOrders = res;
            } catch (error) {}
        },
        
        // 核心播放逻辑：同步实时数据页面的最新配置
        async playCamera(camera) {
            if (!window.flvjs || !window.flvjs.isSupported()) return;
            const videoElement = document.getElementById(`video_${camera.id}`);
            if (!videoElement) return;

            this.stopCamera(camera.id);
            this.$set(this.loadingStatus, camera.id, true);
            this.$set(this.cameraErrors, camera.id, false);
            this.lastCheckTime[camera.id] = -1;
            this.stuckCount[camera.id] = 0;

            const playUrl = `${this.API_BASE}/api/HK/flv-stream/${camera.id}`;

            try {
                const player = window.flvjs.createPlayer({
                    type: 'flv',
                    url: playUrl,
                    isLive: true,
                    hasAudio: false
                }, {
                    enableStashBuffer: false,
                    stashInitialSize: 128,
                    enableWorker: false, // 禁用 Worker 防止首页 TypeError
                    lazyLoad: false,
                    autoCleanupSourceBuffer: true
                });

                player.attachMediaElement(videoElement);
                player.load();
                player.play().then(() => {
                    this.$set(this.loadingStatus, camera.id, false);
                }).catch(() => {
                    this.$set(this.cameraErrors, camera.id, true);
                    this.$set(this.loadingStatus, camera.id, false);
                });

                player.on(window.flvjs.Events.ERROR, () => {
                    this.$set(this.cameraErrors, camera.id, true);
                    this.stopCamera(camera.id);
                });

                this.playerInstances[camera.id] = player;
            } catch (e) {
                this.$set(this.cameraErrors, camera.id, true);
            }
        },
        
        stopCamera(cameraId) {
            const player = this.playerInstances[cameraId];
            if (player) {
                try {
                    player.pause();
                    player.unload();
                    player.detachMediaElement();
                    player.destroy();
                } catch (e) {}
                delete this.playerInstances[cameraId];
            }
        },

        // 健康检查：卡死监控 + 极速追帧
        checkHealth() {
            Object.keys(this.playerInstances).forEach(id => {
                const video = document.getElementById(`video_${id}`);
                if (!video) return;

                // 1. 卡死检测：仅在出图后触发
                if (video.currentTime > 0 && video.currentTime === this.lastCheckTime[id] && !video.paused) {
                    this.stuckCount[id]++;
                    if (this.stuckCount[id] >= 3) { // 约 7.5 秒画面不动
                        const camera = this.cameras.find(c => c.id == id);
                        if (camera) {
                            console.warn(`[首页] 摄像头 ${camera.name} 卡死，重连中...`);
                            this.retryPlay(camera);
                        }
                    }
                } else {
                    this.stuckCount[id] = 0;
                    this.lastCheckTime[id] = video.currentTime;
                }

                // 2. 追帧逻辑
                if (video.buffered.length > 0) {
                    const end = video.buffered.end(0);
                    const diff = end - video.currentTime;
                    if (diff > 1.5) {
                        video.currentTime = end - 0.2;
                    } else if (diff > 0.5) {
                        video.playbackRate = 1.1;
                    } else {
                        video.playbackRate = 1.0;
                    }
                }
            });
        },

        retryPlay(camera) {
            this.stopCamera(camera.id);
            setTimeout(() => this.playCamera(camera), 3000); 
        },

        autoPlayAll() {
            this.cameras.forEach(camera => this.playCamera(camera));
        },

        cleanupAll() {
            Object.keys(this.playerInstances).forEach(id => this.stopCamera(id));
        },

        toRealtime(cameraId) {
            this.$router.push({
                path: '/home/CameraDetail',
                query: { cameraId: cameraId }
            });
        }
    }
}
</script>

<style scoped>
/* 保持原有样式不变 */
.monitoring-dashboard {
    width: 100%;
    height: 100vh;
    background: linear-gradient(135deg, #0a0e1a 0%, #1a2332 50%, #0f1419 100%);
    color: #ffffff;
    font-family: 'Microsoft YaHei', 'PingFang SC', 'Helvetica Neue', Arial, sans-serif;
    overflow: hidden;
    position: relative;
    zoom: 0.8;
    transform-origin: top left;
}

/* ... 后面样式省略，保持 Main.vue 原有美观设计 ... */
.monitoring-dashboard::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: 
        radial-gradient(circle at 20% 50%, rgba(64, 158, 255, 0.05) 0%, transparent 50%),
        radial-gradient(circle at 80% 80%, rgba(103, 194, 58, 0.05) 0%, transparent 50%);
    pointer-events: none;
    z-index: 0;
}

.header {
    height: 70px;
    background: linear-gradient(135deg, rgba(0, 0, 0, 0.9) 0%, rgba(26, 35, 50, 0.9) 100%);
    backdrop-filter: blur(10px);
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 30px;
    border-bottom: 2px solid rgba(64, 158, 255, 0.2);
    box-shadow: 0 2px 20px rgba(0, 0, 0, 0.3);
    position: relative;
    z-index: 10;
}

.header h1 {
    margin: 0;
    font-size: 26px;
    font-weight: 600;
    background: linear-gradient(135deg, #409eff 0%, #66b1ff 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    letter-spacing: 1px;
}

.content-area {
    flex: 1;
    display: flex;
    padding: 0;
    gap: 0;
    position: relative;
    height: calc(100vh - 70px);
}

.left-panel {
    width: 420px;
    display: flex;
    flex-direction: column;
    gap: 20px;
    position: absolute;
    top: 20px;
    left: 20px;
    z-index: 2;
    padding: 0;
    max-height: calc(100vh - 110px);
    overflow-y: auto;
}

.left-panel::-webkit-scrollbar {
    width: 6px;
}

.left-panel::-webkit-scrollbar-track {
    background: rgba(0, 0, 0, 0.3);
    border-radius: 3px;
}

.left-panel::-webkit-scrollbar-thumb {
    background: rgba(64, 158, 255, 0.5);
    border-radius: 3px;
}

.left-panel::-webkit-scrollbar-thumb:hover {
    background: rgba(64, 158, 255, 0.7);
}

.right-panel {
    flex: 1;
    position: relative;
    height: 100%;
}

/* 时间显示卡片 */
.time-display {
    background: linear-gradient(135deg, rgba(64, 158, 255, 0.1) 0%, rgba(0, 0, 0, 0.7) 100%);
    padding: 18px;
    border-radius: 12px;
    border: 1px solid rgba(64, 158, 255, 0.2);
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3), inset 0 1px 0 rgba(255, 255, 255, 0.1);
    backdrop-filter: blur(10px);
    transition: all 0.3s ease;
    flex-shrink: 0;
}

.time-display:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 25px rgba(64, 158, 255, 0.2), inset 0 1px 0 rgba(255, 255, 255, 0.1);
}

.current-time {
    font-size: 32px;
    font-weight: 700;
    background: linear-gradient(135deg, #409eff 0%, #66b1ff 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    margin-bottom: 6px;
    letter-spacing: 2px;
    text-shadow: 0 0 20px rgba(64, 158, 255, 0.3);
}

.current-date {
    font-size: 14px;
    color: rgba(255, 255, 255, 0.8);
    font-weight: 400;
}

/* 设备总数卡片 */
.device-summary-card {
    background: linear-gradient(135deg, rgba(0, 0, 0, 0.7) 0%, rgba(26, 35, 50, 0.7) 100%);
    padding: 18px;
    border-radius: 12px;
    border: 1px solid rgba(64, 158, 255, 0.2);
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3), inset 0 1px 0 rgba(255, 255, 255, 0.1);
    backdrop-filter: blur(10px);
    transition: all 0.3s ease;
    flex-shrink: 0;
}

.device-summary-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 25px rgba(64, 158, 255, 0.2), inset 0 1px 0 rgba(255, 255, 255, 0.1);
}

.card-title {
    font-size: 16px;
    font-weight: 600;
    margin-bottom: 18px;
    color: rgba(255, 255, 255, 0.9);
    display: flex;
    align-items: center;
    gap: 8px;
}

.card-title::before {
    content: '📊';
    font-size: 18px;
}

.device-count {
    font-size: 48px;
    font-weight: 700;
    background: linear-gradient(135deg, #409eff 0%, #66b1ff 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    text-align: center;
    margin-bottom: 15px;
    text-shadow: 0 0 20px rgba(64, 158, 255, 0.3);
}

.device-types {
    display: flex;
    justify-content: space-around;
    gap: 15px;
}

.device-type, .device-type-wrapper {
    flex: 1;
    text-align: center;
}

.device-type {
    padding: 12px;
    background: rgba(0, 0, 0, 0.3);
    border-radius: 8px;
    border: 1px solid rgba(255, 255, 255, 0.1);
    transition: all 0.3s ease;
    width: 100%;
    box-sizing: border-box;
}

.device-type:hover {
    background: rgba(64, 158, 255, 0.1);
    transform: scale(1.05);
}

.type-name {
    display: block;
    font-size: 13px;
    color: rgba(255, 255, 255, 0.7);
    margin-bottom: 8px;
    font-weight: 500;
}

.type-count {
    display: block;
    font-size: 24px;
    font-weight: 700;
    color: #ffffff;
}

.type-count.online {
    color: #ffffff;
}

.type-count.offline {
    color: #ffffff;
}

/* 视频面板 */
.video-panel {
    background: rgba(0, 0, 0, 0.5);
    backdrop-filter: blur(20px);
    padding: 20px;
    border-radius: 20px;
    border: 1px solid rgba(64, 158, 255, 0.2);
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
    flex: 1;
    min-height: 0;
    display: flex;
    flex-direction: column;
    overflow: hidden;
}

.panel-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 18px;
    padding-bottom: 12px;
    border-bottom: 1px solid rgba(64, 158, 255, 0.2);
}

.panel-title {
    font-size: 18px;
    font-weight: 600;
    color: #ffffff;
    display: flex;
    align-items: center;
    gap: 8px;
}

.panel-title::before {
    content: '📹';
    font-size: 20px;
}

.more-videos {
    color: #409eff;
    cursor: pointer;
    font-size: 14px;
    padding: 6px 12px;
    border-radius: 6px;
    transition: all 0.3s ease;
    border: 1px solid transparent;
}

.more-videos:hover {
    background: rgba(64, 158, 255, 0.1);
    border-color: rgba(64, 158, 255, 0.3);
}

.video-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 15px;
    flex: 1;
}

.video-item {
    background: #000;
    border-radius: 8px;
    overflow: hidden;
    border: 1px solid rgba(64, 158, 255, 0.2);
    transition: all 0.3s ease;
    display: flex;
    flex-direction: column;
}

.video-item:hover {
    transform: scale(1.02);
    box-shadow: 0 4px 15px rgba(64, 158, 255, 0.3);
}

.video-container {
    position: relative;
    width: 100%;
    flex: 1;
    min-height: 130px;
    background: #000;
}

.video-stream {
    width: 100%;
    height: 100%;
}

.video-loading, .video-error {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(0, 0, 0, 0.7);
    z-index: 5;
}

.video-loading i {
    font-size: 24px;
    color: #409eff;
}

.video-error {
    flex-direction: column;
    color: #f56c6c;
}

.video-error i {
    font-size: 24px;
    margin-bottom: 8px;
}

.video-error span {
    font-size: 12px;
    cursor: pointer;
    text-decoration: underline;
}

.video-info {
    padding: 10px;
    font-size: 13px;
    color: rgba(255, 255, 255, 0.8);
    text-align: center;
    background: rgba(0, 0, 0, 0.5);
    font-weight: 500;
}

/* 地图容器 */
.map-container {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    border-radius: 0;
    overflow: hidden;
    z-index: 1;
}

/* 表格样式优化 */
.gas-monitoring-table, .alarm-table {
    position: absolute;
    background: rgba(0, 0, 0, 0.6);
    backdrop-filter: blur(20px);
    border-radius: 20px;
    border: 1px solid rgba(64, 158, 255, 0.2);
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.6);
    max-width: 480px;
    width: 480px;
    z-index: 3;
    overflow: hidden;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.gas-monitoring-table:hover, .alarm-table:hover {
    box-shadow: 0 10px 40px rgba(64, 158, 255, 0.2), inset 0 1px 0 rgba(255, 255, 255, 0.1);
}

.gas-monitoring-table {
    top: 15px;
    right: 15px;
    min-height: 120px;
    max-height: calc(50vh - 25px);
    height: auto;
    overflow: hidden;
    display: flex;
    flex-direction: column;
}

.alarm-table {
    bottom: 15px;
    right: 15px;
    min-height: 120px;
    max-height: calc(50vh - 25px);
    height: auto;
    overflow: hidden;
    display: flex;
    flex-direction: column;
}

.gas-monitoring-table::-webkit-scrollbar,
.alarm-table::-webkit-scrollbar {
    width: 6px;
}

.gas-monitoring-table::-webkit-scrollbar-track,
.alarm-table::-webkit-scrollbar-track {
    background: rgba(0, 0, 0, 0.3);
    border-radius: 3px;
}

.gas-monitoring-table::-webkit-scrollbar-thumb,
.alarm-table::-webkit-scrollbar-thumb {
    background: rgba(64, 158, 255, 0.5);
    border-radius: 3px;
}

.gas-monitoring-table::-webkit-scrollbar-thumb:hover,
.alarm-table::-webkit-scrollbar-thumb:hover {
    background: rgba(64, 158, 255, 0.7);
}

.table-header {
    padding: 16px 20px;
    background: linear-gradient(90deg, rgba(64, 158, 255, 0.1), transparent);
    color: #409eff;
    font-weight: 700;
    font-size: 16px;
    border-bottom: 1px solid rgba(64, 158, 255, 0.2);
    display: flex;
    align-items: center;
    gap: 12px;
    letter-spacing: 1px;
    text-transform: uppercase;
    flex-shrink: 0;
}

.gas-monitoring-table .table-header::before {
    content: '💨';
    font-size: 18px;
}

.alarm-table .table-header::before {
    content: '⌚';
    font-size: 18px;
}

.data-table {
    width: 100%;
    border-collapse: collapse;
    flex: 0 1 auto;
    display: table;
    overflow: visible;
}

.data-table thead {
    display: table-header-group;
}

.data-table tbody {
    display: table-row-group;
}

.data-table thead tr,
.data-table tbody tr {
    display: table-row;
}

.data-table th,
.data-table td {
    padding: 8px 10px;
    text-align: left;
    border-bottom: 1px solid rgba(64, 158, 255, 0.1);
    font-size: 13px;
    word-wrap: break-word;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    line-height: 1.5;
}

.data-table th {
    background: rgba(0, 0, 0, 0.4);
    color: rgba(64, 158, 255, 0.9);
    font-weight: 600;
    text-transform: uppercase;
    font-size: 12px;
    letter-spacing: 0.5px;
    white-space: nowrap;
    padding: 10px;
}

.data-table tbody tr {
    transition: all 0.2s ease;
}

.data-table tbody tr:hover {
    background: transparent;
}

.data-table td {
    color: rgba(255, 255, 255, 0.9);
    font-weight: 400;
}


/* 响应式设计 */
@media (max-width: 1400px) {
    .left-panel {
        width: 380px;
    }
    
    .gas-monitoring-table, .alarm-table {
        max-width: 400px;
    }
}

@media (max-width: 1200px) {
    .content-area {
        flex-direction: column;
    }
    
    .left-panel {
        width: 100%;
        position: relative;
        top: 0;
        left: 0;
        flex-direction: row;
        overflow-x: auto;
        max-height: auto;
        padding: 15px;
    }
    
    .right-panel {
        height: 500px;
    }
    
    .gas-monitoring-table, .alarm-table {
        max-width: 100%;
        position: relative;
        top: auto;
        right: auto;
        margin: 10px;
    }
}

@media (max-width: 768px) {
    .monitoring-dashboard .header h1 { font-size: 16px !important; }
    .content-area { flex-direction: column; padding: 8px !important; }
    .left-panel { flex-direction: column !important; padding: 12px !important; min-height: auto !important; }
    .right-panel { height: 350px !important; min-height: 300px !important; }
    .video-grid { grid-template-columns: 1fr !important; gap: 10px !important; }
    .device-summary-card { padding: 12px !important; }
    .device-summary-card .device-count { font-size: 24px !important; }
    .gas-monitoring-table, .alarm-table {
        width: 100% !important; max-width: 100% !important;
        max-height: 200px !important; font-size: 12px !important;
    }
    .data-table th, .data-table td { padding: 6px 8px !important; font-size: 11px !important; white-space: normal !important; word-break: break-all; }
    .table-header { font-size: 13px !important; padding: 10px 12px !important; }
}
</style>

<style>
/* 全局 Popover 样式，用于覆盖 Element UI 默认样式 */
.dark-popover {
    background: rgba(0, 0, 0, 0.8) !important;
    backdrop-filter: blur(20px) !important;
    border: 1px solid rgba(64, 158, 255, 0.2) !important;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.8) !important;
    padding: 0 !important;
}

.dark-popover[x-placement^="right"] .popper__arrow::after {
    border-right-color: #0a0e1a !important;
}

.dark-popover[x-placement^="right"] .popper__arrow {
    border-right-color: #409eff !important;
}

.popover-content {
    color: #ffffff;
    background: #0a0e1a;
}

.popover-header {
    background: linear-gradient(135deg, rgba(64, 158, 255, 0.3) 0%, transparent 100%);
    padding: 12px 15px;
    font-weight: bold;
    border-bottom: 1px solid rgba(64, 158, 255, 0.3);
    font-size: 15px;
    color: #409eff;
}

.popover-table {
    width: 100%;
    border-collapse: collapse;
    font-size: 13px;
}

.popover-table th {
    text-align: left;
    padding: 10px 15px;
    color: rgba(255, 255, 255, 0.6);
    border-bottom: 1px solid rgba(64, 158, 255, 0.2);
    background: rgba(0, 0, 0, 0.2);
}

.popover-table td {
    padding: 10px 15px;
    border-bottom: 1px solid rgba(255, 255, 255, 0.05);
    color: #fff;
}

.popover-table tr:hover {
    background: transparent;
}
</style>
