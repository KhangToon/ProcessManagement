namespace ProcessManagement.Models.Authorization
{
    /// <summary>
    /// Định nghĩa tất cả các permissions trong hệ thống
    /// Format: [Module].[Action] (VD: KHSX.View, KhoNVL.Create)
    /// </summary>
    public static class Permissions
    {
        // Kế hoạch sản xuất (KHSX)
        public const string KHSX_View = "KHSX.View";
        public const string KHSX_Create = "KHSX.Create";
        public const string KHSX_Update = "KHSX.Update";
        public const string KHSX_Delete = "KHSX.Delete";

        // Kho nguyên vật liệu (KhoNVL)
        public const string KhoNVL_View = "KhoNVL.View";
        public const string KhoNVL_Create = "KhoNVL.Create";
        public const string KhoNVL_Update = "KhoNVL.Update";
        public const string KhoNVL_Delete = "KhoNVL.Delete";
        public const string KhoNVL_NhapKho = "KhoNVL.NhapKho";
        public const string KhoNVL_XuatKho = "KhoNVL.XuatKho";
        public const string KhoNVL_KiemKe = "KhoNVL.KiemKe";

        // Sản phẩm (SanPham)
        public const string SanPham_View = "SanPham.View";
        public const string SanPham_Create = "SanPham.Create";
        public const string SanPham_Update = "SanPham.Update";
        public const string SanPham_Delete = "SanPham.Delete";

        // Máy móc - Thiết bị (MayMoc)
        public const string MayMoc_View = "MayMoc.View";
        public const string MayMoc_Create = "MayMoc.Create";
        public const string MayMoc_Update = "MayMoc.Update";
        public const string MayMoc_Delete = "MayMoc.Delete";

        // Nhân viên (NhanVien)
        public const string NhanVien_View = "NhanVien.View";
        public const string NhanVien_Create = "NhanVien.Create";
        public const string NhanVien_Update = "NhanVien.Update";
        public const string NhanVien_Delete = "NhanVien.Delete";

        // Kho thành phẩm (KhoThanhPham)
        public const string KhoThanhPham_View = "KhoThanhPham.View";
        public const string KhoThanhPham_Create = "KhoThanhPham.Create";
        public const string KhoThanhPham_Update = "KhoThanhPham.Update";
        public const string KhoThanhPham_Delete = "KhoThanhPham.Delete";
        public const string KhoThanhPham_NhapKho = "KhoThanhPham.NhapKho";
        public const string KhoThanhPham_XuatKho = "KhoThanhPham.XuatKho";

        // Công đoạn/Nguyên công (NguyenCong)
        public const string NguyenCong_View = "NguyenCong.View";
        public const string NguyenCong_Create = "NguyenCong.Create";
        public const string NguyenCong_Update = "NguyenCong.Update";
        public const string NguyenCong_Delete = "NguyenCong.Delete";

        // Quản lý tài khoản (Users & Roles)
        public const string Users_View = "Users.View";
        public const string Users_Create = "Users.Create";
        public const string Users_Update = "Users.Update";
        public const string Users_Delete = "Users.Delete";
        public const string Users_ManagePermissions = "Users.ManagePermissions";

        public const string Roles_View = "Roles.View";
        public const string Roles_Create = "Roles.Create";
        public const string Roles_Update = "Roles.Update";
        public const string Roles_Delete = "Roles.Delete";

        // Đóng thùng (DongThung) - chức năng đặc biệt
        public const string DongThung_View = "DongThung.View";
        public const string DongThung_Create = "DongThung.Create";
        public const string DongThung_Update = "DongThung.Update";

        /// <summary>
        /// Lấy danh sách tất cả permissions, nhóm theo module
        /// </summary>
        public static Dictionary<string, List<string>> GetAllPermissionsByModule()
        {
            return new Dictionary<string, List<string>>
            {
                ["Kế hoạch sản xuất"] = new List<string> { KHSX_View, KHSX_Create, KHSX_Update, KHSX_Delete },
                ["Kho nguyên vật liệu"] = new List<string> { KhoNVL_View, KhoNVL_Create, KhoNVL_Update, KhoNVL_Delete, KhoNVL_NhapKho, KhoNVL_XuatKho, KhoNVL_KiemKe },
                ["Sản phẩm"] = new List<string> { SanPham_View, SanPham_Create, SanPham_Update, SanPham_Delete },
                ["Máy móc - Thiết bị"] = new List<string> { MayMoc_View, MayMoc_Create, MayMoc_Update, MayMoc_Delete },
                ["Nhân viên"] = new List<string> { NhanVien_View, NhanVien_Create, NhanVien_Update, NhanVien_Delete },
                ["Kho thành phẩm"] = new List<string> { KhoThanhPham_View, KhoThanhPham_Create, KhoThanhPham_Update, KhoThanhPham_Delete, KhoThanhPham_NhapKho, KhoThanhPham_XuatKho },
                ["Công đoạn"] = new List<string> { NguyenCong_View, NguyenCong_Create, NguyenCong_Update, NguyenCong_Delete },
                ["Quản lý tài khoản"] = new List<string> { Users_View, Users_Create, Users_Update, Users_Delete, Users_ManagePermissions, Roles_View, Roles_Create, Roles_Update, Roles_Delete },
                ["Đóng thùng"] = new List<string> { DongThung_View, DongThung_Create, DongThung_Update }
            };
        }

        /// <summary>
        /// Lấy tất cả permissions dạng list phẳng
        /// </summary>
        public static List<string> GetAllPermissions()
        {
            return GetAllPermissionsByModule().SelectMany(x => x.Value).ToList();
        }
    }
}

