using Microsoft.AspNetCore.Mvc;
using T2305M_API.Entities;
using T2305M_API.DTO.User;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using T2305M_API.Services.Implements;
namespace T2305M_API.Controllers
{
    [ApiController]
    [Route("/api/auth")]
    public class AuthController : Controller
    {
        private readonly T2305mApiContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;
        private readonly EmailService _emailService;

        public AuthController(T2305mApiContext context,
            IConfiguration configuration,
            ILogger<AuthController> logger,
             EmailService emailService)
        {
            _context = context;
            _config = configuration;
            _logger = logger;
            _emailService = emailService;

        }

        private string GenJWT(User user)
        {
            string key = _config["JWT:Key"];
            string issuerX = _config["JWT:Issuer"];
            string audienceX = _config["JWT:Audience"];
            int lifeTime = Convert.ToInt32(_config["JWT:Lifetime"]);

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var signatureKey = new SigningCredentials(secretKey,
                            SecurityAlgorithms.HmacSha256);
            var payload = new[] {
                new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.Role,user.Role)
            };

            var token = new JwtSecurityToken(
                    issuer: issuerX,
                    audience: audienceX,
                    payload,
                    expires: DateTime.Now.AddMinutes(lifeTime),
                    signingCredentials: signatureKey
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(UserRegisterDTO model)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Please input valid user information."
                    });
                }

                // Check if the email is already registered
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        code = 2,
                        message = "Email is already registered.",
                    });
                }

                existingUser = await _context.Users.FirstOrDefaultAsync(u => u.CustomerId == model.CustomerId);

                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        code = 3,
                        message = "CustomerId is already registered."
                    });
                }
                existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Phone == model.Phone);

                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        code = 4,
                        message = "Phone is already registered."
                    });
                }

                existingUser = await _context.Users.FirstOrDefaultAsync(u => u.CitizenId == model.CitizenId);

                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        code = 5,
                        message = "CitizenId is already registered."
                    });
                }

                var existingAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.AccountNumber == model.AccountNumber);

                if (existingAccount != null)
                {
                    return BadRequest(new
                    {
                        code = 6,
                        message = "AccountNumber is already registered."
                    });
                }

                // Hash the password
                string salt = BCrypt.Net.BCrypt.GenerateSalt(12); // Use a higher work factor
                string pwd_hashed = BCrypt.Net.BCrypt.HashPassword(model.Password, salt);

                // Create new user entity
                User user = new User
                {
                    Email = model.Email,
                    CustomerId = model.CustomerId,
                    Password = pwd_hashed,
                    Address = model.Address,
                    Phone = model.Phone,
                    Name = model.Name,
                    CitizenId = model.CitizenId,
                    CitizenIdFront = model.CitizenIdFront,
                    CitizenIdRear = model.CitizenIdRear,
                    EmailConfirmationToken = Guid.NewGuid().ToString(), // Generate a unique token
                };
                // Create new user entity
                // Create the Account entity associated with the User
                Account account = new Account
                {
                    User = user, // Assuming there's a relationship between User and Account
                    AccountNumber = model.AccountNumber,
                    IsDefault = true
                };
                // Add and save both the User and Account entities in a single transaction
                await _context.Users.AddAsync(user);
                await _context.Accounts.AddAsync(account);
                await _context.SaveChangesAsync();

                // Send confirmation email
                SendConfirmationEmail(user.Email, user.EmailConfirmationToken);

                // Return a response indicating that a confirmation email was sent
                return Ok(new
                {
                    Message = "Registration successful. A confirmation email has been sent to your email address.",
                    Email = user.Email // Optionally return the email
                });
            }
            catch (Exception e)
            {
                // Log the error and return a generic message
                _logger.LogError(e, "Error registering user");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        private async void SendConfirmationEmail(string email, string token)
        {
            // Base URL construction
            //var baseUrl = _config["ClientUrl"]; // Get the frontend base URL from configuration

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            // Confirmation route
            var confirmRoute = "/api/auth/confirm-email";

            // Construct the full confirmation URL
            var confirmationUrl = $"{baseUrl}{confirmRoute}?email={email}&token={token}";

            // Email content
            var subject = "Confirm your email address";
            var body = $@"
        <p>Dear User,</p>
        <p>Thank you for registering! Please confirm your email by clicking the link below:</p>
        <a href='{confirmationUrl}'>Confirm Email</a>
        <p>If you did not register for this account, please ignore this email.</p>";

            // Send the email (this method depends on your email sending library/service)
            //string templateFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Templates", "EmailTemplate", "UserArticleSubmit.html");

            //var placeholders = new Dictionary<string, string>
            //        {
            //            { "{title}", "Congratulations! We received you Story, will take a review, and approve soon!" },
            //            { "{userName}", userEmailClaim},
            //        };

            //// List of dynamic ticket codes

            //// Send email
            //await _emailService.SendEmailTemplateAsync(
            //    to: userEmailClaim,
            //    subject: "Your Story has been submitted! Thank you!",
            //    templateFilePath: templateFilePath,
            //    placeholders: placeholders,
            //    ticketCodes: new List<string>()
            //);

            await _emailService.SendEmailAsync(email, subject, body);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            // Validate the user ID and token
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || user.EmailConfirmationToken != token)
            {
                return BadRequest("Invalid confirmation link.");
            }

            // Mark user as confirmed
            user.IsEmailConfirmed = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok("Email confirmed successfully!");
        }


        [HttpPost]
        [Route("login")]
        public IActionResult Login(LoginModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Invalid input",
                    });
                User user = _context.Users.Where(
                        u => u.CustomerId.Equals(model.CustomerId) || u.Email.Equals(model.CustomerId) || u.Phone.Equals(model.CustomerId)).FirstOrDefault();
                if (user == null)
                    return Unauthorized(new
                    {
                        code = 2,
                        message = "Invalid Credential Information.",
                    });

                if (user.Status == "LOCKED")
                {
                    return Unauthorized(new
                    {
                        code = 3,
                        message = "Your Account is Locked, Please Contact Us for more infomation.",
                    });
                }
                bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);

                if (!isPasswordCorrect)
                {
                    user.LoginAttempt += 1;
                    if (user.LoginAttempt >= 3)
                    {
                        // Lock the account
                        user.Status = "LOCKED";
                        _context.SaveChanges();

                        // Send notification email
                        SendAccountLockedEmail(user.Email);

                        return Unauthorized(new
                        {
                            code = 3,
                            message = "Your Account is Locked, Please Contact Us for more infomation.",
                        });
                    }

                    _context.SaveChanges(); // Save login attempt changes
                    return Unauthorized(new
                    {
                        code = 2,
                        message = "Invalid Credential Information.",
                    });
                }

                // Successful login: reset login attempts and proceed
                user.LoginAttempt = 0; // Reset login attempts
                _context.SaveChanges();
                if (!user.IsEmailConfirmed)
                {
                    SendConfirmationEmail(user.Email, user.EmailConfirmationToken);
                    return BadRequest(new
                    {
                        code = 4,
                        message = "You have not verified you Gmail, Please verify it first!"
                    });

                }
                return Ok(new AuthResponse
                {
                    Jwt = GenJWT(user),
                    Expired_at = DateTime.Now.AddMinutes(Convert.ToInt32(_config["JWT:Lifetime"])),
                    Role = user.Role
                });
            }
            catch (Exception e)
            {
                return Unauthorized(e.Message);
            }
        }

        private async void SendAccountLockedEmail(string email)
        {
            // Email content
            var subject = "Sadly, Your Account is temporarily locked";
            var body = $@"
        <p>Dear User,</p>
        <p>Sadly, Your Account is temporarily locked due to multiple failed login attemps. For more infomation, please contact us via.</p>
        <p>If you did not recognize this email, please ignore it.</p>";

            await _emailService.SendEmailAsync(email, subject, body);
        }


        [HttpPost]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Invalid format input",
                    });
                // Check if the user exists
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.CitizenId == model.CitizenId);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        code = 2,
                        message = "Invalid Credential Information",
                    });
                }

                // Generate reset token and maybe expiry
                user.PasswordResetToken = Guid.NewGuid().ToString();

                // Save changes to the user
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Send reset email
                SendPasswordResetEmail(user.Email, user.PasswordResetToken);

                return Ok("A password reset email has been sent to your registered email.");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in forgot password");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        private async void SendPasswordResetEmail(string email, string token)
        {
            //var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            var baseUrl = _config["ClientUrl"]; // Get the frontend base URL from configuration
            var resetPasswordRoute = "/api/auth/reset-password";
            var resetLink = $"{baseUrl}{resetPasswordRoute}?token={token}";

            // Email content
            var subject = "Request to reset password";
            var body = $@"
        <p>Dear User,</p>
        <p>Thank you for using our system! Please click the follwing link to reset you password:</p>
        <a href='{resetLink}'>Reset Password</a>
        <p>If you did not request this, please ignore this email.</p>";


            await _emailService.SendEmailAsync(email, subject, body);
        }

        [HttpPost]
        [Route("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            try
            {
                // Check if the model is valid
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        code = 1,
                        message = "Invalid input"
                    });
                }

                // Check if user is authenticated
                if (User.Identity.IsAuthenticated)
                {
                    // Authenticated user, token is not required
                    var authenticatedUserId = User.FindFirst("UserId")?.Value;
                    var user = await _context.Users.FindAsync(authenticatedUserId);

                    if (user == null)
                    {
                        return Unauthorized(new
                        {
                            code = 2,
                            message = "User is not authorized to change password."
                        });
                    }

                    // Proceed with password change without needing a token
                    string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                    user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, salt);
                    await _context.SaveChangesAsync();

                    return Ok("Your password has been changed successfully.");
                }
                else
                {
                    // Unauthenticated user, token is required
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == model.Token);
                    if (user == null)
                    {
                        return BadRequest(new
                        {
                            code = 3,
                            message = "Invalid or expired password change token."
                        });
                    }

                    // Hash the new password and update user
                    string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                    user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, salt);
                    user.PasswordResetToken = null; // Clear the reset token

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    return Ok("Your password has been changed successfully.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error resetting password");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


    }



}

