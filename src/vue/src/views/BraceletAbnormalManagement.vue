<template>
    <div class="container" >
        <el-col :span="24" class="toolbar">
            <el-select 
                v-model="filterDeviceId" 
                placeholder="选择设备" 
                style="width: 200px; margin-right: 10px;"
                clearable
                filterable
                @change="handleDeviceChange">
                <el-option
                    v-for="(item, i) in DeviceList"
                    :key="i"
                    :label="`${item.Name} (${item.OnlineStatus || '离线'})`"
                    :value="item.Id">
                    <span style="float: left">{{ item.Name }}</span>
                    <span style="float: right; color: #8492a6; font-size: 13px">
                        <el-tag size="mini">
                            {{ item.OnlineStatus || '离线' }}
                        </el-tag>
                    </span>
                </el-option>
            </el-select>
            <el-autocomplete
                style="width: 200px; margin-right: 10px;"
                v-model="filterWorkOrderCode"
                :fetch-suggestions="queryWorkOrderCode"
                placeholder="搜索工单编号"
                clearable
                @select="handleWorkOrderCodeSelect"
                @input="queryData"
                @clear="queryData">
            </el-autocomplete>
            <el-autocomplete
                style="width: 200px; margin-right: 10px;"
                v-model="filterWorkOrderContent"
                :fetch-suggestions="queryWorkOrderContent"
                placeholder="搜索工单内容"
                clearable
                @select="handleWorkOrderContentSelect"
                @input="queryData"
                @clear="queryData">
            </el-autocomplete>
            <el-button @click="queryData()">查询</el-button>
            <el-button @click="clearQuery()">清空</el-button>
            <el-button type="danger" @click="refreshTable()">刷新列表</el-button>
            <el-button type="primary" @click="showHeartRateConfigDialog()" icon="el-icon-setting">设置心率正常范围</el-button>
        </el-col>
        <el-table 
            :data="tableData" 
            style="width: 100%; min-height:70vh" 
            highlight-current-row
            @sort-change="sortChange" 
            :default-sort="{ prop: 'CreateTime', order: 'descending' }"  
            v-loading="operationDisabled"
            :row-class-name="tableRowClassName"
        >
            <el-table-column type="index" width="55"></el-table-column>
            
            <el-table-column :show-overflow-tooltip="true" prop="WorkOrderId" label="工单" width="220" >
                <template slot-scope="scope">
                    <div>{{ WorkOrderList.find(x => x.Id == scope.row.WorkOrderId)?.Code || '未分配' }}</div>
                </template>
            </el-table-column>

            <el-table-column :show-overflow-tooltip="true" prop="WorkerName" label="工人姓名" width="150" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="HeartRate" label="心率" width="100" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="EntryExitStatus" label="进离场状态" width="150" >
                <template slot-scope="scope">
                    <el-tag>
                        {{ getEntryExitStatusText(scope.row.EntryExitStatus) }}
                    </el-tag>
                </template>
            </el-table-column>
    
            <el-table-column
                :show-overflow-tooltip="true"
                prop="CreateTime"
                label="记录时间"
                width="200"
                sortable="custom"
            ></el-table-column>
        </el-table>
        <div class="block">
            <el-pagination
                @size-change="handleSizeChange"
                @current-change="handleCurrentChange"
                :current-page="currentPage"
                :page-sizes="pageSizes"
                :page-size="pageSize"
                layout="total, sizes, prev, pager, next, jumper"
                :total="totalNumber"
            ></el-pagination>
        </div>
        
        <!-- 设置心率正常范围对话框 -->
        <el-dialog title="设置心率正常范围" :visible.sync="heartRateConfigDialogVisible" width="500px">
            <el-form :model="heartRateConfig" label-width="150px">
                <el-form-item label="最小值">
                    <el-input-number v-model="heartRateConfig.MinValue" :min="0" :max="300" style="width: 100%;"></el-input-number>
                </el-form-item>
                <el-form-item label="最大值">
                    <el-input-number v-model="heartRateConfig.MaxValue" :min="0" :max="300" style="width: 100%;"></el-input-number>
                </el-form-item>
            </el-form>
            <div slot="footer" class="dialog-footer">
                <el-button @click="heartRateConfigDialogVisible = false">取 消</el-button>
                <el-button type="primary" @click="saveHeartRateConfig()" :disabled="operationDisabled">确 定</el-button>
            </div>
        </el-dialog>
    </div>
</template>

<script>
import { SelectBraceletAbnormal, SelectWorkOrder, SelectALLDevice, GetAbnormalConfig, SaveAbnormalConfig } from "../api/api";

export default {
    data() {
        return {
            tableData: [],
            currentPage: 1,
            pageSize: 10,
            pageSizes: [10, 20, 50, 100],
            totalNumber: 0,
            operationDisabled: false,
            WorkOrderList: [],
            DeviceList: [],
            Query: [],
            Orderby: [{ SortField: "CreateTime", IsDesc: true }],
            filterDeviceId: null,
            filterWorkOrderCode: "",
            filterWorkOrderContent: "",
            heartRateConfigDialogVisible: false,
            heartRateConfig: {
                ConfigType: "HeartRate",
                ConfigName: "HeartRate",
                MinValue: 60,
                MaxValue: 100,
                IsEnabled: true
            }
        };
    },
    async mounted() {
        this.DeviceList = await SelectALLDevice();
        
        // 默认选择在线设备的第一个
        const onlineDevices = this.DeviceList.filter(d => d.OnlineStatus === '在线');
        if (onlineDevices.length > 0) {
            this.filterDeviceId = onlineDevices[0].Id;
        }
        
        await this.loadWorkOrderList();
        this.setDefaultWorkOrder();
        this.loadHeartRateConfig();
        this.getTableData();
    },
    methods: {
        async loadWorkOrderList() {
            const res = await SelectWorkOrder({ Query: [], Orderby: [], CurrentPage: 0, PageNumber: 1000 });
            this.WorkOrderList = res.data || [];
        },
        setDefaultWorkOrder() {
            const filteredWorkOrders = this.getFilteredWorkOrders();
            const startedWorkOrder = filteredWorkOrders.find(wo => wo.Status === 1);
            if (startedWorkOrder) {
                this.filterWorkOrderCode = startedWorkOrder.Code || "";
                this.filterWorkOrderContent = startedWorkOrder.Content || "";
            } else {
                this.filterWorkOrderCode = "";
                this.filterWorkOrderContent = "";
            }
        },
        getFilteredWorkOrders() {
            if (this.filterDeviceId) {
                return this.WorkOrderList.filter(wo => wo.DeviceId === this.filterDeviceId);
            }
            return this.WorkOrderList;
        },
        async loadHeartRateConfig() {
            try {
                const config = await GetAbnormalConfig("HeartRate");
                if (config) {
                    this.heartRateConfig = {
                        ConfigType: config.ConfigType || "HeartRate",
                        ConfigName: config.ConfigName || "HeartRate",
                        MinValue: config.MinValue || 60,
                        MaxValue: config.MaxValue || 100,
                        IsEnabled: config.IsEnabled !== undefined ? config.IsEnabled : true
                    };
                }
            } catch (error) {
                console.error("加载心率配置失败:", error);
            }
        },
        showHeartRateConfigDialog() {
            this.loadHeartRateConfig();
            this.heartRateConfigDialogVisible = true;
        },
        saveHeartRateConfig() {
            if (this.heartRateConfig.MinValue >= this.heartRateConfig.MaxValue) {
                this.$message.error("最小值必须小于最大值");
                return;
            }
            // 确保启用检测默认为true
            this.heartRateConfig.IsEnabled = true;
            this.operationDisabled = true;
            SaveAbnormalConfig(this.heartRateConfig).then(res => {
                if (res) {
                    this.$message.success("保存成功");
                    this.heartRateConfigDialogVisible = false;
                }
                this.operationDisabled = false;
            }).catch(() => {
                this.operationDisabled = false;
                this.$message.error("保存失败");
            });
        },
        getTableData() {
            this.operationDisabled = true;
            this.Query = [];
            
            let filteredWorkOrders = [...this.WorkOrderList];
            
            if (this.filterDeviceId) {
                filteredWorkOrders = filteredWorkOrders.filter(wo => wo.DeviceId === this.filterDeviceId);
            }
            
            if (this.filterWorkOrderCode && this.filterWorkOrderCode.trim()) {
                filteredWorkOrders = filteredWorkOrders.filter(wo => 
                    wo.Code && wo.Code.includes(this.filterWorkOrderCode.trim())
                );
            }
            
            if (this.filterWorkOrderContent && this.filterWorkOrderContent.trim()) {
                filteredWorkOrders = filteredWorkOrders.filter(wo => 
                    wo.Content && wo.Content.includes(this.filterWorkOrderContent.trim())
                );
            }

            if (filteredWorkOrders.length > 0) {
                const workOrderIds = filteredWorkOrders.map(wo => wo.Id);
                this.Query.push({
                    QueryField: "WorkOrderId",
                    QueryStr: workOrderIds.join(","),
                });
            } else {
                this.Query.push({
                    QueryField: "WorkOrderId",
                    QueryStr: "__NO_MATCH__",
                });
            }

            const pageData = {
                Query: this.Query,
                Orderby: this.Orderby,
                CurrentPage: this.currentPage - 1,
                PageNumber: this.pageSize,
            };

            SelectBraceletAbnormal(pageData).then(res => {
                this.tableData = res.data || [];
                this.totalNumber = res.count || 0;
                
                if (this.currentPage > 1 && this.tableData.length == 0) {
                    this.currentPage -= 1;
                    this.getTableData();
                }
                this.operationDisabled = false;
            }).catch((err) => {
                console.error(err);
                this.operationDisabled = false;
                this.$message.error("加载数据失败");
            });
        },
        handleSizeChange(val) {
            this.pageSize = val;
            this.getTableData();
        },
        handleCurrentChange(val) {
            this.currentPage = val;
            this.getTableData();
        },
        queryData() {
            this.currentPage = 1;
            this.getTableData();
        },
        clearQuery() {
            this.filterDeviceId = null;
            this.filterWorkOrderCode = "";
            this.filterWorkOrderContent = "";
            this.queryData();
        },
        refreshTable() {
            this.getTableData();
        },
        sortChange(column) {
            if (column.order == null) {
                this.Orderby = [{ SortField: "CreateTime", IsDesc: true }];
            } else {
                this.Orderby = [{ SortField: column.prop, IsDesc: column.order === 'descending' }];
            }
            this.getTableData();
        },
        getEntryExitStatusText(status) {
            return { '0': '未进入', '1': '申请进入', '2': '刷卡成功', '3': '进入', '4': '申请签出', '5': '已经签出' }[status] || '未知';
        },
        queryWorkOrderCode(queryString, cb) {
            const filteredWorkOrders = this.getFilteredWorkOrders();
            const results = filteredWorkOrders
                .filter(wo => !queryString || (wo.Code && wo.Code.toLowerCase().includes(queryString.toLowerCase())))
                .map(wo => ({ value: wo.Code }));
            cb(results);
        },
        queryWorkOrderContent(queryString, cb) {
            const filteredWorkOrders = this.getFilteredWorkOrders();
            const results = filteredWorkOrders
                .filter(wo => !queryString || (wo.Content && wo.Content.toLowerCase().includes(queryString.toLowerCase())))
                .map(wo => ({ value: wo.Content }));
            cb(results);
        },
        handleWorkOrderCodeSelect(item) {
            this.filterWorkOrderCode = item.value;
            this.queryData();
        },
        handleWorkOrderContentSelect(item) {
            this.filterWorkOrderContent = item.value;
            this.queryData();
        },
        handleDeviceChange() {
            this.setDefaultWorkOrder();
            this.queryData();
        },
        // 判断记录是异常还是正常
        isAbnormalRecord(row) {
            if (!this.heartRateConfig || !this.heartRateConfig.IsEnabled) {
                return false; // 未配置，默认正常
            }
            const heartRate = parseInt(row.HeartRate);
            if (isNaN(heartRate)) {
                return false; // 无效数据，默认正常
            }
            return heartRate < this.heartRateConfig.MinValue || heartRate > this.heartRateConfig.MaxValue;
        },
        // 表格行样式
        tableRowClassName({row, rowIndex}) {
            if (this.isAbnormalRecord(row)) {
                return 'abnormal-row'; // 异常行 - 红色
            } else {
                return 'normal-row'; // 正常行 - 绿色
            }
        }
    }
}
</script>

<style scoped>
.container {
    padding: 20px;
}
.toolbar {
    margin-bottom: 20px;
}
</style>

<style>
/* 异常行 - 红色背景 */
.el-table .abnormal-row {
    background-color: #fef0f0 !important;
}
.el-table .abnormal-row:hover > td {
    background-color: #fde2e2 !important;
}

/* 正常行 - 绿色背景 */
.el-table .normal-row {
    background-color: #f0f9ff !important;
}
.el-table .normal-row:hover > td {
    background-color: #e1f5fe !important;
}
</style>

