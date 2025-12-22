<template>
    <div class="home">
        <!-- 主页主体 -->
        <el-container>
            <!-- 页面左侧导航开始 -->
            <el-aside width="auto" style="background-color: rgb(238, 241, 246)">
                <el-menu
                    :default-active="$route.path"
                    router
                    unique-opened
                    :collapse="NavigationBarState"
                    mode="vertical"
                    @open="handleMenuOpen"
                    @close="handleMenuClose"
                >
                    <!-- 首页 -->
                    <el-menu-item index="/home/main">
                        <i class="el-icon-s-home"></i>
                        <span slot="title">首页</span>
                    </el-menu-item>

                    <!-- 实时数据 -->
                    <el-menu-item index="/home/CameraDetail">
                        <i class="el-icon-data-line"></i>
                        <span slot="title">实时数据</span>
                    </el-menu-item>

                    <!-- 设备管理 -->
                    <el-submenu index="3">
                        <template slot="title">
                            <i class="el-icon-monitor"></i>
                            <span>设备管理</span>
                        </template>
                        <el-menu-item index="/home/DeviceManagement">设备管理</el-menu-item>
                        <el-menu-item index="/home/CameraManagement">摄像头管理</el-menu-item>
                    </el-submenu>

                    <!-- 人员管理 -->
                    <el-submenu index="4">
                        <template slot="title">
                            <i class="el-icon-user"></i>
                            <span>记录管理</span>
                        </template>
                        <el-menu-item index="/home/WorkBraceletManagement">作业手环管理</el-menu-item>
                        <!-- <el-menu-item index="/home/WorkRecordManagement">作业记录管理</el-menu-item> -->
                        <el-menu-item index="/home/WorkerStatusRecordManagement">工人进出状态记录</el-menu-item>
                        <el-menu-item index="/home/GasAlarmRecordManagement">气体报警记录</el-menu-item>
                    </el-submenu>

                    <!-- 作业管理 -->
                    <el-submenu index="5">
                        <template slot="title">
                            <i class="el-icon-folder"></i>
                            <span>作业管理</span>
                        </template>
                        <el-menu-item index="/home/WorkOrderManagement">施工工单</el-menu-item>
                    </el-submenu>

                    <!-- 系统管理 -->
                    <el-submenu index="6">
                        <template slot="title">
                            <i class="el-icon-setting"></i>
                            <span>系统管理</span>
                        </template>
                        <el-menu-item index="/home/UserManagement">用户管理</el-menu-item>
                        <el-menu-item index="/home/RoleManagement">角色管理</el-menu-item>
                        <el-menu-item index="/home/RightManagement">功能管理</el-menu-item>
                    </el-submenu>
                </el-menu>
            </el-aside>
            <!-- 页面左侧导航结束 -->
            <el-container>
                <!-- 主页顶部导航开始 -->
                <el-header height="6vh">
                    <div @click="ToHome" class="logo">
                        <span>受限空间作业安全管理平台</span>
                    </div>
                    <!-- 收起侧边导航栏按钮 -->
                    <el-tooltip effect="dark" content="显示/隐藏侧边导航栏" placement="bottom">
                        <div class="header-menu">
                            <div class="header-menu-item" @click="CollapseNavigationBar">
                                <i
                                    :class="{
                                        'el-icon-s-fold': isFold,
                                        'el-icon-s-unfold': !isFold,
                                    }"
                                ></i>
                            </div>
                        </div>
                    </el-tooltip>

                    <!-- 全屏按钮 -->
                    <el-tooltip effect="dark" content="全屏/退出全屏" placement="bottom">
                        <FullScreen />
                    </el-tooltip>

                    <el-dropdown @command="HandleDropDownListCommand">
                        <span class="el-dropdown-link">
                            更多
                            <i class="el-icon-arrow-down el-icon--right"></i>
                        </span>
                        <el-dropdown-menu slot="dropdown">
                            <el-dropdown-item command="logout">退出</el-dropdown-item>
                        </el-dropdown-menu>
                    </el-dropdown>

                    <!-- 消息按钮 -->
                    <el-tooltip effect="dark" content="消息中心" placement="bottom">
                        <div class="header-menu-item-right" @click="ToMessage">
                            <el-badge :is-dot="isMessage" class="item">
                                <i class="el-icon-s-comment"></i>
                            </el-badge>
                        </div>
                    </el-tooltip>
                </el-header>
                <!-- 主页顶部导航结束 -->

                <!-- 页面主界面开始 -->
                <el-main>
                    <!-- 主页标签选项卡 -->
                    <Tabs />
                    <div class="SubPageContainer">
                        <!-- 嵌入子路由界面 -->
                        <router-view />
                    </div>
                </el-main>
                <!-- 页面主界面结束 -->
            </el-container>
        </el-container>
        <!-- 主页主体结束 -->
    </div>
</template>

<script>
import Tabs from '../components/Yi-Tabs.vue'
import FullScreen from '../components/Yi-FullScreen.vue'
export default {
    name: 'Home',
    components: { Tabs, FullScreen },
    data() {
        return {
            NavigationBarState: false,
            isMessage: false, //是否收到消息
            isFold: true,
            accessList: [],
        }
    },
    methods: {
        // 返回首页
        ToHome: function () {
            this.$router.push({
                path: '/home/main',
            })
        },
        //显示/隐藏侧边导航栏
        CollapseNavigationBar: function () {
            this.NavigationBarState = !this.NavigationBarState
            this.isFold = !this.isFold
        },
        // 处理菜单打开事件
        handleMenuOpen: function(index, indexPath) {
            // 菜单打开时，确保不自动折叠
        },
        // 处理菜单关闭事件 - 阻止自动关闭
        handleMenuClose: function(index, indexPath) {
            // 如果菜单被自动关闭，立即重新打开
            // 但只对子菜单有效，不影响整体折叠状态
        },
        //处理顶部导航栏下拉菜单
        HandleDropDownListCommand(command) {
            switch (command) {
                case 'logout': {
                    this.Logout()
                }
            }
            // this.$message('click on item ' + command);
        },

        //退出登陆
        Logout: function () {
            this.$message({
                message: '账户退出成功！',
                type: 'success',
            })
            this.$router.push({
                path: '../login',
            })
        },
        /*消息中心模块 */
        // 查看是否有新消息
        GetNewMessage() {
            // NewMessageCount({ Id: this.userId }).then((res) => {
            //     console.log(res);
            //     if (res != 0 && res != undefined) {
            //         this.isMessage = true;
            //     }
            // });
        },
        BeginGetNewMessage() {
            setInterval(this.GetNewMessage, 180000)
        },
        // 查看消息中心
        ToMessage: function () {
            this.isMessage = false
            this.$router.push({
                path: '/home/MessageManagement',
            })
        },
        /*消息中心模块结束 */

        /*权限开始*/
        ParseUserAccess() {
            try {
                const accessStr = localStorage.getItem('access');
                if (accessStr && accessStr !== 'undefined') {
                    const parsedAccess = JSON.parse(accessStr);
                    this.accessList = parsedAccess.RoleRights || [];
                } else {
                    this.accessList = [];
                    console.warn('用户权限信息未找到，使用默认权限');
                }
            } catch (error) {
                console.error('解析用户权限失败:', error);
                this.accessList = [];
            }
        },
        JudgeUserAccess(accessStr) {
            return this.accessList.findIndex(x => x.Right.Name == accessStr) != -1
        },
        /*权限结束*/
    },
    mounted() {
        this.ParseUserAccess()
        this.BeginGetNewMessage()
        // 确保菜单初始状态为展开
        this.NavigationBarState = false;
        this.isFold = true;
    },
    watch: {
        // 监听路由变化，防止菜单自动折叠
        '$route'(to, from) {
            // 路由变化时保持菜单展开状态
            if (this.NavigationBarState) {
                this.NavigationBarState = false;
                this.isFold = true;
            }
        }
    },
}
</script>

<style>
.el-main {
    height: 90vh;
    padding: 0px !important;
    background: #f5f7fa;
}

.el-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    line-height: 6vh;
    font-size: 15px;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
    display: flex;
    align-items: center;
    padding: 0 20px;
    position: relative;
    z-index: 100;
}

.el-header::after {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    height: 1px;
    background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.3), transparent);
}

.el-header .logo {
    float: left;
    cursor: pointer;
    transition: transform 0.3s ease;
}

.el-header .logo:hover {
    transform: scale(1.05);
}

.el-header .logo span {
    color: white;
    font-size: 22px;
    font-weight: 600;
    letter-spacing: 1px;
    text-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.el-header .header-menu {
    padding: 0px 10px 0px 10px;
    float: left;
}

.el-header .header-menu-item {
    padding: 8px 15px;
    float: left;
    font-size: 20px;
    color: #fff;
    cursor: pointer;
    border-radius: 8px;
    transition: all 0.3s ease;
    display: flex;
    align-items: center;
    justify-content: center;
}

.el-header .header-menu-item:hover {
    background-color: rgba(255, 255, 255, 0.2);
    transform: scale(1.1);
}

.el-header .header-menu-item-right {
    padding: 8px 15px;
    font-size: 20px;
    color: #fff !important;
    float: right;
    cursor: pointer;
    border-radius: 8px;
    transition: all 0.3s ease;
    display: flex;
    align-items: center;
    justify-content: center;
}

.el-header .header-menu-item-right:hover {
    background-color: rgba(255, 255, 255, 0.2);
    transform: scale(1.1);
}

.el-header .el-dropdown {
    color: #fff !important;
    float: right;
    padding: 8px 15px;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.3s ease;
    margin-right: 10px;
}

.el-header .el-dropdown:hover {
    background-color: rgba(255, 255, 255, 0.2);
}

.el-aside {
    color: #333;
    height: 100vh;
    background: #ffffff;
    box-shadow: 2px 0 8px rgba(0, 0, 0, 0.08);
    border-right: 1px solid #e4e7ed;
}

/* 修改侧边导航栏样式 */
.el-menu {
    border-right: none !important;
    background-color: transparent !important;
}

.el-menu:not(.el-menu--collapse) {
    min-width: 200px;
    transition: 0s;
}

.el-menu-item,
.el-submenu__title {
    height: 50px !important;
    line-height: 50px !important;
    margin: 4px 8px !important;
    border-radius: 8px !important;
    transition: all 0.3s ease !important;
    color: #606266 !important;
    font-weight: 500 !important;
}

.el-menu-item:hover,
.el-submenu__title:hover {
    background-color: rgba(102, 126, 234, 0.1) !important;
    color: #667eea !important;
}

.el-menu-item.is-active {
    background: linear-gradient(135deg, rgba(102, 126, 234, 0.15) 0%, rgba(118, 75, 162, 0.15) 100%) !important;
    color: #667eea !important;
    font-weight: 600 !important;
    border-left: 3px solid #667eea !important;
}

.el-menu-item i,
.el-submenu__title i {
    margin-right: 8px;
    font-size: 18px;
    width: 20px;
    text-align: center;
}

.el-submenu .el-menu-item {
    padding-left: 50px !important;
    margin-left: 8px !important;
}

.el-submenu .el-menu-item.is-active {
    border-left: 3px solid #667eea !important;
}

/* 子页面路由 */
.SubPageBar {
    display: flex;
    justify-content: space-between;
    margin-bottom: 12px;
}

/* 子页面容器 */
.SubPageContainer {
    border: none;
    border-radius: 12px;
    min-height: 70vh;
    padding: 30px;
    background: #ffffff;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
    margin: 0 10px;
}

/* 美化标签页 */
.el-tabs__header {
    margin-bottom: 1vh !important;
    margin-left: 10px;
    margin-right: 10px;
}

.el-tabs--card >>> .el-tabs__header {
    border-bottom: 2px solid #e4e7ed;
    margin-bottom: 15px;
}

.el-tabs--card >>> .el-tabs__item {
    border: none !important;
    border-radius: 8px 8px 0 0 !important;
    margin-right: 4px !important;
    background: #f5f7fa !important;
    color: #606266 !important;
    transition: all 0.3s ease !important;
    font-weight: 500 !important;
}

.el-tabs--card >>> .el-tabs__item:hover {
    color: #667eea !important;
    background: rgba(102, 126, 234, 0.1) !important;
}

.el-tabs--card >>> .el-tabs__item.is-active {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
    color: #ffffff !important;
    font-weight: 600 !important;
    box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3) !important;
}

.el-tabs--card >>> .el-tabs__item .el-icon-close {
    color: inherit !important;
    transition: all 0.3s ease !important;
}

.el-tabs--card >>> .el-tabs__item .el-icon-close:hover {
    background-color: rgba(255, 255, 255, 0.3) !important;
    border-radius: 50% !important;
}
</style>
