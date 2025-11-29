-- Migration: Add RolePermissions Table
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio
-- để tạo bảng RolePermissions

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RolePermissions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RolePermissions] (
        [RoleName] NVARCHAR(256) NOT NULL,
        [Permission] NVARCHAR(200) NOT NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY NONCLUSTERED ([RoleName], [Permission])
    );
    
    -- Tạo clustered index trên RoleName để tăng hiệu suất query theo role
    CREATE CLUSTERED INDEX [IX_RolePermissions_RoleName_Clustered] 
        ON [dbo].[RolePermissions]([RoleName]);

    -- Tạo NONCLUSTERED INDEX trên Permission (để query theo permission)
    CREATE NONCLUSTERED INDEX [IX_RolePermissions_Permission] 
        ON [dbo].[RolePermissions]([Permission]);

    PRINT 'Bảng RolePermissions đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng RolePermissions đã tồn tại.';
END
GO


