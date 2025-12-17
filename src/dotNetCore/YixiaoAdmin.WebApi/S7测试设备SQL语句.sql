-- ============================================
-- S7 PLC测试设备SQL语句
-- ============================================

-- ============================================
-- 方式1: 插入新的测试设备（推荐）
-- ============================================

-- 插入单个测试设备
INSERT INTO Device (
    Id,
    Name,
    IP,
    Model,
    BelongToUnit,
    CreateTime,
    CreateUsername,
    ModificationTime,
    ModificationUsername,
    State,
    Type
) VALUES (
    NEWID(),                                    -- Id: 自动生成GUID
    N'S7测试设备1',                             -- Name: 设备名称
    N'192.168.0.100',                           -- IP: PLC的IP地址（必需）
    N'S7-1200',                                 -- Model: 设备型号
    N'测试车间',                                 -- BelongToUnit: 所属单位
    GETDATE(),                                  -- CreateTime: 创建时间
    N'admin',                                   -- CreateUsername: 创建人
    GETDATE(),                                  -- ModificationTime: 修改时间
    N'admin',                                   -- ModificationUsername: 修改人
    N'1',                                       -- State: 状态（1=启用）
    N'PLC'                                      -- Type: 设备类型
);

-- ============================================
-- 方式2: 批量插入多个测试设备
-- ============================================

INSERT INTO Device (
    Id,
    Name,
    IP,
    Model,
    BelongToUnit,
    CreateTime,
    CreateUsername,
    ModificationTime,
    ModificationUsername,
    State,
    Type
) VALUES 
    (NEWID(), N'S7测试设备1', N'192.168.0.100', N'S7-1200', N'测试车间1', GETDATE(), N'admin', GETDATE(), N'admin', N'1', N'PLC'),
    (NEWID(), N'S7测试设备2', N'192.168.0.101', N'S7-1200', N'测试车间2', GETDATE(), N'admin', GETDATE(), N'admin', N'1', N'PLC'),
    (NEWID(), N'S7测试设备3', N'192.168.0.102', N'S7-1500', N'测试车间3', GETDATE(), N'admin', GETDATE(), N'admin', N'1', N'PLC');

-- ============================================
-- 方式3: 更新现有设备的IP地址
-- ============================================

-- 根据设备ID更新IP地址
UPDATE Device 
SET 
    IP = N'192.168.0.100',
    ModificationTime = GETDATE(),
    ModificationUsername = N'admin'
WHERE Id = '设备ID';  -- 替换为实际的设备ID

-- 根据设备名称更新IP地址
UPDATE Device 
SET 
    IP = N'192.168.0.100',
    ModificationTime = GETDATE(),
    ModificationUsername = N'admin'
WHERE Name = N'设备名称';  -- 替换为实际的设备名称

-- 批量更新所有没有IP的设备（设置为测试IP）
UPDATE Device 
SET 
    IP = N'192.168.0.100',
    ModificationTime = GETDATE(),
    ModificationUsername = N'admin'
WHERE IP IS NULL OR IP = '';

-- ============================================
-- 方式4: 使用GUID格式的ID（与系统一致）
-- ============================================

-- 如果系统使用特定格式的GUID（32位大写，无连字符）
-- 可以使用以下方式生成：
INSERT INTO Device (
    Id,
    Name,
    IP,
    Model,
    BelongToUnit,
    CreateTime,
    CreateUsername,
    ModificationTime,
    ModificationUsername,
    State,
    Type
) VALUES (
    UPPER(REPLACE(NEWID(), '-', '')),          -- Id: 32位大写GUID（无连字符）
    N'S7测试设备1',
    N'192.168.0.100',
    N'S7-1200',
    N'测试车间',
    GETDATE(),
    N'admin',
    GETDATE(),
    N'admin',
    N'1',
    N'PLC'
);

-- ============================================
-- 方式5: 快速测试 - 最简单的插入语句
-- ============================================

-- 只填写必需字段和IP地址
INSERT INTO Device (
    Id,
    Name,
    IP,
    CreateTime,
    ModificationTime
) VALUES (
    NEWID(),
    N'S7测试设备',
    N'192.168.0.100',  -- 修改为实际的PLC IP地址
    GETDATE(),
    GETDATE()
);

-- ============================================
-- 查询和验证SQL语句
-- ============================================

-- 查询所有设备及其IP地址
SELECT 
    Id,
    Name,
    IP,
    Model,
    BelongToUnit,
    CreateTime,
    State
FROM Device
ORDER BY CreateTime DESC;

-- 查询所有有IP地址的设备
SELECT 
    Id,
    Name,
    IP,
    Model
FROM Device
WHERE IP IS NOT NULL AND IP != ''
ORDER BY Name;

-- 查询所有没有IP地址的设备
SELECT 
    Id,
    Name,
    IP,
    Model
FROM Device
WHERE IP IS NULL OR IP = ''
ORDER BY Name;

-- 查询特定IP地址的设备
SELECT 
    Id,
    Name,
    IP,
    Model,
    BelongToUnit
FROM Device
WHERE IP = N'192.168.0.100';

-- ============================================
-- 删除测试数据（谨慎使用）
-- ============================================

-- 删除特定测试设备
-- DELETE FROM Device WHERE Name LIKE N'S7测试设备%';

-- 删除特定IP地址的设备
-- DELETE FROM Device WHERE IP = N'192.168.0.100';

-- ============================================
-- 常用IP地址示例（根据实际情况修改）
-- ============================================

-- 本地回环地址（用于本地测试）
-- IP: 127.0.0.1 或 localhost

-- 局域网常见IP段
-- 192.168.0.x
-- 192.168.1.x
-- 10.0.0.x

-- 示例：
-- 192.168.0.100
-- 192.168.0.101
-- 192.168.1.100
-- 10.0.0.100

-- ============================================
-- 使用说明
-- ============================================
/*
1. 推荐使用方式1或方式5插入新设备
2. 如果设备已存在，使用方式3更新IP地址
3. 插入后使用查询语句验证数据
4. IP地址必须填写，否则系统无法连接PLC
5. 修改IP地址后，系统会在下次采集时自动重连
*/

