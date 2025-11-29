using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProcessManagement.Data;
using ProcessManagement.Models;
using ProcessManagement.Models.Authorization;

namespace ProcessManagement.Services.Authorization
{
    /// <summary>
    /// Service để quản lý và kiểm tra permissions
    /// </summary>
    public class PermissionService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PermissionService(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Kiểm tra user có permission không
        /// Admin luôn có tất cả permissions
        /// </summary>
        public async Task<bool> HasPermissionAsync(string userId, string permission)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(permission))
                return false;

            // Kiểm tra nếu user là Admin
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return true;
            }

            // Kiểm tra permission trong database
            var hasPermission = await _context.Set<UserPermission>()
                .AnyAsync(up => up.UserId == userId && up.Permission == permission);

            return hasPermission;
        }

        /// <summary>
        /// Kiểm tra user có bất kỳ permission nào trong danh sách không
        /// </summary>
        public async Task<bool> HasAnyPermissionAsync(string userId, params string[] permissions)
        {
            if (permissions == null || permissions.Length == 0)
                return false;

            foreach (var permission in permissions)
            {
                if (await HasPermissionAsync(userId, permission))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Lấy tất cả permissions của user
        /// </summary>
        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<string>();

            // Nếu là Admin, trả về tất cả permissions
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return Permissions.GetAllPermissions();
            }

            // Lấy permissions từ database
            var permissions = await _context.Set<UserPermission>()
                .Where(up => up.UserId == userId)
                .Select(up => up.Permission)
                .ToListAsync();

            return permissions;
        }

        /// <summary>
        /// Gán permissions cho user
        /// </summary>
        public async Task<bool> AssignPermissionsAsync(string userId, List<string> permissions)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            try
            {
                // Xóa tất cả permissions hiện tại của user
                var existingPermissions = await _context.Set<UserPermission>()
                    .Where(up => up.UserId == userId)
                    .ToListAsync();

                _context.Set<UserPermission>().RemoveRange(existingPermissions);

                // Thêm permissions mới
                if (permissions != null && permissions.Any())
                {
                    var userPermissions = permissions
                        .Where(p => !string.IsNullOrEmpty(p))
                        .Select(p => new UserPermission
                        {
                            UserId = userId,
                            Permission = p
                        })
                        .ToList();

                    await _context.Set<UserPermission>().AddRangeAsync(userPermissions);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Thêm permission cho user (không xóa permissions hiện có)
        /// </summary>
        public async Task<bool> AddPermissionAsync(string userId, string permission)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(permission))
                return false;

            try
            {
                // Kiểm tra xem permission đã tồn tại chưa
                var exists = await _context.Set<UserPermission>()
                    .AnyAsync(up => up.UserId == userId && up.Permission == permission);

                if (exists)
                    return true; // Đã có rồi

                var userPermission = new UserPermission
                {
                    UserId = userId,
                    Permission = permission
                };

                await _context.Set<UserPermission>().AddAsync(userPermission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Xóa permission của user
        /// </summary>
        public async Task<bool> RemovePermissionAsync(string userId, string permission)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(permission))
                return false;

            try
            {
                var userPermission = await _context.Set<UserPermission>()
                    .FirstOrDefaultAsync(up => up.UserId == userId && up.Permission == permission);

                if (userPermission != null)
                {
                    _context.Set<UserPermission>().Remove(userPermission);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ========== ROLE PERMISSIONS ==========

        /// <summary>
        /// Lấy tất cả permissions của role
        /// </summary>
        public async Task<List<string>> GetRolePermissionsAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return new List<string>();

            try
            {
                var permissions = await _context.Set<RolePermission>()
                    .Where(rp => rp.RoleName == roleName)
                    .Select(rp => rp.Permission)
                    .ToListAsync();

                return permissions;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Gán permissions cho role
        /// </summary>
        public async Task<bool> AssignRolePermissionsAsync(string roleName, List<string> permissions)
        {
            if (string.IsNullOrEmpty(roleName))
                return false;

            try
            {
                // Xóa tất cả permissions hiện tại của role
                var existingPermissions = await _context.Set<RolePermission>()
                    .Where(rp => rp.RoleName == roleName)
                    .ToListAsync();

                _context.Set<RolePermission>().RemoveRange(existingPermissions);

                // Thêm permissions mới
                if (permissions != null && permissions.Any())
                {
                    var rolePermissions = permissions
                        .Where(p => !string.IsNullOrEmpty(p))
                        .Select(p => new RolePermission
                        {
                            RoleName = roleName,
                            Permission = p
                        })
                        .ToList();

                    await _context.Set<RolePermission>().AddRangeAsync(rolePermissions);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

