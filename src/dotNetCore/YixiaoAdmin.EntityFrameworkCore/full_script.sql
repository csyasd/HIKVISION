IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Device] (
    [Id] nvarchar(450) NOT NULL,
    [Model] nvarchar(max) NULL,
    [ManufactureDate] nvarchar(max) NULL,
    [BelongToUnit] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_Device] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Right] (
    [Id] nvarchar(450) NOT NULL,
    [Code] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_Right] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Role] (
    [Id] nvarchar(450) NOT NULL,
    [Code] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Camera] (
    [Id] nvarchar(450) NOT NULL,
    [Model] nvarchar(max) NULL,
    [DeviceId] nvarchar(450) NULL,
    [DeviceCode] nvarchar(max) NULL,
    [IP] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_Camera] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Camera_Device_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Device] ([Id])
);
GO

CREATE TABLE [WorkRecord] (
    [Id] nvarchar(450) NOT NULL,
    [DeviceId] nvarchar(450) NULL,
    [StartTime] nvarchar(max) NULL,
    [EndTime] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_WorkRecord] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkRecord_Device_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Device] ([Id])
);
GO

CREATE TABLE [RoleRight] (
    [Id] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NULL,
    [RightId] nvarchar(450) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_RoleRight] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RoleRight_Right_RightId] FOREIGN KEY ([RightId]) REFERENCES [Right] ([Id]),
    CONSTRAINT [FK_RoleRight_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id])
);
GO

CREATE TABLE [User] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(max) NULL,
    [Password] nvarchar(max) NULL,
    [RoleId] nvarchar(450) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_User] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_User_Role_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Role] ([Id])
);
GO

CREATE TABLE [CameraRecord] (
    [Id] nvarchar(450) NOT NULL,
    [CameraId] nvarchar(450) NULL,
    [StartTime] nvarchar(max) NULL,
    [EndTime] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_CameraRecord] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CameraRecord_Camera_CameraId] FOREIGN KEY ([CameraId]) REFERENCES [Camera] ([Id])
);
GO

CREATE INDEX [IX_Camera_DeviceId] ON [Camera] ([DeviceId]);
GO

CREATE INDEX [IX_CameraRecord_CameraId] ON [CameraRecord] ([CameraId]);
GO

CREATE INDEX [IX_RoleRight_RightId] ON [RoleRight] ([RightId]);
GO

CREATE INDEX [IX_RoleRight_RoleId] ON [RoleRight] ([RoleId]);
GO

CREATE INDEX [IX_User_RoleId] ON [User] ([RoleId]);
GO

CREATE INDEX [IX_WorkRecord_DeviceId] ON [WorkRecord] ([DeviceId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250930031954_init', N'6.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [WorkRecord];
GO

CREATE TABLE [WorkOrder] (
    [Id] nvarchar(450) NOT NULL,
    [DeviceId] nvarchar(450) NULL,
    [StartTime] nvarchar(max) NULL,
    [EndTime] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_WorkOrder] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkOrder_Device_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Device] ([Id])
);
GO

CREATE INDEX [IX_WorkOrder_DeviceId] ON [WorkOrder] ([DeviceId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251015071408_Rebuildworkorder', N'6.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [WorkOrder] ADD [Code] nvarchar(max) NULL;
GO

ALTER TABLE [WorkOrder] ADD [Content] nvarchar(max) NULL;
GO

ALTER TABLE [WorkOrder] ADD [GasAlarm] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

ALTER TABLE [WorkOrder] ADD [ToxicGasAlarmOnlineStatus] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE TABLE [Bracelet] (
    [Id] nvarchar(450) NOT NULL,
    [BraceletNumber] nvarchar(max) NULL,
    [BraceletRecord] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_Bracelet] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Personnel] (
    [Id] nvarchar(450) NOT NULL,
    [EmployeeNumber] nvarchar(max) NULL,
    [FullName] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_Personnel] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [WorkBracelet] (
    [Id] nvarchar(450) NOT NULL,
    [WorkerName] nvarchar(max) NULL,
    [EntryExitStatus] nvarchar(max) NULL,
    [EmergencyCallStatus] nvarchar(max) NULL,
    [EntryTime] nvarchar(max) NULL,
    [ExitTime] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_WorkBracelet] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [WorkRecord] (
    [Id] nvarchar(450) NOT NULL,
    [WorkOrderId] nvarchar(450) NULL,
    [WorkBraceletId] nvarchar(450) NULL,
    [HeartRate] nvarchar(max) NULL,
    [EntryExitStatus] nvarchar(max) NULL,
    [EmergencyCallStatus] nvarchar(max) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_WorkRecord] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkRecord_WorkBracelet_WorkBraceletId] FOREIGN KEY ([WorkBraceletId]) REFERENCES [WorkBracelet] ([Id]),
    CONSTRAINT [FK_WorkRecord_WorkOrder_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [WorkOrder] ([Id])
);
GO

CREATE INDEX [IX_WorkRecord_WorkBraceletId] ON [WorkRecord] ([WorkBraceletId]);
GO

CREATE INDEX [IX_WorkRecord_WorkOrderId] ON [WorkRecord] ([WorkOrderId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251015075753_NewCreat', N'6.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [WorkBracelet] ADD [BraceletId] nvarchar(450) NULL;
GO

ALTER TABLE [WorkBracelet] ADD [HeartRate] nvarchar(max) NULL;
GO

ALTER TABLE [Device] ADD [EntryAreaScanner] nvarchar(max) NULL;
GO

ALTER TABLE [Device] ADD [GpsLatitude] nvarchar(max) NULL;
GO

ALTER TABLE [Device] ADD [GpsLongitude] nvarchar(max) NULL;
GO

ALTER TABLE [Device] ADD [GpsOnlineStatus] nvarchar(max) NULL;
GO

ALTER TABLE [Device] ADD [ToxicGasAlarmOnlineStatus] nvarchar(max) NULL;
GO

ALTER TABLE [Device] ADD [WorkAreaScanner] nvarchar(max) NULL;
GO

ALTER TABLE [Bracelet] ADD [DeviceNumber] nvarchar(max) NULL;
GO

CREATE INDEX [IX_WorkBracelet_BraceletId] ON [WorkBracelet] ([BraceletId]);
GO

ALTER TABLE [WorkBracelet] ADD CONSTRAINT [FK_WorkBracelet_Bracelet_BraceletId] FOREIGN KEY ([BraceletId]) REFERENCES [Bracelet] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251103015931_addnew', N'6.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Device] ADD [IP] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251103021325_addip', N'6.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [WorkBracelet] DROP CONSTRAINT [FK_WorkBracelet_Bracelet_BraceletId];
GO

ALTER TABLE [WorkRecord] DROP CONSTRAINT [FK_WorkRecord_WorkBracelet_WorkBraceletId];
GO

DROP INDEX [IX_WorkRecord_WorkBraceletId] ON [WorkRecord];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkRecord]') AND [c].[name] = N'EmergencyCallStatus');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [WorkRecord] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [WorkRecord] DROP COLUMN [EmergencyCallStatus];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkRecord]') AND [c].[name] = N'WorkBraceletId');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [WorkRecord] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [WorkRecord] DROP COLUMN [WorkBraceletId];
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkOrder]') AND [c].[name] = N'GasAlarm');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [WorkOrder] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [WorkOrder] DROP COLUMN [GasAlarm];
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WorkBracelet]') AND [c].[name] = N'EmergencyCallStatus');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [WorkBracelet] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [WorkBracelet] DROP COLUMN [EmergencyCallStatus];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Device]') AND [c].[name] = N'EntryAreaScanner');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Device] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Device] DROP COLUMN [EntryAreaScanner];
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Device]') AND [c].[name] = N'WorkAreaScanner');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Device] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Device] DROP COLUMN [WorkAreaScanner];
GO

EXEC sp_rename N'[WorkBracelet].[BraceletId]', N'WorkOrderId', N'COLUMN';
GO

EXEC sp_rename N'[WorkBracelet].[IX_WorkBracelet_BraceletId]', N'IX_WorkBracelet_WorkOrderId', N'INDEX';
GO

ALTER TABLE [WorkOrder] ADD [Status] int NOT NULL DEFAULT 0;
GO

CREATE TABLE [GasAlarmRecord] (
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
    CONSTRAINT [PK_GasAlarmRecord] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GasAlarmRecord_WorkOrder_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [WorkOrder] ([Id])
);
GO

CREATE INDEX [IX_GasAlarmRecord_WorkOrderId] ON [GasAlarmRecord] ([WorkOrderId]);
GO

ALTER TABLE [WorkBracelet] ADD CONSTRAINT [FK_WorkBracelet_WorkOrder_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [WorkOrder] ([Id]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251219042119_UpdateTablesAndAddGasAlarmRecord', N'6.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [WorkRecord] ADD [WorkerName] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251219044308_AddWorkerNameToWorkRecord', N'6.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Device] ADD [OnlineStatus] nvarchar(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251220225059_AddDeviceOnlineStatus', N'6.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [UserDevice] (
    [Id] nvarchar(450) NOT NULL,
    [UserId] nvarchar(450) NULL,
    [DeviceId] nvarchar(450) NULL,
    [Name] nvarchar(max) NULL,
    [CreateUsername] nvarchar(max) NULL,
    [CreateTime] datetime2 NOT NULL,
    [ModificationUsername] nvarchar(max) NULL,
    [ModificationTime] datetime2 NOT NULL,
    [ParentId] nvarchar(max) NULL,
    [SortCode] int NULL,
    [Type] nvarchar(max) NULL,
    [State] nvarchar(max) NULL,
    CONSTRAINT [PK_UserDevice] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserDevice_Device_DeviceId] FOREIGN KEY ([DeviceId]) REFERENCES [Device] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserDevice_User_UserId] FOREIGN KEY ([UserId]) REFERENCES [User] ([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_UserDevice_DeviceId] ON [UserDevice] ([DeviceId]);
GO

CREATE INDEX [IX_UserDevice_UserId] ON [UserDevice] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260107021711_AddUserDeviceTable', N'6.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260207052004_InitialMigration', N'6.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260208093102_Initial', N'6.0.13');
GO

COMMIT;
GO

