-- 恢复所有缺失的表
-- 包括 WorkerStatusRecord 和新的异常表

USE [HIKVISION]
GO

PRINT '开始恢复表...';
GO

-- ============================================
-- 1. 恢复 WorkerStatusRecord 表（工人进出状态记录）
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkerStatusRecord]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[WorkerStatusRecord] (
        [Id] nvarchar(450) NOT NULL,
        [WorkOrderId] nvarchar(450) NULL,
        [HeartRate] nvarchar(max) NULL,
        [EntryExitStatus] nvarchar(max) NULL,
        [WorkerName] nvarchar(max) NULL,
        [Name] nvarchar(max) NULL,
        [CreateUsername] nvarchar(max) NULL,
        [CreateTime] datetime2 NOT NULL,
        [ModificationUsername] nvarchar(max) NULL,
        [ModificationTime] datetime2 NOT NULL,
        [ParentId] nvarchar(max) NULL,
        [SortCode] int NULL,
        [Type] nvarchar(max) NULL,
        [State] nvarchar(max) NULL,
        CONSTRAINT [PK_WorkerStatusRecord] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkerStatusRecord_WorkOrder_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [dbo].[WorkOrder] ([Id])
    );
    
    CREATE INDEX [IX_WorkerStatusRecord_WorkOrderId] ON [dbo].[WorkerStatusRecord] ([WorkOrderId]);
    PRINT '已创建表 WorkerStatusRecord';
END
ELSE
BEGIN
    PRINT '表 WorkerStatusRecord 已存在';
END
GO

-- ============================================
-- 2. 创建 AbnormalConfig 表（异常配置）
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AbnormalConfig]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AbnormalConfig] (
        [Id] nvarchar(450) NOT NULL,
        [ConfigType] nvarchar(max) NULL,
        [ConfigName] nvarchar(max) NULL,
        [MinValue] real NOT NULL,
        [MaxValue] real NOT NULL,
        [IsEnabled] bit NOT NULL,
        [Name] nvarchar(max) NULL,
        [CreateUsername] nvarchar(max) NULL,
        [CreateTime] datetime2 NOT NULL,
        [ModificationUsername] nvarchar(max) NULL,
        [ModificationTime] datetime2 NOT NULL,
        [ParentId] nvarchar(max) NULL,
        [SortCode] int NULL,
        [Type] nvarchar(max) NULL,
        [State] nvarchar(max) NULL,
        CONSTRAINT [PK_AbnormalConfig] PRIMARY KEY ([Id])
    );
    PRINT '已创建表 AbnormalConfig';
END
ELSE
BEGIN
    PRINT '表 AbnormalConfig 已存在';
END
GO

-- ============================================
-- 3. 创建 BraceletAbnormal 表（手环异常记录）
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BraceletAbnormal]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[BraceletAbnormal] (
        [Id] nvarchar(450) NOT NULL,
        [WorkOrderId] nvarchar(450) NULL,
        [HeartRate] nvarchar(max) NULL,
        [EntryExitStatus] nvarchar(max) NULL,
        [WorkerName] nvarchar(max) NULL,
        [Name] nvarchar(max) NULL,
        [CreateUsername] nvarchar(max) NULL,
        [CreateTime] datetime2 NOT NULL,
        [ModificationUsername] nvarchar(max) NULL,
        [ModificationTime] datetime2 NOT NULL,
        [ParentId] nvarchar(max) NULL,
        [SortCode] int NULL,
        [Type] nvarchar(max) NULL,
        [State] nvarchar(max) NULL,
        CONSTRAINT [PK_BraceletAbnormal] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_BraceletAbnormal_WorkOrder_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [dbo].[WorkOrder] ([Id])
    );
    
    CREATE INDEX [IX_BraceletAbnormal_WorkOrderId] ON [dbo].[BraceletAbnormal] ([WorkOrderId]);
    PRINT '已创建表 BraceletAbnormal';
END
ELSE
BEGIN
    PRINT '表 BraceletAbnormal 已存在';
END
GO

-- ============================================
-- 4. 创建 GasAbnormal 表（气体异常记录）
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GasAbnormal]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[GasAbnormal] (
        [Id] nvarchar(450) NOT NULL,
        [WorkOrderId] nvarchar(450) NULL,
        [Gas1] real NOT NULL,
        [Gas2] real NOT NULL,
        [Gas3] real NOT NULL,
        [Gas4] real NOT NULL,
        [Gas5] real NOT NULL,
        [Gas6] real NOT NULL,
        [Gas7] real NOT NULL,
        [Gas8] real NOT NULL,
        [Gas9] real NOT NULL,
        [Gas10] real NOT NULL,
        [Name] nvarchar(max) NULL,
        [CreateUsername] nvarchar(max) NULL,
        [CreateTime] datetime2 NOT NULL,
        [ModificationUsername] nvarchar(max) NULL,
        [ModificationTime] datetime2 NOT NULL,
        [ParentId] nvarchar(max) NULL,
        [SortCode] int NULL,
        [Type] nvarchar(max) NULL,
        [State] nvarchar(max) NULL,
        CONSTRAINT [PK_GasAbnormal] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GasAbnormal_WorkOrder_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [dbo].[WorkOrder] ([Id])
    );
    
    CREATE INDEX [IX_GasAbnormal_WorkOrderId] ON [dbo].[GasAbnormal] ([WorkOrderId]);
    PRINT '已创建表 GasAbnormal';
END
ELSE
BEGIN
    PRINT '表 GasAbnormal 已存在';
END
GO

-- ============================================
-- 5. 更新迁移历史记录
-- ============================================

-- 确保 WorkerStatusRecord 迁移记录存在
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251222055654_AddWorkerStatusRecord')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251222055654_AddWorkerStatusRecord', '6.0.13');
    PRINT '已插入迁移记录 20251222055654_AddWorkerStatusRecord';
END
GO

-- 确保 AbnormalTables 迁移记录存在
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251225025823_AddAbnormalTables')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251225025823_AddAbnormalTables', '6.0.13');
    PRINT '已插入迁移记录 20251225025823_AddAbnormalTables';
END
GO

PRINT '所有表恢复完成！';
GO


