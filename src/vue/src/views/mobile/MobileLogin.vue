<template>
  <div class="mobile-login">
    <img src="@/assets/logo.png" alt="Logo" class="logo" />
    <div class="login-form">
      <div class="title">登录</div>
      <el-input v-model="username" placeholder="用户名" prefix-icon="el-icon-user">
      </el-input>
      <el-input v-model="password" type="password" placeholder="密码" prefix-icon="el-icon-lock" show-password>
      </el-input>
      <div class="captcha-row">
        <el-input v-model="verificationCode" placeholder="验证码" class="captcha-input">
        </el-input>
        <v-sidentify @changeCode="changeVerificationCode" class="captcha-box"></v-sidentify>
      </div>
      <el-button type="primary" class="login-btn" @click="login" :loading="operateDisable">
        登录
      </el-button>
    </div>
  </div>
</template>

<script>
import { Login } from '@/api/api'
import Sidentify from '@/components/Sidentify'

export default {
  components: { 'v-sidentify': Sidentify },
  data() {
    return {
      username: '',
      password: '',
      verificationCode: '',
      systemVerificationCode: '',
      operateDisable: false
    }
  },
  methods: {
    login() {
      this.operateDisable = true
      if (this.verificationCode !== this.systemVerificationCode) {
        this.$message.error('验证码错误')
        this.operateDisable = false
        return
      }
      Login({ name: this.username, pass: this.password }).then((res) => {
        const data = res?.data || res
        if (!data) {
          this.$message.error('登录失败')
          this.operateDisable = false
          return
        }
        const access = res?.data?.role || data.role
        const token = res?.data?.token || data.token
        if (!access || !token) {
          this.$message.error('获取信息失败')
          this.operateDisable = false
          return
        }
        localStorage.setItem('access', JSON.stringify(access))
        localStorage.setItem('token', token)
        this.$message.success('登录成功')
        this.$router.push('/m/main')
        this.operateDisable = false
      }).catch(() => {
        this.$message.error('登录失败')
        this.operateDisable = false
      })
    },
    changeVerificationCode(code) {
      this.systemVerificationCode = code
    }
  }
}
</script>

<style scoped>
.mobile-login {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 24px;
  background: #0d1117;
}
.logo { height: 80px; margin-bottom: 24px; }
.login-form {
  width: 100%;
  max-width: 340px;
  padding: 32px 24px;
  background: rgba(255,255,255,0.05);
  border-radius: 20px;
  border: 1px solid rgba(255,255,255,0.1);
}
.title {
  font-size: 24px;
  font-weight: 700;
  color: #fff;
  text-align: center;
  margin-bottom: 24px;
}
.login-form >>> .el-input { margin-bottom: 16px; }
.login-form >>> .el-input__inner {
  background: rgba(255,255,255,0.05) !important;
  border: 1px solid rgba(255,255,255,0.1) !important;
  color: #fff !important;
  height: 48px;
}
.captcha-row { display: flex; gap: 12px; margin-bottom: 16px; }
.captcha-input { flex: 1; }
.captcha-box { flex-shrink: 0; }
.login-btn { width: 100%; height: 48px; font-size: 16px; }
</style>
