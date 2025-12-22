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
    padding: 24px;
    color: var(--text-bright);
}

.realtime-data h2 {
    font-size: 28px;
    font-weight: 800;
    margin-bottom: 30px;
    letter-spacing: 2px;
    background: linear-gradient(135deg, #409eff 0%, #7948ea 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    text-shadow: 0 0 20px rgba(64, 158, 255, 0.2);
}

.data-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
    gap: 24px;
}

.data-card {
    background: var(--glass-bg);
    backdrop-filter: blur(20px);
    padding: 24px;
    border-radius: 20px;
    border: 1px solid var(--glass-border);
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
    transition: all 0.3s ease;
}

.data-card:hover {
    border-color: rgba(64, 158, 255, 0.3);
    box-shadow: 0 0 20px rgba(64, 158, 255, 0.1);
    transform: translateY(-5px);
}

.data-card h3 {
    margin: 0 0 20px 0;
    font-weight: 700;
    color: #409eff;
    display: flex;
    align-items: center;
    gap: 10px;
    letter-spacing: 1px;
}

.status-item, .gas-item, .alarm-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 12px 0;
    border-bottom: 1px solid rgba(255, 255, 255, 0.05);
}

.status-item:last-child, .gas-item:last-child, .alarm-item:last-child {
    border-bottom: none;
}

.status-label, .gas-label, .alarm-label {
    color: var(--text-muted);
    font-size: 14px;
}

.status-value, .gas-value, .alarm-value {
    color: var(--text-bright);
    font-weight: 700;
    font-size: 16px;
    font-family: 'JetBrains Mono', monospace;
}

.status-value.online {
    color: #67c23a;
    text-shadow: 0 0 10px rgba(103, 194, 58, 0.3);
}

.status-value.offline {
    color: #f56c6c;
}

.alarm-value.urgent {
    color: #f56c6c;
    animation: pulse 2s infinite;
}

@keyframes pulse {
    0% { opacity: 1; }
    50% { opacity: 0.6; }
    100% { opacity: 1; }
}
</style>








