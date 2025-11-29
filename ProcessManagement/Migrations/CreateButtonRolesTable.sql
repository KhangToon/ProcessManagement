-- Migration: Add ButtonRoles Table
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio
-- để tạo bảng ButtonRoles

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ButtonRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ButtonRoles] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ButtonId] NVARCHAR(200) NOT NULL,
        [PagePath] NVARCHAR(500) NOT NULL,
        [ButtonText] NVARCHAR(200) NULL,
        [RoleName] NVARCHAR(256) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_ButtonRoles] PRIMARY KEY CLUSTERED ([Id])
    );
    
    -- Tạo index trên ButtonId để tăng hiệu suất query
    CREATE NONCLUSTERED INDEX [IX_ButtonRoles_ButtonId] 
        ON [dbo].[ButtonRoles]([ButtonId]);

    -- Tạo index trên PagePath để query theo trang
    CREATE NONCLUSTERED INDEX [IX_ButtonRoles_PagePath] 
        ON [dbo].[ButtonRoles]([PagePath]);

    -- Tạo composite index trên ButtonId và RoleName
    CREATE NONCLUSTERED INDEX [IX_ButtonRoles_ButtonId_RoleName] 
        ON [dbo].[ButtonRoles]([ButtonId], [RoleName]);

    PRINT N'Bảng ButtonRoles đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT N'Bảng ButtonRoles đã tồn tại.';
END
GO


