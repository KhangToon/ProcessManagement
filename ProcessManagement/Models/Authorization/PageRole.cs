namespace ProcessManagement.Models.Authorization
{
    /// <summary>
    /// Model để lưu mapping giữa Page và Role trong database
    /// Admin có thể set role cho page để kiểm soát quyền truy cập
    /// </summary>
    public class PageRole
    {
        public int Id { get; set; }
        public string PagePath { get; set; } = string.Empty; // Đường dẫn trang (VD: "/spmanagementv2", "/mmtbmanagement")
        public string PageName { get; set; } = string.Empty; // Tên trang (để admin dễ nhận biết)
        public string RoleName { get; set; } = string.Empty; // Role được phép truy cập trang này
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

