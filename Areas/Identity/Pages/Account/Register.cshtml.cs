
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace JobMatch.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private const string DEFAULT_ROLE = "Jobseeker";

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [BindProperty] public InputModel Input { get; set; } = new();
        public string ReturnUrl { get; set; } = "/";
        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();
        public List<SelectListItem> Roles { get; set; } = new();

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, StringLength(100, MinimumLength = 6,
                ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Display(Name = "Role (optional)")]
            public string? SelectedRole { get; set; }
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                
                var allowed = new[] { "Jobseeker", "Recruiter" };
                var requested = string.IsNullOrWhiteSpace(Input.SelectedRole) ? DEFAULT_ROLE : Input.SelectedRole!;
                if (!allowed.Contains(requested, StringComparer.OrdinalIgnoreCase))
                    requested = DEFAULT_ROLE;

                
                requested = allowed.First(r => r.Equals(requested, StringComparison.OrdinalIgnoreCase));

                if (await _roleManager.RoleExistsAsync(requested))
                {
                    await _userManager.AddToRoleAsync(user, requested);
                }

                
                await _signInManager.SignInAsync(user, isPersistent: false);

                
                var roles = await _userManager.GetRolesAsync(user);

                
                if (roles.Contains("Administrator"))
                    return LocalRedirect(Url.Action("Settings", "Admin") ?? Url.Content("~/"));

                if (roles.Contains("Recruiter"))
                    return LocalRedirect(Url.Action("Create", "Jobs") ?? Url.Content("~/"));

                
                return LocalRedirect(Url.Action("Index", "Jobs") ?? Url.Content("~/"));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return Page();
        }

        private List<SelectListItem> _role_manager_roles() =>
            _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem { Value = r.Name!, Text = r.Name! })
                .ToList();
    }
}