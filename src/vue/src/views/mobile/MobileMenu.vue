<template>
  <div class="mobile-menu">
    <div class="menu-list">
      <div class="menu-item" @click="go('/m/work-bracelet')">
        <i class="el-icon-watch"></i>
        <span>作业手环管理</span>
        <i class="el-icon-arrow-right"></i>
      </div>
      <div class="menu-item" @click="go('/m/worker-status')">
        <i class="el-icon-s-custom"></i>
        <span>工人进出状态记录</span>
        <i class="el-icon-arrow-right"></i>
      </div>
      <div class="menu-item" @click="go('/m/gas-alarm')">
        <i class="el-icon-bell"></i>
        <span>气体报警记录</span>
        <i class="el-icon-arrow-right"></i>
      </div>
      <div class="menu-item" @click="go('/m/bracelet-abnormal')">
        <i class="el-icon-warning"></i>
        <span>心率异常记录</span>
        <i class="el-icon-arrow-right"></i>
      </div>
      <div class="menu-item" @click="go('/m/gas-abnormal')">
        <i class="el-icon-warning-outline"></i>
        <span>气体异常记录</span>
        <i class="el-icon-arrow-right"></i>
      </div>
      <div class="menu-item" @click="go('/m/work-order')">
        <i class="el-icon-document"></i>
        <span>施工工单</span>
        <i class="el-icon-arrow-right"></i>
      </div>
      <div class="menu-item" @click="go('/m/device')">
        <i class="el-icon-monitor"></i>
        <span>设备管理</span>
        <i class="el-icon-arrow-right"></i>
      </div>
      <div class="menu-item" @click="go('/m/camera')">
        <i class="el-icon-video-camera"></i>
        <span>摄像头管理</span>
        <i class="el-icon-arrow-right"></i>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  methods: {
    go(path) {
      this.$router.push(path)
    }
  }
}
</script>

<style scoped>
.mobile-menu { padding-bottom: 12px; }
.menu-list { display: flex; flex-direction: column; gap: 8px; }
.menu-item {
  display: flex; align-items: center; gap: 12px; padding: 16px;
  background: rgba(255,255,255,0.05); border-radius: 12px;
  color: #fff; font-size: 15px; cursor: pointer;
  border: 1px solid rgba(255,255,255,0.08);
}
.menu-item i:first-child { font-size: 20px; color: #409eff; }
.menu-item span { flex: 1; }
.menu-item .el-icon-arrow-right { color: rgba(255,255,255,0.4); font-size: 14px; }
</style>
