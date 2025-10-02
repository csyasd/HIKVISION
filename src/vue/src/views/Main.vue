<template>
    <div class="video-container">
        <h2>海康摄像头视频流监控</h2>
        
        <div id="status" class="status">{{ statusText }}</div>
        
        <div class="controls">
            <el-button @click="refreshCameras" type="primary">刷新摄像头</el-button>
            <el-button @click="checkAllStatus" type="info">检查状态</el-button>
            <el-button @click="clearLog" type="warning">清除日志</el-button>
        </div>
        
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
                
                <div class="video-wrapper">
                    <video
                        :id="`videoPlayer_${camera.Id}`"
                        class="video-js vjs-default-skin"
                        controls
                        preload="auto"
                        data-setup='{}'>
                    </video>
                </div>
                
                <div class="camera-controls">
                    <el-button 
                        @click="playCameraStream(camera)" 
                        type="success" 
                        size="mini"
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
        
        <div id="log" class="log"></div>
    </div>
</template>

<script>
import { PTZControl } from '../api/api'

export default {
    data() {
        return {
            cameras: [],
            players: {},
            cameraErrors: {},
            streamStatus: {},
            loading: false,
            statusText: '正在初始化...',
            API_BASE: window.location.hostname === '127.0.0.1' ? 'http://127.0.0.1:5002' : 'http://localhost:5002',
            ptzSpeed: 4  // 云台速度 1-7
        }
    },
    mounted() {
        this.loadVideoJS();  // 使用Video.js
    },
    beforeDestroy() {
        // 清理所有播放器
        Object.values(this.players).forEach(player => {
            if (player && player.dispose) {
                player.dispose();
            }
        });
    },
    methods: {
        // 动态加载Video.js
        loadVideoJS() {
            // 加载CSS
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = 'https://vjs.zencdn.net/8.5.2/video-js.css';
            document.head.appendChild(link);
            
            // 加载JS
            const script = document.createElement('script');
            script.src = 'https://vjs.zencdn.net/8.5.2/video.min.js';
            script.onload = () => {
                this.log('Video.js加载完成');
                this.loadCameras();
            };
            document.head.appendChild(script);
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
            this.cameras.forEach(camera => {
                this.initCameraPlayer(camera.Id);
            });
        },
        
        // 初始化单个摄像头播放器（优化配置）
        initCameraPlayer(cameraId) {
            const playerId = `videoPlayer_${cameraId}`;
            
            // 清理旧播放器
            if (this.players[cameraId]) {
                this.players[cameraId].dispose();
                delete this.players[cameraId];
            }
            
            try {
                const player = window.videojs(playerId, {
                    fluid: true,
                    responsive: true,
                    controls: true,
                    autoplay: false,
                    preload: 'none',  // 不预加载，减少初始延迟
                    liveui: true,     // 启用直播UI
                    html5: {
                        vhs: {
                            // 低延迟配置
                            overrideNative: true,
                            enableLowInitialPlaylist: true,
                            smoothQualityChange: true,
                            // 关键：最小缓冲
                            goalBufferLength: 2,
                            maxGoalBufferLength: 3,
                            backBufferLength: 2
                        }
                    }
                });
                
                player.ready(() => {
                    this.log(`摄像头 ${cameraId} 播放器就绪（低延迟配置）`);
                });
                
                player.on('error', () => {
                    const error = player.error();
                    this.log(`播放器错误: ${error?.message || '未知错误'}`);
                    this.$set(this.cameraErrors, cameraId, true);
                });
                
                this.players[cameraId] = player;
                
            } catch (error) {
                this.log(`初始化播放器失败: ${error.message}`);
            }
        },
        
        // 自动播放所有摄像头
        async autoPlayAll() {
            this.log('请手动点击播放按钮');
            this.statusText = `已加载 ${this.cameras.length} 个摄像头，请点击"播放"按钮`;
        },
        
        // 播放指定摄像头流（优化HLS）
        async playCameraStream(camera) {
            try {
                this.log(`开始播放摄像头: ${camera.Name}(${camera.IP})`);
                
                const player = this.players[camera.Id];
                if (!player) {
                    throw new Error('播放器未初始化');
                }
                
                const hlsUrl = `${this.API_BASE}/hls/camera_${camera.Id}.m3u8`;
                this.log(`摄像头 ${camera.Name} HLS地址: ${hlsUrl}`);
                
                player.src({
                    src: hlsUrl,
                    type: 'application/x-mpegURL'
                });
                
                // 监听元数据加载，跳到实时位置
                player.one('loadedmetadata', () => {
                    const duration = player.duration();
                    if (duration && duration > 2 && duration !== Infinity) {
                        // 跳到最后2秒（最新画面）
                        player.currentTime(duration - 2);
                        this.log(`跳转到实时位置（${duration.toFixed(1)}秒）`);
                    }
                });
                
                await player.play();
                this.log(`摄像头 ${camera.Name} 播放开始（低延迟模式）`);
                this.$set(this.cameraErrors, camera.Id, false);
                
            } catch (error) {
                this.log(`播放失败: ${error.message}`);
                this.$set(this.cameraErrors, camera.Id, true);
            }
        },
        
        // 停止指定摄像头流
        stopCameraStream(cameraId) {
            try {
                const player = this.players[cameraId];
                if (player) {
                    player.pause();
                    player.src('');
                    delete this.players[cameraId];
                    this.log(`摄像头 ${cameraId} 已停止`);
                    this.$set(this.cameraErrors, cameraId, false);
                }
            } catch (error) {
                this.log(`停止失败: ${error.message}`);
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
        
        // 获取摄像头状态
        getCameraStatus(cameraId) {
            const streamInfo = this.streamStatus[cameraId];
            if (!streamInfo) return '未知';
            
            if (streamInfo.IsActive) {
                return '在线';
            } else if (streamInfo.LastError) {
                return '错误';
            } else {
                return '离线';
            }
        },
        
        // 获取摄像头状态样式类
        getCameraStatusClass(cameraId) {
            const status = this.getCameraStatus(cameraId);
            switch (status) {
                case '在线': return 'status-online';
                case '错误': return 'status-error';
                case '离线': return 'status-offline';
                default: return 'status-unknown';
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

.video-js {
    width: 100% !important;
    height: 280px !important;
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
