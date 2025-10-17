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
                        <div class="device-count">97</div>
                        <div class="device-types">
                            <div class="device-type">
                                <span class="type-name">便携式</span>
                                <span class="type-count">58</span>
                            </div>
                            <div class="device-type">
                                <span class="type-name">移动式</span>
                                <span class="type-count">37</span>
                            </div>
                            <div class="device-type">
                                <span class="type-name">布控球</span>
                                <span class="type-count">2</span>
                            </div>
                        </div>
                    </div>

                    <!-- 场景名称表格 -->
                    <div class="scene-table-card">
                        <div class="card-title">场景名称</div>
                        <table class="scene-table">
                            <thead>
                                <tr>
                                    <th>便携式</th>
                                    <th>移动式</th>
                                    <th>报警</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>4</td>
                                    <td>28</td>
                                    <td>0</td>
                                </tr>
                                <tr>
                                    <td>0</td>
                                    <td>0</td>
                                    <td>0</td>
                                </tr>
                                <tr>
                                    <td>0</td>
                                    <td>0</td>
                                    <td>0</td>
                                </tr>
                                <tr>
                                    <td>1</td>
                                    <td>0</td>
                                    <td>0</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <!-- 设备视频区域 -->
                    <div class="video-panel">
                        <div class="panel-header">
                            <span class="panel-title">设备视频</span>
                            <span class="more-videos">更多视频</span>
                        </div>
                        <div class="video-grid">
                            <div class="video-item" v-for="camera in cameras" :key="camera.id">
                                <div class="video-container">
                                    <video 
                                        :id="`videoPlayer_${camera.id}`"
                                        class="video-stream"
                                        muted
                                        playsinline
                                        autoplay>
                                    </video>
                                    <div class="video-controls">
                                        <button class="control-btn" @click="playCamera(camera)">播放</button>
                                        <button class="control-btn" @click="stopCamera(camera.id)">停止</button>
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
                                    <th>设备名称及编号</th>
                                    <th>气体名称</th>
                                    <th>检测数值</th>
                                    <th>状态</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>试流13/920210074</td>
                                    <td>O2</td>
                                    <td>20.9%VOL</td>
                                    <td class="status-online">在线</td>
                                </tr>
                                <tr>
                                    <td>ZJ测试3333/913813</td>
                                    <td>CH4</td>
                                    <td>0%LEL</td>
                                    <td class="status-online">在线</td>
                                </tr>
                                <tr>
                                    <td>ZJ测试3333/913813</td>
                                    <td>H2S</td>
                                    <td>0ppm</td>
                                    <td class="status-online">在线</td>
                                </tr>
                                <tr>
                                    <td>ZJ测试3333/913813</td>
                                    <td>CO</td>
                                    <td>7ppm</td>
                                    <td class="status-online">在线</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <!-- 报警信息表格 -->
                    <div class="alarm-table">
                        <div class="table-header">报警信息</div>
                        <table class="data-table">
                            <thead>
                                <tr>
                                    <th>设备名称及编号</th>
                                    <th>报警类型</th>
                                    <th>报警时间</th>
                                    <th>操作</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>ZJ测试-6366/913813</td>
                                    <td class="alarm-type">跌倒报警</td>
                                    <td>2024-07-01 11:15:23</td>
                                    <td><button class="handle-btn">处理</button></td>
                                </tr>
                                <tr>
                                    <td>ZJ测试-6360/913813</td>
                                    <td class="alarm-type">跌倒报警</td>
                                    <td>2024-07-01 11:12:45</td>
                                    <td><button class="handle-btn">处理</button></td>
                                </tr>
                                <tr>
                                    <td>ZJ测试-6362/913813</td>
                                    <td class="alarm-type">SOS求救</td>
                                    <td>2024-07-01 11:08:12</td>
                                    <td><button class="handle-btn">处理</button></td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
</template>

<script>
export default {
    name: 'MonitoringDashboard',
    data() {
        return {
            currentTime: '',
            currentDate: '',
            cameras: [
                { id: 1, name: 'ZJ测试-6366', ip: '192.168.100.101' },
                { id: 2, name: 'ZJ测试-6360', ip: '192.168.100.102' },
                { id: 3, name: 'ZJ测试-6362', ip: '192.168.100.103' },
                { id: 4, name: 'ZJ测试3332', ip: '192.168.100.104' }
            ],
            map: null,
            markers: []
        }
    },
    mounted() {
        this.updateTime();
        this.initMap();
        this.loadCameras();
        setInterval(this.updateTime, 1000);
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
            // 动态加载高德地图API
            if (typeof AMap === 'undefined') {
                const script = document.createElement('script');
                script.src = 'https://webapi.amap.com/maps?v=2.0&key=933b70f0dfaf67b0f950d1682dc27ca1';
                script.onload = () => {
                    this.createMap();
                };
                document.head.appendChild(script);
            } else {
                this.createMap();
            }
        },
        
        createMap() {
            // 初始化地图
            this.map = new AMap.Map('map', {
                viewMode: '3D',
                zoom: 12,
                center: [116.4074, 39.9042], // 北京坐标
                mapStyle: 'amap://styles/dark',
                pitch: 45,
                rotation: 0
            });

            // 添加摄像头位置标记
            this.addCameraMarkers();
        },
        
        addCameraMarkers() {
            if (!this.map) return;
            
            // 模拟摄像头GPS坐标（实际应该从后端获取）
            const cameraPositions = [
                { id: 1, name: 'ZJ测试-6366', lng: 116.4074, lat: 39.9042 },
                { id: 2, name: 'ZJ测试-6360', lng: 116.4174, lat: 39.9142 },
                { id: 3, name: 'ZJ测试-6362', lng: 116.3974, lat: 39.8942 },
                { id: 4, name: 'ZJ测试3332', lng: 116.4274, lat: 39.9242 }
            ];

            cameraPositions.forEach(camera => {
                const marker = new AMap.Marker({
                    position: [camera.lng, camera.lat],
                    title: camera.name,
                    icon: new AMap.Icon({
                        size: new AMap.Size(32, 32),
                        image: 'data:image/svg+xml;base64,' + btoa(`
                            <svg width="32" height="32" viewBox="0 0 32 32" xmlns="http://www.w3.org/2000/svg">
                                <circle cx="16" cy="16" r="12" fill="#ff4444" stroke="#fff" stroke-width="2"/>
                                <circle cx="16" cy="16" r="6" fill="#fff"/>
                            </svg>
                        `)
                    })
                });

                // 添加点击事件
                marker.on('click', () => {
                    this.showCameraInfo(camera);
                });

                this.map.add(marker);
                this.markers.push(marker);
            });
        },
        
        showCameraInfo(camera) {
            const infoWindow = new AMap.InfoWindow({
                content: `
                    <div style="color: white; padding: 10px;">
                        <h4>${camera.name}</h4>
                        <p>经度: ${camera.lng}</p>
                        <p>纬度: ${camera.lat}</p>
                        <button onclick="this.playCamera(${camera.id})" style="background: #409eff; color: white; border: none; padding: 5px 10px; border-radius: 3px; cursor: pointer;">播放视频</button>
                    </div>
                `,
                offset: new AMap.Pixel(0, -30)
            });
            
            infoWindow.open(this.map, [camera.lng, camera.lat]);
        },
        
        async loadCameras() {
            try {
                // 这里应该从后端API加载摄像头列表
                console.log('加载摄像头列表...');
            } catch (error) {
                console.error('加载摄像头失败:', error);
            }
        },
        
        async playCamera(camera) {
            try {
                console.log(`播放摄像头: ${camera.name}`);
                // 这里应该启动视频流播放
            } catch (error) {
                console.error('播放失败:', error);
            }
        },
        
        stopCamera(cameraId) {
            console.log(`停止摄像头: ${cameraId}`);
            // 这里应该停止视频流
        }
    }
}
</script>

<style scoped>
.monitoring-dashboard {
    width: 100%;
    height: 100vh;
    background: linear-gradient(135deg, #0c1426 0%, #1a2332 100%);
    color: #ffffff;
    font-family: 'Microsoft YaHei', sans-serif;
    overflow: hidden;
}

.header {
    height: 60px;
    background: rgba(0, 0, 0, 0.8);
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 20px;
    border-bottom: 1px solid #2a3441;
}

.header h1 {
    margin: 0;
    font-size: 24px;
    font-weight: bold;
    color: #ffffff;
}

.header-actions {
    display: flex;
    align-items: center;
    gap: 20px;
}

.user-info, .app-download {
    color: #409eff;
    cursor: pointer;
    font-size: 14px;
}

.main-content {
    display: flex;
    height: calc(100vh - 60px);
}

.sidebar {
    width: 200px;
    background: rgba(0, 0, 0, 0.6);
    border-right: 1px solid #2a3441;
    padding: 20px 0;
}

.nav-item {
    display: flex;
    align-items: center;
    padding: 15px 20px;
    cursor: pointer;
    transition: all 0.3s;
    border-left: 3px solid transparent;
}

.nav-item:hover {
    background: rgba(64, 158, 255, 0.1);
}

.nav-item.active {
    background: rgba(64, 158, 255, 0.2);
    border-left-color: #409eff;
    color: #409eff;
}

.nav-item i {
    width: 20px;
    height: 20px;
    margin-right: 10px;
    background: #666;
}

.nav-item.active i {
    background: #409eff;
}

.content-area {
    flex: 1;
    display: flex;
    padding: 20px;
    gap: 20px;
}

.left-panel {
    width: 400px;
    display: flex;
    flex-direction: column;
    gap: 20px;
}

.right-panel {
    flex: 1;
    position: relative;
}

.time-display {
    background: rgba(0, 0, 0, 0.6);
    padding: 20px;
    border-radius: 8px;
    border: 1px solid #2a3441;
}

.current-time {
    font-size: 32px;
    font-weight: bold;
    color: #409eff;
    margin-bottom: 5px;
}

.current-date {
    font-size: 16px;
    color: #ccc;
}

.device-summary-card, .scene-table-card {
    background: rgba(0, 0, 0, 0.6);
    padding: 20px;
    border-radius: 8px;
    border: 1px solid #2a3441;
}

.card-title {
    font-size: 18px;
    font-weight: bold;
    margin-bottom: 15px;
    color: #ffffff;
}

.device-count {
    font-size: 48px;
    font-weight: bold;
    color: #409eff;
    text-align: center;
    margin-bottom: 15px;
}

.device-types {
    display: flex;
    justify-content: space-between;
}

.device-type {
    text-align: center;
}

.type-name {
    display: block;
    font-size: 14px;
    color: #ccc;
    margin-bottom: 5px;
}

.type-count {
    display: block;
    font-size: 20px;
    font-weight: bold;
    color: #ffffff;
}

.scene-table {
    width: 100%;
    border-collapse: collapse;
}

.scene-table th,
.scene-table td {
    padding: 8px;
    text-align: center;
    border: 1px solid #2a3441;
}

.scene-table th {
    background: rgba(64, 158, 255, 0.2);
    color: #409eff;
}

.video-panel {
    background: rgba(0, 0, 0, 0.6);
    padding: 20px;
    border-radius: 8px;
    border: 1px solid #2a3441;
    flex: 1;
}

.panel-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 15px;
}

.panel-title {
    font-size: 18px;
    font-weight: bold;
    color: #ffffff;
}

.more-videos {
    color: #409eff;
    cursor: pointer;
    font-size: 14px;
}

.video-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 15px;
}

.video-item {
    background: #000;
    border-radius: 6px;
    overflow: hidden;
}

.video-container {
    position: relative;
    width: 100%;
    height: 120px;
}

.video-stream {
    width: 100%;
    height: 100%;
    object-fit: cover;
}

.video-controls {
    position: absolute;
    bottom: 5px;
    left: 5px;
    display: flex;
    gap: 5px;
}

.control-btn {
    background: rgba(64, 158, 255, 0.8);
    color: white;
    border: none;
    padding: 3px 8px;
    border-radius: 3px;
    font-size: 12px;
    cursor: pointer;
}

.video-info {
    padding: 8px;
    font-size: 12px;
    color: #ccc;
    text-align: center;
}

.map-container {
    width: 100%;
    height: 100%;
    border-radius: 8px;
    overflow: hidden;
}

.gas-monitoring-table, .alarm-table {
    position: absolute;
    background: rgba(0, 0, 0, 0.8);
    border-radius: 6px;
    border: 1px solid #2a3441;
    max-width: 400px;
}

.gas-monitoring-table {
    top: 20px;
    right: 20px;
}

.alarm-table {
    bottom: 20px;
    left: 20px;
}

.table-header {
    padding: 10px 15px;
    background: rgba(64, 158, 255, 0.2);
    color: #409eff;
    font-weight: bold;
    border-bottom: 1px solid #2a3441;
}

.data-table {
    width: 100%;
    border-collapse: collapse;
}

.data-table th,
.data-table td {
    padding: 8px 12px;
    text-align: left;
    border-bottom: 1px solid #2a3441;
    font-size: 12px;
}

.data-table th {
    background: rgba(0, 0, 0, 0.5);
    color: #409eff;
    font-weight: bold;
}

.data-table td {
    color: #ffffff;
}

.status-online {
    color: #67c23a !important;
}

.alarm-type {
    color: #f56c6c !important;
}

.handle-btn {
    background: #409eff;
    color: white;
    border: none;
    padding: 4px 8px;
    border-radius: 3px;
    font-size: 12px;
    cursor: pointer;
}

.handle-btn:hover {
    background: #66b1ff;
}

/* 响应式设计 */
@media (max-width: 1200px) {
    .content-area {
        flex-direction: column;
    }
    
    .left-panel {
        width: 100%;
        flex-direction: row;
        overflow-x: auto;
    }
    
    .right-panel {
        height: 400px;
    }
}
</style>
