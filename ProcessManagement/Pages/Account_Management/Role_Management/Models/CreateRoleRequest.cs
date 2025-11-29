namespace ProcessManagement.Pages.Account_Management.Role_Management.Models
{
    public class CreateRoleRequest
    {
        public string RoleName { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }
}

