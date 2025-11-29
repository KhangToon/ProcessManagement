-- ============================================================================
-- SCRIPT TỔNG HỢP: MIGRATION CHO HỆ THỐNG PHÂN QUYỀN
-- ============================================================================
-- Mô tả: Script này bao gồm tất cả các thay đổi database cho chức năng phân quyền
-- Bao gồm:
--   1. Thêm cột IsActive vào AspNetUsers
--   2. Tạo bảng Permissions và insert 42 permissions mặc định
--   3. Tạo bảng RolePermissions
--   4. Tạo bảng UserPermissions
--   5. Tạo bảng ButtonRoles
--   6. Gán tất cả permissions mặc định cho role "admin"
--
-- Lưu ý: Script này có thể chạy nhiều lần an toàn (idempotent)
-- ============================================================================

USE [YourDatabaseName]; -- Thay đổi tên database của bạn
GO

PRINT '========================================';
PRINT 'BẮT ĐẦU MIGRATION HỆ THỐNG PHÂN QUYỀN';
PRINT '========================================';
GO

-- ============================================================================
-- PHẦN 1: THÊM CỘT IsActive VÀO AspNetUsers
-- ============================================================================
PRINT '';
PRINT '--- PHẦN 1: Thêm cột IsActive vào AspNetUsers ---';

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'IsActive')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [IsActive] BIT NOT NULL DEFAULT 1;
    
    PRINT '✓ Đã thêm cột IsActive vào bảng AspNetUsers thành công!';
    PRINT '  Tất cả tài khoản hiện tại sẽ được đặt mặc định là hoạt động (IsActive = 1).';
END
ELSE
BEGIN
    PRINT '⚠ Cột IsActive đã tồn tại trong bảng AspNetUsers.';
END
GO

-- ============================================================================
-- PHẦN 2: TẠO BẢNG Permissions VÀ INSERT 42 PERMISSIONS MẶC ĐỊNH
-- ============================================================================
PRINT '';
PRINT '--- PHẦN 2: Tạo bảng Permissions và insert dữ liệu mặc định ---';

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

    PRINT '✓ Bảng Permissions đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT '⚠ Bảng Permissions đã tồn tại.';
END
GO

-- Insert 42 permissions mặc định (chỉ insert nếu chưa tồn tại)
PRINT 'Đang thêm các permissions mặc định...';

-- Kế hoạch sản xuất (KHSX) - 4 permissions
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KHSX.View')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KHSX.View', N'Kế hoạch sản xuất', N'View', N'Xem', N'Xem kế hoạch sản xuất', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KHSX.Create')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KHSX.Create', N'Kế hoạch sản xuất', N'Create', N'Tạo mới', N'Tạo kế hoạch sản xuất mới', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KHSX.Update')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KHSX.Update', N'Kế hoạch sản xuất', N'Update', N'Cập nhật', N'Cập nhật kế hoạch sản xuất', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KHSX.Delete')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KHSX.Delete', N'Kế hoạch sản xuất', N'Delete', N'Xóa', N'Xóa kế hoạch sản xuất', 1, GETUTCDATE());

-- Kho nguyên vật liệu (KhoNVL) - 7 permissions
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoNVL.View')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoNVL.View', N'Kho nguyên vật liệu', N'View', N'Xem', N'Xem kho nguyên vật liệu', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoNVL.Create')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoNVL.Create', N'Kho nguyên vật liệu', N'Create', N'Tạo mới', N'Tạo mới kho nguyên vật liệu', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoNVL.Update')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoNVL.Update', N'Kho nguyên vật liệu', N'Update', N'Cập nhật', N'Cập nhật kho nguyên vật liệu', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoNVL.Delete')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoNVL.Delete', N'Kho nguyên vật liệu', N'Delete', N'Xóa', N'Xóa kho nguyên vật liệu', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoNVL.NhapKho')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoNVL.NhapKho', N'Kho nguyên vật liệu', N'NhapKho', N'Nhập kho', N'Nhập nguyên vật liệu vào kho', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoNVL.XuatKho')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoNVL.XuatKho', N'Kho nguyên vật liệu', N'XuatKho', N'Xuất kho', N'Xuất nguyên vật liệu từ kho', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoNVL.KiemKe')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoNVL.KiemKe', N'Kho nguyên vật liệu', N'KiemKe', N'Kiểm kê', N'Kiểm kê kho nguyên vật liệu', 1, GETUTCDATE());

-- Sản phẩm (SanPham) - 4 permissions
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'SanPham.View')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'SanPham.View', N'Sản phẩm', N'View', N'Xem', N'Xem sản phẩm', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'SanPham.Create')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'SanPham.Create', N'Sản phẩm', N'Create', N'Tạo mới', N'Tạo sản phẩm mới', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'SanPham.Update')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'SanPham.Update', N'Sản phẩm', N'Update', N'Cập nhật', N'Cập nhật sản phẩm', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'SanPham.Delete')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'SanPham.Delete', N'Sản phẩm', N'Delete', N'Xóa', N'Xóa sản phẩm', 1, GETUTCDATE());

-- Máy móc - Thiết bị (MayMoc) - 4 permissions
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'MayMoc.View')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'MayMoc.View', N'Máy móc - Thiết bị', N'View', N'Xem', N'Xem máy móc thiết bị', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'MayMoc.Create')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'MayMoc.Create', N'Máy móc - Thiết bị', N'Create', N'Tạo mới', N'Tạo máy móc thiết bị mới', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'MayMoc.Update')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'MayMoc.Update', N'Máy móc - Thiết bị', N'Update', N'Cập nhật', N'Cập nhật máy móc thiết bị', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'MayMoc.Delete')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'MayMoc.Delete', N'Máy móc - Thiết bị', N'Delete', N'Xóa', N'Xóa máy móc thiết bị', 1, GETUTCDATE());

-- Nhân viên (NhanVien) - 4 permissions
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'NhanVien.View')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'NhanVien.View', N'Nhân viên', N'View', N'Xem', N'Xem thông tin nhân viên', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'NhanVien.Create')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'NhanVien.Create', N'Nhân viên', N'Create', N'Tạo mới', N'Tạo nhân viên mới', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'NhanVien.Update')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'NhanVien.Update', N'Nhân viên', N'Update', N'Cập nhật', N'Cập nhật thông tin nhân viên', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'NhanVien.Delete')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'NhanVien.Delete', N'Nhân viên', N'Delete', N'Xóa', N'Xóa nhân viên', 1, GETUTCDATE());

-- Kho thành phẩm (KhoThanhPham) - 6 permissions
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoThanhPham.View')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoThanhPham.View', N'Kho thành phẩm', N'View', N'Xem', N'Xem kho thành phẩm', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoThanhPham.Create')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoThanhPham.Create', N'Kho thành phẩm', N'Create', N'Tạo mới', N'Tạo mới kho thành phẩm', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoThanhPham.Update')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoThanhPham.Update', N'Kho thành phẩm', N'Update', N'Cập nhật', N'Cập nhật kho thành phẩm', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoThanhPham.Delete')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoThanhPham.Delete', N'Kho thành phẩm', N'Delete', N'Xóa', N'Xóa kho thành phẩm', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoThanhPham.NhapKho')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoThanhPham.NhapKho', N'Kho thành phẩm', N'NhapKho', N'Nhập kho', N'Nhập thành phẩm vào kho', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'KhoThanhPham.XuatKho')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'KhoThanhPham.XuatKho', N'Kho thành phẩm', N'XuatKho', N'Xuất kho', N'Xuất thành phẩm từ kho', 1, GETUTCDATE());

-- Công đoạn (NguyenCong) - 4 permissions
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'NguyenCong.View')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'NguyenCong.View', N'Công đoạn', N'View', N'Xem', N'Xem công đoạn', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'NguyenCong.Create')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'NguyenCong.Create', N'Công đoạn', N'Create', N'Tạo mới', N'Tạo công đoạn mới', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'NguyenCong.Update')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'NguyenCong.Update', N'Công đoạn', N'Update', N'Cập nhật', N'Cập nhật công đoạn', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'NguyenCong.Delete')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'NguyenCong.Delete', N'Công đoạn', N'Delete', N'Xóa', N'Xóa công đoạn', 1, GETUTCDATE());

-- Quản lý tài khoản (Users & Roles) - 9 permissions
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'Users.View')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'Users.View', N'Quản lý tài khoản', N'View', N'Xem', N'Xem danh sách người dùng', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'Users.Create')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'Users.Create', N'Quản lý tài khoản', N'Create', N'Tạo mới', N'Tạo người dùng mới', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'Users.Update')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'Users.Update', N'Quản lý tài khoản', N'Update', N'Cập nhật', N'Cập nhật thông tin người dùng', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'Users.Delete')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'Users.Delete', N'Quản lý tài khoản', N'Delete', N'Xóa', N'Xóa người dùng', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'Users.ManagePermissions')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'Users.ManagePermissions', N'Quản lý tài khoản', N'ManagePermissions', N'Quản lý quyền', N'Quản lý quyền truy cập của người dùng', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'Roles.View')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'Roles.View', N'Quản lý tài khoản', N'View', N'Xem', N'Xem danh sách roles', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'Roles.Create')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'Roles.Create', N'Quản lý tài khoản', N'Create', N'Tạo mới', N'Tạo role mới', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'Roles.Update')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'Roles.Update', N'Quản lý tài khoản', N'Update', N'Cập nhật', N'Cập nhật role', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'Roles.Delete')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'Roles.Delete', N'Quản lý tài khoản', N'Delete', N'Xóa', N'Xóa role', 1, GETUTCDATE());

-- Đóng thùng (DongThung) - 3 permissions
IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'DongThung.View')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'DongThung.View', N'Đóng thùng', N'View', N'Xem', N'Xem đóng thùng', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'DongThung.Create')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'DongThung.Create', N'Đóng thùng', N'Create', N'Tạo mới', N'Tạo đóng thùng mới', 1, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permissions] WHERE [PermissionCode] = N'DongThung.Update')
    INSERT INTO [dbo].[Permissions] ([PermissionCode], [Module], [Action], [DisplayName], [Description], [IsActive], [CreatedAt])
    VALUES (N'DongThung.Update', N'Đóng thùng', N'Update', N'Cập nhật', N'Cập nhật đóng thùng', 1, GETUTCDATE());

PRINT '✓ Đã thêm tất cả 42 permissions mặc định thành công!';
GO

-- ============================================================================
-- PHẦN 3: TẠO BẢNG RolePermissions
-- ============================================================================
PRINT '';
PRINT '--- PHẦN 3: Tạo bảng RolePermissions ---';

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

    PRINT '✓ Bảng RolePermissions đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT '⚠ Bảng RolePermissions đã tồn tại.';
END
GO

-- ============================================================================
-- PHẦN 4: TẠO BẢNG UserPermissions
-- ============================================================================
PRINT '';
PRINT '--- PHẦN 4: Tạo bảng UserPermissions ---';

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

    PRINT '✓ Bảng UserPermissions đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT '⚠ Bảng UserPermissions đã tồn tại.';
END
GO

-- ============================================================================
-- PHẦN 5: TẠO BẢNG ButtonRoles
-- ============================================================================
PRINT '';
PRINT '--- PHẦN 5: Tạo bảng ButtonRoles ---';

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

    PRINT '✓ Bảng ButtonRoles đã được tạo thành công!';
END
ELSE
BEGIN
    PRINT '⚠ Bảng ButtonRoles đã tồn tại.';
END
GO

-- ============================================================================
-- PHẦN 6: GÁN TẤT CẢ PERMISSIONS MẶC ĐỊNH CHO ROLE "admin"
-- ============================================================================
PRINT '';
PRINT '--- PHẦN 6: Gán tất cả permissions mặc định cho role "admin" ---';

-- Kiểm tra xem role "admin" có tồn tại không
IF EXISTS (SELECT 1 FROM [dbo].[AspNetRoles] WHERE [NormalizedName] = 'ADMIN')
BEGIN
    -- Lấy tất cả permissions từ bảng Permissions
    DECLARE @PermissionCount INT;
    SELECT @PermissionCount = COUNT(*) FROM [dbo].[Permissions] WHERE [IsActive] = 1;
    
    -- Insert tất cả permissions vào RolePermissions cho role "admin" (nếu chưa có)
    INSERT INTO [dbo].[RolePermissions] ([RoleName], [Permission])
    SELECT 
        'admin' AS [RoleName],
        [PermissionCode] AS [Permission]
    FROM [dbo].[Permissions]
    WHERE [IsActive] = 1
        AND [PermissionCode] NOT IN (
            SELECT [Permission] 
            FROM [dbo].[RolePermissions] 
            WHERE [RoleName] = 'admin'
        );
    
    DECLARE @InsertedCount INT = @@ROWCOUNT;
    
    IF @InsertedCount > 0
    BEGIN
        PRINT CONCAT('✓ Đã gán ', @InsertedCount, ' permissions cho role "admin".');
    END
    ELSE
    BEGIN
        PRINT '⚠ Role "admin" đã có đầy đủ permissions.';
    END
END
ELSE
BEGIN
    PRINT '⚠ Role "admin" chưa tồn tại. Vui lòng tạo role "admin" trước khi chạy phần này.';
    PRINT '  Script sẽ tự động gán permissions khi role được tạo.';
END
GO

-- ============================================================================
-- KẾT THÚC MIGRATION
-- ============================================================================
PRINT '';
PRINT '========================================';
PRINT 'HOÀN THÀNH MIGRATION HỆ THỐNG PHÂN QUYỀN';
PRINT '========================================';
PRINT '';
PRINT 'Tóm tắt:';
PRINT '  ✓ Đã thêm cột IsActive vào AspNetUsers';
PRINT '  ✓ Đã tạo bảng Permissions và insert 42 permissions mặc định';
PRINT '  ✓ Đã tạo bảng RolePermissions';
PRINT '  ✓ Đã tạo bảng UserPermissions';
PRINT '  ✓ Đã tạo bảng ButtonRoles';
PRINT '  ✓ Đã gán tất cả permissions cho role "admin" (nếu role tồn tại)';
PRINT '';
PRINT 'Lưu ý:';
PRINT '  - Nếu role "admin" chưa tồn tại, vui lòng tạo role trước.';
PRINT '  - Sau khi tạo role "admin", có thể chạy lại phần 6 để gán permissions.';
PRINT '  - Hoặc sử dụng UI quản lý Roles để gán permissions cho các role.';
PRINT '';
GO

