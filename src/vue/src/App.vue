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
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'PingFang SC', 'Hiragino Sans GB',
        'Microsoft YaHei', 'Helvetica Neue', Helvetica, Arial, sans-serif;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
    background-color: #f5f7fa;
}

/* 美化对话框 */
.el-dialog {
    width: 1000px !important;
    border-radius: 16px !important;
    overflow: hidden;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15) !important;
}

.el-dialog__header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    padding: 20px 24px !important;
    margin: 0 !important;
}

.el-dialog__title {
    color: #ffffff !important;
    font-size: 18px !important;
    font-weight: 600 !important;
    letter-spacing: 0.5px;
}

.el-dialog__headerbtn {
    top: 20px !important;
    right: 20px !important;
}

.el-dialog__headerbtn .el-dialog__close {
    color: #ffffff !important;
    font-size: 20px !important;
    transition: transform 0.3s ease;
}

.el-dialog__headerbtn .el-dialog__close:hover {
    transform: rotate(90deg);
}

.el-dialog__body {
    padding: 24px !important;
    background: #ffffff;
}

.el-dialog__footer {
    padding: 16px 24px !important;
    background: #fafafa;
    border-top: 1px solid #e4e7ed;
}

.wideDialog {
    width: 1250px !important;
}

/* 美化上传框 */
.el-upload-dragger {
    width: 750px !important;
    height: 350px !important;
    border: 2px dashed #d9d9d9 !important;
    border-radius: 12px !important;
    background: #fafafa !important;
    transition: all 0.3s ease !important;
}

.el-upload-dragger:hover {
    border-color: #667eea !important;
    background: rgba(102, 126, 234, 0.05) !important;
}

.el-upload-dragger .el-icon-upload {
    margin: 120px 0 16px !important;
    font-size: 80px !important;
    color: #667eea !important;
}

/* 美化表格 */
.el-table {
    border-radius: 12px !important;
    overflow: hidden;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08) !important;
}

.el-table th {
    background: linear-gradient(135deg, rgba(102, 126, 234, 0.1) 0%, rgba(118, 75, 162, 0.1) 100%) !important;
    color: #606266 !important;
    font-weight: 600 !important;
    border-bottom: 2px solid #e4e7ed !important;
    padding: 12px 0 !important;
}

.el-table td {
    padding: 12px 0 !important;
    border-bottom: 1px solid #f0f0f0 !important;
}

.el-table--border {
    border: 1px solid #e4e7ed !important;
}

.el-table--border td,
.el-table--border th {
    border-right: 1px solid #e4e7ed !important;
}

.el-table--border::after,
.el-table--group::after {
    background-color: #e4e7ed !important;
}

.el-table--border::before {
    background-color: #e4e7ed !important;
}

.el-table--striped .el-table__body tr.el-table__row--striped td {
    background: #fafafa !important;
}

.el-table--enable-row-hover .el-table__body tr:hover > td {
    background-color: rgba(102, 126, 234, 0.05) !important;
}

.el-table .el-checkbox__inner {
    width: 18px !important;
    height: 18px !important;
    border-radius: 4px !important;
    border-color: #dcdfe6 !important;
    transition: all 0.3s ease !important;
}

.el-table .el-checkbox__inner:hover {
    border-color: #667eea !important;
}

.el-table .el-checkbox__input.is-checked .el-checkbox__inner {
    background-color: #667eea !important;
    border-color: #667eea !important;
}

.el-table .el-checkbox__inner::after {
    left: 5px !important;
    top: 2px !important;
    width: 4px !important;
    height: 8px !important;
}

/* 小行高表格 */
.shortLineHeightTable td {
    padding: 5px 0px !important;
    height: 20px;
    line-height: 20px;
}

.shortLineHeightTable .el-button--small {
    padding: 0px, 15px;
}

/* 表格行颜色 */
.el-table .dangerous-row {
    background: #fee !important;
}

.el-table .dangerous-row:hover > td {
    background-color: #fdd !important;
}

.el-table .warning-row {
    background: #fff8e1 !important;
}

.el-table .warning-row:hover > td {
    background-color: #fff3c4 !important;
}

.el-table .success-row {
    background: #f0f9eb !important;
}

.el-table .success-row:hover > td {
    background-color: #e8f5e9 !important;
}

/* 美化工具栏 */
.toolbar {
    margin-bottom: 20px !important;
    padding: 16px;
    background: #ffffff;
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 10px;
}

.toolbar .el-input {
    flex: 0 0 auto;
}

.toolbar .el-button {
    border-radius: 8px;
    padding: 10px 20px;
    font-weight: 500;
    transition: all 0.3s ease;
}

.toolbar .el-button--primary {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border: none;
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

.toolbar .el-button--primary:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(102, 126, 234, 0.4);
}

.toolbar .el-button--danger {
    background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
    border: none;
    box-shadow: 0 4px 12px rgba(245, 87, 108, 0.3);
}

.toolbar .el-button--danger:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(245, 87, 108, 0.4);
}

/* 美化分页 */
.el-pagination {
    margin-top: 20px;
    text-align: center;
}

.el-pagination .el-pager li {
    border-radius: 6px;
    margin: 0 4px;
    transition: all 0.3s ease;
}

.el-pagination .el-pager li:hover {
    color: #667eea;
    background-color: rgba(102, 126, 234, 0.1);
}

.el-pagination .el-pager li.active {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: #ffffff;
    font-weight: 600;
}

.el-pagination .btn-prev,
.el-pagination .btn-next {
    border-radius: 6px;
    transition: all 0.3s ease;
}

.el-pagination .btn-prev:hover,
.el-pagination .btn-next:hover {
    color: #667eea;
    background-color: rgba(102, 126, 234, 0.1);
}

/* 美化表单 */
.el-form-item__label {
    font-weight: 500;
    color: #606266;
}

.el-input__inner {
    border-radius: 8px;
    border: 1px solid #dcdfe6;
    transition: all 0.3s ease;
}

.el-input__inner:focus {
    border-color: #667eea;
    box-shadow: 0 0 0 2px rgba(102, 126, 234, 0.1);
}

.el-select .el-input__inner:focus {
    border-color: #667eea;
}

/* 美化按钮 */
.el-button {
    border-radius: 8px;
    font-weight: 500;
    transition: all 0.3s ease;
}

.el-button--text {
    color: #667eea;
    padding: 8px 12px;
}

.el-button--text:hover {
    background-color: rgba(102, 126, 234, 0.1);
    color: #764ba2;
}

/* 美化标签 */
.el-tag {
    border-radius: 6px;
    border: none;
    padding: 4px 12px;
    font-weight: 500;
}

.el-tag--success {
    background: linear-gradient(135deg, #67c23a 0%, #85ce61 100%);
    color: #ffffff;
}

.el-tag--info {
    background: linear-gradient(135deg, #909399 0%, #b1b3b8 100%);
    color: #ffffff;
}

/* 美化加载动画 */
.el-loading-mask {
    background-color: rgba(255, 255, 255, 0.9);
    backdrop-filter: blur(2px);
}

/* 美化消息提示 */
.el-message {
    border-radius: 12px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    padding: 16px 20px;
}

.el-notification {
    border-radius: 12px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
}

/* 美化确认框 */
.el-message-box {
    border-radius: 16px;
    overflow: hidden;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
}

.el-message-box__header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    padding: 20px 24px;
}

.el-message-box__title {
    color: #ffffff;
    font-weight: 600;
}

.el-message-box__headerbtn .el-message-box__close {
    color: #ffffff;
}

.el-message-box__content {
    padding: 24px;
}

.el-message-box__btns {
    padding: 16px 24px;
    background: #fafafa;
    border-top: 1px solid #e4e7ed;
}

.el-message-box__btns .el-button {
    border-radius: 8px;
    padding: 10px 20px;
    font-weight: 500;
}

.el-message-box__btns .el-button--primary {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border: none;
}

/* 美化管理页面容器 */
.container {
    padding: 0;
    background: transparent;
}

/* 美化表格容器 */
.el-table {
    background: #ffffff;
}

.el-table__body-wrapper {
    border-radius: 0 0 12px 12px;
}

/* 美化分页容器 */
.block {
    margin-top: 20px;
    display: flex;
    justify-content: center;
    padding: 16px;
    background: #ffffff;
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
}

/* 美化表单 */
.el-form {
    padding: 10px 0;
}

.el-form-item {
    margin-bottom: 22px;
}

.el-form-item__label {
    padding-right: 12px;
}

.el-input,
.el-select,
.el-date-picker {
    width: 100%;
}

.el-input__inner,
.el-textarea__inner {
    transition: all 0.3s ease;
}

.el-input__inner:focus,
.el-textarea__inner:focus {
    border-color: #667eea;
    box-shadow: 0 0 0 2px rgba(102, 126, 234, 0.1);
}

/* 美化下拉菜单 */
.el-dropdown-menu {
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    border: none;
    padding: 8px 0;
}

.el-dropdown-menu__item {
    padding: 10px 20px;
    transition: all 0.3s ease;
}

.el-dropdown-menu__item:hover {
    background-color: rgba(102, 126, 234, 0.1);
    color: #667eea;
}

/* 美化徽章 */
.el-badge__content {
    background: linear-gradient(135deg, #f5576c 0%, #f093fb 100%);
    border: 2px solid #ffffff;
    box-shadow: 0 2px 8px rgba(245, 87, 108, 0.3);
}

/* 美化工具提示 */
.el-tooltip__popper {
    border-radius: 8px;
    padding: 8px 12px;
    font-size: 12px;
}

/* 美化日期选择器 */
.el-picker-panel {
    border-radius: 12px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    border: none;
}

.el-date-picker__header-label {
    font-weight: 600;
}

.el-picker-panel__icon-btn:hover {
    color: #667eea;
}

.el-date-table td.available:hover {
    color: #667eea;
}

.el-date-table td.current:not(.disabled) {
    color: #ffffff;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border-radius: 6px;
}

/* 美化选择器 */
.el-select-dropdown {
    border-radius: 8px;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
    border: none;
}

.el-select-dropdown__item {
    padding: 10px 20px;
    transition: all 0.3s ease;
}

.el-select-dropdown__item:hover {
    background-color: rgba(102, 126, 234, 0.1);
    color: #667eea;
}

.el-select-dropdown__item.selected {
    color: #667eea;
    font-weight: 600;
    background-color: rgba(102, 126, 234, 0.1);
}

/* 美化滚动条 */
::-webkit-scrollbar {
    width: 8px;
    height: 8px;
}

::-webkit-scrollbar-track {
    background: #f1f1f1;
    border-radius: 4px;
}

::-webkit-scrollbar-thumb {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
    background: linear-gradient(135deg, #764ba2 0%, #667eea 100%);
}
</style>
