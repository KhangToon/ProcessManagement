using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcessManagement.Models;
using ProcessManagement.Pages.Account_Management.User_Management.Models;
using ProcessManagement.Data;
using ProcessManagement.Services;
using ProcessManagement.Services.Authorization;

namespace ProcessManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")] // Restrict access to admins

    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly PasswordEncryptionService _passwordEncryption;
        private readonly PermissionService _permissionService;

        public UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context, PasswordEncryptionService passwordEncryption, PermissionService permissionService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _passwordEncryption = passwordEncryption;
            _permissionService = permissionService;
        }

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
        {
            // Fetch all users first
            var users = await _userManager.Users
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Username = u.UserName ?? string.Empty, // Handle possible null values
                    Email = u.Email ?? string.Empty,      // Handle possible null values
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            // Fetch roles for each user
            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id);
                if (appUser != null)
                {
                    user.Roles = (await _userManager.GetRolesAsync(appUser)).ToList();
                }
            }

            return Ok(users);
        }


        // POST: api/user
        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username and password are required.");
            }

            // Đảm bảo Email được set - nếu không có thì dùng Username@default.com
            var email = string.IsNullOrWhiteSpace(request.Email) 
                ? $"{request.Username}@default.com" 
                : request.Email;

            var user = new AppUser
            {
                UserName = request.Username,
                Email = email,
                EmailConfirmed = true, // Tự động confirm email để có thể login ngay
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true // Mặc định tài khoản mới là hoạt động
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                // Gán role cho user
                string roleToAssign = string.IsNullOrWhiteSpace(request.RoleName) ? "User" : request.RoleName;
                var roleExists = await _roleManager.RoleExistsAsync(roleToAssign);
                if (roleExists)
                {
                    await _userManager.AddToRoleAsync(user, roleToAssign);
                    
                    // Gán permissions mặc định từ role cho user
                    var rolePermissions = await _permissionService.GetRolePermissionsAsync(roleToAssign);
                    if (rolePermissions != null && rolePermissions.Any())
                    {
                        await _permissionService.AssignPermissionsAsync(user.Id, rolePermissions);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(request.RoleName))
                {
                    // Role được chỉ định không tồn tại, gán role mặc định "User"
                    var defaultRole = "User";
                    var defaultRoleExists = await _roleManager.RoleExistsAsync(defaultRole);
                    if (defaultRoleExists)
                    {
                        await _userManager.AddToRoleAsync(user, defaultRole);
                        
                        // Gán permissions mặc định từ role "User"
                        var rolePermissions = await _permissionService.GetRolePermissionsAsync(defaultRole);
                        if (rolePermissions != null && rolePermissions.Any())
                        {
                            await _permissionService.AssignPermissionsAsync(user.Id, rolePermissions);
                        }
                    }
                }

                // Lưu password đã encrypt để có thể xem lại sau
                var encryptedPassword = _passwordEncryption.Encrypt(request.Password);
                var userPassword = new UserPassword
                {
                    UserId = user.Id,
                    EncryptedPassword = encryptedPassword,
                    LastUpdated = DateTime.UtcNow
                };
                
                _context.UserPasswords.Add(userPassword);
                await _context.SaveChangesAsync();

                // Trả về user với thông tin đầy đủ để client có thể gán permissions
                var userResponse = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.FirstName,
                    user.LastName
                };
                
                return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, userResponse);
            }

            return BadRequest(result.Errors);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Bảo vệ tài khoản admin@admin
            if (user.Email?.ToLower() == "admin@admin" || user.UserName?.ToLower() == "admin")
            {
                return BadRequest("Không thể chỉnh sửa tài khoản admin@admin. Chỉ có thể thay đổi trực tiếp trong database.");
            }

            user.Email = request.Email ?? user.Email;
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.IsActive = request.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Bảo vệ tài khoản admin@admin
            if (user.Email?.ToLower() == "admin@admin" || user.UserName?.ToLower() == "admin")
            {
                return BadRequest("Không thể xóa tài khoản admin@admin. Chỉ có thể xóa trực tiếp trong database.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        // PUT: api/user/{id}/toggle-active
        [HttpPut("{id}/toggle-active")]
        public async Task<ActionResult> ToggleUserActive(string id, [FromBody] bool isActive)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Bảo vệ tài khoản admin@admin
            if (user.Email?.ToLower() == "admin@admin" || user.UserName?.ToLower() == "admin")
            {
                return BadRequest("Không thể thay đổi trạng thái tài khoản admin@admin. Chỉ có thể thay đổi trực tiếp trong database.");
            }

            user.IsActive = isActive;
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                return Ok(new { IsActive = user.IsActive });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (result.Succeeded)
            {
                return Ok("Password reset successful.");
            }

            return BadRequest(result.Errors);
        }

        // POST: api/user/{id}/change-password
        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Đảm bảo Email được set nếu chưa có
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                user.Email = $"{user.UserName}@default.com";
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            // Remove current password và set password mới (Admin có thể đổi mật khẩu cho user)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            
            if (result.Succeeded)
            {
                // Cập nhật password đã encrypt
                var userPassword = await _context.UserPasswords.FirstOrDefaultAsync(up => up.UserId == id);
                if (userPassword != null)
                {
                    userPassword.EncryptedPassword = _passwordEncryption.Encrypt(request.NewPassword);
                    userPassword.LastUpdated = DateTime.UtcNow;
                    _context.UserPasswords.Update(userPassword);
                }
                else
                {
                    // Nếu chưa có, tạo mới
                    userPassword = new UserPassword
                    {
                        UserId = id,
                        EncryptedPassword = _passwordEncryption.Encrypt(request.NewPassword),
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.UserPasswords.Add(userPassword);
                }
                
                await _context.SaveChangesAsync();
                
                return Ok("Password changed successfully.");
            }

            return BadRequest(result.Errors);
        }

        // GET: api/user/{id}/get-password
        [HttpGet("{id}/get-password")]
        public async Task<IActionResult> GetPassword(string id)
        {
            var userPassword = await _context.UserPasswords.FirstOrDefaultAsync(up => up.UserId == id);
            if (userPassword == null)
            {
                return NotFound("Password not found for this user.");
            }

            var decryptedPassword = _passwordEncryption.Decrypt(userPassword.EncryptedPassword);
            
            return Ok(new 
            { 
                Password = decryptedPassword,
                LastUpdated = userPassword.LastUpdated
            });
        }

        public class ResetPasswordRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
            public string NewPassword { get; set; } = string.Empty;
        }

        public class ChangePasswordRequest
        {
            public string NewPassword { get; set; } = string.Empty;
        }

        // ⚠️ TEMPORARY ENDPOINT - FOR DEVELOPMENT ONLY - REMOVE IN PRODUCTION ⚠️
        // POST: api/user/emergency-reset-password
        // Reset password without authentication (ONLY FOR DEVELOPMENT)
        [HttpPost("emergency-reset-password")]
        [AllowAnonymous] // Cho phép không cần đăng nhập
        public async Task<IActionResult> EmergencyResetPassword([FromBody] EmergencyResetPasswordRequest request)
        {
            // Tìm user theo username hoặc email
            AppUser? user = null;
            
            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                user = await _userManager.FindByNameAsync(request.Username);
            }
            
            if (user == null && !string.IsNullOrWhiteSpace(request.Email))
            {
                user = await _userManager.FindByEmailAsync(request.Email);
            }

            if (user == null)
            {
                return NotFound($"User not found with username '{request.Username}' or email '{request.Email}'.");
            }

            // Reset password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);
            
            if (result.Succeeded)
            {
                // Đảm bảo Email được set
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    user.Email = $"{user.UserName}@default.com";
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }

                // Cập nhật password đã encrypt
                var userPassword = await _context.UserPasswords.FirstOrDefaultAsync(up => up.UserId == user.Id);
                if (userPassword != null)
                {
                    userPassword.EncryptedPassword = _passwordEncryption.Encrypt(request.NewPassword);
                    userPassword.LastUpdated = DateTime.UtcNow;
                    _context.UserPasswords.Update(userPassword);
                }
                else
                {
                    userPassword = new UserPassword
                    {
                        UserId = user.Id,
                        EncryptedPassword = _passwordEncryption.Encrypt(request.NewPassword),
                        LastUpdated = DateTime.UtcNow
                    };
                    _context.UserPasswords.Add(userPassword);
                }
                
                await _context.SaveChangesAsync();
                
                return Ok(new 
                { 
                    Success = true,
                    Message = $"Password reset successfully for user: {user.UserName}",
                    Username = user.UserName,
                    Email = user.Email
                });
            }

            return BadRequest(new { Errors = result.Errors });
        }

        public class EmergencyResetPasswordRequest
        {
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string NewPassword { get; set; } = string.Empty;
        }
    }
    // This API using Models.UserResponse for the response data
}
