# 📊 Tình Trạng Phân Quyền Hiện Tại (CẬP NHẬT)

File này dùng để tóm tắt nhanh **toàn bộ các chức năng phân quyền** đã/đang triển khai trong dự án, để Chat/Dev đọc lại là hiểu ngay bức tranh tổng thể.

---

## ✅ ĐÃ HOÀN THÀNH

### 1. **Hạ Tầng Permission & Role (Infrastructure)**
- ✅ **Permissions.cs** - Định nghĩa đầy đủ 9 modules với 42 permissions:
  - Kế hoạch sản xuất (KHSX): View, Create, Update, Delete  
  - Kho nguyên vật liệu (KhoNVL): View, Create, Update, Delete, NhapKho, XuatKho, KiemKe  
  - Sản phẩm (SanPham): View, Create, Update, Delete  
  - Máy móc - Thiết bị (MayMoc): View, Create, Update, Delete  
  - Nhân viên (NhanVien): View, Create, Update, Delete  
  - Kho thành phẩm (KhoThanhPham): View, Create, Update, Delete, NhapKho, XuatKho  
  - Công đoạn (NguyenCong): View, Create, Update, Delete  
  - Quản lý tài khoản (Users & Roles): View, Create, Update, Delete, ManagePermissions  
  - Đóng thùng (DongThung): View, Create, Update  
- ✅ **Models & Tables**:
  - `Permission`, `RolePermission`, `UserPermission`
  - `ButtonRole` – lưu mapping giữa `ButtonId` và `RoleName`
  - `AppUser` thêm thuộc tính `IsActive`
- ✅ **AppDbContext**:
  - Khai báo `DbSet` cho `Permissions`, `RolePermissions`, `UserPermissions`, `ButtonRoles`
  - Cấu hình quan hệ many-to-many

### 2. **PermissionService & Extensions**
- ✅ `PermissionService`:
  - `HasPermissionAsync()`, `HasAnyPermissionAsync()`, `HasAllPermissionsAsync()`
  - `GetUserPermissionsAsync()`
  - `AssignRolePermissionsAsync(roleName, permissions)` – set default permissions cho Role
  - Admin mặc định có tất cả permissions
- ✅ `PermissionExtensions`:
  - `HasPermissionAsync(AuthenticationState, permission)`
  - `HasPermissionAsync(ClaimsPrincipal, permission)`
  - `HasAnyPermissionAsync(ClaimsPrincipal, params string[] permissions)`

### 3. **ButtonRoleService – Phân Quyền Theo ButtonId**
- ✅ Các hàm chính:
  - `GetRolesForButtonAsync(buttonId)` – lấy danh sách roles của 1 button
  - `AssignRolesToButtonAsync(buttonId, pagePath, buttonText, roleNames)`
  - `GetButtonRolesByPageAsync(pagePath)` – lấy tất cả button roles của trang (có cache 30s)
  - `BulkLoadButtonRolesAsync(pagePath, buttonIds)` – **MỚI**: Bulk load cho danh sách ButtonIds cụ thể, chỉ load khi Admin mode enabled để tối ưu performance
  - `RemoveButtonRolesAsync(buttonId)`
  - `IsRoleUsedByButtonsAsync(roleName)` + `GetButtonsUsingRoleAsync(roleName)`
  - `RemoveButtonRolesByRoleNameAsync(roleName)` – dọn dẹp khi xóa role
  - `UpdateRoleNameInButtonRolesAsync(oldRoleName, newRoleName)` – cập nhật khi đổi tên role

### 4. **API User & Role (enforce phân quyền cơ bản)**
- ✅ **UserController**:
  - Tạo user: gán 1 Role (theo `RoleName` gửi lên), tự động gán default permissions từ `RolePermissions`
  - Cập nhật user: thông tin + `IsActive`
  - Bật/tắt tài khoản (`ToggleUserActive`), chặn xóa / disable `admin@admin`
- ✅ **RoleController**:
  - Tạo/Sửa/Xóa Role
  - `AssignRole` – gán 1 role cho user
  - Khi xóa/đổi tên role → cập nhật/dọn dẹp các bản ghi `ButtonRoles` tương ứng

### 5. **UI Quản Lý Users / Roles / Permissions**
- ✅ **Account_Management.razor**:
  - Tab **Quản lý Users**
  - Tab **Quản lý Roles**
  - Tab **Quản lý quyền truy cập (Permissions)**
  - Nút **"Xem hướng dẫn"** mở `AccountManagementGuide.razor`
- ✅ **UserManagement.razor**:
  - Danh sách users + cột trạng thái `IsActive` (badge)
  - Nút Edit mở `EditUserDialog`
  - Chặn xóa `admin@admin`
- ✅ **CreateUserDialog.razor**:
  - Tạo user mới kèm chọn Role
  - Nút **"Xem quyền"** để xem permissions của Role trước khi gán
- ✅ **EditUserDialog.razor**:
  - Chỉnh sửa thông tin user + đổi mật khẩu
  - Bật/tắt tài khoản qua `IsActive` (không áp dụng cho `admin@admin`)
  - Chọn/sửa Role cho user (mỗi user chỉ 1 role)
- ✅ **RoleManagement.razor + dialogs**:
  - Tạo/Sửa/Xóa Role
  - Gán 1 Role cho user
  - `RolePermissionsDialog` – xem/sửa default permissions của Role
  - `CreateRoleDialog` – tạo role mới + chọn sẵn permissions
- ✅ **PermissionManagement.razor + AddEditPermissionDialog.razor**:
  - Quản lý toàn bộ `Permissions` động (thêm module, thêm/sửa/xóa permission)
  - Validate không trùng `Action` trong cùng module

### 6. **UI Phân Quyền – Theo Permission & Theo ButtonId**
- ✅ **AuthorizedAction.razor**:
  - Tham số: `RequiredPermission`, `RequiredRole`, `ButtonId`
  - Logic:
    - Nếu có `ButtonId` → lấy roles từ `ButtonRoles`:
      - Dùng `GetButtonRolesByPageAsync(pagePath)` để **load 1 lần toàn bộ mapping ButtonId → Roles cho cả trang**, kết hợp với **cache 30s** trong `ButtonRoleService`:
        - Cache dạng `ConcurrentDictionary<string, (LoadedAt, Dictionary<ButtonId, List<RoleName>>)>`, key là `pagePath`.
        - Trong vòng 30s, mọi `AuthorizedAction` trên cùng trang chỉ đọc lại từ cache, **không query DB nữa**.
        - Khi có thay đổi (gán role mới cho button, xóa role, đổi tên role, cleanup orphaned, v.v.) → service tự **invalidates cache** tương ứng để lần sau load lại dữ liệu mới nhất.
      - Với mỗi button (kể cả lặp lại trong DataGrid), chỉ lookup trong dictionary theo `ButtonId` → rất phù hợp case **nhiều nút giống nhau nhưng cùng phân quyền**.
      - Nếu đã gán roles → user có ít nhất 1 role đó (hoặc Admin) mới thấy
      - Nếu chưa gán roles → chỉ **Admin** thấy (để set role)
    - Nếu có `RequiredRole` → user có role đó hoặc Admin
    - Nếu có `RequiredPermission` → check qua `PermissionService` (Admin luôn true)
  - Đã update `OnParametersSetAsync` để re-check khi parameters thay đổi
- ✅ **AuthorizedButton.razor**:
  - Bọc quanh `RadzenButton` (hoặc bất kỳ nội dung nào)
  - Nhận `ButtonId`, `ButtonText`, `RequiredPermission`, `RequiredRole`
  - Với Admin:
    - Hiển thị badge 🔧
    - Cho phép **right-click** hoặc click badge để mở `SetButtonRoleDialog`
  - Bên trong dùng `AuthorizedAction` → kế thừa đầy đủ logic phân quyền
- ✅ **SetButtonRoleDialog.razor**:
  - Hiển thị `ButtonId`, `PagePath`, `ButtonText`
  - Dropdown multi-select roles (có search, filter, clear)
  - Role "Admin" luôn được gán mặc định và không thể bỏ chọn
  - Lưu roles vào bảng `ButtonRoles` thông qua `ButtonRoleService`
- ✅ **ButtonRoleGuideDialog.razor**:
  - Dialog hướng dẫn chi tiết cách sử dụng chức năng gán role cho button
  - Bao gồm: tổng quan, các bước thực hiện, lưu ý quan trọng, ví dụ cụ thể
  - Có thể mở từ link "Xem hướng dẫn" trong dropdown Admin menu
- ✅ **AuthorizedPage.razor**:
  - Bọc toàn bộ nội dung page, nhận `RequiredPermission` hoặc `RequiredRole`
  - Nếu không đủ quyền → hiển thị thông báo Access Denied
  - Admin luôn được vào
- ✅ **AdminPermissionModeService** + **Admin adjust mode**:
  - Service singleton lưu trạng thái `IsEnabled` (bật/tắt chế độ "điều chỉnh phân quyền").
  - `UserInfoDisplay.razor`:
    - Khi user là **Admin**, click vào khối thông tin account (ô "👤 Admin 001") sẽ mở dropdown "Chức năng Admin".
    - Trong dropdown có:
      - **Switch "Bật chế độ điều chỉnh phân quyền"** (RadzenSwitch):
        - Khi switch bật: `AdminPermissionModeService.SetEnabled(true)` → toàn bộ `AuthorizedButton` vào **chế độ chỉnh quyền** (hiện icon 🔧, cho right-click để gán role).
        - Khi switch tắt: `IsEnabled = false` → icon 🔧 và context-menu chỉnh quyền ẩn đi, UI chỉ còn behavior phân quyền bình thường.
      - **Link "Xem hướng dẫn"** (có gạch chân, màu xanh):
        - Click vào sẽ mở `ButtonRoleGuideDialog` với hướng dẫn chi tiết cách sử dụng chức năng gán role cho button.
  - `AuthorizedButton.razor`:
    - Chỉ hiển thị icon 🔧 / cho phép gán role khi:
      - User là Admin, **và**
      - `AdminPermissionModeService.IsEnabled == true`.
    - Subscribes `AdminPermissionModeService.OnModeChanged` để tự `StateHasChanged` khi admin bật/tắt chế độ.

### 7. **Áp Dụng Thực Tế – Trang Sản Phẩm**
- ✅ **Sanpham_Management.razor**:
  - Trang bọc bởi:
    ```razor
    <AuthorizedPage RequiredPermission="@Permissions.SanPham_View">
    ```
  - Các nút chính dùng `AuthorizedButton` với `ButtonId` thống nhất:
    - `SanPham_Create_Button` – Thêm sản phẩm mới
    - `SanPham_AddNVL_Button` – Thêm/Sửa nguyên liệu (1 ButtonId cho mọi dòng DataGrid)
    - `SanPham_RenameThongTin_Button` – Đổi tên trường thông tin (trong foreach)
    - `SanPham_DeleteThongTin_Button` – Xóa trường thông tin
    - `SanPham_AddThongTinKhac_Button` – Thông tin khác
    - `SanPham_Update_Button` – Lưu thay đổi / Chỉnh sửa
    - `SanPham_Delete_Button` – Xóa sản phẩm
  - **Tối ưu Performance**:
    - Subscribe `AdminPermissionModeService.OnModeChanged` để preload ButtonRoles khi Admin bật chế độ điều chỉnh phân quyền
    - Sử dụng `BulkLoadButtonRolesAsync` để load tất cả ButtonRoles của trang trong 1 query thay vì load từng button
    - Chỉ preload khi Admin mode enabled → giảm số lần query DB cho user thường
- ✅ Khi admin gán role cho 1 `ButtonId`, **tất cả các button cùng `ButtonId`** trên trang sẽ hiển thị/ẩn đồng bộ theo role.

### 8. **Hiển Thị Thông Tin Account Trên Header**
- ✅ **UserInfoDisplay.razor** (nhúng trong `LoginDisplay.razor`):
  - Lấy thông tin user hiện tại (FirstName, LastName, UserName)
  - Hiển thị tên + icon + danh sách Roles dạng badge
  - Nút Logout

---

## 🔄 ĐANG / CẦN TRIỂN KHAI TIẾP

### 1. **Áp Dụng Mẫu Mới Cho Các Trang Khác**
- ⏳ Chưa áp dụng `AuthorizedPage` + `AuthorizedButton` cho:
  - KhoNVL
  - MayMoc
  - NhanVien
  - NguyenCong
  - KhoThanhPham
  - DongThung
  - KHSX (hiện tại dùng pattern cũ)

### 2. **API-Level Permission Check Chi Tiết**
- ⏳ Mới enforce cơ bản ở `UserController` / `RoleController` theo role.
- ⏳ Chưa thêm `[Authorize(Policy = ...)]` cho từng API nghiệp vụ (KHSX, KhoNVL, …).

### 3. **Authorization Policies Trong Program.cs**
- ⏳ Chưa khai báo policy theo từng permission (ví dụ: `builder.Services.AddAuthorization(options => options.AddPolicy(Permissions.KHSX_Create, ...))`).

### 4. **Refactor Các Trang Cũ Dùng Pattern Permission Cũ**
- ⏳ KHSX & một số trang vẫn đang dùng pattern hard-code hoặc role-based đơn giản → cần migrate sang:
  - `AuthorizedPage` cho quyền vào trang
  - `AuthorizedButton` + `ButtonId` cho quyền theo nút

---

## 📋 TÓM TẮT NHANH

- ✅ **Đã có:**
  - Hạ tầng Permission/Role đầy đủ (models, tables, services)
  - UI quản lý User/Role/Permission
  - Cơ chế phân quyền theo:
    - Permission (page + nút)
    - Role
    - ButtonId ↔ Role (ButtonRoles)
  - Đã áp dụng đầy đủ cho **trang Sản phẩm**
  - Header hiển thị thông tin tài khoản + roles
- ⏳ **Đang thiếu / cần làm tiếp:**
  - Áp dụng mẫu mới cho các module còn lại (KhoNVL, MayMoc, NhanVien, NguyenCong, …)
  - Thêm policy-based authorization cho API
  - Refactor các trang cũ còn dùng pattern phân quyền cũ

---

## 📊 THỐNG KÊ (ƯỚC LƯỢNG)

- **Permissions đã định nghĩa:** 42  
- **Modules:** 9  
- **UI Components phân quyền:** `AuthorizedPage`, `AuthorizedAction`, `AuthorizedButton`, `SetButtonRoleDialog`, `ButtonRoleGuideDialog`, `UserPermissionsDialog`, `RolePermissionsDialog`, `PermissionManagement`  
- **Trang đã áp dụng phân quyền đầy đủ:** 1 (SanPham – làm mẫu)  
- **Controllers đã tích hợp vào hệ thống phân quyền mới:** UserController, RoleController  
- **Tiến độ tổng quan:** ~60–70% (hạ tầng + 1 module mẫu + quản lý account; còn thiếu rollout cho các module nghiệp vụ khác)

