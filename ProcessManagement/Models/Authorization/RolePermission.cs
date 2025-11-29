namespace ProcessManagement.Models.Authorization
{
    /// <summary>
    /// Quan hệ nhiều-nhiều giữa Role và Permission
    /// Lưu các quyền mặc định cho từng role
    /// </summary>
    public class RolePermission
    {
        public string RoleName { get; set; } = string.Empty;
        public string Permission { get; set; } = string.Empty;
    }
}


