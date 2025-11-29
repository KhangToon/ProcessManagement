-- Migration: Add UserPermissions Table
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio
-- để tạo bảng UserPermissions

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPermissions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[UserPermissions] (
        [UserId] NVARCHAR(450) NOT NULL,
        [Permission] NVARCHAR(200) NOT NULL,
        CONSTRAINT [PK_UserPermissions] PRIMARY KEY NONCLUSTERED ([UserId], [Permission])
    );
    
    -- Tạo clustered index trên UserId để tăng hiệu suất query theo user
    CREATE CLUSTERED INDEX [IX_UserPermissions_UserId_Clustered] 
        ON [dbo].[UserPermissions]([UserId]);

    -- Tạo NONCLUSTERED INDEX trên Permission (để query theo permission)
    CREATE NONCLUSTERED INDEX [IX_UserPermissions_Permission] 
        ON [dbo].[UserPermissions]([Permission]);

    -- Foreign key đến bảng AspNetUsers
    ALTER TABLE [dbo].[UserPermissions]
        ADD CONSTRAINT [FK_UserPermissions_AspNetUsers_UserId]
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
        ON DELETE CASCADE;

    PRINT 'Bảng UserPermissions đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng UserPermissions đã tồn tại.';
END
GO

