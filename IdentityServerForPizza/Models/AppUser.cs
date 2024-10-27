using Microsoft.AspNetCore.Identity;

namespace duendeIdentityServer.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
