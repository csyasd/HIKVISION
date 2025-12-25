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
            <el-button type="primary" @click="showGasConfigDialog()" icon="el-icon-setting">设置气体含量范围</el-button>
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
    
            <el-table-column :show-overflow-tooltip="true" prop="Gas1" label="第一种气体含量" width="150" >
                <template slot-scope="scope">
                    {{ scope.row.Gas1 ? scope.row.Gas1.toFixed(2) : '0.00' }}
                </template>
            </el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="Gas2" label="第二种气体含量" width="150" >
                <template slot-scope="scope">
                    {{ scope.row.Gas2 ? scope.row.Gas2.toFixed(2) : '0.00' }}
                </template>
            </el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="Gas3" label="第三种气体含量" width="150" >
                <template slot-scope="scope">
                    {{ scope.row.Gas3 ? scope.row.Gas3.toFixed(2) : '0.00' }}
                </template>
            </el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="Gas4" label="第四种气体含量" width="150" >
                <template slot-scope="scope">
                    {{ scope.row.Gas4 ? scope.row.Gas4.toFixed(2) : '0.00' }}
                </template>
            </el-table-column>
    
            <el-table-column
                :show-overflow-tooltip="true"
                prop="CreateTime"
                label="创建时间"
                width="220"
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
        
        <!-- 设置气体含量范围对话框 -->
        <el-dialog title="设置气体含量范围" :visible.sync="gasConfigDialogVisible" width="600px">
            <el-form :model="gasConfigs" label-width="150px">
                <el-form-item label="第一种气体 - 最小值">
                    <el-input-number v-model="gasConfigs.Gas1.MinValue" :min="0" :precision="2" style="width: 100%;"></el-input-number>
                </el-form-item>
                <el-form-item label="第一种气体 - 最大值">
                    <el-input-number v-model="gasConfigs.Gas1.MaxValue" :min="0" :precision="2" style="width: 100%;"></el-input-number>
                </el-form-item>
                
                <el-divider></el-divider>
                
                <el-form-item label="第二种气体 - 最小值">
                    <el-input-number v-model="gasConfigs.Gas2.MinValue" :min="0" :precision="2" style="width: 100%;"></el-input-number>
                </el-form-item>
                <el-form-item label="第二种气体 - 最大值">
                    <el-input-number v-model="gasConfigs.Gas2.MaxValue" :min="0" :precision="2" style="width: 100%;"></el-input-number>
                </el-form-item>
                
                <el-divider></el-divider>
                
                <el-form-item label="第三种气体 - 最小值">
                    <el-input-number v-model="gasConfigs.Gas3.MinValue" :min="0" :precision="2" style="width: 100%;"></el-input-number>
                </el-form-item>
                <el-form-item label="第三种气体 - 最大值">
                    <el-input-number v-model="gasConfigs.Gas3.MaxValue" :min="0" :precision="2" style="width: 100%;"></el-input-number>
                </el-form-item>
                
                <el-divider></el-divider>
                
                <el-form-item label="第四种气体 - 最小值">
                    <el-input-number v-model="gasConfigs.Gas4.MinValue" :min="0" :precision="2" style="width: 100%;"></el-input-number>
                </el-form-item>
                <el-form-item label="第四种气体 - 最大值">
                    <el-input-number v-model="gasConfigs.Gas4.MaxValue" :min="0" :precision="2" style="width: 100%;"></el-input-number>
                </el-form-item>
            </el-form>
            <div slot="footer" class="dialog-footer">
                <el-button @click="gasConfigDialogVisible = false">取 消</el-button>
                <el-button type="primary" @click="saveGasConfigs()" :disabled="operationDisabled">确 定</el-button>
            </div>
        </el-dialog>
    </div>
</template>

<script>
import { SelectGasAbnormal, SelectWorkOrder, SelectALLDevice, GetAbnormalConfig, SaveAbnormalConfig } from "../api/api";

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
            gasConfigDialogVisible: false,
            gasConfigs: {
                Gas1: { ConfigType: "Gas", ConfigName: "Gas1", MinValue: 0, MaxValue: 100, IsEnabled: true },
                Gas2: { ConfigType: "Gas", ConfigName: "Gas2", MinValue: 0, MaxValue: 100, IsEnabled: true },
                Gas3: { ConfigType: "Gas", ConfigName: "Gas3", MinValue: 0, MaxValue: 100, IsEnabled: true },
                Gas4: { ConfigType: "Gas", ConfigName: "Gas4", MinValue: 0, MaxValue: 100, IsEnabled: true }
            }
        };
    },
    async mounted() {
        this.DeviceList = await SelectALLDevice();
        
        const onlineDevices = this.DeviceList.filter(d => d.OnlineStatus === '在线');
        if (onlineDevices.length > 0) {
            this.filterDeviceId = onlineDevices[0].Id;
        }
        
        await this.loadWorkOrderList();
        this.setDefaultWorkOrder();
        this.loadGasConfigs();
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
        async loadGasConfigs() {
            try {
                const configs = await Promise.all([
                    GetAbnormalConfig("Gas1"),
                    GetAbnormalConfig("Gas2"),
                    GetAbnormalConfig("Gas3"),
                    GetAbnormalConfig("Gas4")
                ]);
                
                if (configs[0]) {
                    this.gasConfigs.Gas1 = {
                        ConfigType: configs[0].ConfigType || "Gas",
                        ConfigName: configs[0].ConfigName || "Gas1",
                        MinValue: configs[0].MinValue || 0,
                        MaxValue: configs[0].MaxValue || 100,
                        IsEnabled: configs[0].IsEnabled !== undefined ? configs[0].IsEnabled : true
                    };
                }
                if (configs[1]) {
                    this.gasConfigs.Gas2 = {
                        ConfigType: configs[1].ConfigType || "Gas",
                        ConfigName: configs[1].ConfigName || "Gas2",
                        MinValue: configs[1].MinValue || 0,
                        MaxValue: configs[1].MaxValue || 100,
                        IsEnabled: configs[1].IsEnabled !== undefined ? configs[1].IsEnabled : true
                    };
                }
                if (configs[2]) {
                    this.gasConfigs.Gas3 = {
                        ConfigType: configs[2].ConfigType || "Gas",
                        ConfigName: configs[2].ConfigName || "Gas3",
                        MinValue: configs[2].MinValue || 0,
                        MaxValue: configs[2].MaxValue || 100,
                        IsEnabled: configs[2].IsEnabled !== undefined ? configs[2].IsEnabled : true
                    };
                }
                if (configs[3]) {
                    this.gasConfigs.Gas4 = {
                        ConfigType: configs[3].ConfigType || "Gas",
                        ConfigName: configs[3].ConfigName || "Gas4",
                        MinValue: configs[3].MinValue || 0,
                        MaxValue: configs[3].MaxValue || 100,
                        IsEnabled: configs[3].IsEnabled !== undefined ? configs[3].IsEnabled : true
                    };
                }
            } catch (error) {
                console.error("加载气体配置失败:", error);
            }
        },
        showGasConfigDialog() {
            this.loadGasConfigs();
            this.gasConfigDialogVisible = true;
        },
        async saveGasConfigs() {
            // 验证配置
            for (let i = 1; i <= 4; i++) {
                const config = this.gasConfigs[`Gas${i}`];
                // 确保启用检测默认为true
                config.IsEnabled = true;
                if (config.MinValue >= config.MaxValue) {
                    this.$message.error(`第${i}种气体的最小值必须小于最大值`);
                    return;
                }
            }
            
            this.operationDisabled = true;
            try {
                const promises = [
                    SaveAbnormalConfig(this.gasConfigs.Gas1),
                    SaveAbnormalConfig(this.gasConfigs.Gas2),
                    SaveAbnormalConfig(this.gasConfigs.Gas3),
                    SaveAbnormalConfig(this.gasConfigs.Gas4)
                ];
                
                await Promise.all(promises);
                this.$message.success("保存成功");
                this.gasConfigDialogVisible = false;
            } catch (error) {
                this.$message.error("保存失败");
            } finally {
                this.operationDisabled = false;
            }
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

            SelectGasAbnormal(pageData).then(res => {
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
            // 检查4种气体是否在正常范围内
            if (this.gasConfigs.Gas1.IsEnabled) {
                if (row.Gas1 < this.gasConfigs.Gas1.MinValue || row.Gas1 > this.gasConfigs.Gas1.MaxValue) {
                    return true;
                }
            }
            if (this.gasConfigs.Gas2.IsEnabled) {
                if (row.Gas2 < this.gasConfigs.Gas2.MinValue || row.Gas2 > this.gasConfigs.Gas2.MaxValue) {
                    return true;
                }
            }
            if (this.gasConfigs.Gas3.IsEnabled) {
                if (row.Gas3 < this.gasConfigs.Gas3.MinValue || row.Gas3 > this.gasConfigs.Gas3.MaxValue) {
                    return true;
                }
            }
            if (this.gasConfigs.Gas4.IsEnabled) {
                if (row.Gas4 < this.gasConfigs.Gas4.MinValue || row.Gas4 > this.gasConfigs.Gas4.MaxValue) {
                    return true;
                }
            }
            return false; // 所有启用的气体都在正常范围内
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

