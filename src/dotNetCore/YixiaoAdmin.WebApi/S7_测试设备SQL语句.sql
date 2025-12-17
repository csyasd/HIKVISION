-- ============================================
-- S7 PLC 测试设备 SQL 语句
-- ============================================

-- ============================================
-- 方式1：添加新的测试设备（推荐）
-- ============================================

-- 添加单个测试设备
INSERT INTO Device (
    Id,
    Name,
    IP,
    Model,
    CreateTime,
    ModificationTime,
    State,
    Type
)
VALUES (
    NEWID(),                                    -- 自动生成GUID作为ID
    'S7测试PLC设备1',                           -- 设备名称
    '192.168.0.100',                            -- PLC的IP地址（重要！）
    'S7-1200',                                  -- 设备型号（可选）
    GETDATE(),                                  -- 创建时间
    GETDATE(),                                  -- 修改时间
    '1',                                        -- 状态（1=启用）
    'PLC'                                       -- 类型
);

-- ============================================
-- 方式2：批量添加多个测试设备
-- ============================================

INSERT INTO Device (
    Id,
    Name,
    IP,
    Model,
    CreateTime,
    ModificationTime,
    State,
    Type
)
VALUES 
    (NEWID(), 'S7测试PLC设备1', '192.168.0.100', 'S7-1200', GETDATE(), GETDATE(), '1', 'PLC'),
    (NEWID(), 'S7测试PLC设备2', '192.168.0.101', 'S7-1200', GETDATE(), GETDATE(), '1', 'PLC'),
    (NEWID(), 'S7测试PLC设备3', '192.168.0.102', 'S7-1500', GETDATE(), GETDATE(), '1', 'PLC'),
    (NEWID(), '生产车间PLC1', '192.168.1.100', 'S7-1200', GETDATE(), GETDATE(), '1', 'PLC'),
    (NEWID(), '生产车间PLC2', '192.168.1.101', 'S7-1200', GETDATE(), GETDATE(), '1', 'PLC');

-- ============================================
-- 方式3：更新已有设备的IP地址
-- ============================================

-- 更新指定设备的IP地址（根据设备ID）
UPDATE Device 
SET 
    IP = '192.168.0.100',
    ModificationTime = GETDATE()
WHERE Id = '设备ID';  -- 替换为实际的设备ID

-- 更新指定设备的IP地址（根据设备名称）
UPDATE Device 
SET 
    IP = '192.168.0.100',
    ModificationTime = GETDATE()
WHERE Name = '设备名称';  -- 替换为实际的设备名称

-- 批量更新所有没有IP地址的设备（设置为测试IP）
UPDATE Device 
SET 
    IP = '192.168.0.100',
    ModificationTime = GETDATE()
WHERE IP IS NULL OR IP = '';

-- ============================================
-- 方式4：使用指定ID添加测试设备（便于管理）
-- ============================================

-- 添加测试设备1
INSERT INTO Device (
    Id,
    Name,
    IP,
    Model,
    BelongToUnit,
    CreateTime,
    ModificationTime,
    State,
    Type,
    SortCode
)
VALUES (
    'PLC-TEST-001',                             -- 自定义ID（便于识别）
    'S7测试PLC设备1',
    '192.168.0.100',                            -- 修改为实际的PLC IP地址
    'S7-1200',
    '测试单位',
    GETDATE(),
    GETDATE(),
    '1',
    'PLC',
    1
);

-- 添加测试设备2
INSERT INTO Device (
    Id,
    Name,
    IP,
    Model,
    BelongToUnit,
    CreateTime,
    ModificationTime,
    State,
    Type,
    SortCode
)
VALUES (
    'PLC-TEST-002',
    'S7测试PLC设备2',
    '192.168.0.101',                            -- 修改为实际的PLC IP地址
    'S7-1200',
    '测试单位',
    GETDATE(),
    GETDATE(),
    '1',
    'PLC',
    2
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
    State,
    CreateTime
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
    Model
FROM Device
WHERE IP IS NULL OR IP = '';

-- 查询指定IP地址的设备
SELECT 
    Id,
    Name,
    IP,
    Model
FROM Device
WHERE IP = '192.168.0.100';

-- ============================================
-- 删除测试数据（谨慎使用）
-- ============================================

-- 删除指定测试设备
DELETE FROM Device WHERE Id = 'PLC-TEST-001';

-- 删除所有测试设备（根据名称前缀）
DELETE FROM Device WHERE Name LIKE 'S7测试%';

-- ============================================
-- 使用说明
-- ============================================
/*
使用步骤：
1. 选择合适的方式（推荐使用方式1或方式4）
2. 修改IP地址为实际的PLC IP地址
3. 根据需要修改设备名称和其他字段
4. 执行SQL语句
5. 使用查询语句验证数据是否正确

注意事项：
- IP地址格式：192.168.0.100（确保格式正确）
- 确保IP地址与PLC实际IP地址一致
- 如果设备已存在，使用UPDATE语句更新IP地址
- 建议先查询现有设备，避免重复添加

测试建议：
- 先添加1个测试设备，确认连接成功后再添加其他设备
- IP地址可以使用本地回环地址 127.0.0.1 进行测试（如果PLC在本机）
- 确保PLC与数据库服务器网络连通
*/

