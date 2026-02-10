<template>
  <div class="mobile-realtime">
    <div class="camera-select-wrap">
      <el-select v-model="selectedCameraId" placeholder="选择摄像头" class="camera-select" @change="onCameraChange">
        <el-option v-for="c in cameras" :key="c.Id" :label="c.Name || '未命名'" :value="c.Id" />
      </el-select>
    </div>
    <div class="video-area" v-if="currentCamera">
      <div class="camera-name">{{ currentCamera.Name || '未命名摄像头' }}</div>
      <div class="video-box">
        <video :id="`m_video_${currentCamera.Id}`" muted playsinline style="width:100%;height:100%;object-fit:contain;background:#000"></video>
        <div v-if="loading" class="loading-state"><i class="el-icon-loading"></i> 连接中...</div>
        <div v-if="error" class="error-state" @click="retryPlay">
          <i class="el-icon-warning"></i>
          <span>点击重试</span>
        </div>
      </div>
      <div class="data-panel">
        <div class="data-section">
          <div class="data-title">气体</div>
          <div class="data-list" v-if="currentGas.length">
            <div class="data-row" v-for="(g, i) in currentGas" :key="i">
              <span>{{ g.GasName }}</span><span>{{ g.GasValue }}</span>
            </div>
          </div>
          <div v-else class="no-data">暂无</div>
        </div>
        <div class="data-section">
          <div class="data-title">工人</div>
          <div class="data-list" v-if="currentBracelet.length">
            <div class="data-row" v-for="(b, i) in currentBracelet" :key="i">
              <span>{{ b.WorkerName }}</span><span>{{ b.HeartRate || '-' }} bpm</span>
            </div>
          </div>
          <div v-else class="no-data">暂无</div>
        </div>
      </div>
      <div class="ptz-panel">
        <div class="ptz-title">
          <span class="ptz-label">云台控制</span>
          <el-slider v-model="ptzSpeed" :min="1" :max="7" class="ptz-slider"></el-slider>
        </div>
        <div class="ptz-grid">
          <div></div>
          <button class="ptz-btn" @touchstart.prevent="ptzStart(currentCamera, 21)" @touchend.prevent="ptzStop(currentCamera, 21)" @mousedown.prevent="ptzStart(currentCamera, 21)" @mouseup.prevent="ptzStop(currentCamera, 21)" @mouseleave="ptzStop(currentCamera, 21)">↑</button>
          <div></div>
          <button class="ptz-btn" @touchstart.prevent="ptzStart(currentCamera, 23)" @touchend.prevent="ptzStop(currentCamera, 23)" @mousedown.prevent="ptzStart(currentCamera, 23)" @mouseup.prevent="ptzStop(currentCamera, 23)" @mouseleave="ptzStop(currentCamera, 23)">←</button>
          <div class="ptz-center">⊙</div>
          <button class="ptz-btn" @touchstart.prevent="ptzStart(currentCamera, 24)" @touchend.prevent="ptzStop(currentCamera, 24)" @mousedown.prevent="ptzStart(currentCamera, 24)" @mouseup.prevent="ptzStop(currentCamera, 24)" @mouseleave="ptzStop(currentCamera, 24)">→</button>
          <div></div>
          <button class="ptz-btn" @touchstart.prevent="ptzStart(currentCamera, 22)" @touchend.prevent="ptzStop(currentCamera, 22)" @mousedown.prevent="ptzStart(currentCamera, 22)" @mouseup.prevent="ptzStop(currentCamera, 22)" @mouseleave="ptzStop(currentCamera, 22)">↓</button>
          <div></div>
        </div>
        <div class="ptz-extra">
          <button class="ptz-small-btn" @touchstart.prevent="ptzStart(currentCamera, 11)" @touchend.prevent="ptzStop(currentCamera, 11)" @mousedown.prevent="ptzStart(currentCamera, 11)" @mouseup.prevent="ptzStop(currentCamera, 11)" @mouseleave="ptzStop(currentCamera, 11)">放大</button>
          <button class="ptz-small-btn" @touchstart.prevent="ptzStart(currentCamera, 12)" @touchend.prevent="ptzStop(currentCamera, 12)" @mousedown.prevent="ptzStart(currentCamera, 12)" @mouseup.prevent="ptzStop(currentCamera, 12)" @mouseleave="ptzStop(currentCamera, 12)">缩小</button>
        </div>
      </div>
    </div>
    <div v-else class="empty-tip">请选择摄像头</div>
  </div>
</template>

<script>
import { SelectALLCamera, GetRealtimeGasData, GetRealtimeBraceletInfo, GetRealtimeWorkOrders, PTZControl, BaseUrl } from '@/api/api.js'

export default {
  data() {
    return {
      cameras: [],
      selectedCameraId: null,
      gasMonitoringData: [],
      braceletInfoData: [],
      workOrders: [],
      loading: false,
      error: false,
      player: null,
      ptzSpeed: 4,
      API_BASE: BaseUrl.replace(/\/$/, '')
    }
  },
  computed: {
    currentCamera() {
      return this.cameras.find(c => c.Id === this.selectedCameraId) || null
    },
    currentGas() {
      if (!this.currentCamera?.DeviceId) return []
      const order = this.workOrders.find(w => w.DeviceId === this.currentCamera.DeviceId && w.Status === 1)
      if (!order) return []
      return this.gasMonitoringData.filter(g => g.DeviceId === this.currentCamera.DeviceId && g.WorkOrderCode === order.Code)
    },
    currentBracelet() {
      if (!this.currentCamera?.DeviceId) return []
      const order = this.workOrders.find(w => w.DeviceId === this.currentCamera.DeviceId && w.Status === 1)
      if (!order) return []
      return this.braceletInfoData.filter(b => b.DeviceId === this.currentCamera.DeviceId && b.WorkOrderCode === order.Code)
    }
  },
  async mounted() {
    await this.loadCameras()
    await this.fetchData()
    this.refreshTimer = setInterval(this.fetchData, 5000)
  },
  beforeDestroy() {
    if (this.refreshTimer) clearInterval(this.refreshTimer)
    this.destroyPlayer()
  },
  methods: {
    async loadCameras() {
      const list = await SelectALLCamera()
      this.cameras = list || []
      if (this.cameras.length && !this.selectedCameraId) {
        this.selectedCameraId = this.cameras[0].Id
      }
    },
    async fetchData() {
      try {
        const [gasRes, braceletRes, workRes] = await Promise.all([
          GetRealtimeGasData(),
          GetRealtimeBraceletInfo(),
          GetRealtimeWorkOrders()
        ])
        this.gasMonitoringData = Array.isArray(gasRes) ? gasRes : []
        this.braceletInfoData = Array.isArray(braceletRes) ? braceletRes : []
        this.workOrders = workRes || []
      } catch (e) {}
    },
    onCameraChange() {
      this.destroyPlayer()
      this.$nextTick(() => this.initPlayer())
    },
    initPlayer() {
      if (!this.currentCamera || !window.flvjs?.isSupported()) return
      this.destroyPlayer()
      const el = document.getElementById(`m_video_${this.currentCamera.Id}`)
      if (!el) return
      this.loading = true
      this.error = false
      const url = `${this.API_BASE}/api/HK/flv-stream/${this.currentCamera.Id}`
      const p = window.flvjs.createPlayer({ type: 'flv', url, isLive: true }, { enableStashBuffer: false })
      p.attachMediaElement(el)
      p.load()
      p.play()
      this.player = p
      p.on(window.flvjs.Events.ERROR, () => { this.error = true; this.loading = false })
      p.on(window.flvjs.Events.LOADING_COMPLETE, () => { this.loading = false })
    },
    destroyPlayer() {
      if (this.player) {
        try { this.player.destroy() } catch (e) {}
        this.player = null
      }
    },
    retryPlay() {
      this.initPlayer()
    },
    async ptzStart(camera, command) {
      if (!camera?.IP) return
      try {
        await PTZControl({
          ip: camera.IP,
          port: 8000,
          username: 'admin',
          password: 'Cnh321456$',
          channel: 1,
          command,
          stop: 0,
          speed: this.ptzSpeed
        })
      } catch (e) {}
    },
    async ptzStop(camera, command) {
      if (!camera?.IP) return
      try {
        await PTZControl({
          ip: camera.IP,
          port: 8000,
          username: 'admin',
          password: 'Cnh321456$',
          channel: 1,
          command,
          stop: 1,
          speed: this.ptzSpeed
        })
      } catch (e) {}
    }
  },
  watch: {
    currentCamera: {
      handler() { this.$nextTick(() => this.initPlayer()) },
      immediate: true
    }
  }
}
</script>

<style scoped>
.mobile-realtime { padding-bottom: 12px; }
.camera-select-wrap { margin-bottom: 12px; }
.camera-select { width: 100%; }
.camera-select >>> .el-input__inner { background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1); color: #fff; }
.video-area { border-radius: 12px; overflow: hidden; background: #000; }
.camera-name { padding: 10px 12px; background: rgba(0,0,0,0.5); color: #fff; font-size: 14px; }
.video-box {
  width: 100%; height: 220px; background: #000; position: relative;
}
.video-box video { display: block; }
.video-box video { width: 100%; height: 100%; object-fit: contain; }
.loading-state, .error-state {
  position: absolute; inset: 0; display: flex; flex-direction: column; align-items: center; justify-content: center;
  background: rgba(0,0,0,0.8); color: #fff; gap: 8px; font-size: 14px;
}
.error-state { cursor: pointer; color: #f56c6c; }
.data-panel { display: flex; gap: 12px; padding: 12px; background: rgba(255,255,255,0.03); }
.data-section { flex: 1; }
.data-title { font-size: 12px; color: #409eff; margin-bottom: 8px; font-weight: 600; }
.data-list { display: flex; flex-direction: column; gap: 6px; }
.data-row { display: flex; justify-content: space-between; font-size: 13px; color: rgba(255,255,255,0.9); }
.no-data { font-size: 12px; color: rgba(255,255,255,0.4); }
.empty-tip { text-align: center; padding: 60px 20px; color: rgba(255,255,255,0.5); }
.ptz-panel {
  padding: 12px;
  background: rgba(255,255,255,0.03);
  border-top: 1px solid rgba(255,255,255,0.06);
}
.ptz-title { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
.ptz-label { font-size: 13px; font-weight: 600; color: #409eff; }
.ptz-slider { width: 80px; flex-shrink: 0; }
.ptz-slider >>> .el-slider__runway { margin: 4px 0; height: 4px; }
.ptz-grid { display: grid; grid-template-columns: repeat(3, 40px); gap: 6px; justify-content: center; margin-bottom: 10px; }
.ptz-btn {
  width: 40px; height: 40px; background: rgba(255,255,255,0.08);
  border: 1px solid rgba(255,255,255,0.12); border-radius: 8px;
  color: #fff; font-size: 16px; cursor: pointer; -webkit-tap-highlight-color: transparent;
  touch-action: manipulation; user-select: none;
}
.ptz-btn:active { background: #409eff; border-color: #409eff; }
.ptz-center { display: flex; align-items: center; justify-content: center; color: rgba(255,255,255,0.25); font-size: 14px; }
.ptz-extra { display: flex; gap: 8px; justify-content: center; }
.ptz-small-btn {
  flex: 1; max-width: 80px; padding: 8px 0; background: rgba(255,255,255,0.08);
  border: 1px solid rgba(255,255,255,0.12); border-radius: 6px;
  color: #fff; font-size: 12px; cursor: pointer; -webkit-tap-highlight-color: transparent;
  touch-action: manipulation; user-select: none;
}
.ptz-small-btn:active { background: rgba(64,158,255,0.3); border-color: #409eff; }
</style>
