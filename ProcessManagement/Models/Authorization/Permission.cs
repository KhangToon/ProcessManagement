namespace ProcessManagement.Models.Authorization
{
    /// <summary>
    /// Model để lưu permissions trong database
    /// Cho phép quản lý permissions động
    /// </summary>
    public class Permission
    {
        public int Id { get; set; }
        public string PermissionCode { get; set; } = string.Empty; // VD: "KHSX.Create"
        public string Module { get; set; } = string.Empty; // VD: "Kế hoạch sản xuất"
        public string Action { get; set; } = string.Empty; // VD: "Create"
        public string DisplayName { get; set; } = string.Empty; // VD: "Tạo mới"
        public string Description { get; set; } = string.Empty; // Mô tả chi tiết
        public bool IsActive { get; set; } = true; // Có đang sử dụng không
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

