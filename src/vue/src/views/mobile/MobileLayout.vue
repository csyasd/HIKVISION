<template>
  <div class="mobile-layout">
    <div class="mobile-header">
      <span v-if="showBack" class="back-btn" @click="goBack">
        <i class="el-icon-arrow-left"></i>
      </span>
      <h1 class="mobile-title">{{ headerTitle }}</h1>
      <div class="header-actions">
        <span v-if="showBack" class="header-placeholder"></span>
        <span v-else class="logout-btn" @click="logout">退出</span>
      </div>
    </div>
    <main class="mobile-content">
      <router-view />
    </main>
    <nav class="mobile-bottom-nav">
      <router-link to="/m/main" class="nav-item" active-class="active">
        <i class="el-icon-s-home"></i>
        <span>首页</span>
      </router-link>
      <router-link to="/m/realtime" class="nav-item" active-class="active">
        <i class="el-icon-data-line"></i>
        <span>实时数据</span>
      </router-link>
      <router-link to="/m/menu" class="nav-item" :class="{ active: isMenuActive }">
        <i class="el-icon-menu"></i>
        <span>更多</span>
      </router-link>
    </nav>
  </div>
</template>

<script>
const PAGE_TITLES = {
  'work-bracelet': '作业手环管理',
  'worker-status': '工人进出状态记录',
  'gas-alarm': '气体报警记录',
  'bracelet-abnormal': '心率异常记录',
  'gas-abnormal': '气体异常记录',
  'work-order': '施工工单',
  'device': '设备管理',
  'camera': '摄像头管理'
}

export default {
  computed: {
    showBack() {
      const p = this.$route.path
      return p !== '/m/main' && p !== '/m/realtime' && p !== '/m/menu'
    },
    isMenuActive() {
      const p = this.$route.path
      return p === '/m/menu' || (p.startsWith('/m/') && p !== '/m/main' && p !== '/m/realtime')
    },
    headerTitle() {
      if (this.showBack) {
        const name = this.$route.name
        return PAGE_TITLES[name] || name || '详情'
      }
      return '川纳海智慧云安'
    }
  },
  methods: {
    goBack() {
      this.$router.push('/m/menu')
    },
    logout() {
      this.$message.success('已退出')
      localStorage.removeItem('token')
      localStorage.removeItem('access')
      this.$router.push('/m/login')
    }
  }
}
</script>

<style scoped>
.mobile-layout {
  min-height: 100vh;
  padding-bottom: 60px;
  background: #0d1117;
}
.mobile-header {
  position: sticky;
  top: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  background: rgba(13, 17, 23, 0.95);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}
.mobile-title {
  margin: 0;
  font-size: 18px;
  font-weight: 700;
  color: #409eff;
  background: linear-gradient(135deg, #409eff, #7948ea);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}
.back-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  color: #409eff;
  font-size: 20px;
  cursor: pointer;
  flex-shrink: 0;
}
.header-placeholder {
  width: 36px;
  flex-shrink: 0;
}
.mobile-header .mobile-title {
  flex: 1;
  text-align: center;
}
.logout-btn {
  font-size: 14px;
  color: rgba(255, 255, 255, 0.7);
  padding: 6px 12px;
  cursor: pointer;
}
.mobile-content {
  padding: 12px;
  min-height: calc(100vh - 120px);
}
.mobile-bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 56px;
  display: flex;
  align-items: center;
  justify-content: space-around;
  background: rgba(13, 17, 23, 0.98);
  backdrop-filter: blur(20px);
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  z-index: 100;
  padding-bottom: env(safe-area-inset-bottom);
}
.nav-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  color: rgba(255, 255, 255, 0.5);
  text-decoration: none;
  font-size: 11px;
  transition: all 0.2s;
}
.nav-item i {
  font-size: 22px;
}
.nav-item.active {
  color: #409eff;
}
.mobile-content >>> .el-input__inner,
.mobile-content >>> .el-select .el-input .el-input__inner {
  background: rgba(255,255,255,0.05);
  border: 1px solid rgba(255,255,255,0.1);
  color: #fff;
}
</style>
