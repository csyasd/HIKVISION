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

                <!-- 语音对讲 -->
                <div class="intercom-panel">
                    <div class="intercom-title">语音对讲</div>
                    <div class="intercom-controls">
                        <button 
                            class="intercom-btn"
                            :class="getIntercomBtnClass(camera.Id)"
                            @click="toggleIntercom(camera)"
                            :disabled="intercomStatus[camera.Id] === 'connecting'"
                        >
                            <i :class="getIntercomIcon(camera.Id)"></i>
                            <span>{{ getIntercomText(camera.Id) }}</span>
                        </button>
                        <!-- 音量控制 -->
                        <div v-if="intercomStatus[camera.Id] === 'active'" class="intercom-volume">
                            <span class="volume-label">音量:</span>
                            <el-slider 
                                v-model="intercomVolume" 
                                :min="0" 
                                :max="100" 
                                :show-tooltip="false"
                                class="volume-slider"
                                @input="updateVolume"
                            ></el-slider>
                        </div>
                    </div>
                    <!-- 对讲状态提示 -->
                    <div v-if="intercomStatus[camera.Id]" class="intercom-status" :class="'status-' + intercomStatus[camera.Id]">
                        <span v-if="intercomStatus[camera.Id] === 'connecting'">
                            <i class="el-icon-loading"></i> 正在建立对讲连接...
                        </span>
                        <span v-else-if="intercomStatus[camera.Id] === 'active'">
                            <i class="el-icon-microphone"></i> 对讲中 — 请对着麦克风说话
                        </span>
                        <span v-else-if="intercomStatus[camera.Id] === 'error'">
                            <i class="el-icon-warning"></i> {{ intercomError[camera.Id] || '对讲连接失败' }}
                        </span>
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
import { SelectALLCamera, PTZControl, SelectWorkOrder, SelectALLDevice, GetRealtimeGasData, GetRealtimeBraceletInfo, BaseUrl } from '../api/api'

export default {
    data() {
        return {
            cameras: [],
            players: {},  // FLV播放器实例
            cameraErrors: {},
            streamStatus: {},
            loading: false,
            statusText: '正在初始化...',
            API_BASE: BaseUrl.replace(/\/$/, ''),
            ptzSpeed: 4,  // 云台速度 1-7
            h5PlayerLoaded: false,  // 播放器加载状态
            gasMonitoringData: [],
            braceletInfoData: [],
            devices: [],
            refreshTimer: null,
            // 语音对讲相关
            intercomStatus: {},    // idle / connecting / active / error
            intercomError: {},     // 错误消息
            intercomWs: {},        // WebSocket 连接
            intercomAudioCtx: {},  // AudioContext
            intercomStream: {},    // MediaStream
            intercomProcessor: {}, // ScriptProcessorNode
            intercomVolume: 80,    // 播放音量 0-100
            intercomGainNode: {},  // GainNode
            intercomNextPlayTime: {} // 音频调度时间
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
        // 清理所有对讲连接
        this.cameras.forEach(camera => {
            this.stopIntercom(camera.Id);
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
                    this.gasMonitoringData = gasRes.map(item => {
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
                
                 this.log(`播放地址: ${playUrl}`);

                // 创建H5播放器
                const player = new JSPlugin({
                    szId: playWindowId,
                    szBasePath: "/static/h5player/",
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
                    password: 'Cnh321456$',
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
        },

        // ====== 语音对讲功能 ======

        // 切换对讲状态
        toggleIntercom(camera) {
            const status = this.intercomStatus[camera.Id];
            if (status === 'active' || status === 'connecting') {
                this.stopIntercom(camera.Id);
            } else {
                this.startIntercom(camera);
            }
        },

        // 开始对讲
        async startIntercom(camera) {
            const cameraId = camera.Id;
            this.$set(this.intercomStatus, cameraId, 'connecting');
            this.$set(this.intercomError, cameraId, '');
            this.log(`[对讲] 开始连接 ${camera.Name}(${camera.IP})...`);

            try {
                // 1. 请求麦克风权限
                const stream = await navigator.mediaDevices.getUserMedia({
                    audio: {
                        echoCancellation: true,
                        noiseSuppression: true,
                        autoGainControl: true,
                        channelCount: 1
                    }
                });
                this.$set(this.intercomStream, cameraId, stream);

                // 2. 创建 AudioContext（采样率 8000Hz 用于语音通信）
                let audioCtx;
                try {
                    audioCtx = new (window.AudioContext || window.webkitAudioContext)({ sampleRate: 8000 });
                } catch (e) {
                    // 若不支持 8000Hz，使用默认采样率后续手动重采样
                    audioCtx = new (window.AudioContext || window.webkitAudioContext)();
                }
                this.$set(this.intercomAudioCtx, cameraId, audioCtx);

                // 创建 GainNode 控制播放音量
                const gainNode = audioCtx.createGain();
                gainNode.gain.value = this.intercomVolume / 100;
                gainNode.connect(audioCtx.destination);
                this.$set(this.intercomGainNode, cameraId, gainNode);

                // 3. 建立 WebSocket 连接
                const wsProtocol = this.API_BASE.startsWith('https') ? 'wss' : 'ws';
                const wsHost = this.API_BASE.replace(/^https?:\/\//, '');
                const wsUrl = `${wsProtocol}://${wsHost}/api/HK/voice-talk/${cameraId}`;
                this.log(`[对讲] WebSocket: ${wsUrl}`);

                const ws = new WebSocket(wsUrl);
                ws.binaryType = 'arraybuffer';
                this.$set(this.intercomWs, cameraId, ws);

                // 初始化播放调度时间
                this.$set(this.intercomNextPlayTime, cameraId, 0);

                ws.onopen = () => {
                    this.log(`[对讲] WebSocket 已连接`);
                    // 开始采集麦克风音频
                    this.startAudioCapture(cameraId, stream, audioCtx);
                };

                ws.onmessage = (event) => {
                    if (typeof event.data === 'string') {
                        // JSON 控制消息
                        try {
                            const msg = JSON.parse(event.data);
                            this.log(`[对讲] 收到消息: ${msg.type} - ${msg.message || msg.status || ''}`);
                            if (msg.type === 'status' && msg.status === 'connected') {
                                this.$set(this.intercomStatus, cameraId, 'active');
                                this.$message.success('对讲已连接');
                            } else if (msg.type === 'error') {
                                this.$set(this.intercomStatus, cameraId, 'error');
                                this.$set(this.intercomError, cameraId, msg.message);
                                this.$message.error(msg.message);
                            }
                        } catch (e) {
                            console.error('[对讲] 解析消息失败:', e);
                        }
                    } else if (event.data instanceof ArrayBuffer) {
                        // 二进制音频数据（PCM Int16 LE 8000Hz 单声道）
                        this.playReceivedAudio(cameraId, event.data);
                    }
                };

                ws.onerror = (err) => {
                    console.error('[对讲] WebSocket 错误:', err);
                    this.$set(this.intercomStatus, cameraId, 'error');
                    this.$set(this.intercomError, cameraId, 'WebSocket 连接错误');
                };

                ws.onclose = (event) => {
                    this.log(`[对讲] WebSocket 关闭: code=${event.code}, reason=${event.reason}`);
                    if (this.intercomStatus[cameraId] === 'active') {
                        this.$set(this.intercomStatus, cameraId, 'idle');
                        this.$message.info('对讲已结束');
                    }
                    this.cleanupIntercom(cameraId);
                };

            } catch (error) {
                console.error('[对讲] 启动失败:', error);
                this.$set(this.intercomStatus, cameraId, 'error');
                let errorMsg = '启动对讲失败';
                if (error.name === 'NotAllowedError') {
                    errorMsg = '麦克风权限被拒绝，请在浏览器设置中允许麦克风访问';
                } else if (error.name === 'NotFoundError') {
                    errorMsg = '未检测到麦克风设备';
                } else {
                    errorMsg = error.message || errorMsg;
                }
                this.$set(this.intercomError, cameraId, errorMsg);
                this.$message.error(errorMsg);
            }
        },

        // 开始采集麦克风音频
        startAudioCapture(cameraId, stream, audioCtx) {
            const source = audioCtx.createMediaStreamSource(stream);
            
            // 使用 ScriptProcessorNode 处理音频（bufferSize=2048 约 256ms @ 8kHz）
            const processor = audioCtx.createScriptProcessor(2048, 1, 1);
            const targetSampleRate = 8000;
            const actualSampleRate = audioCtx.sampleRate;
            const needResample = Math.abs(actualSampleRate - targetSampleRate) > 100;

            processor.onaudioprocess = (event) => {
                const ws = this.intercomWs[cameraId];
                if (!ws || ws.readyState !== WebSocket.OPEN) return;

                let inputData = event.inputBuffer.getChannelData(0); // Float32Array

                // 如果采样率不是 8000Hz，进行简单降采样
                if (needResample) {
                    const ratio = actualSampleRate / targetSampleRate;
                    const newLength = Math.floor(inputData.length / ratio);
                    const resampled = new Float32Array(newLength);
                    for (let i = 0; i < newLength; i++) {
                        resampled[i] = inputData[Math.floor(i * ratio)];
                    }
                    inputData = resampled;
                }

                // Float32 → Int16 PCM
                const int16Data = new Int16Array(inputData.length);
                for (let i = 0; i < inputData.length; i++) {
                    const s = Math.max(-1, Math.min(1, inputData[i]));
                    int16Data[i] = s < 0 ? s * 0x8000 : s * 0x7FFF;
                }

                // 发送 PCM Int16 二进制数据
                ws.send(int16Data.buffer);
            };

            source.connect(processor);
            // 连接到静音输出（ScriptProcessor 需要连接到 destination 才能工作）
            const silentGain = audioCtx.createGain();
            silentGain.gain.value = 0;
            processor.connect(silentGain);
            silentGain.connect(audioCtx.destination);

            this.$set(this.intercomProcessor, cameraId, { processor, source, silentGain });
            this.log(`[对讲] 麦克风采集已启动 (采样率: ${actualSampleRate}Hz${needResample ? ' → 重采样到 8000Hz' : ''})`);
        },

        // 播放从摄像头接收到的音频
        playReceivedAudio(cameraId, arrayBuffer) {
            const audioCtx = this.intercomAudioCtx[cameraId];
            const gainNode = this.intercomGainNode[cameraId];
            if (!audioCtx || !gainNode) return;

            // PCM Int16 LE → Float32
            const int16Data = new Int16Array(arrayBuffer);
            const float32Data = new Float32Array(int16Data.length);
            for (let i = 0; i < int16Data.length; i++) {
                float32Data[i] = int16Data[i] / 32768.0;
            }

            // 创建 AudioBuffer 并播放
            const audioBuffer = audioCtx.createBuffer(1, float32Data.length, 8000);
            audioBuffer.getChannelData(0).set(float32Data);

            const source = audioCtx.createBufferSource();
            source.buffer = audioBuffer;
            source.connect(gainNode);

            // 调度连续播放
            const currentTime = audioCtx.currentTime;
            let nextTime = this.intercomNextPlayTime[cameraId] || 0;
            if (nextTime < currentTime) {
                nextTime = currentTime + 0.05; // 50ms 缓冲
            }
            source.start(nextTime);
            this.$set(this.intercomNextPlayTime, cameraId, nextTime + audioBuffer.duration);
        },

        // 停止对讲
        stopIntercom(cameraId) {
            this.log(`[对讲] 停止对讲: ${cameraId}`);
            
            // 发送停止命令
            const ws = this.intercomWs[cameraId];
            if (ws && ws.readyState === WebSocket.OPEN) {
                try {
                    ws.send(JSON.stringify({ type: 'stop' }));
                    ws.close();
                } catch (e) {}
            }

            this.cleanupIntercom(cameraId);
            this.$set(this.intercomStatus, cameraId, 'idle');
        },

        // 清理对讲资源
        cleanupIntercom(cameraId) {
            // 停止麦克风采集
            const procInfo = this.intercomProcessor[cameraId];
            if (procInfo) {
                try {
                    procInfo.source.disconnect();
                    procInfo.processor.disconnect();
                    procInfo.silentGain.disconnect();
                } catch (e) {}
                this.$delete(this.intercomProcessor, cameraId);
            }

            // 停止麦克风流
            const stream = this.intercomStream[cameraId];
            if (stream) {
                stream.getTracks().forEach(track => track.stop());
                this.$delete(this.intercomStream, cameraId);
            }

            // 关闭 AudioContext
            const audioCtx = this.intercomAudioCtx[cameraId];
            if (audioCtx && audioCtx.state !== 'closed') {
                try { audioCtx.close(); } catch (e) {}
                this.$delete(this.intercomAudioCtx, cameraId);
            }

            // 清理 WebSocket
            const ws = this.intercomWs[cameraId];
            if (ws) {
                try {
                    if (ws.readyState === WebSocket.OPEN || ws.readyState === WebSocket.CONNECTING) {
                        ws.close();
                    }
                } catch (e) {}
                this.$delete(this.intercomWs, cameraId);
            }

            this.$delete(this.intercomGainNode, cameraId);
            this.$delete(this.intercomNextPlayTime, cameraId);
        },

        // 更新播放音量
        updateVolume() {
            Object.keys(this.intercomGainNode).forEach(cameraId => {
                const gainNode = this.intercomGainNode[cameraId];
                if (gainNode) {
                    gainNode.gain.value = this.intercomVolume / 100;
                }
            });
        },

        // 获取对讲按钮样式类
        getIntercomBtnClass(cameraId) {
            const status = this.intercomStatus[cameraId];
            return {
                'intercom-active': status === 'active',
                'intercom-connecting': status === 'connecting',
                'intercom-error': status === 'error'
            };
        },

        // 获取对讲图标
        getIntercomIcon(cameraId) {
            const status = this.intercomStatus[cameraId];
            if (status === 'active') return 'el-icon-turn-off-microphone';
            if (status === 'connecting') return 'el-icon-loading';
            return 'el-icon-microphone';
        },

        // 获取对讲按钮文字
        getIntercomText(cameraId) {
            const status = this.intercomStatus[cameraId];
            if (status === 'active') return '结束对讲';
            if (status === 'connecting') return '连接中...';
            return '开始对讲';
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

/* ====== 语音对讲面板样式 ====== */
.intercom-panel {
    background: rgba(0, 0, 0, 0.2);
    border-radius: 12px;
    padding: 15px;
    margin-top: 15px;
    border: 1px solid rgba(255, 255, 255, 0.05);
}

.intercom-title {
    font-size: 14px;
    font-weight: 700;
    color: var(--text-bright);
    margin-bottom: 12px;
    letter-spacing: 1px;
}

.intercom-controls {
    display: flex;
    align-items: center;
    gap: 15px;
    flex-wrap: wrap;
}

.intercom-btn {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 10px 24px;
    border-radius: 25px;
    border: 1px solid rgba(255, 255, 255, 0.15);
    background: rgba(255, 255, 255, 0.05);
    color: #fff;
    font-size: 14px;
    cursor: pointer;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    white-space: nowrap;
}

.intercom-btn:hover {
    background: rgba(64, 158, 255, 0.2);
    border-color: #409eff;
    color: #409eff;
    transform: scale(1.02);
}

.intercom-btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
    transform: none;
}

.intercom-btn.intercom-active {
    background: linear-gradient(135deg, rgba(245, 108, 108, 0.3), rgba(245, 108, 108, 0.1));
    border-color: #f56c6c;
    color: #f56c6c;
    animation: intercom-pulse 2s ease-in-out infinite;
}

.intercom-btn.intercom-connecting {
    background: rgba(230, 162, 60, 0.15);
    border-color: #e6a23c;
    color: #e6a23c;
}

.intercom-btn.intercom-error {
    border-color: rgba(245, 108, 108, 0.5);
    color: rgba(245, 108, 108, 0.8);
}

.intercom-btn i {
    font-size: 16px;
}

@keyframes intercom-pulse {
    0%, 100% { box-shadow: 0 0 0 0 rgba(245, 108, 108, 0.4); }
    50% { box-shadow: 0 0 0 8px rgba(245, 108, 108, 0); }
}

.intercom-volume {
    display: flex;
    align-items: center;
    gap: 8px;
    flex: 1;
    min-width: 120px;
}

.volume-label {
    font-size: 12px;
    color: var(--text-muted);
    white-space: nowrap;
}

.volume-slider {
    width: 100px;
}

.volume-slider /deep/ .el-slider__runway {
    margin: 0;
}

.intercom-status {
    margin-top: 10px;
    padding: 8px 12px;
    border-radius: 8px;
    font-size: 13px;
    display: flex;
    align-items: center;
    gap: 6px;
}

.intercom-status.status-connecting {
    background: rgba(230, 162, 60, 0.1);
    color: #e6a23c;
    border: 1px solid rgba(230, 162, 60, 0.2);
}

.intercom-status.status-active {
    background: rgba(103, 194, 58, 0.1);
    color: #67c23a;
    border: 1px solid rgba(103, 194, 58, 0.2);
    animation: status-glow 2s ease-in-out infinite;
}

.intercom-status.status-error {
    background: rgba(245, 108, 108, 0.1);
    color: #f56c6c;
    border: 1px solid rgba(245, 108, 108, 0.2);
}

@keyframes status-glow {
    0%, 100% { box-shadow: inset 0 0 10px rgba(103, 194, 58, 0); }
    50% { box-shadow: inset 0 0 10px rgba(103, 194, 58, 0.1); }
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
