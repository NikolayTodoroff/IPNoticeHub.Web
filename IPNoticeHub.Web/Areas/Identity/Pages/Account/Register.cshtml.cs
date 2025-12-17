// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using IPNoticeHub.Application.DTOs.UserRegistrationDTOs;
using IPNoticeHub.Application.Services.UserRegistrationServices.Abstractions;
using IPNoticeHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace IPNoticeHub.Web.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IUserRegistrationService _userRegistrationService;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IUserRegistrationService userRegistrationService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _userRegistrationService = userRegistrationService;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(
                100, 
                ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", 
                MinimumLength = 6)]

            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare(
                "Password", 
                ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = 
                (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins =
                (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid) return Page();

            var regResult =
                await _userRegistrationService.RegisterUserAsync(
                new UserRegistrationRequest(Input.Email, Input.Password));

            if (!regResult.Succeeded)
            {
                foreach (var error in regResult.Errors)
                    ModelState.AddModelError(string.Empty, error);

                return Page();
            }

            var userId = regResult.UserId;
            var code = regResult.EmailConfirmationToken;
            var encodedCode = code != null
                ? WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code))
                : null;

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { 
                    area = "Identity",
                    userId = userId,
                    code = encodedCode,
                    returnUrl = returnUrl },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                Input.Email, 
                "Confirm your email",
                $"Please confirm your account by " +
                $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                return RedirectToPage(
                    "RegisterConfirmation", 
                    new { email = Input.Email, returnUrl = returnUrl });
            }

            else
            {
                var user = await _userManager.FindByIdAsync(regResult.UserId!);
                await _signInManager.SignInAsync(user!, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
        }
    }
}
