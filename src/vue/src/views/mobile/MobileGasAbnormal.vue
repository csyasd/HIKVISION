<template>
  <div class="mobile-list-page">
    <div class="filter-bar">
      <el-select v-model="filterDeviceId" placeholder="设备" clearable filterable class="filter-select" @change="onDeviceChange">
        <el-option v-for="d in DeviceList" :key="d.Id" :label="d.Name" :value="d.Id" />
      </el-select>
      <el-select v-model="filterWorkOrderCode" placeholder="工单编号" clearable filterable class="filter-select" @change="queryData">
        <el-option v-for="wo in filteredWorkOrderOptions" :key="wo.Id" :label="wo.Code" :value="wo.Code" />
      </el-select>
      <el-button type="primary" size="small" @click="queryData">查询</el-button>
    </div>
    <div v-loading="loading" class="list-wrap">
      <div v-for="(row, i) in tableData" :key="row.Id || i" class="list-card">
        <div class="card-row">
          <span class="label">工单</span>
          <span>{{ getWorkOrderCode(row.WorkOrderId) }}</span>
        </div>
        <div class="card-row">
          <span class="label">CO</span>
          <span>{{ (row.Gas1 || 0).toFixed(2) }} ppm</span>
        </div>
        <div class="card-row">
          <span class="label">H2S</span>
          <span>{{ (row.Gas2 || 0).toFixed(2) }} ppm</span>
        </div>
        <div class="card-row">
          <span class="label">CH4</span>
          <span>{{ (row.Gas3 || 0).toFixed(2) }} %LEL</span>
        </div>
        <div class="card-row">
          <span class="label">SO2</span>
          <span>{{ (row.Gas4 || 0).toFixed(2) }} ppm</span>
        </div>
        <div class="card-row">
          <span class="label">时间</span>
          <span>{{ row.CreateTime || '-' }}</span>
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
import { SelectGasAbnormal, SelectWorkOrder, SelectALLDevice } from '@/api/api.js'

export default {
  name: 'MobileGasAbnormal',
  data() {
    return {
      DeviceList: [],
      WorkOrderList: [],
      tableData: [],
      filterDeviceId: null,
      filterWorkOrderCode: '',
      currentPage: 1,
      pageSize: 10,
      totalNumber: 0,
      loading: false
    }
  },
  computed: {
    filteredWorkOrderOptions() {
      if (!this.filterDeviceId) return this.WorkOrderList
      return this.WorkOrderList.filter(wo => wo.DeviceId === this.filterDeviceId)
    }
  },
  async mounted() {
    this.DeviceList = await SelectALLDevice() || []
    const res = await SelectWorkOrder({ Query: [], Orderby: [], CurrentPage: 0, PageNumber: 1000 })
    this.WorkOrderList = res?.data || []
    const online = this.DeviceList.filter(d => d.OnlineStatus === '在线')
    if (online.length) this.filterDeviceId = online[0].Id
    this.getTableData()
  },
  methods: {
    onDeviceChange() {
      this.filterWorkOrderCode = ''
      this.queryData()
    },
    getFilteredWorkOrders() {
      let list = this.WorkOrderList
      if (this.filterDeviceId) list = list.filter(wo => wo.DeviceId === this.filterDeviceId)
      if (this.filterWorkOrderCode?.trim()) list = list.filter(wo => wo.Code?.trim() === this.filterWorkOrderCode.trim())
      return list
    },
    async getTableData() {
      this.loading = true
      const filtered = this.getFilteredWorkOrders()
      const Query = filtered.length
        ? [{ QueryField: 'WorkOrderId', QueryStr: filtered.map(wo => wo.Id).join(',') }]
        : [{ QueryField: 'WorkOrderId', QueryStr: '__NO_MATCH__' }]
      try {
        const res = await SelectGasAbnormal({
          Query,
          Orderby: [{ SortField: 'CreateTime', IsDesc: true }],
          CurrentPage: this.currentPage - 1,
          PageNumber: this.pageSize
        })
        this.tableData = res?.data || []
        this.totalNumber = res?.count || 0
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
    getWorkOrderCode(id) {
      const wo = this.WorkOrderList.find(x => x.Id == id)
      return wo?.Code || '未分配'
    }
  }
}
</script>

<style scoped>
.mobile-list-page { padding-bottom: 12px; }
.filter-bar { display: flex; gap: 8px; margin-bottom: 12px; flex-wrap: wrap; }
.filter-select { flex: 1; min-width: 100px; }
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
.no-data { text-align: center; padding: 40px; color: rgba(255,255,255,0.4); }
.pagination-wrap { margin-top: 16px; display: flex; justify-content: center; }
.pagination-wrap >>> .el-pagination .el-pager li { background: rgba(255,255,255,0.1); color: #fff; }
.pagination-wrap >>> .el-pagination .btn-prev, .pagination-wrap >>> .el-pagination .btn-next { background: rgba(255,255,255,0.1); color: #fff; }
</style>
