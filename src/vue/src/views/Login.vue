<template>
    <div class="container">
        <div class="login-page" style>
            <img src="../assets/logo.png" alt="Logo" class="login-logo" />
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
    background: #010409;
    background-image: 
        radial-gradient(circle at 20% 30%, rgba(64, 158, 255, 0.15) 0%, transparent 50%),
        radial-gradient(circle at 80% 70%, rgba(121, 72, 234, 0.15) 0%, transparent 50%),
        radial-gradient(circle at 50% 50%, rgba(13, 17, 23, 1) 0%, #010409 100%);
    position: relative;
    overflow: hidden;
}

.login-logo {
    position: absolute;
    top: 30px;
    left: 30px;
    height: 120px;
    width: auto;
    z-index: 100;
    transition: transform 0.3s ease;
}

.login-logo:hover {
    transform: scale(1.05);
}

/* 动态能量流线 */
.login-page::before, .login-page::after {
    content: '';
    position: absolute;
    width: 200%;
    height: 100%;
    background: repeating-linear-gradient(
        90deg,
        transparent,
        transparent 100px,
        rgba(64, 158, 255, 0.03) 100px,
        rgba(64, 158, 255, 0.03) 101px
    );
    transform: rotate(-45deg);
    animation: energyFlow 20s linear infinite;
    z-index: 1;
}

.login-page::after {
    background: repeating-linear-gradient(
        90deg,
        transparent,
        transparent 150px,
        rgba(121, 72, 234, 0.03) 150px,
        rgba(121, 72, 234, 0.03) 151px
    );
    animation: energyFlow 30s linear infinite reverse;
}

@keyframes energyFlow {
    from { background-position: 0 0; }
    to { background-position: 1000px 0; }
}

.login-main {
    width: 480px;
    padding: 70px 60px;
    background: rgba(13, 17, 23, 0.4);
    backdrop-filter: blur(40px) saturate(180%);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-radius: 32px;
    box-shadow: 
        0 25px 50px -12px rgba(0, 0, 0, 0.6),
        inset 0 0 20px rgba(64, 158, 255, 0.05);
    z-index: 10;
    position: relative;
    transition: all 0.5s cubic-bezier(0.4, 0, 0.2, 1);
}

.login-main::before {
    content: '';
    position: absolute;
    top: -1px; left: -1px; right: -1px; bottom: -1px;
    background: linear-gradient(135deg, rgba(64, 158, 255, 0.3), transparent 40%, transparent 60%, rgba(121, 72, 234, 0.3));
    border-radius: 32px;
    z-index: -1;
    mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0);
    -webkit-mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0);
    -webkit-mask-composite: xor;
    mask-composite: exclude;
}

.login-main:hover {
    transform: translateY(-8px) scale(1.01);
    border-color: rgba(64, 158, 255, 0.4);
    box-shadow: 0 40px 80px -20px rgba(0, 0, 0, 0.8), 0 0 30px rgba(64, 158, 255, 0.1);
}

.login-title {
    text-align: center;
    background: linear-gradient(135deg, #fff 30%, #409eff 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    font-size: 38px;
    font-weight: 900;
    margin-bottom: 40px;
    letter-spacing: 6px;
    text-transform: uppercase;
    text-shadow: 0 10px 20px rgba(0, 0, 0, 0.3);
}

.login-input {
    display: flex;
    flex-direction: column;
    gap: 30px;
}

.login-input >>> .el-input__inner {
    background: transparent !important;
    border: none !important;
    border-bottom: 2px solid rgba(64, 158, 255, 0.2) !important;
    color: #e6f7ff !important;
    height: 50px;
    border-radius: 0;
    padding-left: 60px !important;
    font-size: 16px;
    transition: all 0.4s ease;
}

.login-input >>> .el-input__inner::placeholder {
    color: rgba(64, 158, 255, 0.4) !important;
}

.login-input >>> .el-input__inner:focus {
    border-bottom-color: #409eff !important;
    box-shadow: 0 4px 15px -4px var(--primary-glow) !important;
    padding-left: 70px !important;
}

.login-input >>> .el-input__prefix {
    left: 5px;
    font-size: 20px;
    color: #409eff !important;
    text-shadow: 0 0 10px rgba(64, 158, 255, 0.5);
    transition: all 0.4s ease;
}

.login-input >>> .el-input__inner:focus + .el-input__prefix {
    color: #fff !important;
    text-shadow: 0 0 15px #409eff;
    transform: scale(1.1);
}

.sidentifyContent {
    display: flex;
    gap: 15px;
    align-items: center;
}

#login-btn {
    margin-top: 50px;
    width: 100%;
    height: 60px;
    background: linear-gradient(135deg, #409eff 0%, #7948ea 100%);
    border: none;
    border-radius: 16px;
    color: #fff;
    font-size: 20px;
    font-weight: 800;
    letter-spacing: 4px;
    box-shadow: 0 10px 25px rgba(64, 158, 255, 0.4);
    transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
    cursor: pointer;
    position: relative;
    overflow: hidden;
}

#login-btn::before {
    content: '';
    position: absolute;
    top: 0; left: -100%;
    width: 100%; height: 100%;
    background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
    transition: 0.5s;
}

#login-btn:hover {
    transform: translateY(-3px) scale(1.03);
    box-shadow: 0 15px 35px rgba(64, 158, 255, 0.5);
}

#login-btn:hover::before {
    left: 100%;
}

#login-btn:active {
    transform: scale(0.98);
}
</style>
