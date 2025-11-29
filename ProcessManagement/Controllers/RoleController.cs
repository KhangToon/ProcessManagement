using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcessManagement.Models;
using ProcessManagement.Pages.Account_Management.Role_Management.Models;
using ProcessManagement.Services.Authorization;

namespace ProcessManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")] // Restrict access to admins
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly PermissionService _permissionService;
        private readonly ButtonRoleService _buttonRoleService;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager, PermissionService permissionService, ButtonRoleService buttonRoleService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _permissionService = permissionService;
            _buttonRoleService = buttonRoleService;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IdentityRole>>> GetRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return Ok(roles);
        }

        // POST: api/roles
        [HttpPost]
        public async Task<ActionResult> CreateRole([FromBody] object request)
        {
            string roleName;
            List<string> permissions = new();

            // Try to parse as CreateRoleRequest first
            try
            {
                var jsonString = System.Text.Json.JsonSerializer.Serialize(request);
                var createRoleRequest = System.Text.Json.JsonSerializer.Deserialize<CreateRoleRequest>(jsonString);
                
                if (createRoleRequest != null && !string.IsNullOrWhiteSpace(createRoleRequest.RoleName))
                {
                    roleName = createRoleRequest.RoleName;
                    permissions = createRoleRequest.Permissions ?? new List<string>();
                }
                else
                {
                    // Try to parse as string (backward compatibility)
                    if (request is System.Text.Json.JsonElement jsonElement)
                    {
                        if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
                        {
                            roleName = jsonElement.GetString() ?? string.Empty;
                        }
                        else if (jsonElement.TryGetProperty("RoleName", out var roleNameProp))
                        {
                            roleName = roleNameProp.GetString() ?? string.Empty;
                            if (jsonElement.TryGetProperty("Permissions", out var permsProp))
                            {
                                permissions = permsProp.EnumerateArray()
                                    .Select(p => p.GetString() ?? string.Empty)
                                    .Where(p => !string.IsNullOrEmpty(p))
                                    .ToList();
                            }
                        }
                        else
                        {
                            return BadRequest("RoleName is required.");
                        }
                    }
                    else
                    {
                        roleName = request?.ToString() ?? string.Empty;
                    }
                }
            }
            catch
            {
                // Fallback: try as string
                roleName = request?.ToString() ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(roleName))
            {
                return BadRequest("Role name cannot be empty.");
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                return Conflict("Role already exists.");
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (result.Succeeded)
            {
                // Gán permissions cho role nếu có
                if (permissions != null && permissions.Any())
                {
                    await _permissionService.AssignRolePermissionsAsync(roleName, permissions);
                }

                return CreatedAtAction(nameof(GetRoles), new { name = roleName }, roleName);
            }

            return BadRequest(result.Errors);
        }

        // DELETE: api/roles/{roleName}
        [HttpDelete("{roleName}")]
        public async Task<ActionResult> DeleteRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            // Kiểm tra role có đang được sử dụng bởi buttons không
            var isUsedByButtons = await _buttonRoleService.IsRoleUsedByButtonsAsync(roleName);
            if (isUsedByButtons)
            {
                var buttonsUsingRole = await _buttonRoleService.GetButtonsUsingRoleAsync(roleName);
                var buttonsList = string.Join(", ", buttonsUsingRole.Take(5));
                var message = $"Role '{roleName}' đang được sử dụng bởi các buttons: {buttonsList}" + 
                             (buttonsUsingRole.Count > 5 ? $" và {buttonsUsingRole.Count - 5} buttons khác" : "") +
                             ". Các button roles sẽ tự động bị xóa khi role bị xóa. " +
                             "Sau khi xóa, các buttons này sẽ chỉ hiển thị cho Admin cho đến khi được gán role mới.";
                
                // Vẫn cho phép xóa, nhưng sẽ cleanup ButtonRoles
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                // Cleanup ButtonRoles sau khi xóa role thành công
                await _buttonRoleService.RemoveButtonRolesByRoleNameAsync(roleName);
                
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        // POST: api/roles/remove-list
        [HttpPost("remove-list")]
        public async Task<ActionResult> RemoveRolesFromUser([FromBody] AssignRolesRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (request.RoleNames == null || !request.RoleNames.Any())
            {
                return BadRequest("RoleNames cannot be empty.");
            }

            foreach (var roleName in request.RoleNames)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    return BadRequest($"Role '{roleName}' does not exist.");
                }

                // Check if the user is in the role
                if (await _userManager.IsInRoleAsync(user, roleName))
                {
                    var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                    if (!result.Succeeded)
                    {
                        return BadRequest(result.Errors);
                    }
                }
            }

            return Ok("Roles removed successfully.");
        }

        // POST: api/roles/assign
        [HttpPost("assign")]
        public async Task<ActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Xóa tất cả roles hiện tại của user (đảm bảo mỗi user chỉ có 1 role)
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return BadRequest(removeResult.Errors);
                }
            }

            // Gán role mới
            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        // POST: api/roles/assign-list
        [HttpPost("assign-list")]
        public async Task<ActionResult> AssignRolesToUser([FromBody] AssignRolesRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (request.RoleNames == null || !request.RoleNames.Any())
            {
                // Remove all roles for the user if RoleNames is empty
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!removeResult.Succeeded)
                {
                    return BadRequest(removeResult.Errors);
                }

                return Ok("All roles removed successfully.");
            }

            foreach (var roleName in request.RoleNames)
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    return BadRequest($"Role '{roleName}' does not exist.");
                }

                // Check if the user is already in the role
                if (await _userManager.IsInRoleAsync(user, roleName))
                {
                    continue;
                }
                else
                {
                    // Add the role if it doesn't exist
                    var result = await _userManager.AddToRoleAsync(user, roleName);
                    if (!result.Succeeded)
                    {
                        return BadRequest(result.Errors);
                    }
                }
            }

            return Ok("Roles assigned successfully.");
        }


        // POST: api/roles/assign-by-username
        [HttpPost("assign-by-username")]
        public async Task<ActionResult> AssignRoleByUsername([FromBody] AssignRoleByUsernameRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        // PUT: api/roles/{roleName}
        [HttpPut("{roleName}")]
        public async Task<ActionResult> UpdateRole(string roleName, [FromBody] string newRoleName)
        {
            if (string.IsNullOrWhiteSpace(newRoleName))
            {
                return BadRequest("New role name cannot be empty.");
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            // Check if new role name already exists
            var roleExists = await _roleManager.RoleExistsAsync(newRoleName);
            if (roleExists && newRoleName != roleName)
            {
                return Conflict("Role name already exists.");
            }

            role.Name = newRoleName;
            role.NormalizedName = newRoleName.ToUpperInvariant();

            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded)
            {
                // Cập nhật RoleName trong ButtonRoles khi đổi tên role
                if (newRoleName != roleName)
                {
                    await UpdateButtonRolesRoleNameAsync(roleName, newRoleName);
                }
                
                return Ok();
            }

            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Cập nhật RoleName trong ButtonRoles khi đổi tên role
        /// </summary>
        private async Task UpdateButtonRolesRoleNameAsync(string oldRoleName, string newRoleName)
        {
            await _buttonRoleService.UpdateRoleNameInButtonRolesAsync(oldRoleName, newRoleName);
        }
    }
}
