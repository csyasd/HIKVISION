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
            <span class="camera-ip">{{ camera.IP || '未知IP' }}</span>
            <span 
              class="camera-status" 
              :class="getCameraStatusClass(camera.Id)"
            >
              {{ getCameraStatus(camera.Id) }}
            </span>
          </div>
        </div>

        <!-- 视频播放器 -->
        <div class="video-wrapper">
          <video 
            :id="`flv_video_${camera.Id}`"
            class="video-player"
            muted
            autoplay
            playsinline
            style="width: 100%; height: 400px; background: #000; display: block; object-fit: contain;">
          </video>

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
              <p>系统自动重试中...</p>
              <el-button type="primary" size="mini" @click="retryPlay(camera)">手动刷新</el-button>
            </div>
          </div>
        </div>

        <!-- 云台控制 -->
        <div class="ptz-panel">
          <div class="ptz-title">
            <span class="ptz-label">云台控制</span>
            <div class="ptz-speed-wrapper">
              <span class="speed-text">速度:</span>
              <el-slider v-model="ptzSpeed" :min="1" :max="7" class="ptz-slider"></el-slider>
            </div>
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
            <button class="ptz-small-btn" @mousedown="ptz(camera, 13, 0)" @mouseup="ptz(camera, 13, 1)">近焦</button>
            <button class="ptz-small-btn" @mousedown="ptz(camera, 14, 0)" @mouseup="ptz(camera, 14, 1)">远焦</button>
          </div>
        </div>

        <!-- 语音对讲 -->
        <div class="intercom-panel">
          <div class="intercom-header">
            <span class="intercom-label">语音对讲</span>
          </div>
          <div class="intercom-controls">
            <button 
              class="intercom-btn"
              :class="getIntercomBtnClass(camera.Id)"
              @click="toggleIntercom(camera)"
              :disabled="intercomStatus[camera.Id] === 'connecting'"
            >
              <i :class="getIntercomIcon(camera.Id)"></i>
              {{ getIntercomText(camera.Id) }}
            </button>
            <div v-if="intercomStatus[camera.Id] === 'active'" class="intercom-volume">
              <span class="vol-text">音量:</span>
              <el-slider v-model="intercomVolume" :min="0" :max="100" :show-tooltip="false" class="vol-slider" @input="updateVolume"></el-slider>
            </div>
          </div>
          <div v-if="intercomStatus[camera.Id]" class="intercom-tip" :class="'tip-' + intercomStatus[camera.Id]">
            <i v-if="intercomStatus[camera.Id] === 'connecting'" class="el-icon-loading"></i>
            <i v-else-if="intercomStatus[camera.Id] === 'active'" class="el-icon-microphone"></i>
            <i v-else-if="intercomStatus[camera.Id] === 'error'" class="el-icon-warning"></i>
            <span v-if="intercomStatus[camera.Id] === 'connecting'">正在建立对讲连接...</span>
            <span v-else-if="intercomStatus[camera.Id] === 'active'">对讲中 — 请对着麦克风说话</span>
            <span v-else-if="intercomStatus[camera.Id] === 'error'">{{ intercomError[camera.Id] || '对讲连接失败' }}</span>
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

    <!-- 底部数据 -->
    <div class="bottom-data-sections">
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
import { PTZControl, GetRealtimeGasData, GetRealtimeBraceletInfo, SelectALLDevice, SelectALLCamera, BaseUrl } from '../api/api'

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
      intercomNextPlayTime: {}
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
            this.cameras.forEach(camera => this.initPlayer(camera));
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
        const [gasRes, braceletRes, deviceRes] = await Promise.all([
          GetRealtimeGasData(),
          GetRealtimeBraceletInfo(),
          SelectALLDevice()
        ]);
        if (gasRes) this.gasMonitoringData = gasRes;
        if (braceletRes) this.braceletInfoData = braceletRes;
        if (deviceRes) this.devices = deviceRes;
      } catch (error) {}
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
          this.startAudioCapture(cameraId, stream, audioCtx);
        };

        ws.onmessage = (event) => {
          if (typeof event.data === 'string') {
            try {
              const msg = JSON.parse(event.data);
              if (msg.type === 'status' && msg.status === 'connected') {
                this.$set(this.intercomStatus, cameraId, 'active');
                this.$message.success('对讲已连接');
              } else if (msg.type === 'error') {
                this.$set(this.intercomStatus, cameraId, 'error');
                this.$set(this.intercomError, cameraId, msg.message);
                this.$message.error(msg.message);
              }
            } catch (e) {}
          } else if (event.data instanceof ArrayBuffer) {
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

      processor.onaudioprocess = (event) => {
        const ws = this.intercomWs[cameraId];
        if (!ws || ws.readyState !== WebSocket.OPEN) return;
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
        ws.send(int16.buffer);
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
      if (!audioCtx || !gainNode) return;
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
      if (next < now) next = now + 0.05;
      src.start(next);
      this.$set(this.intercomNextPlayTime, cameraId, next + buf.duration);
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
.video-container { padding: 24px; background: transparent; }
.video-container h2 { text-align: center; color: var(--text-bright); font-size: 28px; font-weight: 800; margin-bottom: 30px; letter-spacing: 2px; background: linear-gradient(135deg, #409eff 0%, #7948ea 100%); -webkit-background-clip: text; background-clip: text; -webkit-text-fill-color: transparent; }
.cameras-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(400px, 1fr)); gap: 30px; margin: 30px 0; }
.camera-item { border: 1px solid var(--glass-border); border-radius: 20px; padding: 20px; background: var(--glass-bg); backdrop-filter: blur(20px); box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4); position: relative; }
.camera-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; border-bottom: 1px solid var(--glass-border); padding-bottom: 15px; }
.camera-header h3 { margin: 0; color: var(--text-bright); font-size: 18px; }
.camera-ip { font-size: 13px; color: var(--text-muted); margin-right: 10px; }
.video-wrapper { margin: 15px 0; background: #000; border-radius: 12px; overflow: hidden; position: relative; }
.video-player { width: 100%; height: 400px; }
.error-overlay { position: absolute; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0, 0, 0, 0.8); display: flex; align-items: center; justify-content: center; z-index: 20; }
.error-content { text-align: center; color: #fff; }
.error-content i { font-size: 40px; color: #f56c6c; margin-bottom: 10px; }
.ptz-panel { background: rgba(0, 0, 0, 0.2); border-radius: 12px; padding: 15px; margin-top: 15px; }
.ptz-title { display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; color: var(--text-bright); }
.ptz-label { font-size: 14px; font-weight: 700; }
.ptz-speed-wrapper { display: flex; align-items: center; gap: 8px; }
.speed-text { font-size: 12px; color: var(--text-muted); }
.ptz-slider { width: 80px; }
.ptz-slider /deep/ .el-slider__runway { margin: 0; }
.ptz-grid { display: grid; grid-template-columns: repeat(3, 44px); gap: 10px; justify-content: center; margin-bottom: 15px; }
.ptz-btn { width: 44px; height: 44px; background: rgba(255, 255, 255, 0.1); border: none; border-radius: 8px; color: #fff; cursor: pointer; }
.ptz-btn:hover { background: #409eff; }
.ptz-center { display: flex; align-items: center; justify-content: center; color: #555; }
.ptz-extra { display: flex; justify-content: center; gap: 10px; }
.ptz-small-btn { padding: 5px 10px; background: rgba(255, 255, 255, 0.1); border: none; border-radius: 4px; color: #fff; font-size: 12px; cursor: pointer; }
.ptz-small-btn:hover { color: #409eff; }
.bottom-data-sections { margin-top: 40px; display: flex; flex-direction: column; gap: 40px; }
.data-block { background: var(--glass-bg); border-radius: 20px; overflow: hidden; border: 1px solid var(--glass-border); }
.block-header { padding: 15px 20px; background: rgba(64, 158, 255, 0.1); color: #409eff; font-weight: bold; }
.status-online { color: #67c23a; }
.status-offline { color: #f56c6c; }
.status-unknown { color: #909399; }
.loading, .no-cameras { padding: 100px; text-align: center; color: var(--text-muted); }

/* 语音对讲 */
.intercom-panel { background: rgba(0,0,0,0.2); border-radius: 12px; padding: 15px; margin-top: 15px; border: 1px solid rgba(255,255,255,0.05); }
.intercom-header { margin-bottom: 10px; }
.intercom-label { font-size: 14px; font-weight: 700; color: var(--text-bright); }
.intercom-controls { display: flex; align-items: center; gap: 15px; flex-wrap: wrap; }
.intercom-btn { display: flex; align-items: center; gap: 6px; padding: 8px 20px; border-radius: 20px; border: 1px solid rgba(255,255,255,0.15); background: rgba(255,255,255,0.05); color: #fff; font-size: 13px; cursor: pointer; transition: all 0.3s; white-space: nowrap; }
.intercom-btn:hover { background: rgba(64,158,255,0.2); border-color: #409eff; color: #409eff; }
.intercom-btn:disabled { opacity: 0.6; cursor: not-allowed; }
.intercom-btn.intercom-active { background: rgba(245,108,108,0.2); border-color: #f56c6c; color: #f56c6c; animation: ic-pulse 2s ease-in-out infinite; }
.intercom-btn.intercom-connecting { background: rgba(230,162,60,0.15); border-color: #e6a23c; color: #e6a23c; }
.intercom-btn.intercom-error { border-color: rgba(245,108,108,0.5); color: rgba(245,108,108,0.8); }
.intercom-btn i { font-size: 15px; }
@keyframes ic-pulse { 0%,100% { box-shadow: 0 0 0 0 rgba(245,108,108,0.4); } 50% { box-shadow: 0 0 0 6px rgba(245,108,108,0); } }
.intercom-volume { display: flex; align-items: center; gap: 6px; }
.vol-text { font-size: 12px; color: var(--text-muted); }
.vol-slider { width: 80px; }
.vol-slider /deep/ .el-slider__runway { margin: 0; }
.intercom-tip { margin-top: 8px; padding: 6px 10px; border-radius: 6px; font-size: 12px; display: flex; align-items: center; gap: 5px; }
.intercom-tip.tip-connecting { background: rgba(230,162,60,0.1); color: #e6a23c; }
.intercom-tip.tip-active { background: rgba(103,194,58,0.1); color: #67c23a; }
.intercom-tip.tip-error { background: rgba(245,108,108,0.1); color: #f56c6c; }
</style>
