// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Test;
using IdentityModel;
using duendeIdentityServer.Data;
using duendeIdentityServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace duendeIdentityServer.Pages.Create;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly AuthDbContext _authDbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public Index(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager
        )
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _signInManager = signInManager;        
    }

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        Input = new InputModel { ReturnUrl = returnUrl };
        return Page();
    }
        
    public async Task<IActionResult> OnPost()
    {

        if (ModelState.IsValid)
        {
            var user = new AppUser()
            {
                UserName = Input.Username,
                Email = Input.Email,
                EmailConfirmed = true,
                Name = Input.Name
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded) 
            {
                await _userManager.AddClaimsAsync(user, new Claim[]
                {
                    new Claim(JwtClaimTypes.Name, Input.Name),
                    new Claim(JwtClaimTypes.Email, Input.Email)
                });
            }

            var loginResult = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, false, lockoutOnFailure: true);

            if (loginResult.Succeeded) 
            {
                if (Url.IsLocalUrl(Input.ReturnUrl))
                {
                    return Redirect(Input.ReturnUrl);
                }
                else if (string.IsNullOrEmpty(Input.ReturnUrl)) 
                {
                    return Redirect("~/");
                }
                else
                {
                    throw new Exception("invalid return URL");
                }
            }
            
        }

        return Page();
    }
}
