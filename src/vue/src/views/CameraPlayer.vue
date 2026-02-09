<template>
    <div id="camera-player-container" style="width: 100%; height: 100%; background: #000;">
        <!-- WebVideoCtrl全局插件容器 -->
        <div id="divPlugin" style="width: 100%; height: 100%;"></div>
        
        <!-- 加载状态提示 -->
        <div v-if="!webVideoCtrlLoaded" style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); color: white; text-align: center;">
            <i class="el-icon-loading" style="font-size: 24px;"></i>
            <p>正在加载插件...</p>
        </div>
    </div>
</template>

<script>
export default {
    name: 'CameraPlayer',
    data() {
        return {
            webVideoCtrlLoaded: false,
            camera: null,
            deviceLogins: {},
            players: {},
            ptzSpeed: 4
        }
    },
    mounted() {
        // 从URL参数获取摄像头信息
        const cameraId = this.$route.query.cameraId;
        const cameraIP = this.$route.query.ip;
        const cameraPort = this.$route.query.port || 80;
        const cameraUsername = this.$route.query.username || 'admin';
        const cameraPassword = this.$route.query.password || 'Cnh321456$';
        const cameraChannel = this.$route.query.channel || 1;
        const cameraName = this.$route.query.name || '摄像头';
        
        this.camera = {
            Id: cameraId,
            IP: cameraIP,
            Port: cameraPort,
            Username: cameraUsername,
            Password: cameraPassword,
            Channel: cameraChannel,
            Name: cameraName
        };
        
        if (!this.camera.IP) {
            console.error('缺少摄像头IP参数');
            return;
        }
        
        // 监听来自父窗口的PTZ控制消息
        window.addEventListener('message', this.handlePTZMessage);
        
        // 初始化WebVideoCtrl
        this.initWebVideoCtrl();
    },
    beforeDestroy() {
        // 移除消息监听
        window.removeEventListener('message', this.handlePTZMessage);
        
        // 清理播放器
        if (this.players[this.camera?.Id] && typeof WebVideoCtrl !== 'undefined') {
            try {
                WebVideoCtrl.I_Stop({
                    success: () => {
                        console.log('视频已停止');
                    }
                });
            } catch (error) {
                console.error('停止视频失败:', error);
            }
        }
    },
    methods: {
        // 初始化WebVideoCtrl插件
        initWebVideoCtrl() {
            if (typeof WebVideoCtrl === 'undefined') {
                console.log('WebVideoCtrl未加载，等待...');
                setTimeout(() => {
                    this.initWebVideoCtrl();
                }, 1000);
                return;
            }

            if (this.webVideoCtrlLoaded) {
                return;
            }

            console.log('开始初始化WebVideoCtrl插件...');
            
            // 初始化插件参数（单窗口模式）
            WebVideoCtrl.I_InitPlugin({
                bWndFull: true,
                iWndowType: 1,  // 单窗口
                szBasePath: '/static/',
                cbSelWnd: (xmlDoc) => {
                    if (typeof $ !== 'undefined' && xmlDoc) {
                        const wndIndex = parseInt($(xmlDoc).find("SelectWnd").eq(0).text(), 10);
                        console.log(`当前选择的窗口编号：${wndIndex}`);
                    }
                },
                cbDoubleClickWnd: (iWndIndex, bFullScreen) => {
                    console.log(`窗口${iWndIndex} ${bFullScreen ? '全屏' : '还原'}`);
                },
                cbEvent: (iEventType, iParam1, iParam2) => {
                    if (2 == iEventType) {
                        console.log(`窗口${iParam1}回放结束`);
                    } else if (-1 == iEventType) {
                        console.log(`设备${iParam1}网络错误`);
                    }
                },
                cbInitPluginComplete: () => {
                    console.log('✅ WebVideoCtrl插件初始化完成');
                    this.webVideoCtrlLoaded = true;
                    this.$nextTick(() => {
                        this.insertPlugin();
                    });
                }
            });
        },

        // 插入插件
        async insertPlugin() {
            if (!this.webVideoCtrlLoaded || typeof WebVideoCtrl === 'undefined') {
                return;
            }

            console.log('开始插入插件...');
            
            try {
                await WebVideoCtrl.I_InsertOBJECTPlugin("divPlugin");
                console.log('✅ 插件插入成功');
                
                // 等待插件完全初始化
                await new Promise(resolve => setTimeout(resolve, 3000));
                
                // 检查插件版本
                try {
                    await WebVideoCtrl.I_CheckPluginVersion();
                    console.log('✅ 插件就绪检查通过');
                } catch (checkError) {
                    console.log('⚠️ 插件就绪检查失败，但继续尝试');
                }
                
                // 再等待一下
                await new Promise(resolve => setTimeout(resolve, 2000));
                console.log('✅ 插件初始化完成，开始播放');
                
                // 开始播放
                this.playCamera();
            } catch (error) {
                console.error('插件插入失败:', error);
            }
        },

        // 播放摄像头
        async playCamera() {
            if (!this.camera) {
                return;
            }

            try {
                console.log(`准备播放摄像头: ${this.camera.Name}(${this.camera.IP})`);
                
                const deviceIdentify = `${this.camera.IP}_${this.camera.Port || 80}`;
                const wndIndex = 0;
                
                // 先登录设备
                if (!this.deviceLogins[deviceIdentify]) {
                    await this.loginDevice(deviceIdentify);
                }
                
                // 等待一下
                await new Promise(resolve => setTimeout(resolve, 1000));
                
                // 开始预览播放
                const iChannelID = this.camera.Channel || 1;
                const iStreamType = 1; // 主码流
                
                WebVideoCtrl.I_StartRealPlay(deviceIdentify, {
                    iWndIndex: wndIndex,
                    iStreamType: iStreamType,
                    iChannelID: iChannelID,
                    bZeroChannel: false,
                    success: () => {
                        console.log(`✅ ${this.camera.Name} 播放成功`);
                        // 调整窗口大小
                        if (typeof $ !== 'undefined') {
                            setTimeout(() => {
                                WebVideoCtrl.I_Resize();
                            }, 500);
                        }
                    },
                    error: (oError) => {
                        console.error(`❌ ${this.camera.Name} 播放失败: ${oError.errorCode} - ${oError.errorMsg}`);
                    }
                });
                
            } catch (error) {
                console.error('播放初始化异常:', error);
            }
        },

        // 登录设备
        async loginDevice(deviceIdentify) {
            return new Promise((resolve, reject) => {
                const szIP = this.camera.IP;
                const szPort = this.camera.Port || 80;
                const szUsername = this.camera.Username || 'admin';
                const szPassword = this.camera.Password || 'Cnh321456$';
                const szProtoType = 1; // http
                
                console.log(`正在登录设备: ${szIP}:${szPort}, 用户名: ${szUsername}`);
                
                WebVideoCtrl.I_Login(szIP, szProtoType, szPort, szUsername, szPassword, {
                    timeout: 15000,
                    success: (xmlDoc) => {
                        console.log(`✅ 设备登录成功: ${deviceIdentify}`);
                        this.deviceLogins[deviceIdentify] = true;
                        resolve();
                    },
                    error: (oError) => {
                        console.error(`❌ 设备登录失败: ${deviceIdentify} - ${oError.errorCode} - ${oError.errorMsg}`);
                        reject(oError);
                    }
                });
            });
        },
        
        // 处理来自父窗口的PTZ控制消息
        handlePTZMessage(event) {
            // 验证消息来源（可选，提高安全性）
            // if (event.origin !== window.location.origin) return;
            
            if (event.data && event.data.type === 'ptz') {
                const { command, stop, speed } = event.data;
                this.ptzSpeed = speed || this.ptzSpeed;
                this.executePTZ(command, stop);
            }
        },
        
        // 执行云台控制
        async executePTZ(command, stop) {
            if (typeof WebVideoCtrl === 'undefined' || !this.deviceLogins[`${this.camera.IP}_${this.camera.Port || 80}`]) {
                console.warn('WebVideoCtrl未就绪或设备未登录');
                return;
            }
            
            try {
                // WebVideoCtrl PTZ命令映射
                const commandMap = {
                    21: 1,   // 上
                    22: 2,   // 下
                    23: 3,   // 左
                    24: 4,   // 右
                    11: 10,  // 变倍+ (放大)
                    12: 11,  // 变倍- (缩小)
                    13: 12,  // 聚焦+ (近焦)
                    14: 13   // 聚焦- (远焦)
                };
                
                const ptzIndex = commandMap[command] || command;
                
                WebVideoCtrl.I_PTZControl(ptzIndex, stop, {
                    iWndIndex: 0,
                    iPTZSpeed: this.ptzSpeed,
                    success: () => {
                        console.log(`云台控制: ${stop ? '停止' : '执行'} 命令${command}`);
                    },
                    error: (oError) => {
                        console.error(`云台控制失败: ${oError.errorMsg || '未知错误'}`);
                    }
                });
            } catch (error) {
                console.error('PTZ执行错误:', error);
            }
        }
    }
}
</script>

<style scoped>
#camera-player-container {
    width: 100%;
    height: 100%;
    position: relative;
    overflow: hidden;
}
</style>

