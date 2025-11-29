namespace ProcessManagement.Models
{
    /// <summary>
    /// Lưu password đã encrypt của user để có thể xem lại (chỉ dùng trong môi trường nội bộ)
    /// </summary>
    public class UserPassword
    {
        public string UserId { get; set; } = string.Empty;
        public string EncryptedPassword { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
        
        // Navigation property
        public AppUser? User { get; set; }
    }
}


