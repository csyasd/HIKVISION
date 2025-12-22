<template>
    <div id="app">
        <router-view />
    </div>
</template>

<style>
* {
    box-sizing: border-box;
}

body {
    margin: 0;
    font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans_serif;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
    background: radial-gradient(circle at 50% 50%, #0d1117 0%, #010409 100%);
    color: #e6edf3;
    min-height: 100vh;
}

#app {
    background: transparent;
}

/* Glassmorphism Logic */
:root {
    --glass-bg: rgba(13, 17, 23, 0.7);
    --glass-border: rgba(255, 255, 255, 0.1);
    --primary-glow: rgba(64, 158, 255, 0.5);
    --accent-glow: rgba(121, 72, 234, 0.5);
    --text-bright: #ffffff;
    --text-muted: rgba(255, 255, 255, 0.7);
}

/* 美化对话框 */
.el-dialog {
    background: var(--glass-bg) !important;
    backdrop-filter: blur(20px) saturate(180%);
    border: 1px solid var(--glass-border) !important;
    border-radius: 20px !important;
    box-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.8) !important;
}

.el-dialog__header {
    background: linear-gradient(90deg, rgba(64, 158, 255, 0.1), transparent) !important;
    padding: 24px !important;
    border-bottom: 1px solid var(--glass-border);
}

.el-dialog__title {
    color: var(--text-bright) !important;
    font-weight: 700 !important;
    letter-spacing: 0.5px;
}

.el-dialog__body {
    background: transparent !important;
    color: var(--text-bright) !important;
}

.el-dialog__footer {
    background: rgba(255, 255, 255, 0.02) !important;
    border-top: 1px solid var(--glass-border);
}

/* 美化表格 */
.el-table {
    background: transparent !important;
    border: 1px solid var(--glass-border) !important;
    border-radius: 12px !important;
    color: var(--text-bright) !important;
}

.el-table tr, .el-table th {
    background: transparent !important;
}

.el-table th {
    background: rgba(64, 158, 255, 0.05) !important;
    color: var(--text-bright) !important;
    border-bottom: 1px solid var(--glass-border) !important;
    text-transform: uppercase;
    font-size: 12px;
    letter-spacing: 1px;
}

.el-table td {
    border-bottom: 1px solid rgba(255, 255, 255, 0.05) !important;
}

.el-table--enable-row-hover .el-table__body tr:hover > td {
    background-color: rgba(64, 158, 255, 0.1) !important;
}

/* 美化工具栏 */
.toolbar {
    background: var(--glass-bg) !important;
    backdrop-filter: blur(10px);
    border: 1px solid var(--glass-border) !important;
    border-radius: 16px !important;
    padding: 20px !important;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4) !important;
}

/* 美化和自定义按钮 */
.el-button {
    border-radius: 10px !important;
    font-weight: 600 !important;
    letter-spacing: 0.5px;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1) !important;
}

.el-button--primary {
    background: linear-gradient(135deg, #409eff 0%, #7948ea 100%) !important;
    border: none !important;
    box-shadow: 0 4px 15px var(--primary-glow) !important;
}

.el-button--primary:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 25px var(--primary-glow) !important;
    opacity: 0.9;
}

.el-button--danger {
    background: linear-gradient(135deg, #f56c6c 0%, #f78989 100%) !important;
    border: none !important;
    box-shadow: 0 4px 15px rgba(245, 108, 108, 0.3) !important;
}

.el-button--text {
    color: #409eff !important;
}

.el-button--text:hover {
    background: rgba(64, 158, 255, 0.1) !important;
}

/* 美化表单输入 */
.el-input__inner {
    background: rgba(0, 0, 0, 0.2) !important;
    border: 1px solid var(--glass-border) !important;
    color: var(--text-bright) !important;
    border-radius: 10px !important;
}

.el-input__inner:focus {
    border-color: #409eff !important;
    box-shadow: 0 0 0 2px var(--primary-glow) !important;
}

.el-select-dropdown {
    background: var(--glass-bg) !important;
    backdrop-filter: blur(20px);
    border: 1px solid var(--glass-border) !important;
}

.el-select-dropdown__item {
    color: var(--text-muted) !important;
}

.el-select-dropdown__item.hover, .el-select-dropdown__item:hover {
    background: rgba(64, 158, 255, 0.1) !important;
    color: var(--text-bright) !important;
}

.el-select-dropdown__item.selected {
    color: #409eff !important;
    background: rgba(64, 158, 255, 0.15) !important;
}

/* 美化标签 */
.el-tag {
    background: rgba(64, 158, 255, 0.1) !important;
    border: 1px solid rgba(64, 158, 255, 0.2) !important;
    color: #409eff !important;
    border-radius: 6px !important;
}

/* 分页美化 */
.block {
    background: var(--glass-bg) !important;
    border: 1px solid var(--glass-border) !important;
}

.el-pagination button, .el-pagination span:not([class*=suffix]), .el-pager li {
    background: transparent !important;
    color: var(--text-muted) !important;
}

.el-pager li.active {
    color: #409eff !important;
    font-weight: 700;
}

/* 滚动条 */
::-webkit-scrollbar {
    width: 6px;
    height: 6px;
}

::-webkit-scrollbar-track {
    background: transparent;
}

::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.1);
    border-radius: 10px;
}

::-webkit-scrollbar-thumb:hover {
    background: rgba(64, 158, 255, 0.3);
}

/* 全局容器间距 */
.SubPageContainer {
    background: var(--glass-bg) !important;
    backdrop-filter: blur(20px);
    border: 1px solid var(--glass-border) !important;
    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.5) !important;
}
</style>
