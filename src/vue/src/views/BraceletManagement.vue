/***********************************************************************
 * 本文件由T4模板生成，请将本文件复制到前端YixiaoAdmin/views文件夹中使用
    * 文件名：BraceletManagement.vue
************************************************************************/
<template>
    <div class="container" >
        <el-col :span="24" class="toolbar">
            <el-input style="width: 200px" placeholder="搜索手环编号" v-model="Query[0].QueryStr"></el-input>&nbsp;&nbsp;
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
            
            <el-table-column :show-overflow-tooltip="true" prop="BraceletNumber" label="手环编号" width="220" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="DeviceNumber" label="设备编号" width="220" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="BraceletRecord" label="手环记录" width="300" ></el-table-column>
    
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

            <el-form-item label="手环编号" :label-width="formLabelWidth">
                <el-input v-model="addForm.BraceletNumber" autocomplete="off" placeholder="请输入手环编号"></el-input>
            </el-form-item>

            <el-form-item label="设备编号" :label-width="formLabelWidth">
                <el-input v-model="addForm.DeviceNumber" autocomplete="off" placeholder="请输入设备编号"></el-input>
            </el-form-item>

            <el-form-item label="手环记录" :label-width="formLabelWidth">
                <el-input v-model="addForm.BraceletRecord" type="textarea" :rows="4" autocomplete="off" placeholder="请输入手环记录"></el-input>
            </el-form-item>

            </el-form>
            <div slot="footer" class="dialog-footer">
                <el-button @click="addDialogFormVisible = false">取 消</el-button>
                <el-button type="primary" @click="AddConfirm()" :disabled = "operationDisabled">确 定</el-button>
            </div>
        </el-dialog>

        <el-dialog title="编辑" :visible.sync="editDialogFormVisible">
            <el-form :model="editForm">

            <el-form-item label="手环编号" :label-width="formLabelWidth">
                <el-input v-model="editForm.BraceletNumber" autocomplete="off" placeholder="请输入手环编号"></el-input>
            </el-form-item>

            <el-form-item label="设备编号" :label-width="formLabelWidth">
                <el-input v-model="editForm.DeviceNumber" autocomplete="off" placeholder="请输入设备编号"></el-input>
            </el-form-item>

            <el-form-item label="手环记录" :label-width="formLabelWidth">
                <el-input v-model="editForm.BraceletRecord" type="textarea" :rows="4" autocomplete="off" placeholder="请输入手环记录"></el-input>
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
import { SelectBracelet, AddBracelet, EditBracelet, DeleteBracelet, SelectBraceletById } from "../api/api";

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
               BraceletNumber: null,
               DeviceNumber: null,
               BraceletRecord: null,
            },
            editForm: {
            Id:"",
               BraceletNumber: null,
               DeviceNumber: null,
               BraceletRecord: null,
            },
            operationDisabled: false,
            formLabelWidth: "120px",
            Query: [
                {
                    QueryField: "BraceletNumber",
                    QueryStr: this.queryStr,
                },
            ],
            Orderby: [
                {
                    SortField: "CreateTime",
                    IsDesc: false,
                },
            ],
            selectDataArrL: [], //跨页多选所有的项

        };
    },
    mounted() {
       (async () => {
            this.getTableData();
        })();
    },
    methods: {
        //获取表格数据
        getTableData() {
            this.operationDisabled = true;
            var pageData = {
                Query: this.Query,
                Orderby: this.Orderby,
                CurrentPage: this.currentPage - 1,
                PageNumber: this.pageSize,
            };
            SelectBracelet(pageData).then(res => {
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
            for(var item in this.Query){
                this.Query[item].QueryStr = "";
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
            AddBracelet(this.addForm).then(res => {
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
               this.editForm.BraceletNumber = row.BraceletNumber;
               this.editForm.DeviceNumber = row.DeviceNumber;
               this.editForm.BraceletRecord = row.BraceletRecord;
            
            this.editDialogFormVisible = true;
        },
        //点击确认修改按钮
        EditConfirm() {
            this.operationDisabled = true;
            EditBracelet(this.editForm).then(res => {
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
                    DeleteBracelet(Data).then((res) => {
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
    }
};
</script>