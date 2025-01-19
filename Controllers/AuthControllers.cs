using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PipperChat.DTOs;
using PipperChat.Data;
using PipperChat.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.RegularExpressions; 


namespace PipperChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly PipperChatContext _context;
        private readonly ILogger<AuthController> _logger;

        // This endpoint initiates the Google OAuth flow
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback)),
                Items =
                {
                    { "LoginProvider", "Google" },
                    { "returnUrl", "/" }
                }
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // The endpoint handles the callback from Google
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                if (!authenticateResult.Succeeded)
                {
                    _logger.LogWarning("External authentication failed");
                    return Unauthorized(new { Error = "External authentication failed" });
                }

                var email = authenticateResult.Principal?.FindFirstValue(ClaimTypes.Email);
                var name = authenticateResult.Principal?.FindFirstValue(ClaimTypes.Name);
                var GoogleId = authenticateResult.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    user = new User
                    {
                        Email = email   ??  throw   new ArgumentNullException(nameof(email)),
                        GoogleId = GoogleId ??  throw   new ArgumentNullException(nameof(GoogleId)),
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.Users.AddAsync(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"New user created from Google authentication: {email}");
                }
                else if (user.GoogleId != GoogleId)
                {
                    user.GoogleId = GoogleId;
                    await _context.SaveChangesAsync();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("LoginProvider", "Google")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTime.UtcNow.AddDays(30)
                    });

                _logger.LogInformation($"User authenticated successfully with Google: {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Google authentication callback");
                return StatusCode(500, new { Error = "An error occurred during authentication" });
            }

            return Redirect("/");  // Adjust later to frontend URL
        }

        // This endpoint checks if the user is currently authenticated
        [HttpGet("status")]
        public IActionResult GetAuthStatus()
        {
            if (User?.Identity?.IsAuthenticated ==  true)
            {
                return Ok(new
                {
                    IsAuthenticated = true,
                    Email = User.FindFirstValue(ClaimTypes.Email),
                    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    LoginProvider = User.FindFirstValue("LoginProvider")
                });
            }

            return Unauthorized(new { Error = "User is not authenticated" });
        }

        // This endpoint handles user logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("User logged out successfully");
                return Ok(new { Message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout");
                return StatusCode(500, new { Error = "An error occurred during logout" });
            }
        }

        // Constructors for dependencies
        public AuthController(PipperChatContext context, ILogger<AuthController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, pattern);
        }

        // Helper method to validate password strength
        private (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return (false, "Password cannot be empty");

            if (password.Length < 8)
                return (false, "Password must be at least 8 characters long");

            if (!password.Any(char.IsUpper))
                return (false, "Password must contain at least one uppercase letter");

            if (!password.Any(char.IsLower))
                return (false, "Password must contain at least one lowercase letter");

            if (!password.Any(char.IsDigit))
                return (false, "Password must contain at least one number");

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
                return (false, "Password must contain at least one special character");

            return (true, string.Empty);
        }

        // Register API endpoint
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto userDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage) });
                }

                if (!IsValidEmail(userDto.Email))
                {
                    return BadRequest(new { Error = "Invalid email format" });
                }

                var passwordValidation = ValidatePassword(userDto.Password);
                if (!passwordValidation.IsValid)
                {
                    return BadRequest(new { Error = passwordValidation.ErrorMessage });
                }

                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == userDto.Email.ToLower()))
                {
                    return Conflict(new { Error = "Email already registered" });
                }

                var user = new User
                {
                    Email = userDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User registered successfully with email: {user.Email}");

                return Ok(new { Message = "Registration successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration");
                return StatusCode(500, new { Error = "An internal server error occurred during registration" });
            }
        }
    }
}