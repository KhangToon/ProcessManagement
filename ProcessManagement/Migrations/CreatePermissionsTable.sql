-- Migration: Add Permissions Table
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio
-- để tạo bảng Permissions

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Permissions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Permissions] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [PermissionCode] NVARCHAR(200) NOT NULL,
        [Module] NVARCHAR(100) NOT NULL,
        [Action] NVARCHAR(50) NOT NULL,
        [DisplayName] NVARCHAR(200) NULL,
        [Description] NVARCHAR(MAX) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY CLUSTERED ([Id])
    );
    
    -- Tạo unique index trên PermissionCode
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Permissions_PermissionCode] 
        ON [dbo].[Permissions]([PermissionCode]);

    -- Tạo index trên Module để tăng hiệu suất query theo module
    CREATE NONCLUSTERED INDEX [IX_Permissions_Module] 
        ON [dbo].[Permissions]([Module]);

    PRINT 'Bảng Permissions đã được tạo thành công!';
    
    -- Thêm các permissions mặc định
    PRINT 'Đang thêm các permissions mặc định...';
    
    -- Kế hoạch sản xuất (KHSX)
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES 
        (N'KHSX.View', N'Kế hoạch sản xuất', N'View', N'Xem', N'Xem kế hoạch sản xuất', 1, GETUTCDATE()),
        (N'KHSX.Create', N'Kế hoạch sản xuất', N'Create', N'Tạo mới', N'Tạo kế hoạch sản xuất mới', 1, GETUTCDATE()),
        (N'KHSX.Update', N'Kế hoạch sản xuất', N'Update', N'Cập nhật', N'Cập nhật kế hoạch sản xuất', 1, GETUTCDATE()),
        (N'KHSX.Delete', N'Kế hoạch sản xuất', N'Delete', N'Xóa', N'Xóa kế hoạch sản xuất', 1, GETUTCDATE());
    
    -- Kho nguyên vật liệu (KhoNVL)
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES 
        (N'KhoNVL.View', N'Kho nguyên vật liệu', N'View', N'Xem', N'Xem kho nguyên vật liệu', 1, GETUTCDATE()),
        (N'KhoNVL.Create', N'Kho nguyên vật liệu', N'Create', N'Tạo mới', N'Tạo mới kho nguyên vật liệu', 1, GETUTCDATE()),
        (N'KhoNVL.Update', N'Kho nguyên vật liệu', N'Update', N'Cập nhật', N'Cập nhật kho nguyên vật liệu', 1, GETUTCDATE()),
        (N'KhoNVL.Delete', N'Kho nguyên vật liệu', N'Delete', N'Xóa', N'Xóa kho nguyên vật liệu', 1, GETUTCDATE()),
        (N'KhoNVL.NhapKho', N'Kho nguyên vật liệu', N'NhapKho', N'Nhập kho', N'Nhập nguyên vật liệu vào kho', 1, GETUTCDATE()),
        (N'KhoNVL.XuatKho', N'Kho nguyên vật liệu', N'XuatKho', N'Xuất kho', N'Xuất nguyên vật liệu từ kho', 1, GETUTCDATE()),
        (N'KhoNVL.KiemKe', N'Kho nguyên vật liệu', N'KiemKe', N'Kiểm kê', N'Kiểm kê kho nguyên vật liệu', 1, GETUTCDATE());
    
    -- Sản phẩm (SanPham)
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES 
        (N'SanPham.View', N'Sản phẩm', N'View', N'Xem', N'Xem sản phẩm', 1, GETUTCDATE()),
        (N'SanPham.Create', N'Sản phẩm', N'Create', N'Tạo mới', N'Tạo sản phẩm mới', 1, GETUTCDATE()),
        (N'SanPham.Update', N'Sản phẩm', N'Update', N'Cập nhật', N'Cập nhật sản phẩm', 1, GETUTCDATE()),
        (N'SanPham.Delete', N'Sản phẩm', N'Delete', N'Xóa', N'Xóa sản phẩm', 1, GETUTCDATE());
    
    -- Máy móc - Thiết bị (MayMoc)
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES 
        (N'MayMoc.View', N'Máy móc - Thiết bị', N'View', N'Xem', N'Xem máy móc thiết bị', 1, GETUTCDATE()),
        (N'MayMoc.Create', N'Máy móc - Thiết bị', N'Create', N'Tạo mới', N'Tạo máy móc thiết bị mới', 1, GETUTCDATE()),
        (N'MayMoc.Update', N'Máy móc - Thiết bị', N'Update', N'Cập nhật', N'Cập nhật máy móc thiết bị', 1, GETUTCDATE()),
        (N'MayMoc.Delete', N'Máy móc - Thiết bị', N'Delete', N'Xóa', N'Xóa máy móc thiết bị', 1, GETUTCDATE());
    
    -- Nhân viên (NhanVien)
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES 
        (N'NhanVien.View', N'Nhân viên', N'View', N'Xem', N'Xem thông tin nhân viên', 1, GETUTCDATE()),
        (N'NhanVien.Create', N'Nhân viên', N'Create', N'Tạo mới', N'Tạo nhân viên mới', 1, GETUTCDATE()),
        (N'NhanVien.Update', N'Nhân viên', N'Update', N'Cập nhật', N'Cập nhật thông tin nhân viên', 1, GETUTCDATE()),
        (N'NhanVien.Delete', N'Nhân viên', N'Delete', N'Xóa', N'Xóa nhân viên', 1, GETUTCDATE());
    
    -- Kho thành phẩm (KhoThanhPham)
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES 
        (N'KhoThanhPham.View', N'Kho thành phẩm', N'View', N'Xem', N'Xem kho thành phẩm', 1, GETUTCDATE()),
        (N'KhoThanhPham.Create', N'Kho thành phẩm', N'Create', N'Tạo mới', N'Tạo mới kho thành phẩm', 1, GETUTCDATE()),
        (N'KhoThanhPham.Update', N'Kho thành phẩm', N'Update', N'Cập nhật', N'Cập nhật kho thành phẩm', 1, GETUTCDATE()),
        (N'KhoThanhPham.Delete', N'Kho thành phẩm', N'Delete', N'Xóa', N'Xóa kho thành phẩm', 1, GETUTCDATE()),
        (N'KhoThanhPham.NhapKho', N'Kho thành phẩm', N'NhapKho', N'Nhập kho', N'Nhập thành phẩm vào kho', 1, GETUTCDATE()),
        (N'KhoThanhPham.XuatKho', N'Kho thành phẩm', N'XuatKho', N'Xuất kho', N'Xuất thành phẩm từ kho', 1, GETUTCDATE());
    
    -- Công đoạn (NguyenCong)
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES 
        (N'NguyenCong.View', N'Công đoạn', N'View', N'Xem', N'Xem công đoạn', 1, GETUTCDATE()),
        (N'NguyenCong.Create', N'Công đoạn', N'Create', N'Tạo mới', N'Tạo công đoạn mới', 1, GETUTCDATE()),
        (N'NguyenCong.Update', N'Công đoạn', N'Update', N'Cập nhật', N'Cập nhật công đoạn', 1, GETUTCDATE()),
        (N'NguyenCong.Delete', N'Công đoạn', N'Delete', N'Xóa', N'Xóa công đoạn', 1, GETUTCDATE());
    
    -- Quản lý tài khoản (Users & Roles)
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES 
        (N'Users.View', N'Quản lý tài khoản', N'View', N'Xem', N'Xem danh sách người dùng', 1, GETUTCDATE()),
        (N'Users.Create', N'Quản lý tài khoản', N'Create', N'Tạo mới', N'Tạo người dùng mới', 1, GETUTCDATE()),
        (N'Users.Update', N'Quản lý tài khoản', N'Update', N'Cập nhật', N'Cập nhật thông tin người dùng', 1, GETUTCDATE()),
        (N'Users.Delete', N'Quản lý tài khoản', N'Delete', N'Xóa', N'Xóa người dùng', 1, GETUTCDATE()),
        (N'Users.ManagePermissions', N'Quản lý tài khoản', N'ManagePermissions', N'Quản lý quyền', N'Quản lý quyền truy cập của người dùng', 1, GETUTCDATE()),
        (N'Roles.View', N'Quản lý tài khoản', N'View', N'Xem', N'Xem danh sách roles', 1, GETUTCDATE()),
        (N'Roles.Create', N'Quản lý tài khoản', N'Create', N'Tạo mới', N'Tạo role mới', 1, GETUTCDATE()),
        (N'Roles.Update', N'Quản lý tài khoản', N'Update', N'Cập nhật', N'Cập nhật role', 1, GETUTCDATE()),
        (N'Roles.Delete', N'Quản lý tài khoản', N'Delete', N'Xóa', N'Xóa role', 1, GETUTCDATE());
    
    -- Đóng thùng (DongThung)
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES 
        (N'DongThung.View', N'Đóng thùng', N'View', N'Xem', N'Xem đóng thùng', 1, GETUTCDATE()),
        (N'DongThung.Create', N'Đóng thùng', N'Create', N'Tạo mới', N'Tạo đóng thùng mới', 1, GETUTCDATE()),
        (N'DongThung.Update', N'Đóng thùng', N'Update', N'Cập nhật', N'Cập nhật đóng thùng', 1, GETUTCDATE());
    
    PRINT 'Đã thêm tất cả permissions mặc định thành công!';
END
ELSE
BEGIN
    PRINT 'Bảng Permissions đã tồn tại.';
    PRINT 'Để thêm permissions mặc định, vui lòng chạy script seed riêng.';
END
GO

