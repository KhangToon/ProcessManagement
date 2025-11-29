using ProcessManagement.Models.Authorization;

namespace ProcessManagement.Models.Authorization
{
    /// <summary>
    /// Định nghĩa permissions mặc định cho user mới tạo
    /// </summary>
    public static class DefaultPermissions
    {
        /// <summary>
        /// Permissions mặc định cho user thường (không phải Admin)
        /// Có thể xem các module cơ bản nhưng không được tạo/sửa/xóa
        /// </summary>
        public static List<string> GetDefaultUserPermissions()
        {
            return new List<string>
            {
                // Chỉ xem các module cơ bản
                Permissions.KHSX_View,
                Permissions.KhoNVL_View,
                Permissions.SanPham_View,
                Permissions.MayMoc_View,
                Permissions.NhanVien_View,
                Permissions.KhoThanhPham_View,
                Permissions.NguyenCong_View,
                
                // Cho phép nhập/xuất kho (thao tác cơ bản)
                Permissions.KhoNVL_NhapKho,
                Permissions.KhoNVL_XuatKho,
                Permissions.KhoThanhPham_NhapKho,
                Permissions.KhoThanhPham_XuatKho,
                
                // Đóng thùng
                Permissions.DongThung_View,
                Permissions.DongThung_Create
            };
        }

        /// <summary>
        /// Permissions mặc định cho user role "User"
        /// </summary>
        public static List<string> GetUserRolePermissions()
        {
            return GetDefaultUserPermissions();
        }

        /// <summary>
        /// Permissions mặc định cho user role "User-dongthung"
        /// </summary>
        public static List<string> GetUserDongThungPermissions()
        {
            return new List<string>
            {
                Permissions.KHSX_View,
                Permissions.DongThung_View,
                Permissions.DongThung_Create,
                Permissions.DongThung_Update
            };
        }

        /// <summary>
        /// Lấy permissions mặc định dựa trên role đầu tiên của user
        /// </summary>
        public static List<string> GetPermissionsByRole(string? roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return GetDefaultUserPermissions();

            return roleName.ToLower() switch
            {
                "user-dongthung" or "userdongthung" => GetUserDongThungPermissions(),
                "user" => GetUserRolePermissions(),
                "admin" => Permissions.GetAllPermissions(), // Admin có tất cả
                _ => GetDefaultUserPermissions()
            };
        }
    }
}


