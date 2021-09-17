using Microsoft.AspNetCore.Identity;

namespace WebAPItest.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Login { get; set; }
    }
}
