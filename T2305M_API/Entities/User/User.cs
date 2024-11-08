using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace T2305M_API.Entities;

public partial class User
{
    [Key]
    public int UserId { get; set; } // auto-increment
    public string CustomerId { get; set; } // for login and display
    public string Email { get; set; }
    public string Password { get; set; }
    public string Phone { get; set; }
    public string? TransPassword { get; set; } // 8 pin password
    public string Name { get; set; }
    public string CitizenId { get; set; }
    public string CitizenIdFront { get; set; }
    public string CitizenIdRear { get; set; }
    public string? DigitalSignature { get; set; }// img
    public string Address { get; set; }
    public string Role { get; set; } = "USER"; // USER/ ADMIN /
    public string? Avatar { get; set; }
    public string Status { get; set; } = "ACTIVE";  // Account activation status ACTIVE/ LOCKED
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public int LoginAttempt { get; set; } = 0; // reset to 0 when successful login 
    public bool IsEmailConfirmed { get; set; } = false;
    public string? EmailConfirmationToken { get; set; }
    public string? PasswordResetToken { get; set; } // New field

    public string? OtpCode { get; set; }
    public DateTime? OtpExpiryTime { get; set; }//60s

    public virtual ICollection<Account>? Accounts { get; set; }  // Navigation property for many-to-many
    public virtual ICollection<Notification>? Notifications{ get; set; }  // Navigation property for many-to-many
    public virtual ICollection<ServiceRequest>? ServiceRequests { get; set; }  // Navigation property for many-to-many
    public virtual ICollection<CheckBook>? CheckBooks { get; set; }  // Navigation property for many-to-many
    public virtual ICollection<Check>? Checks { get; set; }  // Navigation property for many-to-many

}
