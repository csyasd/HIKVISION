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
                    <!-- 视频容器 -->
                    <div 
                        :id="`play_window_${camera.Id}`"
                        class="video-player video-live"
                        style="width: 100%; height: 400px; background: #000;">
                    </div>
                    <div class="live-badge">直播</div>
                    <!-- H5 Player加载状态提示 -->
                    <div v-if="!h5PlayerLoaded" class="error-overlay">
                        <div class="error-content">
                            <i class="el-icon-warning"></i>
                            <h4>播放器插件未加载</h4>
                            <p>请检查网络连接或刷新页面重试</p>
                            <el-button @click="refreshPage" type="primary" size="mini">刷新页面</el-button>
                        </div>
                    </div>
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
            h5PlayerLoaded: false,  // 播放器加载状态
            gasMonitoringData: [],
            braceletInfoData: [],
            devices: [],
            refreshTimer: null
        }
    },
    mounted() {
        // 检查播放器加载状态
        this.checkPlayerStatus();
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
            this.log('开始尝试自动播放所有摄像头...');
            this.cameras.forEach(camera => {
                this.playCameraStream(camera);
            });
        },
        
        // 播放指定摄像头流（H5 Player方式）
        async playCameraStream(camera) {
            try {
                const playWindowId = `play_window_${camera.Id}`;
                this.log(`准备播放摄像头: ${camera.Name}(${camera.IP}), 窗口: ${playWindowId}`);
                
                // 检查插件是否加载
                if (typeof JSPlugin === 'undefined') {
                    // 尝试等待插件加载
                     const loaded = await this.waitForPlugin();
                    if (!loaded) {
                        this.$message.error('播放器插件加载失败，请刷新页面重试');
                        this.log('❌ JSPlugin加载超时');
                        return;
                    }
                }
                
                const playWindow = document.getElementById(playWindowId);
                if (!playWindow) {
                     this.log(`⚠️ 未找到播放窗口元素: ${playWindowId}`);
                    return;
                }
                
                // 如果已有播放器，先停止
                if (this.players[camera.Id]) {
                    this.stopCameraStream(camera.Id);
                    await new Promise(resolve => setTimeout(resolve, 300));
                }
                
                // 构造流地址
                const playUrl = `${this.API_BASE}/api/HK/flv-stream/${camera.Id}`; 
                // 注意：这里沿用了 FLV 的 URL 逻辑，假设 H5 Player 也能处理该地址或后端做了适配。
                // 实际集成时需确认 H5 Player 需要的 URL 协议 (wss/rtsp/http-flv 等)
                
                 this.log(`播放地址: ${playUrl}`);

                // 创建H5播放器
                const player = new JSPlugin({
                    szId: playWindowId,
                    szBasePath: "./static/h5player/",
                    iMaxSplit: 1,
                    iCurrentSplit: 1,
                    openDebug: true,
                    oStyle: {
                        borderSelect: '#000'
                    }
                });
                
                this.players[camera.Id] = player;

                // 播放
                // mode: 0 (MSE), 1 (Decoder)
                player.JS_Play(playUrl, { playURL: playUrl, mode: 0 }, 0).then(
                    () => {
                        this.log(`✅ ${camera.Name} 播放指令发送成功`);
                        this.$set(this.cameraErrors, camera.Id, false);
                        player.JS_Resize();
                    },
                    (err) => {
                         this.log(`❌ 播放失败: ${err}`);
                         this.$message.error(`播放失败`);
                         this.$set(this.cameraErrors, camera.Id, true);
                    }
                );
                
            } catch (error) {
                this.log(`播放初始化异常: ${error.message}`);
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
        
        // 停止指定摄像头流（H5 Player方式）
        stopCameraStream(cameraId) {
            try {
                const player = this.players[cameraId];
                if (player) {
                    player.JS_Stop(0).then(() => {
                        this.log(`摄像头 ${cameraId} 已停止`);
                    }, (err) => {
                        this.log(`停止失败: ${err}`);
                    });
                     // 移除引用
                    delete this.players[cameraId];
                    this.$set(this.cameraErrors, cameraId, false);
                }
            } catch (error) {
                this.log(`停止异常: ${error.message}`);
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
        
        // 检查播放器插件状态
        checkPlayerStatus() {
            // 延迟检查，确保脚本完全加载
            setTimeout(() => {
                this.h5PlayerLoaded = typeof JSPlugin !== 'undefined';
                this.log(`H5Player插件检查: ${this.h5PlayerLoaded ? '✅ 已加载' : '❌ 未加载'}`);
                
                if (this.h5PlayerLoaded) {
                    this.log('✅ H5播放器已就绪');
                }
            }, 1000);
        },

        // 等待插件加载
        async waitForPlugin() {
            return new Promise((resolve) => {
                if (typeof JSPlugin !== 'undefined') {
                    this.h5PlayerLoaded = true;
                    resolve(true);
                    return;
                }
                
                this.log('等待H5Player插件加载...');
                let count = 0;
                const interval = setInterval(() => {
                    count++;
                    if (typeof JSPlugin !== 'undefined') {
                        clearInterval(interval);
                        this.h5PlayerLoaded = true;
                        this.log('✅ H5Player插件加载成功');
                        resolve(true);
                    }
                    if (count > 50) { // 5秒超时
                        clearInterval(interval);
                        resolve(false);
                    }
                }, 100);
            });
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
    padding: 24px;
    background: transparent;
}

.video-container h2 {
    text-align: center;
    color: var(--text-bright);
    font-size: 28px;
    font-weight: 800;
    margin-bottom: 30px;
    letter-spacing: 2px;
    background: linear-gradient(135deg, #409eff 0%, #7948ea 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    text-shadow: 0 0 20px rgba(64, 158, 255, 0.2);
}

/* 底部数据区块样式 */
.bottom-data-sections {
    margin-top: 40px;
    display: flex;
    flex-direction: column;
    gap: 40px;
}

.data-block {
    background: var(--glass-bg);
    backdrop-filter: blur(20px);
    border-radius: 20px;
    border: 1px solid var(--glass-border);
    overflow: hidden;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
}

.block-header {
    padding: 16px 20px;
    background: linear-gradient(90deg, rgba(64, 158, 255, 0.1), transparent);
    border-left: 4px solid #409eff;
    font-weight: 800;
    color: #409eff;
    font-size: 18px;
    letter-spacing: 1px;
}

/* 摄像头网格布局 */
.cameras-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
    gap: 30px;
    margin: 30px 0;
}

.camera-item {
    border: 1px solid var(--glass-border);
    border-radius: 20px;
    padding: 20px;
    background: var(--glass-bg);
    backdrop-filter: blur(20px);
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.camera-item:hover {
    border-color: rgba(64, 158, 255, 0.3);
    box-shadow: 0 0 20px rgba(64, 158, 255, 0.1);
    transform: translateY(-5px);
}

.camera-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 15px;
    padding-bottom: 15px;
    border-bottom: 1px solid var(--glass-border);
}

.camera-header h3 {
    margin: 0;
    color: var(--text-bright);
    font-size: 18px;
    font-weight: 700;
}

.camera-ip {
    font-size: 13px;
    color: var(--text-muted);
    font-family: 'JetBrains Mono', monospace;
}

.video-wrapper {
    margin: 15px 0;
    background: #000;
    border-radius: 12px;
    overflow: hidden;
    position: relative;
    border: 1px solid rgba(255, 255, 255, 0.05);
}

.video-player {
    width: 100% !important;
    height: 400px !important;
    background: #001;
}

/* 云台控制样式 */
.ptz-panel {
    background: rgba(0, 0, 0, 0.2);
    border-radius: 12px;
    padding: 15px;
    margin-top: 15px;
    border: 1px solid rgba(255, 255, 255, 0.05);
}

.ptz-btn {
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid rgba(255, 255, 255, 0.1);
    color: #fff;
    cursor: pointer;
    border-radius: 8px;
    transition: all 0.2s;
}

.ptz-btn:hover {
    background: rgba(64, 158, 255, 0.2);
    border-color: #409eff;
}

.ptz-small-btn {
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid rgba(255, 255, 255, 0.1);
    color: rgba(255, 255, 255, 0.8);
    padding: 6px 12px;
    border-radius: 6px;
    cursor: pointer;
    font-size: 12px;
    transition: all 0.2s;
}

.ptz-small-btn:hover {
    background: rgba(64, 158, 255, 0.2);
    color: #409eff;
    border-color: #409eff;
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
    color: var(--text-muted);
    font-size: 14px;
}

.camera-controls {
    display: flex;
    justify-content: center;
    gap: 12px;
    margin-top: 15px;
}

.status {
    padding: 12px;
    margin: 10px 0;
    border-radius: 8px;
    background: rgba(64, 158, 255, 0.1);
    color: #409eff;
    text-align: center;
    font-weight: 500;
    border: 1px solid var(--glass-border);
}

.flv-status-success {
    margin-top: 8px;
    color: #67c23a;
    font-weight: 700;
    text-shadow: 0 0 10px rgba(103, 194, 58, 0.3);
}

.flv-status-error {
    margin-top: 8px;
    color: #f56c6c;
    font-weight: 700;
}

/* 加载和空状态 */
.loading, .no-cameras {
    text-align: center;
    padding: 80px 20px;
    color: var(--text-muted);
}

.loading i, .no-cameras i {
    font-size: 56px;
    color: rgba(64, 158, 255, 0.1);
    margin-bottom: 20px;
    display: block;
}

.log {
    margin-top: 30px;
    padding: 20px;
    background: rgba(0, 0, 0, 0.3);
    border-radius: 12px;
    font-family: 'JetBrains Mono', monospace;
    font-size: 12px;
    max-height: 250px;
    overflow-y: auto;
    border: 1px solid var(--glass-border);
    color: var(--text-muted);
}

.log div {
    margin-bottom: 4px;
    line-height: 1.5;
}

/* 云台控制样式补全 */
.ptz-title {
    font-size: 14px;
    font-weight: 700;
    color: var(--text-bright);
    margin-bottom: 15px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    letter-spacing: 1px;
}

.ptz-speed-label {
    font-size: 12px;
    font-weight: normal;
    color: var(--text-muted);
    display: flex;
    align-items: center;
    gap: 8px;
}

.ptz-grid {
    display: grid;
    grid-template-columns: repeat(3, 44px);
    gap: 6px;
    justify-content: center;
    margin-bottom: 15px;
}

.ptz-btn {
    width: 44px;
    height: 44px;
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-radius: 10px;
    cursor: pointer;
    font-size: 18px;
    color: #fff;
    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}

.ptz-btn:hover {
    background: rgba(64, 158, 255, 0.2);
    border-color: #409eff;
    color: #409eff;
}

.ptz-center {
    width: 44px;
    height: 44px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: rgba(255, 255, 255, 0.02);
    border-radius: 10px;
    font-size: 20px;
    color: rgba(255, 255, 255, 0.2);
}

.ptz-extra {
    display: flex;
    justify-content: center;
    gap: 8px;
    flex-wrap: wrap;
}

/* 响应式设计更新 */
@media (max-width: 1200px) {
    .cameras-grid {
        grid-template-columns: repeat(auto-fit, minmax(360px, 1fr));
        gap: 20px;
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
