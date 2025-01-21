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
using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel.Tokens;
using System.Text;
using System.Security.Principal;
using   System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;
using System.Reflection.Metadata.Ecma335;

namespace PipperChat.Controllers
{
    // Request DTOs
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public  interface   IPasswordService
    {
        void    CreatePasswordHash(string   password,   out string  PasswordHash,   out string  PasswordSalt);
        bool    VerifyPasswordHash(string   password,   string  storedHash, string  storedSalt);
    }

    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly PipperChatContext _context;
        private readonly   IPasswordService _passwordService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _enviroment;
        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtExpiryMinutes;
         

        // Constructor
        public AuthController(
            PipperChatContext context,
            ILogger<AuthController> logger,
            IConfiguration configuration,
            IHostEnvironment enviroment,
            IPasswordService    passwordService)
        {
           _passwordService =   _passwordService    ??  throw   new ArgumentNullException(nameof(passwordService));
           _context =   context ??  throw   new ArgumentNullException(nameof(context));
           
           //   Token
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _enviroment = enviroment ?? throw new ArgumentNullException(nameof(enviroment));

            // Initialize Jwt settings from configurations
            _jwtKey = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key configuration is missing");
            _jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer configuration is missing");
            _jwtAudience = _configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience configuration is missing");

            // Correct way to parse the expiry time
            var expiryMinutes = _configuration["Jwt:ExpiryMinutes"];
            _jwtExpiryMinutes = int.TryParse(expiryMinutes, out var minutes) ? minutes : 60; // Default to 60 if parsing fails
        }
        //  Helper  method  to generate Jwt token
        private (string Token,  DateTime    Expiration) GenerateJwtToken(User   user)
        {
            var claims  =   new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,    user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };
            var key =   new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var credentials =   new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token   =   new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience:   _jwtAudience,
                claims: claims,
                expires:    DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes),
                signingCredentials: credentials
            );

            var tokenString  =   new     JwtSecurityTokenHandler().WriteToken(token);
            return  (tokenString,   token.ValidTo);
        }

        //  Helper method   to generate a   refresh token
        private string  GenerateRefreshToken()
        {
            var randomNumberBytes    =   new byte[32];
            
            RandomNumberGenerator.Fill(randomNumberBytes);

            return  Convert.ToBase64String(randomNumberBytes);
        }

        [HttpPost("register")]
        public  async   Task<IActionResult> Register([FromBody] UserRegistrationDto registerDto)
        {
            if(!ModelState.IsValid)
                return  BadRequest(ModelState);

            //  Hash    the password
            string  passwordHash;
            string  passwordSalt;
            
            _passwordService.CreatePasswordHash(registerDto.Password,   out passwordHash,    out passwordSalt);

            var user    =   new User
            {
                Username    =   registerDto.Username,
                Email   =   registerDto.Email,
                PasswordHash    =   passwordHash,
                PasswordSalt =   passwordSalt

            };

            _context.Users.Add(user);
            await   _context.SaveChangesAsync();

            return  Ok(new {    message =       "User   regidtered  successfully"});
        }
        // Login endpoint
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Input validation
                if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                {
                    return BadRequest(new { Error = "Email and password are required" });
                }

                // Normalize email for comparison
                var normalizedEmail = loginDto.Email.Trim().ToLowerInvariant();

                // Find user with email tracking
                _logger.LogWarning($"Login attempt for email: {normalizedEmail}");
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

                if (user == null)
                {
                    _logger.LogWarning($"Login failed - user not found: {normalizedEmail}");
                    return Unauthorized(new { Error = "Invalid credentials" });
                }

                //  Verify  user and Passowrd
                if  (   user    ==   null    ||  !_passwordService.VerifyPasswordHash(
                    loginDto.Password,
                    user.PasswordHash   ??  string.Empty,
                    user.PasswordSalt   ??  string.Empty ))
                    {
                        _logger.LogWarning($"Login  failed  -   invalid credentials:    {normalizedEmail}");
                        return  Unauthorized(new    {   Error   =   "Invalid    credentials"});
                    }

                 // Generate tokens
                var (token, tokenExpiration) = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                // Update user's refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Login successful for user: {normalizedEmail}");

                // Return success response with tokens
                return Ok(new
                {
                    Token = token,
                    TokenExpiration = tokenExpiration,
                    RefreshToken = refreshToken,
                    User = new
                    {
                        Id = user.Id,
                        Email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login process");
                return StatusCode(500, new { Error = "An unexpected error occurred" });
            }
        }
        

        // Google login endpoint
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

        // Google callback endpoint
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
                        Email = email ?? throw new ArgumentNullException(nameof(email)),
                        GoogleId = GoogleId ?? throw new ArgumentNullException(nameof(GoogleId)),
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

        // Get authentication status
        [HttpGet("status")]
        public IActionResult GetAuthStatus()
        {
            if (User?.Identity?.IsAuthenticated == true)
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

        // Logout endpoint
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
    }
}
