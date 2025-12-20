/***********************************************************************
 * 本文件由T4模板生成，请将本文件复制到前端YixiaoAdmin/views文件夹中使用
    * 文件名：WorkBraceletManagement.vue
************************************************************************/
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
                        <el-tag :type="item.OnlineStatus === '在线' ? 'success' : 'danger'" size="mini">
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
                @select="handleWorkOrderCodeSelect">
            </el-autocomplete>
            <el-autocomplete
                style="width: 200px; margin-right: 10px;"
                v-model="filterWorkOrderContent"
                :fetch-suggestions="queryWorkOrderContent"
                placeholder="搜索工单内容"
                clearable
                @select="handleWorkOrderContentSelect">
            </el-autocomplete>
            <el-button @click="queryData()">查询</el-button>
            <el-button  @click="clearQuery()">清空</el-button>
            <el-button type="danger" @click="refreshTable()">刷新列表</el-button>
            <el-button type="primary" @click="changeDialogFormVisible()" :disabled = "operationDisabled">添加</el-button>
        </el-col>
        <el-table 
            :data="tableData" 
            style="width: 100%; min-height:70vh" 
            highlight-current-row
            @sort-change="sortChange" 
            :default-sort="{ prop: 'CreateTime', order: 'descending' }"  
            :row-class-name="tableRowClassName"
            @select="selectChange"
            @select-all="selectAll"
            ref="multipleTable"
            v-loading="operationDisabled"
            
        >
            <el-table-column type="selection" width="55"></el-table-column>
            
            <el-table-column :show-overflow-tooltip="true" prop="WorkerName" label="工人姓名" width="220" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="HeartRate" label="心率" width="120" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="WorkOrder.Code" label="所属工单" width="220" >
                <template slot-scope="scope">
                    <div>{{ WorkOrderList[WorkOrderList.findIndex((x) => x.Id == scope.row.WorkOrderId)]?.Code || '未分配' }}</div>
                </template>
            </el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="EntryExitStatus" label="进离场状态" width="150" >
                <template slot-scope="scope">
                    <el-tag :type="getEntryExitStatusType(scope.row.EntryExitStatus)">
                        {{ getEntryExitStatusText(scope.row.EntryExitStatus) }}
                    </el-tag>
                </template>
            </el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="EntryTime" label="进场时间" width="220" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="ExitTime" label="离场时间" width="220" ></el-table-column>
    
            <el-table-column
                :show-overflow-tooltip="true"
                prop="CreateTime"
                label="创建时间"
                width="220"
                sortable="custom"
            ></el-table-column>
            <el-table-column fixed="right" label="操作" width="100">
                <template slot-scope="scope">
                    <el-button @click="handleEdit(scope.row)" type="text" size="small" :disabled = "operationDisabled">编辑</el-button>
                    <el-button @click="handleDelete(scope.row)" type="text" size="small" :disabled = "operationDisabled">删除</el-button>
                </template>
            </el-table-column>
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
        <el-dialog title="添加" :visible.sync="addDialogFormVisible">
            <el-form :model="addForm">

            <el-form-item label="工人姓名" :label-width="formLabelWidth">
                <el-input v-model="addForm.WorkerName" autocomplete="off" placeholder="请输入工人姓名"></el-input>
            </el-form-item>

            <el-form-item label="心率" :label-width="formLabelWidth">
                <el-input v-model="addForm.HeartRate" autocomplete="off" placeholder="请输入心率"></el-input>
            </el-form-item>

            <el-form-item label="所属工单" :label-width="formLabelWidth">
                <el-select v-model="addForm.WorkOrderId" placeholder="请选择工单" filterable>
                    <el-option
                        v-for="item in WorkOrderList"
                        :key="item.Id"
                        :label="item.Code"
                        :value="item.Id">
                    </el-option>
                </el-select>
            </el-form-item>

            <el-form-item label="进离场状态" :label-width="formLabelWidth">
                <el-select v-model="addForm.EntryExitStatus" placeholder="请选择状态">
                    <el-option label="未进入" value="0"></el-option>
                    <el-option label="申请进入" value="1"></el-option>
                    <el-option label="刷卡成功" value="2"></el-option>
                    <el-option label="进入" value="3"></el-option>
                    <el-option label="申请签出" value="4"></el-option>
                    <el-option label="已经签出" value="5"></el-option>
                </el-select>
            </el-form-item>

            <el-form-item label="进场时间" :label-width="formLabelWidth">
                <el-date-picker
                    v-model="addForm.EntryTime"
                    type="datetime"
                    placeholder="选择进场时间"
                    value-format="yyyy-MM-dd HH:mm:ss"
                    >
                </el-date-picker>
             </el-form-item>

            <el-form-item label="离场时间" :label-width="formLabelWidth">
                <el-date-picker
                    v-model="addForm.ExitTime"
                    type="datetime"
                    placeholder="选择离场时间"
                    value-format="yyyy-MM-dd HH:mm:ss"
                    >
                </el-date-picker>
             </el-form-item>

            </el-form>
            <div slot="footer" class="dialog-footer">
                <el-button @click="addDialogFormVisible = false">取 消</el-button>
                <el-button type="primary" @click="AddConfirm()" :disabled = "operationDisabled">确 定</el-button>
            </div>
        </el-dialog>

        <el-dialog title="编辑" :visible.sync="editDialogFormVisible">
            <el-form :model="editForm">

            <el-form-item label="工人姓名" :label-width="formLabelWidth">
                <el-input v-model="editForm.WorkerName" autocomplete="off" placeholder="请输入工人姓名"></el-input>
            </el-form-item>

            <el-form-item label="心率" :label-width="formLabelWidth">
                <el-input v-model="editForm.HeartRate" autocomplete="off" placeholder="请输入心率"></el-input>
            </el-form-item>

            <el-form-item label="所属工单" :label-width="formLabelWidth">
                <el-select v-model="editForm.WorkOrderId" placeholder="请选择工单" filterable>
                    <el-option
                        v-for="item in WorkOrderList"
                        :key="item.Id"
                        :label="item.Code"
                        :value="item.Id">
                    </el-option>
                </el-select>
            </el-form-item>

            <el-form-item label="进离场状态" :label-width="formLabelWidth">
                <el-select v-model="editForm.EntryExitStatus" placeholder="请选择状态">
                    <el-option label="未进入" value="0"></el-option>
                    <el-option label="申请进入" value="1"></el-option>
                    <el-option label="刷卡成功" value="2"></el-option>
                    <el-option label="进入" value="3"></el-option>
                    <el-option label="申请签出" value="4"></el-option>
                    <el-option label="已经签出" value="5"></el-option>
                </el-select>
            </el-form-item>

            <el-form-item label="进场时间" :label-width="formLabelWidth">
                <el-date-picker
                    v-model="editForm.EntryTime"
                    type="datetime"
                    placeholder="选择进场时间"
                    value-format="yyyy-MM-dd HH:mm:ss"
                    >
                </el-date-picker>
             </el-form-item>

            <el-form-item label="离场时间" :label-width="formLabelWidth">
                <el-date-picker
                    v-model="editForm.ExitTime"
                    type="datetime"
                    placeholder="选择离场时间"
                    value-format="yyyy-MM-dd HH:mm:ss"
                    >
                </el-date-picker>
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
import { SelectWorkBracelet, AddWorkBracelet, EditWorkBracelet, DeleteWorkBracelet, SelectWorkBraceletById, SelectWorkOrder, SelectALLDevice } from "../api/api";

export default {
    data() {
        return {
            tableData: [], //表数据
            queryStr: "",
            currentPage: 1, //当前是第几页
            pageSize: 7,
            pageSizes: [7, 10, 20, 30],
            totalNumber: 0, //共计多少条数据
            orderBy: "",
            desc: true, //正序倒序
            addDialogFormVisible: false,
            editDialogFormVisible: false,
            addForm: {
               WorkerName: null,
               HeartRate: null,
               WorkOrderId: null,
               EntryExitStatus: null,
               EntryTime: null,
               ExitTime: null,
            },
            editForm: {
            Id:"",
               WorkerName: null,
               HeartRate: null,
               WorkOrderId: null,
               EntryExitStatus: null,
               EntryTime: null,
               ExitTime: null,
            },
            operationDisabled: false,
            formLabelWidth: "120px",
            WorkOrderList: [],
            DeviceList: [],
            Query: [],
            Orderby: [
                {
                    SortField: "CreateTime",
                    IsDesc: true,
                },
            ],
            selectDataArrL: [], //跨页多选所有的项
            filterDeviceId: null,
            filterWorkOrderCode: "",
            filterWorkOrderContent: "",

        };
    },
    mounted() {
       (async () => {
            this.DeviceList = await SelectALLDevice();
            
            // 默认选择在线设备的第一个
            const onlineDevices = this.DeviceList.filter(d => d.OnlineStatus === '在线');
            if (onlineDevices.length > 0) {
                this.filterDeviceId = onlineDevices[0].Id;
            }
            
            await this.loadWorkOrderList();
            this.setDefaultWorkOrder();
            this.getTableData();
        })();
    },
    methods: {
        //获取表格数据
        getTableData() {
            this.operationDisabled = true;
            
            // 构建查询条件：先筛选符合条件的工单，然后获取工单ID列表
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
            
            // 构建查询条件
            this.Query = [];
            
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
            
            var pageData = {
                Query: this.Query,
                Orderby: this.Orderby,
                CurrentPage: this.currentPage - 1,
                PageNumber: this.pageSize,
            };
            SelectWorkBracelet(pageData).then(res => {
                this.tableData = res.data;
                this.totalNumber = res.count;

                //如果发现加载了一个空页尝试向前翻一页，针对当一页只有一条数据时将该数据删除，页面显示异常问题
                if (this.currentPage > 1 && this.tableData.length == 0) {
                    this.currentPage -= 1;
                    this.getTableData();
                }
                this.operationDisabled = false;
            })
            .catch((res) => {
                    this.operationDisabled = false;
                    this.$message.error("网络异常！");
            });
        },
        //切换表格每页显示条数
        handleSizeChange(val) {
            this.pageSize = val;
            this.getTableData();
            console.log(`每页 ${val} 条`);
        },
        //切换表格当前页
        handleCurrentChange(val) {
            this.currentPage = val;
            this.getTableData();
            console.log(`当前页: ${val}`);
        },
        //清空查询条件
        clearQuery(){
            // 重新设置默认设备
            const onlineDevices = this.DeviceList.filter(d => d.OnlineStatus === '在线');
            if (onlineDevices.length > 0) {
                this.filterDeviceId = onlineDevices[0].Id;
            } else {
                this.filterDeviceId = null;
            }
            
            // 重新设置默认工单
            this.setDefaultWorkOrder();
            
            this.queryData();
        },
        
        // 设备选择改变事件
        handleDeviceChange() {
            // 当设备改变时，重新设置默认工单
            this.setDefaultWorkOrder();
        },
        //刷新表格
        refreshTable() {
            this.queryData();
        },
        //查询
        queryData() {
            this.currentPage = 1; //搜索时重置页码
            this.getTableData();
        },
        //点击添加按钮（改变添加dialog状态）
        changeDialogFormVisible() {
            this.addDialogFormVisible = true;
        },
        //点击确认添加按钮
        AddConfirm() {
            this.operationDisabled = true;
            AddWorkBracelet(this.addForm).then(res => {
                if (res) {
                    this.$message({
                        message: "创建成功！",
                        type: "success",
                    });
                    this.getTableData();
                } else {
                    this.$message.error("创建失败！");
                }
                this.operationDisabled = false;
            })
            .catch((res) => {
                    this.operationDisabled = false;
                    this.$message.error("创建失败！");
            });
        },
        //点击修改按钮
        handleEdit(row) {
               this.editForm.Id = row.Id;
               this.editForm.WorkerName = row.WorkerName;
               this.editForm.HeartRate = row.HeartRate;
               this.editForm.WorkOrderId = row.WorkOrderId;
               this.editForm.EntryExitStatus = row.EntryExitStatus;
               this.editForm.EntryTime = row.EntryTime;
               this.editForm.ExitTime = row.ExitTime;
            
            this.editDialogFormVisible = true;
        },
        //点击确认修改按钮
        EditConfirm() {
            this.operationDisabled = true;
            EditWorkBracelet(this.editForm).then(res => {
                    if (res) {
                        this.$message({
                            message: "编辑成功！",
                            type: "success",
                        });
                        this.getTableData();
                    } else {
                        this.$message.error("编辑失败！");
                    }
                    this.operationDisabled = false;
                    this.editDialogFormVisible = false;
            })
            .catch((res) => {
                    this.operationDisabled = false;
                    this.$message.error("修改失败！");
            });
        },
        //点击删除按钮
        handleDelete(row) {
            this.operationDisabled = true;
            var Data = {
                id: row.Id
            };
            this.$confirm("此操作将删除该条记录, 是否继续?", "提示", {
                confirmButtonText: "确定",
                cancelButtonText: "取消",
                type: "warning",
            })
                .then(() => {
                    DeleteWorkBracelet(Data).then((res) => {
                        if (res) {
                            this.$message({
                                type: "success",
                                message: "删除成功!",
                            });
                        } else {
                            this.$message({
                                message: "删除异常，请重试!",
                            });
                        }
                        this.getTableData();
                        this.operationDisabled = false;
                    });
                })
                .catch(() => {
                    this.$message({
                        type: "info",
                        message: "已取消删除",
                    });
                    this.operationDisabled = false;
                });
        },
        sortChange(column) {
            let index = this.Orderby.findIndex(function (value, index, arr) {
                return value.SortField == column.prop;
            });
            console.log(index);

            if (column.order == null) {
                if (index != -1) {
                    this.Orderby.splice(index, 1);
                }
                this.getTableData();
                return;
            }
            let desc = false;
            if (column.order == "ascending") {
                desc = false;
            } else {
                desc = true;
            }

            if (index == -1) {
                this.Orderby.push({
                    SortField: column.prop,
                    IsDesc: desc,
                });
            } else {
                this.Orderby[index].SortField = column.prop;
                this.Orderby[index].IsDesc = desc;
            }
            console.log(column.order);

            this.getTableData();
        },

        //行颜色
        tableRowClassName({ row, rowIndex }) {
            return "";
        },

        /*行多选开始*/
        //当选中的时触发
        handleSelectionChange(val) {
            this.multipleSelection = val;
            console.log(this.multipleSelection[0]);
        },
        //初始化订单选中状态
        selectInit() {
            this.$nextTick(() => {
                this.selectDataArrL.forEach((item) => {
                    console.log(item);
                    this.tableData.forEach((listitem) => {
                        if (item.Id == listitem.Id) {
                            console.log(listitem);
                            this.$refs.multipleTable.toggleRowSelection(
                                listitem,
                                true
                            );
                        }
                    });
                });
                console.log(this.selectDataArrL);
            });
        },
        //多选
        selectChange(arr, row) {
            let isHaveItem = this.selectDataArrL.find(
                (item) => item.Id == row.Id
            );

            if (isHaveItem) {
                this.selectDataArrL = this.selectDataArrL.filter(
                    (item) => item.Id != isHaveItem.Id
                );
            } else {
                this.selectDataArrL.push(row);
            }
        },
        // 全选
        selectAll(arr) {
            if (arr.length > 0) {
                this.addRows(arr);
            } else {
                this.removeRows(this.tableData);
            }
        },
        // 添加选中行
        addRows(rows) {
            for (let key of rows) {
                // 如果选中的数据中没有这条就添加进去
                if (!this.selectDataArrL.find((item) => item.Id === key.Id)) {
                    this.selectDataArrL.push(key);
                }
            }
        },
        // 取消选中行
        removeRows(rows) {
            if (this.selectDataArrL.length > 0) {
                for (let row of rows) {
                    this.selectDataArrL = this.selectDataArrL.filter(
                        (item) => item.Id !== row.Id
                    );
                }
            }
        },
        /*行多选结束*/

        /*请将自定义函数写在我的下面*/
        
        //获取进离场状态文本
        getEntryExitStatusText(status) {
            const statusMap = {
                '0': '未进入',
                '1': '申请进入',
                '2': '刷卡成功',
                '3': '进入',
                '4': '申请签出',
                '5': '已经签出'
            };
            return statusMap[status] || '未知';
        },
        
        //获取进离场状态标签类型
        getEntryExitStatusType(status) {
            const typeMap = {
                '0': 'info',      // 未进入 - 灰色
                '1': 'warning',   // 申请进入 - 橙色
                '2': 'success',   // 刷卡成功 - 绿色
                '3': 'success',   // 进入 - 绿色
                '4': 'warning',   // 申请签出 - 橙色
                '5': 'info'       // 已经签出 - 灰色
            };
            return typeMap[status] || 'info';
        },
        
        //加载工单列表
        loadWorkOrderList() {
            return SelectWorkOrder({ Query: [], Orderby: [], CurrentPage: 0, PageNumber: 1000 }).then(res => {
                this.WorkOrderList = res.data || [];
            }).catch(error => {
                console.log(error);
                this.$message.error("加载工单列表失败！");
            });
        },
        
        // 设置默认工单（工单状态为开始的第一个）
        setDefaultWorkOrder() {
            const filteredWorkOrders = this.getFilteredWorkOrders();
            // 找到工单状态为1（工单开始）的第一个工单
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
        
        // 工单编号自动完成查询
        queryWorkOrderCode(queryString, cb) {
            const filteredWorkOrders = this.getFilteredWorkOrders();
            const results = filteredWorkOrders
                .filter(wo => {
                    if (!queryString) return true;
                    return wo.Code && wo.Code.toLowerCase().includes(queryString.toLowerCase());
                })
                .map(wo => ({
                    value: wo.Code,
                    workOrder: wo
                }));
            cb(results);
        },
        
        // 工单内容自动完成查询
        queryWorkOrderContent(queryString, cb) {
            const filteredWorkOrders = this.getFilteredWorkOrders();
            const results = filteredWorkOrders
                .filter(wo => {
                    if (!queryString) return true;
                    return wo.Content && wo.Content.toLowerCase().includes(queryString.toLowerCase());
                })
                .map(wo => ({
                    value: wo.Content,
                    workOrder: wo
                }));
            cb(results);
        },
        
        // 工单编号选择事件
        handleWorkOrderCodeSelect(item) {
            this.filterWorkOrderCode = item.value;
        },
        
        // 工单内容选择事件
        handleWorkOrderContentSelect(item) {
            this.filterWorkOrderContent = item.value;
        },
    }
};
</script>