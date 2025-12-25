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

.el-table tbody tr {
    background: transparent !important;
}

.el-table__body tr {
    background: transparent !important;
}

/* 确保所有表格行在任何状态下都保持透明背景 */
.el-table tbody tr td,
.el-table__body tr td {
    background-color: transparent !important;
    background: transparent !important;
}

/* 操作列单元格背景完全不透明 - 使用不透明背景色 */
.el-table__fixed-right .el-table__fixed-body-wrapper tr td,
.el-table__fixed-right .el-table__fixed-body-wrapper tr td:hover,
.el-table__fixed-right .el-table__fixed-body-wrapper tr.current-row td,
.el-table__fixed-right-patch,
.el-table__body-wrapper .el-table__body tr td:last-child,
.el-table__body tr td:last-child,
.el-table tbody tr td:last-child {
    background-color: rgba(13, 17, 23, 1) !important;
    background: rgba(13, 17, 23, 1) !important;
    backdrop-filter: none !important;
}

/* 操作列头部完全不透明 */
.el-table__fixed-right .el-table__fixed-header-wrapper th,
.el-table__header-wrapper th:last-child {
    background-color: rgba(13, 17, 23, 1) !important;
    background: rgba(13, 17, 23, 1) !important;
    backdrop-filter: none !important;
}

/* 操作列整个固定区域不透明 */
.el-table__fixed-right {
    background-color: rgba(13, 17, 23, 1) !important;
    background: rgba(13, 17, 23, 1) !important;
}

.el-table__fixed-right .el-table__fixed-body-wrapper,
.el-table__fixed-right .el-table__fixed-header-wrapper {
    background-color: rgba(13, 17, 23, 1) !important;
    background: rgba(13, 17, 23, 1) !important;
}


/* 工具栏和操作区域不透明 */
.toolbar,
.el-toolbar {
    background: var(--glass-bg) !important;
    backdrop-filter: blur(10px);
}

/* 选择框不透明 */
.el-select .el-input__inner,
.el-autocomplete .el-input__inner,
.el-date-editor .el-input__inner {
    background: var(--glass-bg) !important;
}

/* 按钮区域确保不透明 */
.el-button:not(.el-button--text) {
    background-color: initial !important;
}

/* 操作列中的按钮样式 - 只在操作列中应用 */
.el-table td:last-child .el-button--text,
.el-table__body td:last-child .el-button--text,
.el-table tbody td:last-child .el-button--text,
.el-table__fixed-right td .el-button--text {
    background-color: var(--glass-bg) !important;
    padding: 5px 10px !important;
    border-radius: 4px !important;
    color: #409eff !important;
    visibility: visible !important;
    opacity: 1 !important;
    display: inline-block !important;
    margin: 0 4px !important;
    vertical-align: middle !important;
}

/* 数据列中的按钮完全隐藏 - 使用更精确的选择器 */
.el-table .el-table__body-wrapper .el-table__body tr td:not(:last-child) .el-button,
.el-table .el-table__body-wrapper .el-table__body tr td:not(:last-child) .el-button--text,
.el-table__body tr td:not(:last-child) .el-button,
.el-table__body tr td:not(:last-child) .el-button--text,
.el-table tbody tr td:not(:last-child) .el-button,
.el-table tbody tr td:not(:last-child) .el-button--text {
    display: none !important;
    visibility: hidden !important;
    opacity: 0 !important;
    width: 0 !important;
    height: 0 !important;
    padding: 0 !important;
    margin: 0 !important;
    overflow: hidden !important;
}

/* 确保固定列中非操作列的按钮也被隐藏 */
.el-table__fixed-body-wrapper tr td:not(:last-child) .el-button,
.el-table__fixed-body-wrapper tr td:not(:last-child) .el-button--text {
    display: none !important;
    visibility: hidden !important;
    opacity: 0 !important;
}

/* 操作列单元格确保按钮水平排列 */
.el-table td:last-child,
.el-table__body td:last-child,
.el-table tbody td:last-child,
.el-table__fixed-right td {
    white-space: nowrap !important;
}


/* 使用属性选择器覆盖所有可能的行状态 */
.el-table tr[class*="current"],
.el-table tr[class*="hover"],
.el-table tr[class*="selected"],
.el-table tr[class*="active"] {
    background-color: transparent !important;
    background: transparent !important;
}

.el-table tr[class*="current"] td,
.el-table tr[class*="hover"] td,
.el-table tr[class*="selected"] td,
.el-table tr[class*="active"] td {
    background-color: transparent !important;
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

/* 禁用所有表格行的hover、点击、选中等所有变亮效果 */
.el-table--enable-row-hover .el-table__body tr:hover > td,
.el-table__body tr:hover > td,
.el-table__body tr.hover-row > td,
.el-table tbody tr:hover td,
.el-table tbody tr.hover-row td,
.el-table__row:hover,
.el-table__row.hover-row,
.el-table__row:hover td,
.el-table__row.hover-row td,
.el-table .el-table__body tr:hover,
.el-table .el-table__body tr.hover-row,
/* 禁用点击选中的current-row高亮效果 */
.el-table__body tr.current-row > td,
.el-table__body tr.current-row,
.el-table tbody tr.current-row td,
.el-table tbody tr.current-row,
.el-table__row.current-row,
.el-table__row.current-row td,
.el-table .el-table__body tr.current-row,
/* 禁用选中状态 */
.el-table__body tr.selected > td,
.el-table__body tr.selected,
.el-table tbody tr.selected td,
.el-table tbody tr.selected,
.el-table__row.selected,
.el-table__row.selected td,
.el-table .el-table__body tr.selected,
/* 禁用focus和active状态 */
.el-table__body tr:focus > td,
.el-table__body tr:active > td,
.el-table tbody tr:focus td,
.el-table tbody tr:active td,
.el-table__row:focus,
.el-table__row:active,
.el-table__row:focus td,
.el-table__row:active td {
    background-color: transparent !important;
    background: transparent !important;
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

/* 美化表单输入 - 操作区域不透明 */
.el-input__inner {
    background: var(--glass-bg) !important;
    border: none !important;
    border-bottom: 2px solid rgba(64, 158, 255, 0.2) !important;
    color: #e6f7ff !important;
    border-radius: 0 !important;
    padding-left: 8px !important;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1) !important;
}

.el-input__inner::placeholder {
    color: rgba(64, 158, 255, 0.4) !important;
}

.el-input__icon {
    color: #409eff !important;
    font-size: 16px !important;
    text-shadow: 0 0 10px rgba(64, 158, 255, 0.5);
}

.el-input__inner:focus {
    border-bottom-color: #409eff !important;
    background: var(--glass-bg) !important;
    box-shadow: 0 4px 15px -4px var(--primary-glow) !important;
}

.el-input__inner:focus + .el-input__prefix .el-input__icon {
    color: #fff !important;
    text-shadow: 0 0 15px #409eff;
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
    border: 1px solid var(--glass-border) !important;
    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.5) !important;
}

/* 修复加载遮罩层“黑屏”问题 - 改为磨砂玻璃 */
.el-loading-mask {
    background-color: rgba(0, 0, 0, 0.3) !important;
    backdrop-filter: blur(4px);
    transition: all 0.3s;
}

/* 修复对话框遮罩层“死黑”问题 */
.v-modal {
    opacity: 0.4 !important;
    background: #000 !important;
    backdrop-filter: blur(2px);
}
</style>
