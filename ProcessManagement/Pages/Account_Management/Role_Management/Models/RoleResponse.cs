namespace ProcessManagement.Pages.Account_Management.Role_Management.Models
{
    public class RoleResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class AssignRoleRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }

    public class AssignRoleByUsernameRequest
    {
        public string Username { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
