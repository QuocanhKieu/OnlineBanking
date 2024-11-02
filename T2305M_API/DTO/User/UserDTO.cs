using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using T2305M_API.Entities;


namespace T2305M_API.DTO.User
{
    public class GetBasicUserDTO
    {
        [Key]
        public int UserId { get; set; }
        public bool IsActive { get; set; } = true;  // Account activation status
    }

    public class GetDetailUserDTO
    {
        public string CustomerId { get; set; } // for login and display
        public string Phone { get; set; }
        public string Name { get; set; }
        public string CitizenId { get; set; }
        public string CitizenIdFront { get; set; }
        public string CitizenIdRear { get; set; }
        public string? DigitalSignature { get; set; }// img
        public string Address { get; set; }
        public string? Avatar { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
   
    public class TransPasswordDTO
    {
        [Required]
        [MaxLength(8)]
        [MinLength(8)]
        [RegularExpression(@"^(?!([0-9])\1{7})\d{8}$", ErrorMessage = "Password must be exactly 8 digits and cannot consist of the same digit repeated.")]
        public string TransPassword { get; set; } // 8 pin password
    }
    public class UpdateUserDTO 
    {
        //public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Education { get; set; }  // This can be a simple string or a list of degrees
        public string? ShortBiography { get; set; }
        [Column(TypeName = "NVARCHAR(MAX)")] // or "VARCHAR(MAX)" if you prefer
        public string? LongBiography { get; set; }
        public int? Age { get; set; }


        // Social Media Accounts
        public string? Facebook { get; set; }
        public string? LinkedIn { get; set; }
        public string? Twitter { get; set; }

        // Personal Website
        public string? PersonalWebsiteUrl { get; set; }
        public string? PersonalWebsiteTitle { get; set; }
        public bool ReceiveNotifications { get; set; } = true;
    }
    //for the photoUrl has specific method for them


    public class UpdateUserAddressDTO
    {
        [Required]
        public string Address { get; set; }
    }


    public class UserQueryParameters
    {
        const int maxPageSize = 50;
        public int Page { get; set; } = 1;
        private int _pageSize = 10;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value > maxPageSize ? maxPageSize : value; }
        }
        public bool isToday { get; set; }
        public bool isThisWeek { get; set; }
        public bool isNextWeek { get; set; }
        public bool IsHostOnline { get; set; }
        public string? Country { get; set; }
        public string? Continent { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; } = "asc";  // "asc" or "desc" for sorting order
    }
}
