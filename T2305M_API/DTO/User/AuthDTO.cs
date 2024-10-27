using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace T2305M_API.DTO.User
{
    public class LoginModel
    {
        [Required]
        public string CustomerId { get; set; } // phone/ email/ cusomerId
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
    }
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MaxLength(20)]
        public string CitizenId { get; set; } // Unique identification code    }
    }
    public class ChangePasswordModel // both for token and non-token but authorised approach
    {
        public string? Token { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
        ErrorMessage = "Password must be at least 6 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string NewPassword { get; set; }
    }

    public class UserRegisterDTO
    {
        [Required]
        [MaxLength(50)]
        public string CustomerId { get; set; } // For login and display

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
        ErrorMessage = "Password must be at least 6 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(20)]
        public string CitizenId { get; set; } // Unique identification code

        public string CitizenIdFront { get; set; } // URL for front image of the identification document

        public string CitizenIdRear { get; set; } // URL for rear image of the identification document

        [Required]
        [MaxLength(255)]
        public string Address { get; set; }
        [Required]
        [MaxLength(14)]
        [MinLength(10)]
        public string AccountNumber { get; set; } // acc mặc định ban đầu
    }

    public class AuthResponse
    {
        public string Jwt { get; set; }
        public DateTime Expired_at { get; set; }
        public string Role { get; set; }
    }
}

