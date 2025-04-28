using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcessManagement.Models;
using ProcessManagement.Pages.Account_Management.Role_Management.Models;

namespace ProcessManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")] // Restrict access to admins
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
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
        public async Task<ActionResult> CreateRole([FromBody] string roleName)
        {
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

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
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
    }
}
