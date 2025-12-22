<template>
    <div class="container">
        <div class="login-page" style>
            <div class="login-main">
                <div class="login-title">登录</div>
                <div class="login-input">
                    <el-input placeholder="请输入用户名" v-model="username">
                        <i
                            slot="prefix"
                            class="el-input__icon el-icon-user-solid"
                        ></i>
                    </el-input>
                    <el-input
                        placeholder="请输入密码"
                        v-model="password"
                        show-password
                    >
                        <i
                            slot="prefix"
                            class="el-input__icon el-icon-lock"
                        ></i>
                    </el-input>
                    <div class="sidentifyContent">
                        <el-input
                            placeholder="请输入验证码"
                            v-model="verificationCode"
                        >
                            <i
                                slot="prefix"
                                class="el-input__icon el-icon-s-claim"
                            ></i>
                        </el-input>
                        <v-sidentify
                            @changeCode="changeVerificationCode"
                        ></v-sidentify>
                    </div>
                </div>
                <el-button
                    id="login-btn"
                    type="primary"
                    @click="login"
                    :disabled="operateDisable"
                    >登录</el-button
                >
            </div>
        </div>
    </div>
</template>
<script>
import { Login } from '../api/api';
import Sidentify from '../components/Sidentify'; //**引入验证码组件**
export default {
    components: {
        'v-sidentify': Sidentify,
    },
    data() {
        return {
            username: '',
            password: '',
            verificationCode: '',
            systemVerificationCode: '',
            operateDisable: false,
        };
    },
    methods: {
        login: function () {
            this.operateDisable = true;
            if (this.verificationCode != this.systemVerificationCode) {
                this.$message.error('验证码输入错误');
                this.operateDisable = false;
                return;
            }

            let data = {
                name: this.username,
                pass: this.password,
            };
            Login(data).then((res) => {
                console.log(res);
                if (res?.status != 200) {
                    this.$message.error('账号或密码错误！');
                    this.operateDisable = false;
                    return;
                }

                let access = res.role;

                localStorage.setItem('access', JSON.stringify(access));

                let token = res.token;

                localStorage.setItem('token', token);

                //提示登录成功
                this.$notify({
                    title: '登录成功',
                    message: '您好，欢迎使用YixiaoAdmin',
                    type: 'success',
                });
                //跳转至首页
                this.$router.push({
                    path: 'home/main',
                });
                this.operateDisable = false;
            });
        },
        changeVerificationCode(code) {
            this.systemVerificationCode = code;
        },
    },
    mounted() {
        if (window.Cypress) {
            window.Login = this;
        }
    },
};
</script>
<style scoped>
.container {
    height: 100vh;
    width: 100vw;
    overflow: hidden;
}

.login-page {
    display: flex;
    align-items: center;
    justify-content: center;
    height: 100vh;
    background: radial-gradient(circle at 20% 20%, #0d1117 0%, #010409 100%);
    position: relative;
    overflow: hidden;
}

.login-page::before {
    content: '';
    position: absolute;
    width: 200%;
    height: 200%;
    background-image: 
        radial-gradient(circle at 50% 50%, rgba(64, 158, 255, 0.05) 0%, transparent 50%),
        radial-gradient(circle at 20% 80%, rgba(121, 72, 234, 0.05) 0%, transparent 40%);
    animation: rotate 60s linear infinite;
}

@keyframes rotate {
    from { transform: rotate(0deg); }
    to { transform: rotate(360deg); }
}

.login-title {
    text-align: center;
    background: linear-gradient(135deg, #409eff 0%, #7948ea 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    font-size: 36px;
    font-weight: 800;
    margin-bottom: 30px;
    letter-spacing: 4px;
    text-transform: uppercase;
    text-shadow: 0 0 30px rgba(64, 158, 255, 0.2);
}

.login-main {
    width: 460px;
    padding: 60px;
    background: rgba(13, 17, 23, 0.6);
    backdrop-filter: blur(30px) saturate(180%);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-radius: 24px;
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
    z-index: 10;
    transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}

.login-main:hover {
    border-color: rgba(64, 158, 255, 0.3);
    box-shadow: 0 0 40px rgba(64, 158, 255, 0.1);
    transform: translateY(-5px);
}

.login-input {
    display: flex;
    flex-direction: column;
    gap: 24px;
}

.login-input >>> .el-input__inner {
    background: rgba(0, 0, 0, 0.3) !important;
    border: 1px solid rgba(255, 255, 255, 0.1) !important;
    color: #fff !important;
    height: 54px;
    border-radius: 12px;
    padding-left: 48px;
    font-size: 15px;
    transition: all 0.3s ease;
}

.login-input >>> .el-input__inner:focus {
    border-color: #409eff !important;
    box-shadow: 0 0 0 4px rgba(64, 158, 255, 0.1) !important;
    background: rgba(0, 0, 0, 0.4) !important;
}

.login-input >>> .el-input__prefix {
    left: 16px;
    font-size: 18px;
    color: rgba(255, 255, 255, 0.3);
}

.login-input >>> .el-input__inner:focus + .el-input__prefix {
    color: #409eff;
}

.sidentifyContent {
    display: flex;
    gap: 12px;
}

#login-btn {
    margin-top: 40px;
    width: 100%;
    height: 56px;
    background: linear-gradient(135deg, #409eff 0%, #7948ea 100%);
    border: none;
    border-radius: 14px;
    color: #fff;
    font-size: 18px;
    font-weight: 700;
    letter-spacing: 2px;
    box-shadow: 0 8px 20px rgba(64, 158, 255, 0.3);
    transition: all 0.3s ease;
}

#login-btn:hover {
    transform: translateY(-2px) scale(1.02);
    box-shadow: 0 12px 30px rgba(64, 158, 255, 0.4);
    opacity: 0.95;
}

#login-btn:active {
    transform: translateY(0);
}
</style>
