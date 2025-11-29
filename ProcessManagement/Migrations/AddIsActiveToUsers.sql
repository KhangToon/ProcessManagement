-- Migration: Add IsActive column to AspNetUsers table
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio
-- để thêm cột IsActive vào bảng AspNetUsers

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'IsActive')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [IsActive] BIT NOT NULL DEFAULT 1;
    
    PRINT 'Đã thêm cột IsActive vào bảng AspNetUsers thành công!';
    PRINT 'Tất cả tài khoản hiện tại sẽ được đặt mặc định là hoạt động (IsActive = 1).';
END
ELSE
BEGIN
    PRINT 'Cột IsActive đã tồn tại trong bảng AspNetUsers.';
END
GO

