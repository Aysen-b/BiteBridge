using Microsoft.AspNetCore.Identity;

namespace BiteBridge.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string RoleName { get; set; }
    }
}