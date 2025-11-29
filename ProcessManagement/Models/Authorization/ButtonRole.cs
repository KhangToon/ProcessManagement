namespace ProcessManagement.Models.Authorization
{
    /// <summary>
    /// Model để lưu mapping giữa Button và Role trong database
    /// Admin có thể chuột phải vào button để set role cho button đó
    /// </summary>
    public class ButtonRole
    {
        public int Id { get; set; }
        public string ButtonId { get; set; } = string.Empty; // Unique ID của button (VD: "MayMoc_Create_Button", "KhoNVL_NhapKho_Button")
        public string PagePath { get; set; } = string.Empty; // Đường dẫn trang (VD: "/mmtbmanagement")
        public string ButtonText { get; set; } = string.Empty; // Text hiển thị của button (để admin dễ nhận biết)
        public string RoleName { get; set; } = string.Empty; // Role được gán cho button này
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}


