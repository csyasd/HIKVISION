 <template>
    <div class="video-container">
        <h2>实时监控</h2>
        
        <!-- 摄像头网格布局 -->
        <div class="cameras-grid" v-if="cameras.length > 0">
            <div 
                v-for="camera in cameras" 
                :key="camera.Id" 
                class="camera-item"
                :class="{ 'camera-error': cameraErrors[camera.Id] }"
            >
                <div class="camera-header">
                    <h3>{{ camera.Name || '未命名摄像头' }}</h3>
                    <div class="camera-info">
                        <span class="camera-ip">{{ camera.IP }}</span>
                        <span 
                            class="camera-status" 
                            :class="getCameraStatusClass(camera.Id)"
                        >
                            {{ getCameraStatus(camera.Id) }}
                        </span>
                    </div>
                </div>
                
                <!-- 视频播放器（FLV直播模式） -->
                <div class="video-wrapper">
                    <video
                        :id="`videoPlayer_${camera.Id}`"
                        class="video-player video-live"
                        muted
                        playsinline
                        autoplay
                        style="width: 100%; height: 400px; background: #000; object-fit: fill;">
                    </video>
                    <div class="live-badge">直播</div>
                    <!-- flv.js加载状态提示 -->
                    <div v-if="!flvjsLoaded" class="error-overlay">
                        <div class="error-content">
                            <i class="el-icon-warning"></i>
                            <h4>flv.js 播放器未加载</h4>
                            <p>请检查网络连接或刷新页面重试</p>
                            <el-button @click="refreshPage" type="primary" size="mini">刷新页面</el-button>
                        </div>
                    </div>
                </div>
                
                <div class="camera-controls">
                    <el-button 
                        @click="playCameraStream(camera)" 
                        type="success" 
                        size="mini"
                        :disabled="!flvjsLoaded"
                    >
                        播放
                    </el-button>
                    <el-button 
                        @click="stopCameraStream(camera.Id)" 
                        type="danger" 
                        size="mini"
                    >
                        停止
                    </el-button>
                </div>
                
                <!-- 云台控制 -->
                <div class="ptz-panel">
                    <div class="ptz-title">
                        云台控制 
                        <span class="ptz-speed-label">
                            速度: <el-slider v-model="ptzSpeed" :min="1" :max="7" style="width: 60px; display: inline-block;"></el-slider>
                        </span>
                    </div>
                    
                    <!-- 方向控制 -->
                    <div class="ptz-grid">
                        <div></div>
                        <button class="ptz-btn" @mousedown="ptz(camera, 21, 0)" @mouseup="ptz(camera, 21, 1)" title="上">↑</button>
                        <div></div>
                        <button class="ptz-btn" @mousedown="ptz(camera, 23, 0)" @mouseup="ptz(camera, 23, 1)" title="左">←</button>
                        <div class="ptz-center">⊙</div>
                        <button class="ptz-btn" @mousedown="ptz(camera, 24, 0)" @mouseup="ptz(camera, 24, 1)" title="右">→</button>
                        <div></div>
                        <button class="ptz-btn" @mousedown="ptz(camera, 22, 0)" @mouseup="ptz(camera, 22, 1)" title="下">↓</button>
                        <div></div>
                    </div>
                    
                    <!-- 其他控制 -->
                    <div class="ptz-extra">
                        <button class="ptz-small-btn" @mousedown="ptz(camera, 11, 0)" @mouseup="ptz(camera, 11, 1)">放大+</button>
                        <button class="ptz-small-btn" @mousedown="ptz(camera, 12, 0)" @mouseup="ptz(camera, 12, 1)">缩小-</button>
                        <button class="ptz-small-btn" @mousedown="ptz(camera, 13, 0)" @mouseup="ptz(camera, 13, 1)">近焦</button>
                        <button class="ptz-small-btn" @mousedown="ptz(camera, 14, 0)" @mouseup="ptz(camera, 14, 1)">远焦</button>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- 加载状态 -->
        <div v-else-if="loading" class="loading">
            <i class="el-icon-loading"></i>
            <span>正在加载摄像头列表...</span>
        </div>
        
        <!-- 无摄像头提示 -->
        <div v-else class="no-cameras">
            <i class="el-icon-video-camera"></i>
            <p>暂无摄像头数据</p>
            <el-button @click="refreshCameras" type="primary">重新加载</el-button>
        </div>
        
        <div id="log" class="log" style="display: none;"></div>

        <!-- 底部实时数据部分 -->
        <div class="bottom-data-sections">
            <!-- 气体监测部分 -->
            <div class="data-block">
                <div class="block-header">气体监测实时数据</div>
                <el-table :data="gasMonitoringData" border style="width: 100%" size="small">
                    <el-table-column prop="DeviceName" label="设备名称及型号"></el-table-column>
                    <el-table-column prop="WorkOrderCode" label="工单编号"></el-table-column>
                    <el-table-column prop="GasName" label="气体名称"></el-table-column>
                    <el-table-column prop="GasValue" label="检测数值"></el-table-column>
                    <el-table-column prop="Status" label="状态">
                        <template slot-scope="scope">
                            <span style="color: #67c23a; font-weight: bold;">{{ scope.row.Status }}</span>
                        </template>
                    </el-table-column>
                </el-table>
            </div>

            <!-- 手环信息部分 -->
            <div class="data-block">
                <div class="block-header">手环实时信息</div>
                <el-table :data="braceletInfoData" border style="width: 100%" size="small">
                    <el-table-column prop="DeviceName" label="设备名称及型号"></el-table-column>
                    <el-table-column prop="WorkOrderCode" label="工单编号"></el-table-column>
                    <el-table-column prop="WorkerName" label="工人姓名"></el-table-column>
                    <el-table-column prop="HeartRate" label="心率"></el-table-column>
                    <el-table-column label="进离场状态">
                        <template slot-scope="scope">
                            <span :style="{ color: (scope.row.EntryExitStatus === '进入' || scope.row.EntryExitStatus === '刷卡成功') ? '#67c23a' : '#909399', fontWeight: 'bold' }">
                                {{ scope.row.EntryExitStatus }}
                            </span>
                        </template>
                    </el-table-column>
                    <el-table-column prop="EntryTime" label="进场时间"></el-table-column>
                    <el-table-column prop="ExitTime" label="出场时间"></el-table-column>
                </el-table>
            </div>
        </div>
    </div>
</template>

<script>
import { PTZControl, GetRealtimeGasData, GetRealtimeBraceletInfo, SelectALLDevice } from '../api/api'

export default {
    data() {
        return {
            cameras: [],
            players: {},  // FLV播放器实例
            cameraErrors: {},
            streamStatus: {},
            loading: false,
            statusText: '正在初始化...',
            API_BASE: window.location.hostname === '127.0.0.1' ? 'http://127.0.0.1:5002' : 'http://localhost:5002',
            ptzSpeed: 4,  // 云台速度 1-7
            flvjsLoaded: false,  // flv.js加载状态
            gasMonitoringData: [],
            braceletInfoData: [],
            devices: [],
            refreshTimer: null
        }
    },
    mounted() {
        // 检查flv.js加载状态
        this.checkFlvjsStatus();
        // 加载摄像头列表
        this.loadCameras();
        // 加载实时数据
        this.fetchRealtimeData();
        this.refreshTimer = setInterval(this.fetchRealtimeData, 5000);
    },
    beforeDestroy() {
        // 清理所有播放器
        Object.keys(this.players).forEach(cameraId => {
            this.stopCameraStream(cameraId);
        });
        if (this.refreshTimer) {
            clearInterval(this.refreshTimer);
        }
    },
    methods: {
        async fetchRealtimeData() {
            try {
                const [gasRes, braceletRes, deviceRes] = await Promise.all([
                    GetRealtimeGasData(),
                    GetRealtimeBraceletInfo(),
                    SelectALLDevice()
                ]);
                
                if (gasRes && Array.isArray(gasRes)) {
                    this.gasMonitoringData = gasRes;
                }
                
                if (braceletRes && Array.isArray(braceletRes)) {
                    this.braceletInfoData = braceletRes;
                }

                if (deviceRes) {
                    this.devices = deviceRes;
                }
            } catch (error) {
                console.error('获取监控页面实时数据失败:', error);
            }
        },
        // 加载摄像头列表
        async loadCameras() {
            try {
                this.loading = true;
                this.statusText = '正在加载摄像头列表...';
                this.log('开始加载摄像头列表');
                
                const response = await fetch(`${this.API_BASE}/Camera/All`);
                
                if (!response.ok) {
                    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
                }
                
                const cameras = await response.json();
                this.cameras = cameras || [];
                
                this.log(`加载到 ${this.cameras.length} 个摄像头`);
                this.statusText = `找到 ${this.cameras.length} 个摄像头`;
                
                if (this.cameras.length > 0) {
                    // 等待DOM更新后初始化播放器
                    this.$nextTick(() => {
                        this.initAllPlayers();
                        this.autoPlayAll();
                    });
                } else {
                    this.statusText = '未找到摄像头';
                }
                
            } catch (error) {
                this.log(`加载摄像头列表失败: ${error.message}`);
                this.statusText = `加载失败: ${error.message}`;
            } finally {
                this.loading = false;
            }
        },
        
        // 初始化所有播放器
        initAllPlayers() {
            // FLV播放器按需创建，不需要预先初始化
            this.log(`摄像头准备就绪（FLV模式）`);
        },
        
        // 自动播放所有摄像头
        async autoPlayAll() {
            this.log('请手动点击播放按钮');
            this.statusText = `已加载 ${this.cameras.length} 个摄像头，请点击"播放"按钮`;
        },
        
        // 播放指定摄像头流（FLV方式 - 低延迟）
        async playCameraStream(camera) {
            try {
                this.log(`开始播放摄像头: ${camera.Name}(${camera.IP})`);
                
                // 检查flv.js是否加载
                if (!this.flvjsLoaded || typeof flvjs === 'undefined' || flvjs === null) {
                    this.$message.error('flv.js播放器未加载，请刷新页面重试');
                    this.log('❌ flv.js未定义或加载失败');
                    this.log('💡 解决方案: 1. 检查网络连接 2. 刷新页面 3. 检查防火墙设置');
                    // 重新检查状态
                    this.checkFlvjsStatus();
                    return;
                }
                
                // 检查浏览器支持
                if (!flvjs.isSupported()) {
                    this.$message.error('当前浏览器不支持FLV播放，请使用Chrome、Firefox或Edge浏览器');
                    this.log('❌ 浏览器不支持flv.js');
                    return;
                }
                
                const videoId = `videoPlayer_${camera.Id}`;
                const videoElement = document.getElementById(videoId);
                
                if (!videoElement) {
                    throw new Error('视频元素未找到');
                }
                
                // 如果已有播放器，先停止
                if (this.players[camera.Id]) {
                    this.stopCameraStream(camera.Id);
                    await new Promise(resolve => setTimeout(resolve, 300));
                }
                
                // 构造FLV流地址
                const flvUrl = `${this.API_BASE}/api/HK/flv-stream/${camera.Id}`;
                this.log(`FLV地址: ${flvUrl}`);
                
                // 创建flv.js播放器
                const flvPlayer = flvjs.createPlayer({
                    type: 'flv',
                    url: flvUrl,
                    isLive: true,
                    hasAudio: false
                }, {
                    enableWorker: false,
                    enableStashBuffer: false,
                    stashInitialSize: 128,
                    // 低延迟配置
                    autoCleanupSourceBuffer: true,
                    autoCleanupMaxBackwardDuration: 3,
                    autoCleanupMinBackwardDuration: 2,
                    liveBufferLatencyChasing: true,
                    liveBufferLatencyChasingOnPaused: false,
                    liveBufferLatencyMaxLatency: 1.5,
                    liveBufferLatencyMinRemain: 0.3
                });
                
                flvPlayer.attachMediaElement(videoElement);
                
                // 绑定事件
                flvPlayer.on(flvjs.Events.ERROR, (errorType, errorDetail) => {
                    this.log(`❌ FLV播放错误: ${errorType} - ${errorDetail}`);
                    this.$message.error(`播放失败: ${errorDetail}`);
                    this.$set(this.cameraErrors, camera.Id, true);
                });
                
                flvPlayer.on(flvjs.Events.LOADING_COMPLETE, () => {
                    this.log(`FLV加载完成`);
                });
                
                // 加载并播放
                flvPlayer.load();
                flvPlayer.play().then(() => {
                    this.log(`✅ ${camera.Name} 播放成功（FLV模式，延迟1-2秒）⚡`);
                    this.$message.success(`${camera.Name} 播放成功`);
                    this.$set(this.cameraErrors, camera.Id, false);
                }).catch(err => {
                    this.log(`❌ 播放失败: ${err}`);
                    this.$message.error(`播放失败`);
                    this.$set(this.cameraErrors, camera.Id, true);
                });
                
                // 保存播放器实例
                this.players[camera.Id] = flvPlayer;
                
            } catch (error) {
                this.log(`播放失败: ${error.message}`);
                this.$message.error(`播放失败: ${error.message}`);
                this.$set(this.cameraErrors, camera.Id, true);
            }
        },

        /* WebRTC功能暂时禁用（与Video.js冲突）
        async playWebRTC(camera) {
            try {
                this.log(`${camera.Name} - WebRTC模式（延迟 < 500ms）`);
                
                // 销毁Video.js播放器（WebRTC需要原生video元素）
                const player = this.players[camera.Id];
                if (player && player.dispose) {
                    player.dispose();
                    delete this.players[camera.Id];
                    this.log(`${camera.Name} 销毁Video.js播放器`);
                }
                
                // 获取原生video元素
                const videoElement = document.getElementById(`videoPlayer_${camera.Id}`);
                if (!videoElement) {
                    throw new Error('视频元素未找到');
                }
                
                // 重置video元素（清除Video.js的影响）
                videoElement.className = '';
                videoElement.removeAttribute('data-setup');
                videoElement.controls = true;
                videoElement.muted = true;
                videoElement.playsinline = true;
                videoElement.autoplay = true;

                // 创建RTCPeerConnection
                const pc = new RTCPeerConnection({ iceServers: [] });

                // 监听连接状态
                pc.onconnectionstatechange = () => {
                    this.log(`${camera.Name} WebRTC: ${pc.connectionState}`);
                    if (pc.connectionState === 'connected') {
                        this.$message.success(`${camera.Name} WebRTC已连接！`);
                        this.$set(this.cameraErrors, camera.Id, false);
                    }
                };

                // 接收视频轨道
                pc.ontrack = (event) => {
                    this.log(`${camera.Name} 接收到视频轨道`);
                    videoElement.srcObject = event.streams[0];
                    // 确保视频播放
                    videoElement.muted = true;  // 静音以允许自动播放
                    videoElement.play().then(() => {
                        this.log(`${camera.Name} WebRTC视频播放中！`);
                        this.$message.success(`${camera.Name} 播放成功（延迟 < 500ms）`);
                    }).catch(e => {
                        this.log(`自动播放受限: ${e.message}，请点击视频播放`);
                    });
                };

                // 创建Offer
                const offer = await pc.createOffer({
                    offerToReceiveVideo: true,
                    offerToReceiveAudio: false
                });
                await pc.setLocalDescription(offer);

                // 与SRS交换SDP
                const response = await fetch(`${this.srsApiUrl}/rtc/v1/play/`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        api: `${this.srsApiUrl}/rtc/v1/play/`,
                        streamurl: `webrtc://localhost/live/camera_${camera.Id}`,
                        sdp: offer.sdp
                    })
                });

                const data = await response.json();
                if (data.code !== 0) {
                    throw new Error(`SRS错误: ${data.code}`);
                }

                // 设置远程描述
                await pc.setRemoteDescription({
                    type: 'answer',
                    sdp: data.sdp
                });

                // 保存连接
                this.peerConnections[camera.Id] = pc;
                this.log(`${camera.Name} WebRTC播放成功！`);
                
            } catch (error) {
                this.log(`WebRTC失败: ${error.message}`);
            }
        },
        */
        
        // 停止指定摄像头流（FLV方式）
        stopCameraStream(cameraId) {
            try {
                const flvPlayer = this.players[cameraId];
                if (flvPlayer) {
                    flvPlayer.pause();
                    flvPlayer.unload();
                    flvPlayer.detachMediaElement();
                    flvPlayer.destroy();
                    delete this.players[cameraId];
                    this.log(`摄像头 ${cameraId} 已停止`);
                    this.$set(this.cameraErrors, cameraId, false);
                }
            } catch (error) {
                this.log(`停止失败: ${error.message}`);
                delete this.players[cameraId];
            }
        },
        
        // 刷新摄像头列表
        refreshCameras() {
            this.log('重新加载摄像头列表...');
            this.loadCameras();
        },
        
        // 检查所有摄像头状态
        async checkAllStatus() {
            try {
                this.log('检查所有摄像头流状态...');
                
                let activeCount = 0;
                for (const camera of this.cameras) {
                    try {
                        const response = await fetch(`${this.API_BASE}/api/HK/camera-hls/${camera.Id}`);
                        const result = await response.json();
                        
                        if (response.ok && result.exists) {
                            activeCount++;
                            this.$set(this.streamStatus, camera.Id, { IsActive: true });
                        } else {
                            this.$set(this.streamStatus, camera.Id, { IsActive: false, LastError: result.message });
                        }
                    } catch (error) {
                        this.$set(this.streamStatus, camera.Id, { IsActive: false, LastError: error.message });
                    }
                }
                
                this.log(`流状态检查完成: ${activeCount}/${this.cameras.length} 个流可用`);
                this.statusText = `活动流: ${activeCount}/${this.cameras.length}`;
                
            } catch (error) {
                this.log(`状态检查失败: ${error.message}`);
                this.statusText = `状态检查失败: ${error.message}`;
            }
        },
        
        // 获取摄像头状态（根据设备的离在线状态）
        getCameraStatus(cameraId) {
            const camera = this.cameras.find(c => c.Id === cameraId);
            if (!camera || !camera.DeviceId) return '未知';

            const device = this.devices.find(d => d.Id === camera.DeviceId);
            if (!device) return '未知';

            return device.OnlineStatus || '离线';
        },
        
        // 获取摄像头状态样式类
        getCameraStatusClass(cameraId) {
            const status = this.getCameraStatus(cameraId);
            if (status === '在线') {
                return 'status-online';
            } else if (status === '离线') {
                return 'status-offline';
            } else {
                return 'status-unknown';
            }
        },
        
        // 日志函数
        log(message) {
            const time = new Date().toLocaleTimeString();
            const logDiv = document.getElementById('log');
            if (logDiv) {
                logDiv.innerHTML += `<div>[${time}] ${message}</div>`;
                logDiv.scrollTop = logDiv.scrollHeight;
            }
            console.log(message);
        },
        
        clearLog() {
            const logDiv = document.getElementById('log');
            if (logDiv) {
                logDiv.innerHTML = '';
            }
        },
        
        // 刷新页面
        refreshPage() {
            window.location.reload();
        },
        
        // 检查flv.js加载状态
        checkFlvjsStatus() {
            // 延迟检查，确保脚本完全加载
            setTimeout(() => {
                this.flvjsLoaded = typeof flvjs !== 'undefined' && flvjs !== null;
                this.log(`flv.js状态检查: ${this.flvjsLoaded ? '✅ 已加载' : '❌ 未加载'}`);
                
                if (this.flvjsLoaded) {
                    this.log('✅ FLV播放器已就绪');
                    this.log('浏览器支持:', flvjs.isSupported() ? '✅' : '❌');
                } else {
                    this.log('❌ flv.js未定义，视频播放功能将不可用');
                }
            }, 1000);
        },

        // 云台控制 - 简化版本
        async ptz(camera, command, stop) {
            try {
                const result = await PTZControl({
                    ip: camera.IP,
                    port: 8000,
                    username: 'admin',
                    password: 'wzxc2025',
                    channel: 1,
                    command: command,
                    stop: stop,
                    speed: this.ptzSpeed
                });
                
                if (result && result.success) {
                    this.log(`云台控制: ${stop ? '停止' : '执行'} 命令${command}`);
                } else {
                    this.$message.error('云台控制失败');
                }
            } catch (error) {
                console.error('PTZ错误:', error);
            }
        }
    }
}
</script>

<style scoped>
.video-container {
    padding: 20px;
    background: white;
    border-radius: 8px;
    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

.video-container h2 {
    text-align: center;
    color: #333;
    margin-bottom: 20px;
}

/* 底部数据区块样式 */
.bottom-data-sections {
    margin-top: 30px;
    display: flex;
    flex-direction: column;
    gap: 30px;
}

.data-block {
    background: #fff;
    border-radius: 8px;
    overflow: hidden;
}

.block-header {
    padding: 12px 15px;
    background: #f5f7fa;
    border-left: 4px solid #409eff;
    font-weight: bold;
    color: #333;
    margin-bottom: 10px;
    font-size: 16px;
}

.controls {
    margin: 20px 0;
    text-align: center;
}

.controls .el-button {
    margin: 5px;
}

/* 摄像头网格布局 */
.cameras-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
    gap: 20px;
    margin: 20px 0;
}

.camera-item {
    border: 2px solid #e9ecef;
    border-radius: 8px;
    padding: 15px;
    background: #fff;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
    transition: all 0.3s ease;
}

.camera-item:hover {
    border-color: #409eff;
    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
}

.camera-item.camera-error {
    border-color: #f56c6c;
    background: #fef0f0;
}

.camera-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 10px;
    padding-bottom: 10px;
    border-bottom: 1px solid #eee;
}

.camera-header h3 {
    margin: 0;
    color: #333;
    font-size: 16px;
    font-weight: 600;
}

.camera-info {
    display: flex;
    flex-direction: column;
    align-items: flex-end;
    gap: 4px;
}

.camera-ip {
    font-size: 12px;
    color: #666;
    font-family: 'Courier New', monospace;
}

.camera-status {
    font-size: 11px;
    padding: 2px 6px;
    border-radius: 10px;
    font-weight: 600;
    text-transform: uppercase;
}

.status-online {
    background: #f0f9ff;
    color: #1e40af;
}

.status-error {
    background: #fef2f2;
    color: #dc2626;
}

.status-offline {
    background: #f5f5f5;
    color: #6b7280;
}

.status-unknown {
    background: #fefce8;
    color: #ca8a04;
}

.video-wrapper {
    margin: 10px 0;
    background: #000;
    border-radius: 6px;
    overflow: hidden;
    position: relative;
}

.video-player {
    width: 100% !important;
    height: 400px !important;
    background: #000;
    border-radius: 6px;
}

/* 隐藏视频控制条 */
.video-live::-webkit-media-controls {
    display: none !important;
}
.video-live::-webkit-media-controls-enclosure {
    display: none !important;
}

/* 直播标识 */
.live-badge {
    position: absolute;
    top: 10px;
    left: 10px;
    background: #ff4444;
    color: #fff;
    padding: 4px 12px;
    border-radius: 4px;
    font-size: 12px;
    font-weight: bold;
    z-index: 10;
    box-shadow: 0 2px 4px rgba(0,0,0,0.3);
}

/* 错误提示覆盖层 */
.error-overlay {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.8);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 20;
    border-radius: 6px;
}

.error-content {
    text-align: center;
    color: white;
    padding: 20px;
}

.error-content i {
    font-size: 48px;
    color: #f56c6c;
    margin-bottom: 16px;
    display: block;
}

.error-content h4 {
    margin: 0 0 8px 0;
    color: #f56c6c;
    font-size: 16px;
}

.error-content p {
    margin: 0 0 16px 0;
    color: #ccc;
    font-size: 14px;
}

.live-badge::before {
    content: '● ';
    animation: blink 1.5s infinite;
}

@keyframes blink {
    0%, 100% { opacity: 1; }
    50% { opacity: 0.3; }
}

.camera-controls {
    display: flex;
    justify-content: center;
    gap: 10px;
    margin-top: 10px;
}

.status {
    padding: 12px;
    margin: 10px 0;
    border-radius: 6px;
    background-color: #e1f5fe;
    color: #0277bd;
    text-align: center;
    font-weight: 500;
    border: 1px solid #b3e5fc;
}

.flv-status-success {
    margin-top: 8px;
    color: #67c23a;
    font-weight: 600;
}

.flv-status-error {
    margin-top: 8px;
    color: #f56c6c;
    font-weight: 600;
}

/* 加载和空状态 */
.loading, .no-cameras {
    text-align: center;
    padding: 60px 20px;
    color: #666;
}

.loading i, .no-cameras i {
    font-size: 48px;
    color: #ddd;
    margin-bottom: 16px;
    display: block;
}

.loading span, .no-cameras p {
    font-size: 16px;
    margin: 0 0 20px 0;
}

.log {
    margin-top: 20px;
    padding: 15px;
    background: #f8f9fa;
    border-radius: 6px;
    font-family: 'Courier New', monospace;
    font-size: 12px;
    max-height: 200px;
    overflow-y: auto;
    border: 1px solid #e9ecef;
}

.log div {
    margin-bottom: 2px;
    line-height: 1.4;
}

/* 云台控制样式 */
.ptz-panel {
    margin-top: 15px;
    padding: 12px;
    background: #f5f7fa;
    border-radius: 6px;
}

.ptz-title {
    font-size: 13px;
    font-weight: 600;
    color: #333;
    margin-bottom: 10px;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.ptz-speed-label {
    font-size: 12px;
    font-weight: normal;
    color: #666;
    display: flex;
    align-items: center;
    gap: 5px;
}

.ptz-grid {
    display: grid;
    grid-template-columns: repeat(3, 40px);
    gap: 3px;
    justify-content: center;
    margin-bottom: 10px;
}

.ptz-btn {
    width: 40px;
    height: 40px;
    border: 2px solid #ddd;
    background: white;
    border-radius: 6px;
    cursor: pointer;
    font-size: 18px;
    color: #333;
    transition: all 0.2s;
}

.ptz-btn:hover {
    background: #409eff;
    color: white;
    border-color: #409eff;
}

.ptz-btn:active {
    transform: scale(0.95);
}

.ptz-center {
    width: 40px;
    height: 40px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: #e9ecef;
    border-radius: 6px;
    font-size: 20px;
    color: #999;
}

.ptz-extra {
    display: flex;
    justify-content: center;
    gap: 5px;
    flex-wrap: wrap;
}

.ptz-small-btn {
    padding: 5px 10px;
    border: 1px solid #ddd;
    background: white;
    border-radius: 4px;
    cursor: pointer;
    font-size: 12px;
    transition: all 0.2s;
}

.ptz-small-btn:hover {
    background: #67c23a;
    color: white;
    border-color: #67c23a;
}

.ptz-small-btn:active {
    transform: scale(0.95);
}

/* 响应式设计 */
@media (max-width: 1200px) {
    .cameras-grid {
        grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
        gap: 15px;
    }
}

@media (max-width: 768px) {
    .video-container {
        padding: 15px;
    }
    
    .cameras-grid {
        grid-template-columns: 1fr;
        gap: 15px;
    }
    
    .video-js {
        height: 220px !important;
    }
    
    .controls .el-button {
        margin: 3px;
        font-size: 12px;
    }
    
    .camera-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 8px;
    }
    
    .camera-info {
        align-items: flex-start;
    }
}

@media (max-width: 480px) {
    .video-container {
        padding: 10px;
    }
    
    .video-js {
        height: 180px !important;
    }
    
    .camera-item {
        padding: 10px;
    }
    
    .camera-controls .el-button {
        font-size: 11px;
        padding: 5px 10px;
    }
}
</style>
