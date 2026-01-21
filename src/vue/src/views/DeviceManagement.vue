
/***********************************************************************
 * 本文件由T4模板生成，请将本文件复制到前端YixiaoAdmin/views文件夹中使用
 * 文件名：DeviceManagement.vue
************************************************************************/
<template>
    <div class="container" >
        <el-col :span="24" class="toolbar">
            <el-input style="width: 150px" placeholder="搜索名称" v-model="filterName" clearable @clear="queryData"></el-input>&nbsp;&nbsp;
            <el-input style="width: 150px" placeholder="搜索型号" v-model="filterModel" clearable @clear="queryData"></el-input>&nbsp;&nbsp;
            <el-select style="width: 120px" v-model="filterOnlineStatus" placeholder="在线状态" clearable @change="queryData">
                <el-option label="在线" value="在线"></el-option>
                <el-option label="离线" value="离线"></el-option>
            </el-select>&nbsp;&nbsp;
            <el-input style="width: 150px" placeholder="搜索单位" v-model="filterBelongToUnit" clearable @clear="queryData"></el-input>&nbsp;&nbsp;
            <el-input style="width: 150px" placeholder="搜索IP" v-model="filterIP" clearable @clear="queryData"></el-input>&nbsp;&nbsp;
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
            
            <el-table-column :show-overflow-tooltip="true" label="绑定用户" width="200" v-if="isAdmin">
                <template slot-scope="scope">
                    <el-tag 
                        v-for="userName in scope.row.BoundUserNames" 
                        :key="userName" 
                        size="small" 
                        style="margin-right: 5px; margin-bottom: 5px;"
                    >
                        {{ userName }}
                    </el-tag>
                    <span v-if="!scope.row.BoundUserNames || scope.row.BoundUserNames.length === 0" style="color: #909399; font-size: 12px;">
                        未绑定
                    </span>
                </template>
            </el-table-column>
            
            <el-table-column :show-overflow-tooltip="true" prop="Model" label="设备型号" width="220" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="ManufactureDate" label="出厂日期" width="220" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="BelongToUnit" label="单位" width="220" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="GpsLongitude" label="GPS经度" width="150" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="GpsLatitude" label="GPS纬度" width="150" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="IP" label="IP地址" width="150" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="Name" label="名称" width="220" ></el-table-column>
    
            <el-table-column :show-overflow-tooltip="true" prop="OnlineStatus" label="在线状态" width="120" >
                <template slot-scope="scope">
                    <el-tag size="small">
                        {{ scope.row.OnlineStatus || '离线' }}
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

            <!-- 绑定用户 - 仅管理员可见，放在第一个位置 -->
            <el-form-item label="绑定用户" :label-width="formLabelWidth" v-if="isAdmin">
                <el-select
                    v-model="addForm.UserIds"
                    multiple
                    filterable
                    placeholder="请选择要绑定的用户（非管理员）"
                    style="width: 100%;"
                >
                    <el-option
                        v-for="user in nonAdminUserList"
                        :key="user.Id"
                        :label="user.UserName"
                        :value="user.Id"
                    ></el-option>
                </el-select>
                <div v-if="nonAdminUserList.length === 0" style="color: #909399; font-size: 12px; margin-top: 5px;">
                    暂无可用用户（请先创建非管理员用户）
                </div>
            </el-form-item>

            <el-form-item label="设备型号" :label-width="formLabelWidth">
                <el-input v-model="addForm.Model" autocomplete="off" placeholder="请输入设备型号"></el-input>
            </el-form-item>

    
    

            <el-form-item label="出厂日期" :label-width="formLabelWidth">
                <el-date-picker
                    v-model="addForm.ManufactureDate"
                    type="date"
                    placeholder="选择出厂日期"
                    value-format="yyyy-MM-dd"
                    >
                </el-date-picker>
            </el-form-item>

    
    

            <el-form-item label="单位" :label-width="formLabelWidth">
                <el-input v-model="addForm.BelongToUnit" autocomplete="off" placeholder="请输入单位"></el-input>
            </el-form-item>

            <el-form-item label="GPS经度" :label-width="formLabelWidth">
                <el-input v-model="addForm.GpsLongitude" autocomplete="off" placeholder="请输入GPS经度"></el-input>
            </el-form-item>

            <el-form-item label="GPS纬度" :label-width="formLabelWidth">
                <el-input v-model="addForm.GpsLatitude" autocomplete="off" placeholder="请输入GPS纬度"></el-input>
            </el-form-item>

            <el-form-item label="IP地址" :label-width="formLabelWidth">
                <el-input v-model="addForm.IP" autocomplete="off" placeholder="请输入IP地址"></el-input>
            </el-form-item>

            <el-form-item label="名称" :label-width="formLabelWidth">
                <el-input v-model="addForm.Name" autocomplete="off" placeholder="请输入名称"></el-input>
            </el-form-item>
    
            </el-form>
            <div slot="footer" class="dialog-footer">
                <el-button @click="addDialogFormVisible = false">取 消</el-button>
                <el-button type="primary" @click="AddConfirm()" :disabled = "operationDisabled">确 定</el-button>
            </div>
        </el-dialog>

        <el-dialog title="编辑" :visible.sync="editDialogFormVisible">
            <el-form :model="editForm">

            <!-- 绑定用户 - 仅管理员可见，放在第一个位置 -->
            <el-form-item label="绑定用户" :label-width="formLabelWidth" v-if="isAdmin">
                <el-select
                    v-model="editForm.UserIds"
                    multiple
                    filterable
                    placeholder="请选择要绑定的用户（非管理员）"
                    style="width: 100%;"
                >
                    <el-option
                        v-for="user in nonAdminUserList"
                        :key="user.Id"
                        :label="user.UserName"
                        :value="user.Id"
                    ></el-option>
                </el-select>
                <div v-if="nonAdminUserList.length === 0" style="color: #909399; font-size: 12px; margin-top: 5px;">
                    暂无可用用户（请先创建非管理员用户）
                </div>
            </el-form-item>

            <el-form-item label="设备型号" :label-width="formLabelWidth">
                <el-input v-model="editForm.Model" autocomplete="off" placeholder="请输入设备型号"></el-input>
            </el-form-item>



            <el-form-item label="出厂日期" :label-width="formLabelWidth">
                <el-date-picker
                    v-model="editForm.ManufactureDate"
                    type="date"
                    placeholder="选择出厂日期"
                    value-format="yyyy-MM-dd"
                    >
                </el-date-picker>
            </el-form-item>



            <el-form-item label="单位" :label-width="formLabelWidth">
                <el-input v-model="editForm.BelongToUnit" autocomplete="off" placeholder="请输入单位"></el-input>
            </el-form-item>

            <el-form-item label="GPS经度" :label-width="formLabelWidth">
                <el-input v-model="editForm.GpsLongitude" autocomplete="off" placeholder="请输入GPS经度"></el-input>
            </el-form-item>

            <el-form-item label="GPS纬度" :label-width="formLabelWidth">
                <el-input v-model="editForm.GpsLatitude" autocomplete="off" placeholder="请输入GPS纬度"></el-input>
            </el-form-item>

            <el-form-item label="IP地址" :label-width="formLabelWidth">
                <el-input v-model="editForm.IP" autocomplete="off" placeholder="请输入IP地址"></el-input>
            </el-form-item>

            <el-form-item label="名称" :label-width="formLabelWidth">
                <el-input v-model="editForm.Name" autocomplete="off" placeholder="请输入名称"></el-input>
            </el-form-item>

            <el-form-item label="在线状态" :label-width="formLabelWidth">
                <el-select v-model="editForm.OnlineStatus" placeholder="请选择在线状态" style="width: 100%;">
                    <el-option label="在线" value="在线"></el-option>
                    <el-option label="离线" value="离线"></el-option>
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
import { SelectDevice, AddDevice, EditDevice, DeleteDevice, SelectDeviceById, GetUserIdsByDeviceId, BindUsersToDevice, SelectUser, SelectALLUser, SelectALLRole } from "../api/api";
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
    
               Model: null,
    
               ManufactureDate: null,
    
               BelongToUnit: null,
    
               GpsLongitude: null,
    
               GpsLatitude: null,
    
               IP: null,
    
               Name: null,
               UserIds: [],
            },
            editForm: {
            Id:"",
    
               Model: null,
    
               ManufactureDate: null,
    
               BelongToUnit: null,
    
               GpsLongitude: null,
    
               GpsLatitude: null,
    
               IP: null,
    
               Name: null,
    
               OnlineStatus: null,
               UserIds: [],
            },
            operationDisabled: false,
            formLabelWidth: "120px",
            Query: [
                {
                    QueryField: "Name",
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
            filterName: "",
            filterModel: "",
            filterOnlineStatus: null,
            filterBelongToUnit: "",
            filterIP: "",
            nonAdminUserList: [], // 非管理员用户列表
            adminRoleId: null, // 管理员角色ID
            isAdmin: false, // 是否是管理员
        };
    },
    mounted() {
       (async () => {
            // 立即检查权限
            this.checkIsAdmin();
            // 延迟检查，确保localStorage已经设置
            setTimeout(() => {
                this.checkIsAdmin();
            }, 300);
            // 再次延迟检查，确保获取到权限
            setTimeout(() => {
                this.checkIsAdmin();
                console.log('设备管理 - 最终isAdmin状态:', this.isAdmin);
            }, 1000);
            await this.loadNonAdminUsers();
            this.getTableData();
        })();
    },
    computed: {
        // 使用computed确保响应式更新
        showBindUserField() {
            // 直接返回isAdmin，Vue会自动追踪isAdmin的变化
            return this.isAdmin;
        }
    },
    watch: {
        // 监听路由变化，重新检查权限
        '$route'() {
            setTimeout(() => {
                this.checkIsAdmin();
            }, 100);
        }
    },
    methods: {
        //获取表格数据
        getTableData() {
            this.operationDisabled = true;
            this.Query = [];
            if (this.filterName) this.Query.push({ QueryField: "Name", QueryStr: this.filterName });
            if (this.filterModel) this.Query.push({ QueryField: "Model", QueryStr: this.filterModel });
            if (this.filterOnlineStatus) this.Query.push({ QueryField: "OnlineStatus", QueryStr: this.filterOnlineStatus });
            if (this.filterBelongToUnit) this.Query.push({ QueryField: "BelongToUnit", QueryStr: this.filterBelongToUnit });
            if (this.filterIP) this.Query.push({ QueryField: "IP", QueryStr: this.filterIP });

            var pageData = {
                Query: this.Query,
                Orderby: this.Orderby,
                CurrentPage: this.currentPage - 1,
                PageNumber: this.pageSize,
            };
            SelectDevice(pageData).then(async res => {
                console.log('设备查询返回结果:', res);
                console.log('res.data:', res.data);
                console.log('res.count:', res.count);
                
                // 处理返回数据结构（可能是 res.data.data 或 res.data）
                if (res && res.data) {
                    if (Array.isArray(res.data)) {
                        this.tableData = res.data;
                        this.totalNumber = res.count || res.data.length;
                    } else if (res.data.data) {
                        // 如果返回的是 PagesResponse 结构
                        this.tableData = res.data.data || [];
                        this.totalNumber = res.data.count || 0;
                    } else {
                        this.tableData = [];
                        this.totalNumber = 0;
                    }
                } else {
                    this.tableData = [];
                    this.totalNumber = 0;
                }

                //如果发现加载了一个空页尝试向前翻一页，针对当一页只有一条数据时将该数据删除，页面显示异常问题
                if (this.currentPage > 1 && this.tableData.length == 0) {
                    this.currentPage -= 1;
                    this.getTableData();
                    return;
                }

                // 如果是管理员，加载每个设备的绑定用户信息
                if (this.isAdmin && this.tableData.length > 0) {
                    // 获取所有用户信息（用于将用户ID转换为用户名）
                    let allUsers = [];
                    try {
                        allUsers = await SelectALLUser();
                    } catch (error) {
                        console.error('获取用户列表失败:', error);
                    }

                    // 为每个设备加载绑定的用户信息
                    const loadBoundUsersPromises = this.tableData.map(async (device) => {
                        try {
                            const userIds = await GetUserIdsByDeviceId({ deviceId: device.Id });
                            // 将用户ID转换为用户名
                            if (userIds && userIds.length > 0 && allUsers.length > 0) {
                                device.BoundUserNames = userIds.map(userId => {
                                    const user = allUsers.find(u => u.Id === userId);
                                    return user ? user.UserName : userId;
                                });
                            } else {
                                device.BoundUserNames = [];
                            }
                        } catch (error) {
                            console.error(`获取设备 ${device.Id} 的绑定用户失败:`, error);
                            device.BoundUserNames = [];
                        }
                    });

                    await Promise.all(loadBoundUsersPromises);
                    // 强制更新视图
                    this.$forceUpdate();
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
            this.filterName = "";
            this.filterModel = "";
            this.filterOnlineStatus = null;
            this.filterBelongToUnit = "";
            this.filterIP = "";
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
            // 重新检查管理员权限（确保权限是最新的）
            this.checkIsAdmin();
            console.log('打开添加对话框 - isAdmin:', this.isAdmin);
            // 确保用户列表已加载
            if (this.isAdmin && this.nonAdminUserList.length === 0) {
                this.loadNonAdminUsers();
            }
            // 使用$nextTick确保Vue更新后再显示对话框
            this.$nextTick(() => {
                this.addDialogFormVisible = true;
                // 再次检查权限，确保对话框显示时权限正确
                this.checkIsAdmin();
                console.log('对话框打开后 - isAdmin:', this.isAdmin);
                // 强制更新视图
                this.$forceUpdate();
            });
        },
        //点击确认添加按钮
        AddConfirm() {
            this.operationDisabled = true;
            // 只有管理员才能绑定用户
            const userIds = this.isAdmin ? (this.addForm.UserIds || []) : [];
            // 先添加设备
            AddDevice(this.addForm).then(res => {
                if (res) {
                    // 如果设备创建成功且有绑定的用户，则绑定用户
                    // 需要重新查询设备以获取ID
                    if (this.isAdmin && userIds.length > 0) {
                        // 延迟一下，确保设备已保存
                        setTimeout(() => {
                            // 通过设备名称查找刚创建的设备
                            const pageData = {
                                Query: [{ QueryField: "Name", QueryStr: this.addForm.Name }],
                                Orderby: [{ SortField: "CreateTime", IsDesc: true }],
                                CurrentPage: 0,
                                PageNumber: 1,
                            };
                            SelectDevice(pageData).then(deviceRes => {
                                if (deviceRes && deviceRes.data && deviceRes.data.length > 0) {
                                    const deviceId = deviceRes.data[0].Id;
                                    BindUsersToDevice({
                                        DeviceId: deviceId,
                                        UserIds: userIds
                                    }).then(bindRes => {
                                        if (bindRes) {
                                            this.$message({
                                                message: "设备创建并绑定用户成功！",
                                                type: "success",
                                            });
                                        } else {
                                            this.$message({
                                                message: "设备创建成功，但用户绑定失败！",
                                                type: "warning",
                                            });
                                        }
                                        this.addDialogFormVisible = false;
                                        this.addForm.UserIds = [];
                                        this.getTableData();
                                        this.operationDisabled = false;
                                    }).catch(() => {
                                        this.$message({
                                            message: "设备创建成功，但用户绑定失败！",
                                            type: "warning",
                                        });
                                        this.addDialogFormVisible = false;
                                        this.addForm.UserIds = [];
                                        this.getTableData();
                                        this.operationDisabled = false;
                                    });
                                } else {
                                    this.$message({
                                        message: "设备创建成功，但无法找到设备ID进行用户绑定！",
                                        type: "warning",
                                    });
                                    this.addDialogFormVisible = false;
                                    this.addForm.UserIds = [];
                                    this.getTableData();
                                    this.operationDisabled = false;
                                }
                            });
                        }, 500);
                    } else {
                        this.$message({
                            message: "创建成功！",
                            type: "success",
                        });
                        this.addDialogFormVisible = false;
                        this.addForm.UserIds = [];
                        this.getTableData();
                        this.operationDisabled = false;
                    }
                } else {
                    this.$message.error("创建失败！");
                    this.operationDisabled = false;
                }
            })
            .catch((res) => {
                    this.operationDisabled = false;
                    this.$message.error("创建失败！");
            });
        },
        //点击修改按钮
        handleEdit(row) {
               // 重新检查管理员权限（确保权限是最新的）
               this.checkIsAdmin();
               console.log('打开编辑对话框 - isAdmin:', this.isAdmin);
               // 确保用户列表已加载
               if (this.isAdmin && this.nonAdminUserList.length === 0) {
                   this.loadNonAdminUsers();
               }
               
               this.editForm.Id = row.Id;
    
               this.editForm.Model = row.Model;
    
               this.editForm.ManufactureDate = row.ManufactureDate;
    
               this.editForm.BelongToUnit = row.BelongToUnit;
    
               this.editForm.GpsLongitude = row.GpsLongitude;
    
               this.editForm.GpsLatitude = row.GpsLatitude;
    
               this.editForm.IP = row.IP;
    
               this.editForm.Name = row.Name;
    
               this.editForm.OnlineStatus = row.OnlineStatus;
    
               // 只有管理员才加载该设备绑定的用户
               if (this.isAdmin) {
                   GetUserIdsByDeviceId({ deviceId: row.Id }).then(res => {
                       this.editForm.UserIds = res || [];
                   }).catch(() => {
                       this.editForm.UserIds = [];
                   });
               } else {
                   this.editForm.UserIds = [];
               }
            
            // 使用$nextTick确保Vue更新后再显示对话框
            this.$nextTick(() => {
                this.editDialogFormVisible = true;
                // 再次检查权限，确保对话框显示时权限正确
                this.checkIsAdmin();
                console.log('编辑对话框打开后 - isAdmin:', this.isAdmin);
                // 强制更新视图
                this.$forceUpdate();
            });
        },
        //点击确认修改按钮
        EditConfirm() {
            this.operationDisabled = true;
            // 先更新设备信息
            EditDevice(this.editForm).then(res => {
                    if (res) {
                        // 只有管理员才能更新用户绑定关系
                        if (this.isAdmin) {
                            // 更新用户绑定关系（即使UserIds为空数组，也会删除所有绑定）
                            const userIds = Array.isArray(this.editForm.UserIds) ? this.editForm.UserIds : [];
                            console.log('更新用户绑定关系，DeviceId:', this.editForm.Id, 'UserIds:', userIds);
                            
                            BindUsersToDevice({
                                DeviceId: this.editForm.Id,
                                UserIds: userIds
                            }).then(bindRes => {
                                console.log('绑定结果:', bindRes);
                                if (bindRes) {
                                    this.$message({
                                        message: "编辑成功！",
                                        type: "success",
                                    });
                                } else {
                                    this.$message({
                                        message: "设备信息更新成功，但用户绑定失败！",
                                        type: "warning",
                                    });
                                }
                                this.getTableData();
                                this.operationDisabled = false;
                                this.editDialogFormVisible = false;
                            }).catch((error) => {
                                console.error('绑定用户失败:', error);
                                this.$message({
                                    message: "设备信息更新成功，但用户绑定失败！",
                                    type: "warning",
                                });
                                this.getTableData();
                                this.operationDisabled = false;
                                this.editDialogFormVisible = false;
                            });
                        } else {
                            // 非管理员用户，只更新设备信息，不更新绑定关系
                            this.$message({
                                message: "编辑成功！",
                                type: "success",
                            });
                            this.getTableData();
                            this.operationDisabled = false;
                            this.editDialogFormVisible = false;
                        }
                    } else {
                        this.$message.error("编辑失败！");
                        this.operationDisabled = false;
                    }
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
                    DeleteDevice(Data).then((res) => {
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
        
        // 检查当前用户是否是管理员
        checkIsAdmin() {
            try {
                const accessStr = localStorage.getItem('access');
                console.log('设备管理 - localStorage中的access原始值:', accessStr);
                
                if (accessStr && accessStr !== 'undefined') {
                    const role = JSON.parse(accessStr);
                    
                    // 调试信息：输出角色信息
                    console.log('设备管理 - 解析后的角色对象:', role);
                    console.log('设备管理 - 角色Code:', role.Code);
                    console.log('设备管理 - 角色Code类型:', typeof role.Code);
                    console.log('设备管理 - 角色Name:', role.Name);
                    console.log('设备管理 - 角色完整结构:', JSON.stringify(role, null, 2));
                    
                    if (role) {
                        // 检查角色Code是否为"Admin"（区分大小写）
                        // 同时也检查角色名称是否包含"管理"或"admin"（不区分大小写）
                        const roleCode = role.Code || '';
                        const roleName = (role.Name || '').toLowerCase();
                        
                        // 多种判断方式
                        const isCodeAdmin = roleCode === 'Admin';
                        const isCodeAdminLower = roleCode === 'admin';
                        const isNameAdmin = roleName.includes('管理') || roleName.includes('admin');
                        
                        this.isAdmin = isCodeAdmin || isCodeAdminLower || isNameAdmin;
                        
                        console.log('设备管理 - Code === "Admin":', isCodeAdmin);
                        console.log('设备管理 - Code === "admin":', isCodeAdminLower);
                        console.log('设备管理 - Name包含"管理"或"admin":', isNameAdmin);
                        console.log('设备管理 - 最终判断是否为管理员:', this.isAdmin);
                    } else {
                        this.isAdmin = false;
                        console.warn('设备管理 - 角色对象为空');
                    }
                } else {
                    this.isAdmin = false;
                    console.warn('设备管理 - 用户权限信息未找到');
                }
            } catch (error) {
                console.error('检查管理员权限失败:', error);
                console.error('错误详情:', error.stack);
                this.isAdmin = false;
            }
        },
        
        // 加载非管理员用户列表
        async loadNonAdminUsers() {
            try {
                // 先获取所有角色，找到管理员角色
                const roles = await SelectALLRole();
                const adminRole = roles.find(r => r.Name === "管理员" || r.Code === "Admin");
                
                if (adminRole) {
                    this.adminRoleId = adminRole.Id;
                }
                
                // 获取所有用户
                const pageData = {
                    Query: [],
                    Orderby: [],
                    CurrentPage: 0,
                    PageNumber: 1000, // 获取足够多的用户
                };
                const userRes = await SelectUser(pageData);
                const allUsers = userRes.data || [];
                
                // 过滤出非管理员用户
                this.nonAdminUserList = allUsers.filter(user => {
                    return user.RoleId !== this.adminRoleId;
                });
            } catch (error) {
                console.error("加载用户列表失败:", error);
                this.nonAdminUserList = [];
            }
        },
    }
};
</script>
