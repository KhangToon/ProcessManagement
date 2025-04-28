using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProcessManagement.Pages.Account_Management.Password_Management
{
    public class DisplayResetTokenModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Token { get; set; } = string.Empty;

        public void OnGet()
        {
            // The token is passed via query string and displayed on the page.
        }
    }
}
