/***********************************************************************
 * 本文件由T4模板生成，请将本文件复制到前端YixiaoAdmin/views文件夹中使用
    * 文件名：GasAlarmRecordManagement.vue
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
            <el-button  @click="clearQuery()">清空</el-button>
            <el-button type="danger" @click="refreshTable()">刷新列表</el-button>
            <el-button type="success" @click="showGasCurve()" icon="el-icon-data-line" :disabled="operationDisabled">气体曲线</el-button>
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
            
            <el-table-column :show-overflow-tooltip="true" prop="WorkOrder" label="工单" width="220" >
                <template slot-scope="scope">
                    <div>{{ WorkOrderList[WorkOrderList.findIndex((x) => x.Id == scope.row.WorkOrderId)]?.Code || '未分配' }}</div>
                </template>
            </el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="Gas1" label="一氧化碳 (ppm)" width="150" >
                <template slot-scope="scope">
                    {{ scope.row.Gas1 ? scope.row.Gas1.toFixed(2) : '0.00' }} ppm
                </template>
            </el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="Gas2" label="硫化氢 (ppm)" width="150" >
                <template slot-scope="scope">
                    {{ scope.row.Gas2 ? scope.row.Gas2.toFixed(2) : '0.00' }} ppm
                </template>
            </el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="Gas3" label="甲烷 (%LEL)" width="150" >
                <template slot-scope="scope">
                    {{ scope.row.Gas3 ? scope.row.Gas3.toFixed(2) : '0.00' }} %LEL
                </template>
            </el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="Gas4" label="二氧化硫 (ppm)" width="150" >
                <template slot-scope="scope">
                    {{ scope.row.Gas4 ? scope.row.Gas4.toFixed(2) : '0.00' }} ppm
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
        <el-dialog title="添加" :visible.sync="addDialogFormVisible" width="600px">
            <el-form :model="addForm">

            <el-form-item label="工单" :label-width="formLabelWidth">
                <el-select v-model="addForm.WorkOrderId" placeholder="请选择工单" filterable>
                    <el-option
						v-for="(item, i) in WorkOrderList"
						:label="item.Code"
						:value="item.Id"
						:key="i"
					></el-option>
                </el-select>
            </el-form-item>

            <el-form-item label="一氧化碳 (ppm)" :label-width="formLabelWidth">
                <el-input-number v-model="addForm.Gas1" :precision="2" :step="0.01" :min="0" placeholder="请输入一氧化碳含量"></el-input-number>
            </el-form-item>

            <el-form-item label="硫化氢 (ppm)" :label-width="formLabelWidth">
                <el-input-number v-model="addForm.Gas2" :precision="2" :step="0.01" :min="0" placeholder="请输入硫化氢含量"></el-input-number>
            </el-form-item>

            <el-form-item label="甲烷 (%LEL)" :label-width="formLabelWidth">
                <el-input-number v-model="addForm.Gas3" :precision="2" :step="0.01" :min="0" placeholder="请输入甲烷含量"></el-input-number>
            </el-form-item>

            <el-form-item label="二氧化硫 (ppm)" :label-width="formLabelWidth">
                <el-input-number v-model="addForm.Gas4" :precision="2" :step="0.01" :min="0" placeholder="请输入二氧化硫含量"></el-input-number>
            </el-form-item>

            </el-form>
            <div slot="footer" class="dialog-footer">
                <el-button @click="addDialogFormVisible = false">取 消</el-button>
                <el-button type="primary" @click="AddConfirm()" :disabled = "operationDisabled">确 定</el-button>
            </div>
        </el-dialog>

        <el-dialog title="编辑" :visible.sync="editDialogFormVisible" width="600px">
            <el-form :model="editForm">

            <el-form-item label="工单" :label-width="formLabelWidth">
                <el-select v-model="editForm.WorkOrderId" placeholder="请选择工单" filterable>
                    <el-option
						v-for="(item, i) in WorkOrderList"
						:label="item.Code"
						:value="item.Id"
						:key="i"
					></el-option>
                </el-select>
            </el-form-item>

            <el-form-item label="一氧化碳 (ppm)" :label-width="formLabelWidth">
                <el-input-number v-model="editForm.Gas1" :precision="2" :step="0.01" :min="0" placeholder="请输入一氧化碳含量"></el-input-number>
            </el-form-item>

            <el-form-item label="硫化氢 (ppm)" :label-width="formLabelWidth">
                <el-input-number v-model="editForm.Gas2" :precision="2" :step="0.01" :min="0" placeholder="请输入硫化氢含量"></el-input-number>
            </el-form-item>

            <el-form-item label="甲烷 (%LEL)" :label-width="formLabelWidth">
                <el-input-number v-model="editForm.Gas3" :precision="2" :step="0.01" :min="0" placeholder="请输入甲烷含量"></el-input-number>
            </el-form-item>

            <el-form-item label="二氧化硫 (ppm)" :label-width="formLabelWidth">
                <el-input-number v-model="editForm.Gas4" :precision="2" :step="0.01" :min="0" placeholder="请输入二氧化硫含量"></el-input-number>
            </el-form-item>

            </el-form>
            <div slot="footer" class="dialog-footer">
                <el-button @click="editDialogFormVisible = false">取 消</el-button>
                <el-button type="primary" @click="EditConfirm()" :disabled = "operationDisabled">确 定</el-button>
            </div>
        </el-dialog>
        
        <!-- 气体曲线对话框 -->
        <el-dialog title="气体浓度趋势图" :visible.sync="curveDialogVisible" width="80%" @opened="onCurveDialogOpened" destroy-on-close>
            <div v-loading="chartLoading">
                <div style="margin-bottom: 20px; text-align: center;">
                    <el-checkbox-group v-model="selectedGases" @change="updateChart">
                        <el-checkbox :label="1">一氧化碳</el-checkbox>
                        <el-checkbox :label="2">硫化氢</el-checkbox>
                        <el-checkbox :label="3">甲烷</el-checkbox>
                        <el-checkbox :label="4">二氧化硫</el-checkbox>
                    </el-checkbox-group>
                </div>
                <div ref="gasChart" style="width: 100%; height: 500px; background: white; border-radius: 12px; padding: 10px; box-shadow: 0 0 20px rgba(0,0,0,0.5);"></div>
            </div>
            <div slot="footer" class="dialog-footer">
                <el-button @click="curveDialogVisible = false">关 闭</el-button>
            </div>
        </el-dialog>
    </div>
</template>
<script>
import { SelectGasAlarmRecord, AddGasAlarmRecord, EditGasAlarmRecord, DeleteGasAlarmRecord, SelectGasAlarmRecordById } from "../api/api";
import { SelectWorkOrder, SelectALLDevice } from "../api/api";
import echarts from 'echarts';

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
               WorkOrderId: null,
               Gas1: 0,
               Gas2: 0,
               Gas3: 0,
               Gas4: 0,
            },
            editForm: {
            Id:"",
               WorkOrderId: null,
               Gas1: 0,
               Gas2: 0,
               Gas3: 0,
               Gas4: 0,
            },
            operationDisabled: false,
            formLabelWidth: "150px",
            WorkOrderList: [],
            DeviceList: [],
            Query: [],
            Orderby: [
                {
                    SortField: "CreateTime",
                    IsDesc: true,
                },
            ],
            filterDeviceId: null,
            filterWorkOrderCode: "",
            filterWorkOrderContent: "",
            selectDataArrL: [], //跨页多选所有的项
            
            // 曲线相关
            curveDialogVisible: false,
            chartLoading: false,
            chartInstance: null,
            selectedGases: [1, 2, 3, 4], // 默认展示前4种
            allCurveData: [], // 存储用于绘制曲线的所有数据
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
            SelectGasAlarmRecord(pageData).then(res => {
                const records = res.data || [];
                // 只显示第一个
                this.tableData = records.length > 0 ? [records[0]] : [];
                this.totalNumber = records.length > 0 ? 1 : 0;

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
            this.filterDeviceId = null;
            this.filterWorkOrderCode = "";
            this.filterWorkOrderContent = "";
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
            AddGasAlarmRecord(this.addForm).then(res => {
                if (res) {
                    this.$message({
                        message: "创建成功！",
                        type: "success",
                    });
                    this.getTableData();
                    this.addDialogFormVisible = false;
                    // 重置表单
                    this.addForm = {
                        WorkOrderId: null,
                        Gas1: 0,
                        Gas2: 0,
                        Gas3: 0,
                        Gas4: 0,
                    };
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
               this.editForm.WorkOrderId = row.WorkOrderId;
               this.editForm.Gas1 = row.Gas1 || 0;
               this.editForm.Gas2 = row.Gas2 || 0;
               this.editForm.Gas3 = row.Gas3 || 0;
               this.editForm.Gas4 = row.Gas4 || 0;
            
            this.editDialogFormVisible = true;
        },
        //点击确认修改按钮
        EditConfirm() {
            this.operationDisabled = true;
            EditGasAlarmRecord(this.editForm).then(res => {
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
                    DeleteGasAlarmRecord(Data).then((res) => {
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
            this.queryData();
        },
        
        // 工单内容选择事件
        handleWorkOrderContentSelect(item) {
            this.filterWorkOrderContent = item.value;
            this.queryData();
        },
        
        // 显示气体曲线
        showGasCurve() {
            this.curveDialogVisible = true;
            this.chartLoading = true;
            this.fetchFullGasData();
        },
        
        // 获取用于绘图的完整数据（不分页）
        fetchFullGasData() {
            // 使用当前的查询条件，但请求更多数据
            var pageData = {
                Query: this.Query,
                Orderby: [{ SortField: "CreateTime", IsDesc: false }], // 曲线应该正序
                CurrentPage: 0,
                PageNumber: 500, // 获取最近500条记录用于绘图
            };
            
            SelectGasAlarmRecord(pageData).then(res => {
                this.allCurveData = res.data || [];
                this.chartLoading = false;
                if (this.curveDialogVisible) {
                    this.$nextTick(() => {
                        this.updateChart();
                    });
                }
            }).catch(err => {
                console.error("加载曲线数据失败:", err);
                this.chartLoading = false;
                this.$message.error("加载曲线数据失败");
            });
        },
        
        // 当对话框打开时初始化图表
        onCurveDialogOpened() {
            setTimeout(() => {
                if (!this.chartInstance) {
                    this.chartInstance = echarts.init(this.$refs.gasChart);
                }
                // 确保数据已加载后再更新
                if (this.allCurveData && this.allCurveData.length) {
                    this.updateChart();
                }
                this.chartInstance.resize();
                
                // 模拟点击以强制刷新（最后的手段）
                setTimeout(() => {
                    const event = new MouseEvent('click', {
                        view: window,
                        bubbles: true,
                        cancelable: true
                    });
                    if (this.$refs.gasChart) {
                        this.$refs.gasChart.dispatchEvent(event);
                    }
                }, 100);
            }, 300);
            
            // 响应窗口大小变化
            window.addEventListener('resize', this.handleResize);
        },
        
        handleResize() {
            if (this.chartInstance) {
                this.chartInstance.resize();
            }
        },
        
        // 更新图表内容
        updateChart() {
            if (!this.chartInstance || !this.allCurveData.length) return;
            
            const times = this.allCurveData.map(item => item.CreateTime);
            const series = [];
            
            const gasNamesMap = {
                1: '一氧化碳 (ppm)',
                2: '硫化氢 (ppm)',
                3: '甲烷 (%LEL)',
                4: '二氧化硫 (ppm)'
            };
            
            this.selectedGases.forEach(gasIndex => {
                series.push({
                    name: gasNamesMap[gasIndex],
                    type: 'line',
                    smooth: true,
                    showSymbol: false,
                    data: this.allCurveData.map(item => item[`Gas${gasIndex}`] || 0)
                });
            });
            
            const option = {
                title: {
                    text: '浓度变化趋势'
                },
                tooltip: {
                    trigger: 'axis'
                },
                legend: {
                    data: this.selectedGases.map(i => gasNamesMap[i])
                },
                grid: {
                    left: '3%',
                    right: '4%',
                    bottom: '3%',
                    containLabel: true
                },
                toolbox: {
                    feature: {
                        saveAsImage: {}
                    }
                },
                xAxis: {
                    type: 'category',
                    boundaryGap: false,
                    data: times,
                    axisLabel: {
                        formatter: function(value) {
                            // 简化时间显示
                            return value.split(' ')[1] || value;
                        }
                    }
                },
                yAxis: {
                    type: 'value',
                    name: '浓度值',
                    min: 0
                },
                series: series
            };
            
            this.chartInstance.setOption(option, true);
        },
        
        beforeDestroy() {
            window.removeEventListener('resize', this.handleResize);
            if (this.chartInstance) {
                this.chartInstance.dispose();
            }
        }
    }
};
</script>

