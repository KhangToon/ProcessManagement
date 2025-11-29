using Microsoft.EntityFrameworkCore;
using ProcessManagement.Data;
using ProcessManagement.Models.Authorization;
using System.Collections.Concurrent;

namespace ProcessManagement.Services.Authorization
{
    /// <summary>
    /// Service để quản lý mapping giữa Button và Role
    /// </summary>
    public class ButtonRoleService
    {
        // Dùng DbContextFactory để mỗi lần gọi sẽ tạo 1 DbContext riêng,
        // tránh lỗi "There is already an open DataReader..." khi dùng chung connection
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        // Cache button roles theo PagePath để tránh query lại nhiều lần cho cùng 1 trang
        private static readonly ConcurrentDictionary<string, (DateTime LoadedAt, Dictionary<string, List<string>> Data)> _pageRolesCache = new();
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

        public ButtonRoleService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Lấy danh sách roles được gán cho một button
        /// </summary>
        public async Task<List<string>> GetRolesForButtonAsync(string buttonId)
        {
            if (string.IsNullOrEmpty(buttonId))
                return new List<string>();

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var roles = await context.Set<ButtonRole>()
                    .Where(br => br.ButtonId == buttonId)
                    .Select(br => br.RoleName)
                    .Distinct()
                    .ToListAsync();

                return roles;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Gán role cho button (thay thế roles cũ)
        /// </summary>
        public async Task<bool> AssignRolesToButtonAsync(string buttonId, string pagePath, string buttonText, List<string> roleNames)
        {
            if (string.IsNullOrEmpty(buttonId) || roleNames == null)
                return false;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                // Xóa tất cả roles hiện có của button
                var existingButtonRoles = await context.Set<ButtonRole>()
                    .Where(br => br.ButtonId == buttonId)
                    .ToListAsync();

                context.Set<ButtonRole>().RemoveRange(existingButtonRoles);

                // Thêm roles mới
                if (roleNames.Any())
                {
                    var newButtonRoles = roleNames.Select(roleName => new ButtonRole
                    {
                        ButtonId = buttonId,
                        PagePath = pagePath,
                        ButtonText = buttonText,
                        RoleName = roleName,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    await context.Set<ButtonRole>().AddRangeAsync(newButtonRoles);
                }

                await context.SaveChangesAsync();
                // Invalidate cache cho trang này
                if (!string.IsNullOrEmpty(pagePath))
                {
                    _pageRolesCache.TryRemove(pagePath, out _);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy tất cả button roles của một trang (có cache theo pagePath)
        /// </summary>
        public async Task<Dictionary<string, List<string>>> GetButtonRolesByPageAsync(string pagePath)
        {
            if (string.IsNullOrEmpty(pagePath))
                return new Dictionary<string, List<string>>();

            try
            {
                // Kiểm tra cache trước
                if (_pageRolesCache.TryGetValue(pagePath, out var cached)
                    && DateTime.UtcNow - cached.LoadedAt < CacheDuration)
                {
                    return cached.Data;
                }

                await using var context = await _contextFactory.CreateDbContextAsync();

                var buttonRoles = await context.Set<ButtonRole>()
                    .Where(br => br.PagePath == pagePath)
                    .ToListAsync();

                var result = buttonRoles
                    .GroupBy(br => br.ButtonId)
                    .ToDictionary(g => g.Key, g => g.Select(br => br.RoleName).Distinct().ToList());

                _pageRolesCache[pagePath] = (DateTime.UtcNow, result);

                return result;
            }
            catch
            {
                return new Dictionary<string, List<string>>();
            }
        }

        /// <summary>
        /// Bulk load button roles cho danh sách ButtonIds cụ thể (tối ưu khi chỉ cần load một số button nhất định)
        /// Chỉ load khi Admin bật chế độ điều chỉnh phân quyền để giảm số lần query DB
        /// </summary>
        public async Task<Dictionary<string, List<string>>> BulkLoadButtonRolesAsync(string pagePath, List<string> buttonIds)
        {
            if (string.IsNullOrEmpty(pagePath) || buttonIds == null || !buttonIds.Any())
                return new Dictionary<string, List<string>>();

            try
            {
                // Kiểm tra cache trước - nếu đã có cache cho pagePath, chỉ lọc ra các ButtonIds cần thiết
                if (_pageRolesCache.TryGetValue(pagePath, out var cached)
                    && DateTime.UtcNow - cached.LoadedAt < CacheDuration)
                {
                    // Lọc chỉ các ButtonIds được yêu cầu
                    var filtered = cached.Data
                        .Where(kvp => buttonIds.Contains(kvp.Key))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    return filtered;
                }

                await using var context = await _contextFactory.CreateDbContextAsync();

                // Query chỉ các ButtonIds được yêu cầu
                var buttonRoles = await context.Set<ButtonRole>()
                    .Where(br => br.PagePath == pagePath && buttonIds.Contains(br.ButtonId))
                    .ToListAsync();

                var result = buttonRoles
                    .GroupBy(br => br.ButtonId)
                    .ToDictionary(g => g.Key, g => g.Select(br => br.RoleName).Distinct().ToList());

                // Cập nhật cache (merge với dữ liệu cũ nếu có)
                if (_pageRolesCache.TryGetValue(pagePath, out var existingCached)
                    && DateTime.UtcNow - existingCached.LoadedAt < CacheDuration)
                {
                    // Merge với cache cũ
                    foreach (var kvp in result)
                    {
                        existingCached.Data[kvp.Key] = kvp.Value;
                    }
                    _pageRolesCache[pagePath] = (existingCached.LoadedAt, existingCached.Data);
                }
                else
                {
                    // Tạo cache mới
                    _pageRolesCache[pagePath] = (DateTime.UtcNow, result);
                }

                return result;
            }
            catch
            {
                return new Dictionary<string, List<string>>();
            }
        }

        /// <summary>
        /// Xóa tất cả roles của một button
        /// </summary>
        public async Task<bool> RemoveButtonRolesAsync(string buttonId)
        {
            if (string.IsNullOrEmpty(buttonId))
                return false;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var buttonRoles = await context.Set<ButtonRole>()
                    .Where(br => br.ButtonId == buttonId)
                    .ToListAsync();

                context.Set<ButtonRole>().RemoveRange(buttonRoles);
                await context.SaveChangesAsync();
                // Invalidate toàn bộ cache vì không biết pagePath của button
                _pageRolesCache.Clear();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Xóa tất cả ButtonRoles của một role (khi role bị xóa)
        /// </summary>
        public async Task<bool> RemoveButtonRolesByRoleNameAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return false;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var buttonRoles = await context.Set<ButtonRole>()
                    .Where(br => br.RoleName == roleName)
                    .ToListAsync();

                context.Set<ButtonRole>().RemoveRange(buttonRoles);
                await context.SaveChangesAsync();
                _pageRolesCache.Clear();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra role có đang được sử dụng bởi button nào không
        /// </summary>
        public async Task<bool> IsRoleUsedByButtonsAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return false;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                return await context.Set<ButtonRole>()
                    .AnyAsync(br => br.RoleName == roleName);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách buttons đang sử dụng role này
        /// </summary>
        public async Task<List<string>> GetButtonsUsingRoleAsync(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return new List<string>();

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                return await context.Set<ButtonRole>()
                    .Where(br => br.RoleName == roleName)
                    .Select(br => $"{br.ButtonText} ({br.ButtonId})")
                    .Distinct()
                    .ToListAsync();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Cleanup orphaned ButtonRoles (roles không còn tồn tại trong AspNetRoles)
        /// </summary>
        public async Task<int> CleanupOrphanedButtonRolesAsync(List<string> existingRoleNames)
        {
            if (existingRoleNames == null || !existingRoleNames.Any())
                return 0;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var orphanedButtonRoles = await context.Set<ButtonRole>()
                    .Where(br => !existingRoleNames.Contains(br.RoleName))
                    .ToListAsync();

                if (orphanedButtonRoles.Any())
                {
                    context.Set<ButtonRole>().RemoveRange(orphanedButtonRoles);
                    await context.SaveChangesAsync();
                }

                _pageRolesCache.Clear();
                return orphanedButtonRoles.Count;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Cập nhật RoleName trong ButtonRoles khi đổi tên role
        /// </summary>
        public async Task<bool> UpdateRoleNameInButtonRolesAsync(string oldRoleName, string newRoleName)
        {
            if (string.IsNullOrEmpty(oldRoleName) || string.IsNullOrEmpty(newRoleName))
                return false;

            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var buttonRoles = await context.Set<ButtonRole>()
                    .Where(br => br.RoleName == oldRoleName)
                    .ToListAsync();

                foreach (var buttonRole in buttonRoles)
                {
                    buttonRole.RoleName = newRoleName;
                    buttonRole.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();
                _pageRolesCache.Clear();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

