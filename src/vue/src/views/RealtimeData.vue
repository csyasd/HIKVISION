<template>
    <div class="realtime-data">
        <h2>实时数据</h2>
        <div class="data-grid">
            <div class="data-card">
                <h3>设备状态</h3>
                <div class="status-item">
                    <span class="status-label">在线设备:</span>
                    <span class="status-value online">97</span>
                </div>
                <div class="status-item">
                    <span class="status-label">离线设备:</span>
                    <span class="status-value offline">3</span>
                </div>
            </div>
            
            <div class="data-card">
                <h3>气体监测</h3>
                <div class="gas-item">
                    <span class="gas-label">O2:</span>
                    <span class="gas-value">20.9%VOL</span>
                </div>
                <div class="gas-item">
                    <span class="gas-label">CH4:</span>
                    <span class="gas-value">0%LEL</span>
                </div>
                <div class="gas-item">
                    <span class="gas-label">H2S:</span>
                    <span class="gas-value">0ppm</span>
                </div>
                <div class="gas-item">
                    <span class="gas-label">CO:</span>
                    <span class="gas-value">7ppm</span>
                </div>
            </div>
            
            <div class="data-card">
                <h3>报警统计</h3>
                <div class="alarm-item">
                    <span class="alarm-label">今日报警:</span>
                    <span class="alarm-value">5</span>
                </div>
                <div class="alarm-item">
                    <span class="alarm-label">未处理:</span>
                    <span class="alarm-value urgent">3</span>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
export default {
    name: 'RealtimeData',
    data() {
        return {
            // 实时数据
        }
    }
}
</script>

<style scoped>
.realtime-data {
    padding: 20px;
}

.data-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: 20px;
    margin-top: 20px;
}

.data-card {
    background: #fff;
    padding: 20px;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.data-card h3 {
    margin: 0 0 15px 0;
    color: #333;
    border-bottom: 2px solid #409eff;
    padding-bottom: 10px;
}

.status-item, .gas-item, .alarm-item {
    display: flex;
    justify-content: space-between;
    margin-bottom: 10px;
    padding: 8px 0;
    border-bottom: 1px solid #f0f0f0;
}

.status-label, .gas-label, .alarm-label {
    color: #666;
}

.status-value, .gas-value, .alarm-value {
    font-weight: bold;
}

.status-value.online {
    color: #67c23a;
}

.status-value.offline {
    color: #f56c6c;
}

.alarm-value.urgent {
    color: #f56c6c;
}
</style>







