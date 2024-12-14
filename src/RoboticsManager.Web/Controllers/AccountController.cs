using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RoboticsManager.Lib.Models;

namespace RoboticsManager.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Store return URL in session
            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["ReturnUrl"] = returnUrl;
            }

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme,
                Url.Action(nameof(ExternalLoginCallback))
            );

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _logger.LogWarning("Error loading external login information");
                return RedirectToAction("Error", "Home");
            }

            // Sign in the user with external login provider
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: true,
                bypassTwoFactor: true
            );

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "User {Name} logged in with {LoginProvider} provider",
                    info.Principal.Identity?.Name,
                    info.LoginProvider
                );

                // Get return URL from session
                var returnUrl = TempData["ReturnUrl"]?.ToString();
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            // If the user does not have an account, create one
            var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (email == null)
            {
                _logger.LogWarning("Unable to retrieve email from external login provider");
                return RedirectToAction("Error", "Home");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true, // Google has already verified the email
                FullName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? email
            };

            var createResult = await _userManager.CreateAsync(user);
            if (createResult.Succeeded)
            {
                createResult = await _userManager.AddLoginAsync(user, info);
                if (createResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: true);
                    _logger.LogInformation(
                        "User {Email} created an account using {LoginProvider} provider",
                        email,
                        info.LoginProvider
                    );

                    // Get return URL from session
                    var returnUrl = TempData["ReturnUrl"]?.ToString();
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
            }

            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return RedirectToAction("AccessDenied", "Error");
        }
    }
}
