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
    background: linear-gradient(135deg, #667eea 0%, #764ba2 50%, #f093fb 100%);
    background-size: 400% 400%;
    animation: gradientShift 15s ease infinite;
    position: relative;
    overflow: hidden;
}

.login-page::before {
    content: '';
    position: absolute;
    top: -50%;
    left: -50%;
    width: 200%;
    height: 200%;
    background: url('../assets/logo.png') no-repeat;
    background-size: 20vw 6vw;
    background-position: 25px 20px;
    opacity: 0.1;
    animation: float 20s ease-in-out infinite;
}

.login-page::after {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: radial-gradient(circle at 20% 50%, rgba(255, 255, 255, 0.1) 0%, transparent 50%),
                radial-gradient(circle at 80% 80%, rgba(255, 255, 255, 0.1) 0%, transparent 50%);
    pointer-events: none;
}

@keyframes gradientShift {
    0% {
        background-position: 0% 50%;
    }
    50% {
        background-position: 100% 50%;
    }
    100% {
        background-position: 0% 50%;
    }
}

@keyframes float {
    0%, 100% {
        transform: translate(0, 0) rotate(0deg);
    }
    33% {
        transform: translate(30px, -30px) rotate(120deg);
    }
    66% {
        transform: translate(-20px, 20px) rotate(240deg);
    }
}

.login-title {
    text-align: center;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    font-size: 32px;
    font-weight: 700;
    margin-bottom: 10px;
    letter-spacing: 2px;
    text-shadow: 0 2px 10px rgba(102, 126, 234, 0.3);
}

.login-main {
    width: 420px;
    min-height: 480px;
    border-radius: 20px;
    background: rgba(255, 255, 255, 0.95);
    backdrop-filter: blur(10px);
    display: flex;
    flex-direction: column;
    justify-content: space-around;
    padding: 40px 50px;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3),
                0 0 0 1px rgba(255, 255, 255, 0.1) inset;
    position: relative;
    z-index: 1;
    animation: slideUp 0.6s ease-out;
    transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.login-main:hover {
    transform: translateY(-5px);
    box-shadow: 0 25px 70px rgba(0, 0, 0, 0.35),
                0 0 0 1px rgba(255, 255, 255, 0.2) inset;
}

@keyframes slideUp {
    from {
        opacity: 0;
        transform: translateY(30px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.login-input {
    min-height: 200px;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    gap: 20px;
}

.login-input >>> .el-input__inner {
    border-radius: 10px;
    border: 2px solid #e4e7ed;
    padding-left: 40px;
    height: 48px;
    font-size: 14px;
    transition: all 0.3s ease;
}

.login-input >>> .el-input__inner:focus {
    border-color: #667eea;
    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.login-input >>> .el-input__prefix {
    left: 12px;
    color: #909399;
}

.login-input >>> .el-input__inner:focus + .el-input__prefix,
.login-input >>> .el-input__inner:focus ~ .el-input__prefix {
    color: #667eea;
}

.sidentifyContent {
    display: flex;
    flex-direction: row;
    gap: 10px;
    align-items: flex-start;
}

.sidentifyContent >>> .el-input {
    flex: 1;
}

#login-btn {
    width: 100%;
    height: 50px;
    border-radius: 10px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border: none;
    font-size: 16px;
    font-weight: 600;
    letter-spacing: 1px;
    transition: all 0.3s ease;
    box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

#login-btn:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 20px rgba(102, 126, 234, 0.5);
}

#login-btn:active {
    transform: translateY(0);
}

#login-btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
    transform: none;
}
</style>
