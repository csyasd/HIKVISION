<template>
  <div class="mobile-list-page">
    <div class="filter-bar">
      <el-select v-model="filterDeviceId" placeholder="设备" clearable filterable class="filter-select" @change="queryData">
        <el-option v-for="d in DeviceList" :key="d.Id" :label="d.Name" :value="d.Id" />
      </el-select>
      <el-input v-model="filterCode" placeholder="编号" clearable class="filter-input" @clear="queryData" />
      <el-button type="primary" size="small" @click="queryData">查询</el-button>
    </div>
    <div v-loading="loading" class="list-wrap">
      <div v-for="(row, i) in tableData" :key="row.Id || i" class="list-card">
        <div class="card-row">
          <span class="label">设备</span>
          <span>{{ getDeviceName(row.DeviceId) }}</span>
        </div>
        <div class="card-row">
          <span class="label">编号</span>
          <span>{{ row.Code || '-' }}</span>
        </div>
        <div class="card-row">
          <span class="label">内容</span>
          <span class="content-text">{{ (row.Content || '-').slice(0, 30) }}{{ (row.Content || '').length > 30 ? '...' : '' }}</span>
        </div>
        <div class="card-row">
          <span class="label">状态</span>
          <span :class="'status-' + row.Status">{{ getStatusText(row.Status) }}</span>
        </div>
        <div class="card-row">
          <span class="label">开始</span>
          <span>{{ row.StartTime || '-' }}</span>
        </div>
        <div class="card-row">
          <span class="label">结束</span>
          <span>{{ row.EndTime || '-' }}</span>
        </div>
      </div>
      <div v-if="!loading && tableData.length === 0" class="no-data">暂无数据</div>
    </div>
    <div class="pagination-wrap">
      <el-pagination
        small
        :current-page="currentPage"
        :page-size="pageSize"
        :total="totalNumber"
        layout="prev, pager, next"
        @current-change="handlePageChange"
      />
    </div>
  </div>
</template>

<script>
import { SelectWorkOrder, SelectALLDevice } from '@/api/api.js'

export default {
  name: 'MobileWorkOrder',
  data() {
    return {
      DeviceList: [],
      tableData: [],
      filterDeviceId: null,
      filterCode: '',
      currentPage: 1,
      pageSize: 10,
      totalNumber: 0,
      loading: false
    }
  },
  async mounted() {
    this.DeviceList = await SelectALLDevice() || []
    const online = this.DeviceList.filter(d => d.OnlineStatus === '在线')
    if (online.length) this.filterDeviceId = online[0].Id
    this.getTableData()
  },
  methods: {
    async getTableData() {
      this.loading = true
      const Query = []
      if (this.filterDeviceId) Query.push({ QueryField: 'DeviceId', QueryStr: this.filterDeviceId })
      if (this.filterCode?.trim()) Query.push({ QueryField: 'Code', QueryStr: this.filterCode.trim() })
      try {
        const res = await SelectWorkOrder({
          Query,
          Orderby: [{ SortField: 'CreateTime', IsDesc: true }],
          CurrentPage: this.currentPage - 1,
          PageNumber: this.pageSize
        })
        this.tableData = res?.data || []
        this.totalNumber = res?.count ?? this.tableData.length
      } catch (e) {
        this.$message.error('加载失败')
      }
      this.loading = false
    },
    queryData() {
      this.currentPage = 1
      this.getTableData()
    },
    handlePageChange(p) {
      this.currentPage = p
      this.getTableData()
    },
    getDeviceName(id) {
      const d = this.DeviceList.find(x => x.Id == id)
      return d?.Name || '-'
    },
    getStatusText(s) {
      return { 0: '未开始', 1: '工单开始', 2: '工单结束' }[s] || '未知'
    }
  }
}
</script>

<style scoped>
.mobile-list-page { padding-bottom: 12px; }
.filter-bar { display: flex; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.filter-select { flex: 1; min-width: 100px; }
.filter-input { flex: 1; min-width: 100px; }
.list-wrap { min-height: 120px; }
.list-card {
  background: rgba(255,255,255,0.05);
  border-radius: 12px;
  padding: 14px;
  margin-bottom: 10px;
  border: 1px solid rgba(255,255,255,0.08);
}
.card-row { display: flex; justify-content: space-between; font-size: 14px; margin-bottom: 6px; }
.card-row:last-child { margin-bottom: 0; }
.card-row .label { color: rgba(255,255,255,0.5); }
.content-text { max-width: 60%; text-align: right; }
.status-1 { color: #67c23a; }
.no-data { text-align: center; padding: 40px; color: rgba(255,255,255,0.4); }
.pagination-wrap { margin-top: 16px; display: flex; justify-content: center; }
.pagination-wrap >>> .el-pagination .el-pager li { background: rgba(255,255,255,0.1); color: #fff; }
.pagination-wrap >>> .el-pagination .btn-prev, .pagination-wrap >>> .el-pagination .btn-next { background: rgba(255,255,255,0.1); color: #fff; }
</style>
