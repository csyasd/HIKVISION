
/***********************************************************************
 * 本文件由T4模板生成，请将本文件复制到前端YixiaoAdmin/views文件夹中使用
    * 文件名：WorkOrderManagement.vue
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
                @change="queryData">
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
                v-model="filterCode"
                :fetch-suggestions="queryWorkOrderCode"
                placeholder="搜索编号"
                clearable
                @select="handleWorkOrderCodeSelect"
                @input="queryData"
                @clear="queryData">
            </el-autocomplete>
            <el-autocomplete
                style="width: 200px; margin-right: 10px;"
                v-model="filterContent"
                :fetch-suggestions="queryWorkOrderContent"
                placeholder="搜索内容"
                clearable
                @select="handleWorkOrderContentSelect"
                @input="queryData"
                @clear="queryData">
            </el-autocomplete>
            <el-button @click="queryData()">查询</el-button>
            <el-button  @click="clearQuery()">清空</el-button>
            <el-button type="danger" @click="refreshTable()">刷新列表</el-button>
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
            
            <el-table-column :show-overflow-tooltip="true" prop="Device" label="设备" width="220" >
                <template slot-scope="scope">
                    <div>{{ DeviceList[DeviceList.findIndex((x) => x.Id == scope.row.DeviceId)]?.Name}}</div>
                </template>
            </el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="StartTime" label="开始时间" width="220" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="EndTime" label="结束时间" width="220" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="Code" label="编号" width="220" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="Content" label="内容" width="220" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="Status" label="工单状态" width="150" >
                <template slot-scope="scope">
                    <el-tag>
                        {{ scope.row.Status === 0 ? '未开始' : scope.row.Status === 1 ? '工单开始' : scope.row.Status === 2 ? '工单结束' : '未知' }}
                    </el-tag>
                </template>
            </el-table-column>

            <el-table-column
                :show-overflow-tooltip="true"
                prop="CreateTime"
                label="创建时间"
                width="220"
                sortable="custom"
            ></el-table-column>

            <el-table-column label="操作" width="150" fixed="right">
                <template slot-scope="scope">
                    <el-button
                        type="text"
                        size="small"
                        :disabled="scope.row.Status !== 1 || operationDisabled"
                        @click="handleEndWorkOrder(scope.row)"
                    >
                        工单结束
                    </el-button>
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


            <el-form-item label="设备" :label-width="formLabelWidth">
                <el-select v-model="addForm.DeviceId" placeholder="请选择" filterable>
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
            </el-form-item>

    
    

            <el-form-item label="开始时间" :label-width="formLabelWidth">
                <el-date-picker
                    v-model="addForm.StartTime"
                    type="datetime"
                    placeholder="选择开始时间"
                    value-format="yyyy-MM-dd HH:mm:ss"
                    >
                </el-date-picker>
             </el-form-item>

    
    

            <el-form-item label="结束时间" :label-width="formLabelWidth">
                <el-date-picker
                    v-model="addForm.EndTime"
                    type="datetime"
                    placeholder="选择结束时间"
                    value-format="yyyy-MM-dd HH:mm:ss"
                    >
                </el-date-picker>
             </el-form-item>

            <el-form-item label="编号" :label-width="formLabelWidth">
                <el-input v-model="addForm.Code" autocomplete="off" placeholder="请输入编号"></el-input>
            </el-form-item>

            <el-form-item label="内容" :label-width="formLabelWidth">
                <el-input v-model="addForm.Content" autocomplete="off" placeholder="请输入内容"></el-input>
            </el-form-item>

            <el-form-item label="工单状态" :label-width="formLabelWidth">
                <el-select v-model="addForm.Status" placeholder="请选择工单状态">
                    <el-option label="未开始" :value="0"></el-option>
                    <el-option label="工单开始" :value="1"></el-option>
                    <el-option label="工单结束" :value="2"></el-option>
                </el-select>
            </el-form-item>
    
    
            </el-form>
            <div slot="footer" class="dialog-footer">
                <el-button @click="addDialogFormVisible = false">取 消</el-button>
                <el-button type="primary" @click="AddConfirm()" :disabled = "operationDisabled">确 定</el-button>
            </div>
        </el-dialog>

        <el-dialog title="编辑" :visible.sync="editDialogFormVisible">
            <el-form :model="editForm">


            <el-form-item label="设备" :label-width="formLabelWidth">
                <el-select v-model="editForm.DeviceId" placeholder="请选择" filterable>
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
            </el-form-item>



            <el-form-item label="开始时间" :label-width="formLabelWidth">
                <el-date-picker
                    v-model="editForm.StartTime"
                    type="datetime"
                    placeholder="选择开始时间"
                    value-format="yyyy-MM-dd HH:mm:ss"
                    >
                </el-date-picker>
             </el-form-item>



            <el-form-item label="结束时间" :label-width="formLabelWidth">
                <el-date-picker
                    v-model="editForm.EndTime"
                    type="datetime"
                    placeholder="选择结束时间"
                    value-format="yyyy-MM-dd HH:mm:ss"
                    >
                </el-date-picker>
             </el-form-item>

            <el-form-item label="编号" :label-width="formLabelWidth">
                <el-input v-model="editForm.Code" autocomplete="off" placeholder="请输入编号"></el-input>
            </el-form-item>

            <el-form-item label="内容" :label-width="formLabelWidth">
                <el-input v-model="editForm.Content" autocomplete="off" placeholder="请输入内容"></el-input>
            </el-form-item>

            <el-form-item label="工单状态" :label-width="formLabelWidth">
                <el-select v-model="editForm.Status" placeholder="请选择工单状态">
                    <el-option label="未开始" :value="0"></el-option>
                    <el-option label="工单开始" :value="1"></el-option>
                    <el-option label="工单结束" :value="2"></el-option>
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
import { SelectWorkOrder, AddWorkOrder, EditWorkOrder, DeleteWorkOrder,SelectWorkOrderById } from "../api/api";
import{

           SelectALLDevice,

} from "../api/api";
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
            cascaderProps: { multiple: true },
            cascaderOptions:  [],
            addForm: {
    
               DeviceId: null,
    
               StartTime: null,
    
               EndTime: null,
    
               Code: null,
    
               Content: null,
    
               Status: 0,
    
            },
            editForm: {
            Id:"",
    
               DeviceId: null,
    
               StartTime: null,
    
               EndTime: null,
    
               Code: null,
    
               Content: null,
    
               Status: 0,
    
            },
            operationDisabled: false,
            formLabelWidth: "120px",
            Query: [],
            Orderby: [
                {
                    SortField: "CreateTime",
                    IsDesc: true,
                },
            ],
            filterDeviceId: null,
            filterCode: "",
            filterContent: "",
            selectDataArrL: [], //跨页多选所有的项

            DeviceList:[],
            WorkOrderList: [],

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
            
            this.loadWorkOrderList();
            this.getTableData();
        })();
    },
    methods: {
        //获取表格数据
        getTableData() {
            this.operationDisabled = true;
            
            // 构建查询条件
            this.Query = [];
            
            // 设备筛选
            if (this.filterDeviceId) {
                this.Query.push({
                    QueryField: "DeviceId",
                    QueryStr: this.filterDeviceId,
                });
            }
            
            // 编号筛选
            if (this.filterCode && this.filterCode.trim()) {
                this.Query.push({
                    QueryField: "Code",
                    QueryStr: this.filterCode.trim(),
                });
            }
            
            // 内容筛选
            if (this.filterContent && this.filterContent.trim()) {
                this.Query.push({
                    QueryField: "Content",
                    QueryStr: this.filterContent.trim(),
                });
            }
            
            var pageData = {
                Query: this.Query,
                Orderby: this.Orderby,
                CurrentPage: this.currentPage - 1,
                PageNumber: this.pageSize,
            };
            SelectWorkOrder(pageData).then(res => {
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
            this.filterCode = "";
            this.filterContent = "";
            
            // 重新设置默认设备
            const onlineDevices = this.DeviceList.filter(d => d.OnlineStatus === '在线');
            if (onlineDevices.length > 0) {
                this.filterDeviceId = onlineDevices[0].Id;
            } else {
                this.filterDeviceId = null;
            }
            
            this.queryData();
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
            AddWorkOrder(this.addForm).then(res => {
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
    
               this.editForm.DeviceId = row.DeviceId;
    
               this.editForm.StartTime = row.StartTime;
    
               this.editForm.EndTime = row.EndTime;
    
               this.editForm.Code = row.Code;
    
               this.editForm.Content = row.Content;
    
               this.editForm.Status = row.Status;
    
            
            this.editDialogFormVisible = true;
        },
        //点击确认修改按钮
        EditConfirm() {
            this.operationDisabled = true;
            EditWorkOrder(this.editForm).then(res => {
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
                    DeleteWorkOrder(Data).then((res) => {
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
            //if (row.Type == "") {
            //    return "dangerous-row";
            //}
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
            SelectWorkOrder({ Query: [], Orderby: [], CurrentPage: 0, PageNumber: 1000 }).then(res => {
                this.WorkOrderList = res.data || [];
            }).catch(error => {
                console.log(error);
                this.$message.error("加载工单列表失败！");
            });
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
            this.filterCode = item.value;
            this.queryData();
        },
        
        // 工单内容选择事件
        handleWorkOrderContentSelect(item) {
            this.filterContent = item.value;
            this.queryData();
        },
        
        // 工单结束
        handleEndWorkOrder(row) {
            this.$confirm('确定要结束该工单吗？', '提示', {
                confirmButtonText: '确定',
                cancelButtonText: '取消',
                type: 'warning'
            }).then(() => {
                this.operationDisabled = true;
                const updateData = {
                    Id: row.Id,
                    DeviceId: row.DeviceId,
                    StartTime: row.StartTime,
                    EndTime: row.EndTime || new Date().toISOString().replace('T', ' ').substring(0, 19),
                    Code: row.Code,
                    Content: row.Content,
                    Status: 2  // 工单结束
                };
                
                EditWorkOrder(updateData).then(res => {
                    if (res) {
                        this.$message({
                            message: '工单已结束！',
                            type: 'success',
                        });
                        this.getTableData();
                    } else {
                        this.$message.error('操作失败！');
                    }
                    this.operationDisabled = false;
                }).catch((error) => {
                    this.operationDisabled = false;
                    this.$message.error('操作失败！');
                    console.error(error);
                });
            }).catch(() => {
                // 用户取消操作
            });
        },
    }
};
</script>
