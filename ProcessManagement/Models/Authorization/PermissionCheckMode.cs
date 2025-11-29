namespace ProcessManagement.Models.Authorization
{
    /// <summary>
    /// Mode để check permissions: Any (OR) hoặc All (AND)
    /// </summary>
    public enum PermissionCheckMode
    {
        /// <summary>
        /// Chỉ cần có 1 permission trong danh sách (OR logic)
        /// </summary>
        Any = 0,
        
        /// <summary>
        /// Phải có tất cả permissions trong danh sách (AND logic)
        /// </summary>
        All = 1
    }
}


