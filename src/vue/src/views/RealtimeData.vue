<template>
  <div class="video-container">
    <div class="header-container">
      <h2>{{ displayTitle }}</h2>
      <div class="header-tools">
        <el-select v-model="selectedCameraId" placeholder="筛选摄像头名称" clearable class="camera-selector">
          <el-option
            v-for="item in cameras"
            :key="item.Id"
            :label="item.Name || (item.Device && item.Device.Name) || '未命名摄像头'"
            :value="item.Id">
          </el-option>
        </el-select>
      </div>
    </div>
    
    <!-- 摄像头网格布局 -->
    <div class="cameras-grid" v-if="filteredCameras.length > 0">
      <div 
        v-for="camera in filteredCameras" 
        :key="camera.Id" 
        class="camera-item"
        :class="{ 'camera-error': cameraErrors[camera.Id] }"
      >
        <div class="camera-header">
          <h3>{{ camera.Name || (camera.Device && camera.Device.Name) || '未命名摄像头' }}</h3>
          <div class="camera-info">
            <span class="camera-ip">{{ camera.IP || '未知IP' }}</span>
            <span 
              class="camera-status" 
              :class="getCameraStatusClass(camera.Id)"
            >
              {{ getCameraStatus(camera.Id) }}
            </span>
          </div>
        </div>

        <!-- 3列布局框架 -->
        <div class="camera-dashboard">
          <!-- 左侧：工单数据 -->
          <div class="dashboard-sidebar sidebar-left">
            <div class="sidebar-section">
              <div class="section-title">实时气体监测</div>
              <div class="section-content">
                <div v-for="(gas, index) in getFilteredGas(camera.DeviceId)" :key="'gas-'+index" class="data-item">
                  <span class="data-name">{{ gas.GasName }}:</span>
                  <span class="data-value">{{ gas.GasValue }}</span>
                </div>
                <div v-if="getFilteredGas(camera.DeviceId).length === 0" class="no-data-text">暂无在线监测数据</div>
              </div>
            </div>
            <div class="sidebar-section">
              <div class="section-title">手环工人信息</div>
              <div class="section-content">
                <div v-for="(bracelet, index) in getFilteredBracelet(camera.DeviceId)" :key="'bracelet-'+index" class="data-item bracelet-item">
                  <div class="worker-name">{{ bracelet.WorkerName }}</div>
                  <div class="heart-rate">
                    <i class="el-icon-headset"></i> {{ bracelet.HeartRate }} bpm
                  </div>
                </div>
                <div v-if="getFilteredBracelet(camera.DeviceId).length === 0" class="no-data-text">暂无在线工人信息</div>
              </div>
            </div>
          </div>

          <!-- 中间：视频播放 -->
          <div class="dashboard-center">
            <div class="video-wrapper" :id="`video_wrapper_${camera.Id}`">
              <video 
                :id="`flv_video_${camera.Id}`"
                class="video-player"
                muted
                autoplay
                playsinline
                style="width: 100%; height: 100%; background: #000; display: block; object-fit: contain;">
              </video>

              <!-- 全屏按钮 -->
              <div class="fullscreen-btn" @click="toggleFullScreen(camera.Id)">
                <i class="el-icon-full-screen"></i>
              </div>

              <!-- 加载中 -->
              <div v-if="loadingStatus[camera.Id] && !cameraErrors[camera.Id]" class="error-overlay">
                <div class="error-content">
                  <i class="el-icon-loading"></i>
                  <h4>正在建立极速连接...</h4>
                </div>
              </div>

              <!-- 播放失败 -->
              <div v-if="cameraErrors[camera.Id]" class="error-overlay">
                <div class="error-content">
                  <i class="el-icon-warning"></i>
                  <h4>视频流连接中断</h4>
                  <el-button type="primary" size="mini" @click="retryPlay(camera)">手动刷新</el-button>
                </div>
              </div>
            </div>
          </div>

          <!-- 右侧：控制面板 -->
          <div class="dashboard-sidebar sidebar-right">
            <!-- 云台控制 -->
            <div class="ptz-panel mini-panel">
              <div class="ptz-title">
                <span class="ptz-label">云台控制</span>
                <el-slider v-model="ptzSpeed" :min="1" :max="7" class="ptz-slider-mini"></el-slider>
              </div>
              <div class="ptz-grid">
                <div></div>
                <button class="ptz-btn" @mousedown="ptz(camera, 21, 0)" @mouseup="ptz(camera, 21, 1)">↑</button>
                <div></div>
                <button class="ptz-btn" @mousedown="ptz(camera, 23, 0)" @mouseup="ptz(camera, 23, 1)">←</button>
                <div class="ptz-center">⊙</div>
                <button class="ptz-btn" @mousedown="ptz(camera, 24, 0)" @mouseup="ptz(camera, 24, 1)">→</button>
                <div></div>
                <button class="ptz-btn" @mousedown="ptz(camera, 22, 0)" @mouseup="ptz(camera, 22, 1)">↓</button>
                <div></div>
              </div>
              <div class="ptz-extra">
                <button class="ptz-small-btn" @mousedown="ptz(camera, 11, 0)" @mouseup="ptz(camera, 11, 1)">放大</button>
                <button class="ptz-small-btn" @mousedown="ptz(camera, 12, 0)" @mouseup="ptz(camera, 12, 1)">缩小</button>
              </div>
            </div>

            <!-- 语音对讲 (根据用户要求暂时隐藏) -->
            <div class="intercom-panel mini-panel" v-if="false">
              <div class="intercom-controls-mini">
                <button 
                  class="intercom-btn-mini"
                  :class="getIntercomBtnClass(camera.Id)"
                  @click="toggleIntercom(camera)"
                  :disabled="intercomStatus[camera.Id] === 'connecting'"
                >
                  <i :class="getIntercomIcon(camera.Id)"></i>
                </button>
                <div class="intercom-vol-wrap">
                  <el-slider v-model="intercomVolume" :min="0" :max="100" class="vol-slider-mini" vertical height="60px" @input="updateVolume"></el-slider>
                </div>
              </div>
              <div v-if="intercomStatus[camera.Id] === 'active'" class="intercom-status-dot pulse">对讲中</div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 加载中/空状态 -->
    <div v-else-if="loading" class="loading">
      <i class="el-icon-loading"></i>
      <span>正在拉取摄像头列表...</span>
    </div>
    <div v-else class="no-cameras">
      <i class="el-icon-video-camera"></i>
      <p>暂无摄像头数据</p>
      <el-button @click="loadCameras" type="primary">重试加载</el-button>
    </div>


  </div>
</template>

<script>
import { PTZControl, GetRealtimeGasData, GetRealtimeBraceletInfo, GetRealtimeWorkOrders, SelectALLDevice, SelectALLCamera, BaseUrl } from '../api/api'

export default {
  data() {
    return {
      cameras: [],
      players: {},
      lastCheckTime: {}, 
      stuckCount: {},     
      loadingStatus: {},
      cameraErrors: {},
      loading: false,
      ptzSpeed: 4,
      gasMonitoringData: [],
      braceletInfoData: [],
      devices: [],
      selectedCameraId: null,
      refreshTimer: null,
      latencyCheckTimer: null,
      API_BASE: BaseUrl.replace(/\/$/, ''),
      // 语音对讲
      intercomStatus: {},
      intercomError: {},
      intercomWs: {},
      intercomAudioCtx: {},
      intercomStream: {},
      intercomProcessor: {},
      intercomVolume: 80,
      intercomGainNode: {},
      intercomNextPlayTime: {},
      workOrders: []
    }
  },
  computed: {
    filteredCameras() {
      if (!this.selectedCameraId) return this.cameras;
      return this.cameras.filter(c => c.Id == this.selectedCameraId);
    },
    displayTitle() {
      if (this.selectedCameraId) {
        const cam = this.cameras.find(c => c.Id == this.selectedCameraId);
        const name = cam ? (cam.Name || (cam.Device && cam.Device.Name) || '未命名摄像头') : '';
        return cam ? `正在监控: ${name}` : '实时监控';
      }
      return '实时监控';
    }
  },
  watch: {
    selectedCameraId(newVal) {
      this.destroyAllPlayers();
      this.$nextTick(() => {
        this.filteredCameras.forEach(camera => this.initPlayer(camera));
      });
    },
    '$route.query.cameraId': {
      handler(val) {
        if (val) {
          if (this.selectedCameraId !== val) {
            this.selectedCameraId = val;
          }
        } else {
          this.selectedCameraId = null;
        }
      },
      immediate: true
    }
  },
  mounted() {
    this.loadCameras();
    this.fetchRealtimeData();
    this.refreshTimer = setInterval(this.fetchRealtimeData, 5000);
    // 每 2.5 秒检查一次健康状态
    this.latencyCheckTimer = setInterval(this.checkHealth, 2500);
  },
  beforeDestroy() {
    if (this.refreshTimer) clearInterval(this.refreshTimer);
    if (this.latencyCheckTimer) clearInterval(this.latencyCheckTimer);
    this.destroyAllPlayers();
    // 清理对讲
    this.cameras.forEach(camera => this.stopIntercom(camera.Id));
  },
  methods: {
    async loadCameras() {
      try {
        this.loading = true;
        // 使用api.js中的SelectALLCamera方法，它会自动添加Authorization header
        const camerasRes = await SelectALLCamera();
        this.cameras = camerasRes || [];
        
        if (this.cameras.length > 0) {
          this.$nextTick(() => {
            this.filteredCameras.forEach(camera => this.initPlayer(camera));
          });
        }
      } catch (err) {
        console.error('加载摄像头失败:', err);
        this.$message.error('加载摄像头失败: ' + (err.message || '未知错误'));
      } finally {
        this.loading = false;
      }
    },

    initPlayer(camera) {
      if (!window.flvjs || !window.flvjs.isSupported()) {
        this.$set(this.cameraErrors, camera.Id, true);
        return;
      }

      const videoElement = document.getElementById(`flv_video_${camera.Id}`);
      if (!videoElement) return;

      this.destroyPlayer(camera.Id);

      this.$set(this.loadingStatus, camera.Id, true);
      this.$set(this.cameraErrors, camera.Id, false);
      this.lastCheckTime[camera.Id] = -1;
      this.stuckCount[camera.Id] = 0;

      const playUrl = `${this.API_BASE}/api/HK/flv-stream/${camera.Id}`;

      try {
        const player = window.flvjs.createPlayer({
          type: 'flv',
          url: playUrl,
          isLive: true,
          hasAudio: false
        }, {
          enableStashBuffer: false,
          stashInitialSize: 128,
          enableWorker: false, // 彻底禁用 Worker，解决 Class extends value undefined 报错
          lazyLoad: false,
          autoCleanupSourceBuffer: true
        });

        player.attachMediaElement(videoElement);
        player.load();
        
        player.play().then(() => {
          this.$set(this.loadingStatus, camera.Id, false);
        }).catch(() => {
          // 播放被拦截通常是由于还没点击页面，这里静默处理
        });

        player.on(window.flvjs.Events.ERROR, () => {
          this.$set(this.cameraErrors, camera.Id, true);
          // 错误后不立刻重连，由 retryPlay 统一控制
        });

        this.players[camera.Id] = player;
      } catch (e) {
        this.$set(this.cameraErrors, camera.Id, true);
      }
    },

    checkHealth() {
      Object.keys(this.players).forEach(id => {
        const video = document.getElementById(`flv_video_${id}`);
        if (!video) return;

        // 1. 卡死检测：只有在 currentTime > 0 且已经开始播放后才启用
        if (video.currentTime > 0 && video.currentTime === this.lastCheckTime[id] && !video.paused) {
          this.stuckCount[id]++;
          if (this.stuckCount[id] >= 3) { // 连续 3 次检查（约 7.5 秒）画面不动
            console.warn(`[${id}] 检测到画面卡死，正在强制重连...`);
            const camera = this.cameras.find(c => c.Id == id);
            if (camera) this.retryPlay(camera);
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

    destroyPlayer(id) {
      if (this.players[id]) {
        try {
          this.players[id].pause();
          this.players[id].unload();
          this.players[id].detachMediaElement();
          this.players[id].destroy();
        } catch (e) {}
        delete this.players[id];
      }
    },

    destroyAllPlayers() {
      Object.keys(this.players).forEach(id => this.destroyPlayer(id));
    },

    retryPlay(camera) {
      if (this.cameraErrors[camera.Id]) return; // 如果已经报错，不再自动循环
      this.destroyPlayer(camera.Id);
      setTimeout(() => this.initPlayer(camera), 3000); // 3秒后重连，防止死循环
    },

    async fetchRealtimeData() {
      try {
        const [gasRes, braceletRes, deviceRes, workOrderRes] = await Promise.all([
          GetRealtimeGasData(),
          GetRealtimeBraceletInfo(),
          SelectALLDevice(),
          GetRealtimeWorkOrders()
        ]);
        if (gasRes) this.gasMonitoringData = gasRes;
        if (braceletRes) this.braceletInfoData = braceletRes;
        if (deviceRes) this.devices = deviceRes;
        if (workOrderRes) this.workOrders = workOrderRes;
      } catch (error) {}
    },

    toggleFullScreen(cameraId) {
      const videoWrapper = document.getElementById(`video_wrapper_${cameraId}`);
      if (!videoWrapper) return;

      if (!document.fullscreenElement) {
        if (videoWrapper.requestFullscreen) {
          videoWrapper.requestFullscreen();
        } else if (videoWrapper.mozRequestFullScreen) { /* Firefox */
          videoWrapper.mozRequestFullScreen();
        } else if (videoWrapper.webkitRequestFullscreen) { /* Chrome, Safari and Opera */
          videoWrapper.webkitRequestFullscreen();
        } else if (videoWrapper.msRequestFullscreen) { /* IE/Edge */
          videoWrapper.msRequestFullscreen();
        }
      } else {
        if (document.exitFullscreen) {
          document.exitFullscreen();
        } else if (document.mozCancelFullScreen) {
          document.mozCancelFullScreen();
        } else if (document.webkitExitFullscreen) {
          document.webkitExitFullscreen();
        } else if (document.msExitFullscreen) {
          document.msExitFullscreen();
        }
      }
    },

    getOnlineWorkOrder(deviceId) {
      if (!deviceId) return null;
      return this.workOrders.find(w => w.DeviceId === deviceId && w.Status === 1);
    },

    getFilteredGas(deviceId) {
      if (!deviceId) return [];
      const order = this.getOnlineWorkOrder(deviceId);
      if (!order) return [];
      return this.gasMonitoringData.filter(g => g.WorkOrderCode === order.Code || g.DeviceId === deviceId);
    },

    getFilteredBracelet(deviceId) {
      if (!deviceId) return [];
      const order = this.getOnlineWorkOrder(deviceId);
      if (!order) return [];
      return this.braceletInfoData.filter(b => b.WorkOrderCode === order.Code || b.DeviceId === deviceId);
    },

    getCameraStatus(cameraId) {
      const camera = this.cameras.find(c => c.Id === cameraId);
      if (!camera || !camera.DeviceId) return '未知';
      const device = this.devices.find(d => d.Id === camera.DeviceId);
      return device ? (device.OnlineStatus || '离线') : '未知';
    },

    getCameraStatusClass(cameraId) {
      const status = this.getCameraStatus(cameraId);
      return status === '在线' ? 'status-online' : (status === '离线' ? 'status-offline' : 'status-unknown');
    },

    async ptz(camera, command, stop) {
      try {
        await PTZControl({
          ip: camera.IP,
          port: 8000,
          username: 'admin',
          password: 'Cnh321456$',
          channel: 1,
          command: command,
          stop: stop,
          speed: this.ptzSpeed
        });
      } catch (error) {}
    },

    // ====== 语音对讲 ======
    toggleIntercom(camera) {
      const status = this.intercomStatus[camera.Id];
      if (status === 'active' || status === 'connecting') {
        this.stopIntercom(camera.Id);
      } else {
        this.startIntercom(camera);
      }
    },

    async startIntercom(camera) {
      const cameraId = camera.Id;
      this.$set(this.intercomStatus, cameraId, 'connecting');
      this.$set(this.intercomError, cameraId, '');

      try {
        const stream = await navigator.mediaDevices.getUserMedia({
          audio: { echoCancellation: true, noiseSuppression: true, autoGainControl: true, channelCount: 1 }
        });
        this.$set(this.intercomStream, cameraId, stream);

        let audioCtx;
        try {
          audioCtx = new (window.AudioContext || window.webkitAudioContext)({ sampleRate: 8000 });
        } catch (e) {
          audioCtx = new (window.AudioContext || window.webkitAudioContext)();
        }
        this.$set(this.intercomAudioCtx, cameraId, audioCtx);

        const gainNode = audioCtx.createGain();
        gainNode.gain.value = this.intercomVolume / 100;
        gainNode.connect(audioCtx.destination);
        this.$set(this.intercomGainNode, cameraId, gainNode);

        const wsProtocol = this.API_BASE.startsWith('https') ? 'wss' : 'ws';
        const wsHost = this.API_BASE.replace(/^https?:\/\//, '');
        const wsUrl = `${wsProtocol}://${wsHost}/api/HK/voice-talk/${cameraId}`;

        const ws = new WebSocket(wsUrl);
        ws.binaryType = 'arraybuffer';
        this.$set(this.intercomWs, cameraId, ws);
        this.$set(this.intercomNextPlayTime, cameraId, 0);

        ws.onopen = () => {
          console.log('[对讲] WebSocket 已连接，等待服务器确认...');
        };

        ws.onmessage = (event) => {
          if (typeof event.data === 'string') {
            try {
              const msg = JSON.parse(event.data);
              console.log('[对讲] 收到:', msg.type, msg.message || msg.status || '');
              if (msg.type === 'status' && msg.status === 'connected') {
                this.$set(this.intercomStatus, cameraId, 'active');
                this.$message.success('对讲已连接');
                this.startAudioCapture(cameraId, stream, audioCtx);
              } else if (msg.type === 'error') {
                this.$set(this.intercomStatus, cameraId, 'error');
                this.$set(this.intercomError, cameraId, msg.message);
                this.$message.error(msg.message);
              }
            } catch (e) {}
          } else if (event.data instanceof ArrayBuffer && event.data.byteLength > 0) {
            this.playReceivedAudio(cameraId, event.data);
          }
        };

        ws.onerror = () => {
          this.$set(this.intercomStatus, cameraId, 'error');
          this.$set(this.intercomError, cameraId, 'WebSocket 连接错误');
        };

        ws.onclose = () => {
          if (this.intercomStatus[cameraId] === 'active') {
            this.$set(this.intercomStatus, cameraId, 'idle');
          }
          this.cleanupIntercom(cameraId);
        };
      } catch (error) {
        this.$set(this.intercomStatus, cameraId, 'error');
        let msg = '启动对讲失败';
        if (error.name === 'NotAllowedError') msg = '麦克风权限被拒绝';
        else if (error.name === 'NotFoundError') msg = '未检测到麦克风';
        else msg = error.message || msg;
        this.$set(this.intercomError, cameraId, msg);
        this.$message.error(msg);
      }
    },

    startAudioCapture(cameraId, stream, audioCtx) {
      const source = audioCtx.createMediaStreamSource(stream);
      const processor = audioCtx.createScriptProcessor(2048, 1, 1);
      const targetRate = 8000;
      const actualRate = audioCtx.sampleRate;
      const needResample = Math.abs(actualRate - targetRate) > 100;

      let sendCount = 0;
      processor.onaudioprocess = (event) => {
        const ws = this.intercomWs[cameraId];
        if (!ws || ws.readyState !== WebSocket.OPEN) return;
        if (this.intercomStatus[cameraId] !== 'active') return;
        let input = event.inputBuffer.getChannelData(0);
        if (needResample) {
          const ratio = actualRate / targetRate;
          const len = Math.floor(input.length / ratio);
          const buf = new Float32Array(len);
          for (let i = 0; i < len; i++) buf[i] = input[Math.floor(i * ratio)];
          input = buf;
        }
        const int16 = new Int16Array(input.length);
        for (let i = 0; i < input.length; i++) {
          const s = Math.max(-1, Math.min(1, input[i]));
          int16[i] = s < 0 ? s * 0x8000 : s * 0x7FFF;
        }
        try {
          ws.send(int16.buffer);
          sendCount++;
          if (sendCount % 20 === 1) console.log(`[对讲] 已发送 ${sendCount} 包音频`);
        } catch (e) {}
      };

      source.connect(processor);
      const silent = audioCtx.createGain();
      silent.gain.value = 0;
      processor.connect(silent);
      silent.connect(audioCtx.destination);
      this.$set(this.intercomProcessor, cameraId, { processor, source, silentGain: silent });
    },

    playReceivedAudio(cameraId, arrayBuffer) {
      const audioCtx = this.intercomAudioCtx[cameraId];
      const gainNode = this.intercomGainNode[cameraId];
      if (!audioCtx || !gainNode || audioCtx.state === 'closed') return;
      if (!this._recvCount) this._recvCount = {};
      if (!this._recvCount[cameraId]) this._recvCount[cameraId] = 0;
      this._recvCount[cameraId]++;
      if (this._recvCount[cameraId] % 20 === 1) console.log(`[对讲] 收到 ${this._recvCount[cameraId]} 包摄像头音频`);
      const int16 = new Int16Array(arrayBuffer);
      const f32 = new Float32Array(int16.length);
      for (let i = 0; i < int16.length; i++) f32[i] = int16[i] / 32768.0;
      const buf = audioCtx.createBuffer(1, f32.length, 8000);
      buf.getChannelData(0).set(f32);
      const src = audioCtx.createBufferSource();
      src.buffer = buf;
      src.connect(gainNode);
      const now = audioCtx.currentTime;
      let next = this.intercomNextPlayTime[cameraId] || 0;
      if (next < now) next = now + 0.02;
      try { src.start(next); this.$set(this.intercomNextPlayTime, cameraId, next + buf.duration); } catch (e) {}
    },

    stopIntercom(cameraId) {
      const ws = this.intercomWs[cameraId];
      if (ws && ws.readyState === WebSocket.OPEN) {
        try { ws.send(JSON.stringify({ type: 'stop' })); ws.close(); } catch (e) {}
      }
      this.cleanupIntercom(cameraId);
      this.$set(this.intercomStatus, cameraId, 'idle');
    },

    cleanupIntercom(cameraId) {
      const p = this.intercomProcessor[cameraId];
      if (p) { try { p.source.disconnect(); p.processor.disconnect(); p.silentGain.disconnect(); } catch (e) {} this.$delete(this.intercomProcessor, cameraId); }
      const s = this.intercomStream[cameraId];
      if (s) { s.getTracks().forEach(t => t.stop()); this.$delete(this.intercomStream, cameraId); }
      const ctx = this.intercomAudioCtx[cameraId];
      if (ctx && ctx.state !== 'closed') { try { ctx.close(); } catch (e) {} this.$delete(this.intercomAudioCtx, cameraId); }
      const ws = this.intercomWs[cameraId];
      if (ws) { try { if (ws.readyState <= 1) ws.close(); } catch (e) {} this.$delete(this.intercomWs, cameraId); }
      this.$delete(this.intercomGainNode, cameraId);
      this.$delete(this.intercomNextPlayTime, cameraId);
    },

    updateVolume() {
      Object.keys(this.intercomGainNode).forEach(id => {
        const g = this.intercomGainNode[id];
        if (g) g.gain.value = this.intercomVolume / 100;
      });
    },

    getIntercomBtnClass(id) {
      const s = this.intercomStatus[id];
      return { 'intercom-active': s === 'active', 'intercom-connecting': s === 'connecting', 'intercom-error': s === 'error' };
    },
    getIntercomIcon(id) {
      const s = this.intercomStatus[id];
      if (s === 'active') return 'el-icon-turn-off-microphone';
      if (s === 'connecting') return 'el-icon-loading';
      return 'el-icon-microphone';
    },
    getIntercomText(id) {
      const s = this.intercomStatus[id];
      if (s === 'active') return '结束对讲';
      if (s === 'connecting') return '连接中...';
      return '开始对讲';
    }
  }
}
</script>

<style scoped>
.video-container { padding: 24px; background: transparent; min-height: 100vh; }
.header-container { display: flex; align-items: center; justify-content: center; position: relative; margin-bottom: 30px; }
.header-tools { position: absolute; left: 0; top: 50%; transform: translateY(-50%); }
.camera-selector { width: 220px; }
.camera-selector /deep/ .el-input__inner { background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1); color: #fff; border-radius: 10px; }
.video-container h2 { text-align: center; color: var(--text-bright); font-size: 28px; font-weight: 800; margin: 0; letter-spacing: 2px; background: linear-gradient(135deg, #409eff 0%, #7948ea 100%); -webkit-background-clip: text; background-clip: text; -webkit-text-fill-color: transparent; }
.cameras-grid { display: grid; grid-template-columns: 1fr; gap: 40px; margin: 30px 0; }

.camera-item { border: 1px solid rgba(255, 255, 255, 0.1); border-radius: 24px; padding: 24px; background: rgba(20, 20, 20, 0.6); backdrop-filter: blur(30px); box-shadow: 0 12px 48px rgba(0, 0, 0, 0.5); position: relative; }
.camera-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; border-bottom: 1px solid rgba(255, 255, 255, 0.05); padding-bottom: 15px; }
.camera-header h3 { margin: 0; color: #fff; font-size: 20px; font-weight: 600; }
.camera-ip { font-size: 14px; color: rgba(255, 255, 255, 0.4); margin-right: 15px; }

/* Dashboard Layout */
.camera-dashboard { display: flex; gap: 20px; height: 450px; background: #000; border-radius: 16px; overflow: hidden; border: 1px solid rgba(255, 255, 255, 0.08); }

.dashboard-sidebar { width: 220px; flex-shrink: 0; padding: 20px; background: rgba(255, 255, 255, 0.02); display: flex; flex-direction: column; gap: 20px; overflow-y: auto; }
.sidebar-left { border-right: 1px solid rgba(255, 255, 255, 0.05); }
.sidebar-right { border-left: 1px solid rgba(255, 255, 255, 0.05); width: 180px; }
.dashboard-center { flex: 1; position: relative; background: #000; }

/* Sections */
.sidebar-section { display: flex; flex-direction: column; gap: 10px; }
.section-title { font-size: 13px; font-weight: 700; color: #409eff; text-transform: uppercase; letter-spacing: 0.5px; opacity: 0.8; }
.section-content { display: flex; flex-direction: column; gap: 12px; }

.data-item { display: flex; justify-content: space-between; align-items: center; padding: 10px 12px; background: rgba(255, 255, 255, 0.03); border-radius: 8px; border: 1px solid rgba(255, 255, 255, 0.05); }
.data-name { font-size: 12px; color: rgba(255, 255, 255, 0.5); }
.data-value { font-size: 14px; color: #fff; font-family: 'Inter', sans-serif; font-weight: 600; }

.bracelet-item { flex-direction: column; align-items: flex-start; gap: 6px; }
.worker-name { font-size: 14px; color: #fff; font-weight: 600; }
.heart-rate { font-size: 13px; color: #f56c6c; display: flex; align-items: center; gap: 4px; }
.no-data-text { font-size: 12px; color: rgba(255, 255, 255, 0.3); text-align: center; margin-top: 10px; }

/* Video & Fullscreen */
.video-wrapper { width: 100%; height: 100%; position: relative; }
.fullscreen-btn { position: absolute; bottom: 15px; right: 15px; width: 36px; height: 36px; background: rgba(0, 0, 0, 0.5); border-radius: 8px; display: flex; align-items: center; justify-content: center; color: #fff; cursor: pointer; transition: all 0.3s; z-index: 30; border: 1px solid rgba(255, 255, 255, 0.1); }
.fullscreen-btn:hover { background: #409eff; transform: scale(1.1); }
.fullscreen-btn i { font-size: 20px; }

/* Mini Panels */
.mini-panel { background: transparent; padding: 0; margin: 0; }
.ptz-title { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; }
.ptz-label { font-size: 13px; font-weight: 700; color: rgba(255, 255, 255, 0.6); }
.ptz-slider-mini { width: 60px; }
.ptz-slider-mini /deep/ .el-slider__runway { margin: 8px 0; height: 4px; }

.ptz-grid { display: grid; grid-template-columns: repeat(3, 36px); gap: 8px; justify-content: center; margin-bottom: 15px; }
.ptz-btn { width: 36px; height: 36px; background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1); border-radius: 6px; color: #fff; cursor: pointer; transition: all 0.2s; font-size: 16px; }
.ptz-btn:hover { background: #409eff; border-color: #409eff; }
.ptz-center { display: flex; align-items: center; justify-content: center; color: rgba(255, 255, 255, 0.2); }
.ptz-extra { display: flex; justify-content: center; gap: 8px; }
.ptz-small-btn { flex: 1; padding: 6px 0; background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1); border-radius: 4px; color: #fff; font-size: 11px; cursor: pointer; transition: all 0.2s; text-align: center; }
.ptz-small-btn:hover { background: rgba(64, 158, 255, 0.2); border-color: #409eff; color: #409eff; }

/* Intercom Mini */
.intercom-panel { margin-top: 10px; padding-top: 15px; border-top: 1px solid rgba(255, 255, 255, 0.05); }
.intercom-controls-mini { display: flex; justify-content: space-between; align-items: center; }
.intercom-btn-mini { width: 44px; height: 44px; border-radius: 50%; border: 2px solid rgba(255, 255, 255, 0.2); background: rgba(255, 255, 255, 0.05); color: #fff; cursor: pointer; transition: all 0.3s; display: flex; align-items: center; justify-content: center; position: relative; }
.intercom-btn-mini:hover { border-color: #409eff; color: #409eff; transform: scale(1.05); }
.intercom-btn-mini i { font-size: 20px; }
.intercom-btn-mini.intercom-active { background: #f56c6c; border-color: #f56c6c; box-shadow: 0 0 15px rgba(245, 108, 108, 0.5); }
.intercom-vol-wrap { width: 40px; }
.vol-slider-mini /deep/ .el-slider__runway { background-color: rgba(255, 255, 255, 0.1); }

.intercom-status-dot { margin-top: 10px; font-size: 11px; color: #67c23a; text-align: center; display: flex; align-items: center; justify-content: center; gap: 5px; }
.intercom-status-dot::before { content: ''; width: 6px; height: 6px; background: #67c23a; border-radius: 50%; }

.pulse { animation: pulse-animation 2s infinite; }
@keyframes pulse-animation { 0% { opacity: 1; } 50% { opacity: 0.5; } 100% { opacity: 1; } }

.status-online { color: #67c23a; font-weight: bold; }
.status-offline { color: #f56c6c; opacity: 0.8; }
.status-unknown { color: #909399; }
.loading, .no-cameras { padding: 100px; text-align: center; color: var(--text-muted); }
.error-overlay { position: absolute; inset: 0; background: rgba(0, 0, 0, 0.85); display: flex; align-items: center; justify-content: center; z-index: 20; }
.error-content { text-align: center; padding: 20px; }
.error-content i { font-size: 48px; color: #f56c6c; margin-bottom: 15px; display: block; }
.error-content h4 { color: #fff; margin: 0 0 15px 0; font-weight: 400; }

/* Responsive adjustments */
@media (max-width: 1200px) {
  .cameras-grid { grid-template-columns: 1fr; }
  .camera-dashboard { height: auto; min-height: 400px; }
}

</style>
