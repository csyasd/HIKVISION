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
                @clear="queryData">
            </el-autocomplete>
            <el-autocomplete
                style="width: 200px; margin-right: 10px;"
                v-model="filterWorkOrderContent"
                :fetch-suggestions="queryWorkOrderContent"
                placeholder="搜索工单内容"
                clearable
                @select="handleWorkOrderContentSelect"
                @clear="queryData">
            </el-autocomplete>
            <el-button @click="queryData()">查询</el-button>
            <el-button @click="clearQuery()">清空</el-button>
            <el-button type="danger" @click="refreshTable()">刷新列表</el-button>
        </el-col>
        <el-table 
            :data="tableData" 
            style="width: 100%; min-height:70vh" 
            highlight-current-row
            @sort-change="sortChange" 
            :default-sort="{ prop: 'CreateTime', order: 'descending' }"  
            v-loading="operationDisabled"
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
        
        <el-dialog title="添加状态记录" :visible.sync="addDialogFormVisible">
            <el-form :model="addForm">
                <el-form-item label="施工工单" :label-width="formLabelWidth">
                    <el-select v-model="addForm.WorkOrderId" placeholder="请选择工单" filterable style="width: 100%;">
                        <el-option v-for="item in WorkOrderList" :key="item.Id" :label="item.Code" :value="item.Id"></el-option>
                    </el-select>
                </el-form-item>
                <el-form-item label="工人姓名" :label-width="formLabelWidth">
                    <el-input v-model="addForm.WorkerName" autocomplete="off" placeholder="请输入工人姓名"></el-input>
                </el-form-item>
                <el-form-item label="心率" :label-width="formLabelWidth">
                    <el-input v-model="addForm.HeartRate" autocomplete="off" placeholder="请输入心率"></el-input>
                </el-form-item>
                <el-form-item label="进离场状态" :label-width="formLabelWidth">
                    <el-select v-model="addForm.EntryExitStatus" placeholder="请选择状态" style="width: 100%;">
                        <el-option label="未进入" value="0"></el-option>
                        <el-option label="申请进入" value="1"></el-option>
                        <el-option label="刷卡成功" value="2"></el-option>
                        <el-option label="进入" value="3"></el-option>
                        <el-option label="申请签出" value="4"></el-option>
                        <el-option label="已经签出" value="5"></el-option>
                    </el-select>
                </el-form-item>
            </el-form>
            <div slot="footer" class="dialog-footer">
                <el-button @click="addDialogFormVisible = false">取 消</el-button>
                <el-button type="primary" @click="AddConfirm()" :disabled = "operationDisabled">确 定</el-button>
            </div>
        </el-dialog>

        <el-dialog title="编辑状态记录" :visible.sync="editDialogFormVisible">
            <el-form :model="editForm">
                <el-form-item label="施工工单" :label-width="formLabelWidth">
                    <el-select v-model="editForm.WorkOrderId" placeholder="请选择工单" filterable style="width: 100%;">
                        <el-option v-for="item in WorkOrderList" :key="item.Id" :label="item.Code" :value="item.Id"></el-option>
                    </el-select>
                </el-form-item>
                <el-form-item label="工人姓名" :label-width="formLabelWidth">
                    <el-input v-model="editForm.WorkerName" autocomplete="off" placeholder="请输入工人姓名"></el-input>
                </el-form-item>
                <el-form-item label="心率" :label-width="formLabelWidth">
                    <el-input v-model="editForm.HeartRate" autocomplete="off" placeholder="请输入心率"></el-input>
                </el-form-item>
                <el-form-item label="进离场状态" :label-width="formLabelWidth">
                    <el-select v-model="editForm.EntryExitStatus" placeholder="请选择状态" style="width: 100%;">
                        <el-option label="未进入" value="0"></el-option>
                        <el-option label="申请进入" value="1"></el-option>
                        <el-option label="刷卡成功" value="2"></el-option>
                        <el-option label="进入" value="3"></el-option>
                        <el-option label="申请签出" value="4"></el-option>
                        <el-option label="已经签出" value="5"></el-option>
                    </el-select>
                </el-form-item>
            </el-form>
            <div slot="footer" class="dialog-footer">
                <el-button @click="editDialogFormVisible = false">取 消</el-button>
                <el-button type="primary" @click="EditConfirm()" :disabled = "operationDisabled">确 定</el-button>
            </div>
        </el-dialog>
    </div>
</template>

<script>
import { SelectWorkerStatusRecord, AddWorkerStatusRecord, EditWorkerStatusRecord, DeleteWorkerStatusRecord, SelectWorkOrder, SelectALLDevice } from "../api/api";

export default {
    data() {
        return {
            tableData: [],
            currentPage: 1,
            pageSize: 10,
            pageSizes: [10, 20, 50, 100],
            totalNumber: 0,
            addDialogFormVisible: false,
            editDialogFormVisible: false,
            addForm: {
               WorkerName: "",
               HeartRate: "",
               WorkOrderId: "",
               EntryExitStatus: "",
            },
            editForm: {
               Id: "",
               WorkerName: "",
               HeartRate: "",
               WorkOrderId: "",
               EntryExitStatus: "",
            },
            operationDisabled: false,
            formLabelWidth: "120px",
            WorkOrderList: [],
            DeviceList: [],
            Query: [],
            Orderby: [{ SortField: "CreateTime", IsDesc: true }],
            filterDeviceId: null,
            filterWorkOrderCode: "",
            filterWorkOrderContent: "",
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
        this.getTableData();
    },
    methods: {
        async loadWorkOrderList() {
            const res = await SelectWorkOrder({ Query: [], Orderby: [], CurrentPage: 0, PageNumber: 1000 });
            this.WorkOrderList = res.data || [];
        },
        // 设置默认工单（工单状态为开始的第一个）
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
        // 获取当前设备对应的工单列表
        getFilteredWorkOrders() {
            if (this.filterDeviceId) {
                return this.WorkOrderList.filter(wo => wo.DeviceId === this.filterDeviceId);
            }
            return this.WorkOrderList;
        },
        getTableData() {
            this.operationDisabled = true;
            this.Query = [];
            
            let filteredWorkOrders = [...this.WorkOrderList];
            
            // 设备筛选
            if (this.filterDeviceId) {
                filteredWorkOrders = filteredWorkOrders.filter(wo => wo.DeviceId === this.filterDeviceId);
            }
            
            // 工单编号筛选
            if (this.filterWorkOrderCode && this.filterWorkOrderCode.trim()) {
                filteredWorkOrders = filteredWorkOrders.filter(wo => 
                    wo.Code && wo.Code.includes(this.filterWorkOrderCode.trim())
                );
            }
            
            // 工单内容筛选
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
                // 如果没有匹配的工单，返回空结果
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

            SelectWorkerStatusRecord(pageData).then(res => {
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
        changeDialogFormVisible() {
            this.addDialogFormVisible = true;
        },
        AddConfirm() {
            this.operationDisabled = true;
            AddWorkerStatusRecord(this.addForm).then(res => {
                if (res) {
                    this.$message.success("添加成功");
                    this.addDialogFormVisible = false;
                    this.addForm = { WorkerName: "", HeartRate: "", WorkOrderId: "", EntryExitStatus: "" };
                    this.getTableData();
                }
                this.operationDisabled = false;
            }).catch(() => {
                this.operationDisabled = false;
                this.$message.error("添加失败");
            });
        },
        handleEdit(row) {
            this.editForm = { ...row };
            this.editDialogFormVisible = true;
        },
        EditConfirm() {
            this.operationDisabled = true;
            EditWorkerStatusRecord(this.editForm).then(res => {
                if (res) {
                    this.$message.success("修改成功");
                    this.editDialogFormVisible = false;
                    this.getTableData();
                }
                this.operationDisabled = false;
            }).catch(() => {
                this.operationDisabled = false;
                this.$message.error("修改失败");
            });
        },
        handleDelete(row) {
            this.$confirm("确定删除吗?", "提示", { type: "warning" }).then(() => {
                this.operationDisabled = true;
                DeleteWorkerStatusRecord({ Id: row.Id }).then(res => {
                    if (res) {
                        this.$message.success("删除成功");
                        this.getTableData();
                    }
                    this.operationDisabled = false;
                }).catch(() => {
                    this.operationDisabled = false;
                    this.$message.error("删除失败");
                });
            });
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
        getEntryExitStatusType(status) {
            return { '0': 'info', '1': 'warning', '2': 'success', '3': 'success', '4': 'warning', '5': 'info' }[status] || 'info';
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
        },
        handleWorkOrderContentSelect(item) {
            this.filterWorkOrderContent = item.value;
        },
        handleDeviceChange() {
            this.setDefaultWorkOrder();
            this.queryData();
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

<style scoped>
.container {
    padding: 20px;
}
.toolbar {
    margin-bottom: 20px;
}
</style>