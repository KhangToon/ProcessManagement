namespace ProcessManagement.Models.Authorization
{
    /// <summary>
    /// Quan hệ nhiều-nhiều giữa User và Permission
    /// </summary>
    public class UserPermission
    {
        public string UserId { get; set; } = string.Empty;
        public string Permission { get; set; } = string.Empty;
        
        // Navigation properties
        public AppUser? User { get; set; }
    }
}

