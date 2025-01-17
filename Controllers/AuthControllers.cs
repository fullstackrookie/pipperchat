using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Security.Cryptography;
using BCrypt.Net;
using System.Text.RegularExpressions;
using PipperChat.Models;
using PipperChat.DTOs;
using PipperChat.Data;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace PiperChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly PipperChatContext _context;
        private readonly ILogger<AuthController> _logger;

        // Constructors for dependencies
        public AuthController(PipperChatContext context, ILogger<AuthController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper method to validate email format
        private bool IsValidEmail(string email)
        {
            // Using a standard email regex pattern
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
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage) });
                }

                // Additional validation for email format
                if (!IsValidEmail(userDto.Email))
                {
                    return BadRequest(new { Error = "Invalid email format" });
                }

                // Additional password strength validation
                var passwordValidation = ValidatePassword(userDto.Password);
                if (!passwordValidation.IsValid)
                {
                    return BadRequest(new { Error = passwordValidation.ErrorMessage });
                }

                // Check if email is already taken (case-insensitive)
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == userDto.Email.ToLower()))
                {
                    return Conflict(new { Error = "Email already registered" });
                }

                // Create new user
                var user = new PipperChat.Data.User
                {
                    Email = userDto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                    CreatedAt = DateTime.UtcNow
                };

                // Add user to database
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // Log successful registration
                _logger.LogInformation($"User registered successfully with email: {user.Email}");

                return Ok(new { Message = "Registration successful" });
            }
            catch (Exception ex)
            {
                // Log the error
                _logger.LogError(ex, "Error occurred during user registration");
                return StatusCode(500, new { Error = "An internal server error occurred during registration" });
            }
        }
    }
}
