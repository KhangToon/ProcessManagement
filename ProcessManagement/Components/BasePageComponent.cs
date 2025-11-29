using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using ProcessManagement.Models;
using ProcessManagement.Services.Authorization;
using System.Security.Claims;
using ProcessManagement.Models.Authorization;

namespace ProcessManagement.Components
{
    /// <summary>
    /// Base component class cung cấp các helper methods để check permissions một cách linh động
    /// </summary>
    public abstract class BasePageComponent : ComponentBase
    {
        [Inject] protected PermissionService PermissionService { get; set; } = null!;
        [Inject] protected UserManager<AppUser> UserManager { get; set; } = null!;
        [CascadingParameter] protected Task<AuthenticationState>? AuthenticationStateTask { get; set; }

        protected ClaimsPrincipal? CurrentUser { get; private set; }
        protected string? CurrentUserId { get; private set; }
        protected bool IsAdmin { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadUserInfo();
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Load thông tin user hiện tại
        /// </summary>
        protected async Task LoadUserInfo()
        {
            if (AuthenticationStateTask != null)
            {
                var authState = await AuthenticationStateTask;
                CurrentUser = authState?.User;
                
                if (CurrentUser != null && CurrentUser.Identity?.IsAuthenticated == true)
                {
                    CurrentUserId = CurrentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    IsAdmin = CurrentUser.IsInRole("Admin");
                }
            }
        }

        /// <summary>
        /// Kiểm tra user có permission không
        /// </summary>
        protected async Task<bool> HasPermissionAsync(string permission)
        {
            if (string.IsNullOrEmpty(CurrentUserId) || string.IsNullOrEmpty(permission))
                return false;

            // Admin luôn có tất cả permissions
            if (IsAdmin)
                return true;

            return await PermissionService.HasPermissionAsync(CurrentUserId, permission);
        }

        /// <summary>
        /// Kiểm tra user có bất kỳ permission nào trong danh sách không (OR logic)
        /// </summary>
        protected async Task<bool> HasAnyPermissionAsync(params string[] permissions)
        {
            if (string.IsNullOrEmpty(CurrentUserId) || permissions == null || permissions.Length == 0)
                return false;

            // Admin luôn có tất cả permissions
            if (IsAdmin)
                return true;

            return await PermissionService.HasAnyPermissionAsync(CurrentUserId, permissions);
        }

        /// <summary>
        /// Kiểm tra user có tất cả permissions trong danh sách không (AND logic)
        /// </summary>
        protected async Task<bool> HasAllPermissionsAsync(params string[] permissions)
        {
            if (string.IsNullOrEmpty(CurrentUserId) || permissions == null || permissions.Length == 0)
                return false;

            // Admin luôn có tất cả permissions
            if (IsAdmin)
                return true;

            foreach (var permission in permissions)
            {
                if (!await PermissionService.HasPermissionAsync(CurrentUserId, permission))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Kiểm tra user có role không
        /// </summary>
        protected bool HasRole(string role)
        {
            if (CurrentUser == null || string.IsNullOrEmpty(role))
                return false;

            // Admin luôn có tất cả roles
            if (IsAdmin)
                return true;

            return CurrentUser.IsInRole(role);
        }

        /// <summary>
        /// Kiểm tra user có bất kỳ role nào trong danh sách không (OR logic)
        /// </summary>
        protected bool HasAnyRole(params string[] roles)
        {
            if (CurrentUser == null || roles == null || roles.Length == 0)
                return false;

            // Admin luôn có tất cả roles
            if (IsAdmin)
                return true;

            return roles.Any(role => CurrentUser.IsInRole(role));
        }

        /// <summary>
        /// Kiểm tra user có tất cả roles trong danh sách không (AND logic)
        /// </summary>
        protected bool HasAllRoles(params string[] roles)
        {
            if (CurrentUser == null || roles == null || roles.Length == 0)
                return false;

            // Admin luôn có tất cả roles
            if (IsAdmin)
                return true;

            return roles.All(role => CurrentUser.IsInRole(role));
        }

    }
}

