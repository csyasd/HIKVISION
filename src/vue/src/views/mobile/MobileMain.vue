<template>
  <div class="mobile-main">
    <div class="time-card">
      <div class="time">{{ currentTime }}</div>
      <div class="date">{{ currentDate }}</div>
    </div>
    <div class="stats-row">
      <div class="stat-card">
        <div class="stat-value">{{ deviceTotal }}</div>
        <div class="stat-label">设备总数</div>
      </div>
      <div class="stat-card online">
        <div class="stat-value">{{ onlineCount }}</div>
        <div class="stat-label">在线</div>
      </div>
      <div class="stat-card offline">
        <div class="stat-value">{{ offlineCount }}</div>
        <div class="stat-label">离线</div>
      </div>
    </div>
    <div class="map-card">
      <div class="section-title">设备分布</div>
      <div id="mobile_map" class="map-container"></div>
    </div>
    <div class="section-card">
      <div class="section-title">气体监测</div>
      <div class="gas-list" v-if="gasMonitoringData.length">
        <div class="gas-item" v-for="(item, i) in gasMonitoringData.slice(0, 4)" :key="i">
          <span class="gas-name">{{ item.GasName }}</span>
          <span class="gas-value">{{ item.GasValue }}</span>
        </div>
      </div>
      <div v-else class="no-data">暂无数据</div>
    </div>
    <div class="section-card">
      <div class="section-title">手环工人</div>
      <div class="worker-list" v-if="braceletInfoData.length">
        <div class="worker-item" v-for="(item, i) in braceletInfoData.slice(0, 6)" :key="i">
          <span class="worker-name">{{ item.WorkerName }}</span>
          <span class="worker-hr">{{ item.HeartRate || '-' }} bpm</span>
        </div>
      </div>
      <div v-else class="no-data">暂无数据</div>
    </div>
    <div class="video-entry" @click="$router.push('/m/realtime')">
      <i class="el-icon-video-camera"></i>
      <span>查看实时视频</span>
    </div>
  </div>
</template>

<script>
import { SelectALLDevice, GetRealtimeGasData, GetRealtimeBraceletInfo, GetRealtimeWorkOrders } from '@/api/api.js'

export default {
  data() {
    return {
      currentTime: '',
      currentDate: '',
      devices: [],
      deviceTotal: 0,
      onlineCount: 0,
      offlineCount: 0,
      gasMonitoringData: [],
      braceletInfoData: [],
      workOrders: [],
      map: null,
      markers: []
    }
  },
  mounted() {
    this.updateTime()
    setInterval(this.updateTime, 1000)
    this.loadData()
    setInterval(this.loadData, 5000)
    this.initMap()
  },
  beforeDestroy() {
    if (this.map) {
      this.map.destroy()
      this.map = null
    }
  },
  methods: {
    updateTime() {
      const now = new Date()
      this.currentTime = now.toLocaleTimeString('zh-CN', { hour12: false })
      this.currentDate = now.toLocaleDateString('zh-CN', { month: '2-digit', day: '2-digit', weekday: 'short' })
    },
    initMap() {
      if (typeof AMap === 'undefined') {
        const script = document.createElement('script')
        script.src = 'https://webapi.amap.com/maps?v=2.0&key=933b70f0dfaf67b0f950d1682dc27ca1'
        script.onload = () => this.$nextTick(() => this.createMap())
        document.head.appendChild(script)
      } else {
        this.$nextTick(() => this.createMap())
      }
    },
    createMap() {
      const container = document.getElementById('mobile_map')
      if (!container || this.map || typeof AMap === 'undefined') return
      this.map = new AMap.Map('mobile_map', {
        zoom: 12,
        center: [116.4074, 39.9042],
        mapStyle: 'amap://styles/dark',
        viewMode: '2D'
      })
      if (this.devices.length > 0) this.updateMapMarkers()
    },
    updateMapMarkers() {
      if (!this.map) return
      this.map.remove(this.markers)
      this.markers = []
      const validDevices = this.devices.filter(d =>
        d.GpsLongitude && d.GpsLatitude &&
        !isNaN(parseFloat(d.GpsLongitude)) && !isNaN(parseFloat(d.GpsLatitude))
      )
      validDevices.forEach(device => {
        const lng = parseFloat(device.GpsLongitude)
        const lat = parseFloat(device.GpsLatitude)
        const marker = new AMap.Marker({
          position: [lng, lat],
          title: device.Name,
          icon: new AMap.Icon({
            size: new AMap.Size(28, 28),
            image: 'data:image/svg+xml;base64,' + btoa(`
              <svg width="28" height="28" viewBox="0 0 28 28" xmlns="http://www.w3.org/2000/svg">
                <circle cx="14" cy="14" r="10" fill="#409eff" stroke="#fff" stroke-width="2"/>
                <circle cx="14" cy="14" r="4" fill="#fff"/>
              </svg>
            `)
          })
        })
        marker.on('click', () => this.showDeviceInfo(device, lng, lat))
        this.map.add(marker)
        this.markers.push(marker)
      })
      if (this.markers.length > 0) this.map.setFitView()
    },
    showDeviceInfo(device, lng, lat) {
      const status = device.OnlineStatus || '离线'
      const color = status === '在线' ? '#67c23a' : '#909399'
      const info = new AMap.InfoWindow({
        content: `
          <div style="color:#e6edf3;padding:12px;background:rgba(13,17,23,0.95);border:1px solid rgba(255,255,255,0.1);border-radius:8px;min-width:180px;font-size:13px;">
            <div style="font-weight:700;color:#409eff;margin-bottom:8px;">${device.Name}</div>
            <div>状态: <span style="color:${color};font-weight:600;">${status}</span></div>
            <div>IP: ${device.IP || '未知'}</div>
          </div>
        `,
        offset: new AMap.Pixel(0, -25)
      })
      info.open(this.map, [lng, lat])
    },
    async loadData() {
      try {
        const devices = await SelectALLDevice()
        this.devices = devices || []
        this.deviceTotal = this.devices.length
        this.onlineCount = this.devices.filter(d => d.OnlineStatus === '在线').length
        this.offlineCount = this.deviceTotal - this.onlineCount
        this.updateMapMarkers()

        const gasRes = await GetRealtimeGasData()
        this.gasMonitoringData = (gasRes && Array.isArray(gasRes)) ? gasRes : []

        const braceletRes = await GetRealtimeBraceletInfo()
        this.braceletInfoData = (braceletRes && Array.isArray(braceletRes)) ? braceletRes : []
      } catch (e) {
        console.error(e)
      }
    }
  }
}
</script>

<style scoped>
.mobile-main { padding-bottom: 12px; }
.time-card {
  background: linear-gradient(135deg, rgba(64, 158, 255, 0.15), rgba(121, 72, 234, 0.1));
  border-radius: 16px;
  padding: 20px;
  margin-bottom: 16px;
  border: 1px solid rgba(64, 158, 255, 0.2);
}
.time { font-size: 28px; font-weight: 700; color: #fff; }
.date { font-size: 14px; color: rgba(255,255,255,0.7); margin-top: 4px; }
.stats-row { display: flex; gap: 12px; margin-bottom: 16px; }
.stat-card {
  flex: 1;
  background: rgba(255,255,255,0.05);
  border-radius: 12px;
  padding: 16px;
  text-align: center;
  border: 1px solid rgba(255,255,255,0.08);
}
.stat-card.online .stat-value { color: #67c23a; }
.stat-card.offline .stat-value { color: #f56c6c; }
.stat-value { font-size: 24px; font-weight: 700; color: #fff; }
.stat-label { font-size: 12px; color: rgba(255,255,255,0.6); margin-top: 4px; }
.section-card {
  background: rgba(255,255,255,0.03);
  border-radius: 16px;
  padding: 16px;
  margin-bottom: 16px;
  border: 1px solid rgba(255,255,255,0.06);
}
.section-title { font-size: 14px; color: #409eff; font-weight: 600; margin-bottom: 12px; }
.gas-list, .worker-list { display: flex; flex-direction: column; gap: 8px; }
.gas-item, .worker-item {
  display: flex; justify-content: space-between; align-items: center;
  padding: 10px 12px; background: rgba(255,255,255,0.03); border-radius: 8px;
}
.gas-name, .worker-name { color: rgba(255,255,255,0.8); font-size: 14px; }
.gas-value, .worker-hr { color: #fff; font-weight: 600; }
.no-data { color: rgba(255,255,255,0.4); font-size: 14px; text-align: center; padding: 20px; }
.video-entry {
  display: flex; align-items: center; justify-content: center; gap: 8px;
  padding: 16px; background: linear-gradient(135deg, #409eff, #7948ea);
  border-radius: 16px; color: #fff; font-weight: 600; cursor: pointer;
}
.video-entry i { font-size: 24px; }
.map-card {
  background: rgba(255,255,255,0.03);
  border-radius: 16px;
  padding: 16px;
  margin-bottom: 16px;
  border: 1px solid rgba(255,255,255,0.06);
}
.map-container {
  width: 100%;
  height: 180px;
  border-radius: 12px;
  overflow: hidden;
  background: #1a1a2e;
}
</style>
