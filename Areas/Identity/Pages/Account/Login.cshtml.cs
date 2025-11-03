
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace JobMatch.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string ReturnUrl { get; set; } = "/";

        public class InputModel
        {
            [Required, EmailAddress] public string Email { get; set; } = "";
            [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
            [Display(Name = "Remember me")] public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                var user = await _userManager.FindByEmailAsync(Input.Email);
                var landing = await GetLandingUrlForUserAsync(user);

                
                return LocalRedirect(landing);

            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account is locked out.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        private async Task<string> GetLandingUrlForUserAsync(IdentityUser? user)
        {
            if (user == null) return Url.Content("~/");

            var roles = await _userManager.GetRolesAsync(user);

            
            if (roles.Contains("Administrator"))
                return Url.Action("Settings", "Admin") ?? Url.Content("~/");

            if (roles.Contains("Recruiter"))
                return Url.Action("Create", "Jobs") ?? Url.Content("~/");

            
            return Url.Action("Index", "Jobs") ?? Url.Content("~/");
        }
    }
}