using IdentityModel;
using duendeIdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace duendeIdentityServer.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly AuthDbContext _authDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(AuthDbContext db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _authDbContext = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public void Initialize()
        {
            if (_roleManager.FindByNameAsync(Configuration.Admin).Result == null)
            { 
                _roleManager.CreateAsync(new IdentityRole(Configuration.Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(Configuration.Customer)).GetAwaiter().GetResult();
            } 
            else { return; }

            AppUser adminUser = new()
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "123456",
                Name = "Ben Admin"
            };

            _userManager.CreateAsync(adminUser, "Admin123*").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(adminUser, Configuration.Admin).GetAwaiter().GetResult();

            var clims1 = _userManager.AddClaimsAsync(adminUser, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, adminUser.Name),
                new Claim(JwtClaimTypes.Role, Configuration.Admin),
            }).Result;


            AppUser customerUser = new()
            {
                UserName = "customer@gmail.com",
                Email = "customer@gmail.com",
                EmailConfirmed = true,
                PhoneNumber = "12345698",
                Name = "Bob Customer"
            };

            _userManager.CreateAsync(customerUser, "Customer123*").GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(customerUser, Configuration.Customer).GetAwaiter().GetResult();

            var clims2 = _userManager.AddClaimsAsync(customerUser, new Claim[]
            {
                new Claim(JwtClaimTypes.Name, adminUser.Name),
                new Claim(JwtClaimTypes.Role, Configuration.Admin),
            }).Result;

        }
    }
}
