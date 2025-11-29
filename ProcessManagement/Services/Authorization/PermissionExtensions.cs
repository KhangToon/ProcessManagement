using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace ProcessManagement.Services.Authorization
{
    /// <summary>
    /// Extension methods để dễ dàng check permissions trong code
    /// </summary>
    public static class PermissionExtensions
    {
        /// <summary>
        /// Kiểm tra user hiện tại có permission không (dùng trong Razor component)
        /// </summary>
        public static async Task<bool> HasPermissionAsync(
            this PermissionService permissionService,
            AuthenticationState authState,
            string permission)
        {
            var userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return false;

            // Admin luôn có tất cả permissions
            if (authState.User.IsInRole("Admin"))
                return true;

            return await permissionService.HasPermissionAsync(userId, permission);
        }

        /// <summary>
        /// Kiểm tra user hiện tại có permission không (dùng trong code-behind)
        /// </summary>
        public static async Task<bool> HasPermissionAsync(
            this PermissionService permissionService,
            ClaimsPrincipal user,
            string permission)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return false;

            // Admin luôn có tất cả permissions
            if (user.IsInRole("Admin"))
                return true;

            return await permissionService.HasPermissionAsync(userId, permission);
        }

        /// <summary>
        /// Kiểm tra user có bất kỳ permission nào trong danh sách (OR logic)
        /// </summary>
        public static async Task<bool> HasAnyPermissionAsync(
            this PermissionService permissionService,
            ClaimsPrincipal user,
            params string[] permissions)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return false;

            // Admin luôn có tất cả permissions
            if (user.IsInRole("Admin"))
                return true;

            return await permissionService.HasAnyPermissionAsync(userId, permissions);
        }

        /// <summary>
        /// Kiểm tra user có tất cả permissions trong danh sách không (AND logic)
        /// </summary>
        public static async Task<bool> HasAllPermissionsAsync(
            this PermissionService permissionService,
            ClaimsPrincipal user,
            params string[] permissions)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || permissions == null || permissions.Length == 0)
                return false;

            // Admin luôn có tất cả permissions
            if (user.IsInRole("Admin"))
                return true;

            foreach (var permission in permissions)
            {
                if (!await permissionService.HasPermissionAsync(userId, permission))
                    return false;
            }

            return true;
        }
    }
}

