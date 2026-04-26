using ApplePaySimulation.Database.Entities;
using ApplePaySimulation.Models.SettingsModels;
using ApplePaySimulation.Models.ViewModels.Authentication;
using ApplePaySimulation.Services.Abstracts;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ApplePaySimulation.Controllers
{
    
    public class UserController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IEmailSenderService _emailSender;
        private readonly EmailSettings emailSettings;
        private readonly IConfiguration _configuration;
       
       

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, IEmailSenderService emailSender, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
            emailSettings = new EmailSettings
            {
                SmtpHost = _configuration["EmailSettings:SmtpHost"],
                SmtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]),
                SmtpUseSSL = bool.Parse(_configuration["EmailSettings:SmtpUseSSL"]),
                SmtpUser = _configuration["EmailSettings:SmtpUser"],
                SmtpPassword = _configuration["EmailSettings:SmtpPassword"],
                FromName = _configuration["EmailSettings:FromName"]
            };
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };
            return View("Login", loginViewModel);
        }
       
      
        [Authorize]
        public async Task<IActionResult> SignOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

      
        [HttpPost]
        public async Task<IActionResult> SaveLogin(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                User appuser = await userManager.FindByEmailAsync(loginViewModel.Email);

                if (appuser != null)
                {

                    bool found = await userManager.CheckPasswordAsync(appuser, loginViewModel.Password);
                    if (found)
                    {
                        if (!appuser.EmailConfirmed)
                        {
                            return RedirectToAction(
                                "LoginConfirmation",
                                "User",
                                new { Email = appuser.Email }
                            );
                        }

                        await signInManager.SignInAsync(appuser, loginViewModel.RememberMe);

                        

                        return RedirectToAction("Index", "Wallet");
                    }
                }

                ModelState.AddModelError("", "Email or password is incorrect.");
            }

          

            return View("Login", loginViewModel);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public async Task<IActionResult> SaveRegister(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Email = registerViewModel.Email,
                    UserName = registerViewModel.Email,
                    FullName = registerViewModel.FullName
                    
                };

                var result = await userManager.CreateAsync(user, registerViewModel.Password);

                if (result.Succeeded)
                {
                    // Generate email confirmation token
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action(
                        "ConfirmEmail",
                        "User",
                        new { userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    // Send email
                    await _emailSender.SendEmailAsync(
       registerViewModel.Email,
       "Confirm your email",
       $@"
    <div style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;'>
        <div style='max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 8px; 
                    box-shadow: 0 2px 8px rgba(0,0,0,0.1); padding: 30px;'>
            <h2 style='color: #cda45e; text-align: center;'>Confirm Your Email</h2>
            <p style='color: #333; font-size: 16px; line-height: 1.6;'>
                Thank you for registering! Please confirm your account by clicking the button below.
            </p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                   style='background-color: #cda45e; color: #fff; text-decoration: none; padding: 12px 25px;
                          border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;'>
                    Confirm My Email
                </a>
            </div>
            <p style='color: #777; font-size: 14px; text-align: center;'>
                If you didn’t create an account, you can ignore this message.
            </p>
        </div>
    </div>
    ", emailSettings);

                    // Don't sign in automatically - require email confirmation first
                    return RedirectToAction("RegisterConfirmation", new { email = registerViewModel.Email });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View("Register", registerViewModel);
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmResetPassword(string userId, string code)
        {
            ResetPasswordEditViewModel resetPasswordEditViewModel = new ResetPasswordEditViewModel();
            resetPasswordEditViewModel.token = code;
            resetPasswordEditViewModel.userId = userId;
            return View("ResetPasswordEdit", resetPasswordEditViewModel);
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Login", "User");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Error confirming email for user with ID '{userId}':");
            }


            return RedirectToAction("Login", "User");
        }


        [HttpGet]
        public IActionResult RegisterConfirmation(string email)
        {
            ViewBag.Email = email;
            return View();
        }
        [HttpGet]
        public IActionResult UpdateProfileConfirmation(string email)
        {
            ViewBag.Email = email;
            return View();
        }
        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return View();
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("RegisterConfirmation", new { email = email });
            }

            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                return RedirectToAction("Login");
            }

            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "User",
                new { userId = user.Id, code = code },
                protocol: Request.Scheme);
            EmailSettings emailSettings = new EmailSettings
            {
                SmtpHost = _configuration["EmailSettings:SmtpHost"],
                SmtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]),
                SmtpUseSSL = bool.Parse(_configuration["EmailSettings:SmtpUseSSL"]),
                SmtpUser = _configuration["EmailSettings:SmtpUser"],
                SmtpPassword = _configuration["EmailSettings:SmtpPassword"],
                FromName = _configuration["EmailSettings:FromName"]
            };
            // Send email
            await _emailSender.SendEmailAsync(
email,
"Confirm your email",
$@"
    <div style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;'>
        <div style='max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 8px; 
                    box-shadow: 0 2px 8px rgba(0,0,0,0.1); padding: 30px;'>
            <h2 style='color: #cda45e; text-align: center;'>Confirm Your Email</h2>
            <p style='color: #333; font-size: 16px; line-height: 1.6;'>
                    Please confirm your account by clicking the button below.
            </p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                   style='background-color: #cda45e; color: #fff; text-decoration: none; padding: 12px 25px;
                          border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;'>
                    Confirm My Email
                </a>
            </div>
            <p style='color: #777; font-size: 14px; text-align: center;'>
                If you didn’t create an account, you can ignore this message.
            </p>
        </div>
    </div>
    ", emailSettings);


            return RedirectToAction("RegisterConfirmation", new { email = email });
        }
       
        public async Task<IActionResult> ResetPassword()
        {
            return View();
        }
        public async Task<IActionResult> ConfirmResetPassword(ResetPasswordViewModel resetPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                User user = await userManager.FindByEmailAsync(resetPasswordViewModel.Email);
                if (user != null)
                {
                    var code = await userManager.GeneratePasswordResetTokenAsync(user);
                    var callbackUrl = Url.Action(
                        "ConfirmResetPassword",
                        "User",
                        new { userId = user.Id, code = code },
                        protocol: Request.Scheme);
                    EmailSettings emailSettings = new EmailSettings
                    {
                        SmtpHost = _configuration["EmailSettings:SmtpHost"],
                        SmtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]),
                        SmtpUseSSL = bool.Parse(_configuration["EmailSettings:SmtpUseSSL"]),
                        SmtpUser = _configuration["EmailSettings:SmtpUser"],
                        SmtpPassword = _configuration["EmailSettings:SmtpPassword"],
                        FromName = _configuration["EmailSettings:FromName"]
                    };

                    // Send email
                    await _emailSender.SendEmailAsync(
        resetPasswordViewModel.Email,
        "Confirm your email",
        $@"
    <div style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;'>
        <div style='max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 8px; 
                    box-shadow: 0 2px 8px rgba(0,0,0,0.1); padding: 30px;'>
            <h2 style='color: #cda45e; text-align: center;'>Confirm Your Email</h2>
            <p style='color: #333; font-size: 16px; line-height: 1.6;'>
                Thank you for registering! Please confirm your account by clicking the button below.
            </p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                   style='background-color: #cda45e; color: #fff; text-decoration: none; padding: 12px 25px;
                          border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;'>
                    Confirm My Email
                </a>
            </div>
            <p style='color: #777; font-size: 14px; text-align: center;'>
                If you didn’t create an account, you can ignore this message.
            </p>
        </div>
    </div>
    ", emailSettings);

                    // Don't sign in automatically - require email confirmation first
                    return RedirectToAction("RegisterConfirmation", new { email = resetPasswordViewModel.Email });

                }
                ModelState.AddModelError("", "Please Enter a valid Email");
            }
            return View("ResetPassword", resetPasswordViewModel);
        }


        public async Task<IActionResult> NewPassword(ResetPasswordEditViewModel resetPasswordEditViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(resetPasswordEditViewModel.userId);
                var result = await userManager.ResetPasswordAsync(user, resetPasswordEditViewModel.token, resetPasswordEditViewModel.NewPassword);

                if (result.Succeeded)
                {
                    // Password was successfully updated.
                    // You can now redirect the user to a success page or the login page.
                    return RedirectToAction("Login");
                }
            }
            return View("ResetPasswordEdit", resetPasswordEditViewModel);
        }
        [HttpGet]
        public async Task<IActionResult> LoginConfirmation(string Email)
        {
            var user = await userManager.FindByEmailAsync(Email);
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "User",
                new { userId = user.Id, code = code },
                protocol: Request.Scheme);

            // Send email
            await _emailSender.SendEmailAsync(
Email,
"Confirm your email",
$@"
    <div style='font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px;'>
        <div style='max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 8px; 
                    box-shadow: 0 2px 8px rgba(0,0,0,0.1); padding: 30px;'>
            <h2 style='color: #cda45e; text-align: center;'>Confirm Your Email</h2>
            <p style='color: #333; font-size: 16px; line-height: 1.6;'>
                Thank you for registering! Please confirm your account by clicking the button below.
            </p>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                   style='background-color: #cda45e; color: #fff; text-decoration: none; padding: 12px 25px;
                          border-radius: 5px; font-size: 16px; font-weight: bold; display: inline-block;'>
                    Confirm My Email
                </a>
            </div>
            <p style='color: #777; font-size: 14px; text-align: center;'>
                If you didn’t create an account, you can ignore this message.
            </p>
        </div>
    </div>
    ", emailSettings);

            // Don't sign in automatically - require email confirmation first
            return RedirectToAction("RegisterConfirmation", new { email = Email });

        }
    }
}
