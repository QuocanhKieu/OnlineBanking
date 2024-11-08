using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using T2305M_API.Entities;


namespace T2305M_API.DTO.CheckBook
{
    public class GetDetailCheckBookDTO
    {
        public int? CheckBookId { get; set; }// auto-increment
        public string? CheckBookCode { get; set; } //= $"CheckBook{Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8)}";
        public string? AssociatedAccountNumber { get; set; } // References the account associated with this checkbook
        public int? TotalChecks { get; set; } // Total number of checks in the checkbook (e.g., 50, 100)
        public int? ChecksRemaining { get; set; } // Tracks how many checks are still available
        public DateTime? LastCheckClearedDate { get; set; }
        public string? LastClearedCheckCode { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalClearedCheckAmount { get; set; } //=0;
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string? DeliveryAddress { get; set; }
        public DateTime? ExpiryDate { get; set; } //= DateTime.UtcNow.AddMonths(6);
        public string? Status { get; set; } //= "PENDING"; // Status of the checkbook (e.g., "PENDING", "CONFIRMED", "Dispatched", "Delivered", "ACTIVE", "Cancelled", "LOCKED", "STOPPED", "EXHAUSTED", "CLOSED")
    }

    public class CreateCheckBookDTO
    {
        [Required]
        public string AccountNumber { get; set; } // References the account associated with this checkbook
        [Required]
        public string DeliveryAddress { get; set; }
        public string? DigitalSignatureUrl { get; set; }
    }

    public class VerifyCheckbookAndAccountRequirements
    {
        [Required]
        public string checkBookCode { get; set; } // References the account associated with this checkbook
        [Required]
        public decimal amountToSubtract { get; set; }
    }

    public class RejectCheckRequestModel
    {
        [Required]
        public string CheckCode { get; set; }
        [Required]
        public string Note { get; set; }
    }

    public class ProcessCheckRequirements
    {
        [Required]
        public string CheckCode { get; set; } // References the account associated with this checkbook
        [Required]
        public string OnCheckAccountNumber { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
        public decimal AmountOnCheck { get; set; } 
        [Required]
        public string DesAccountNumber { get; set; }
        [Required]
        public string CheckFrontImageUrl { get; set; }
    }

    public class StopCheckbookRequest
    {
        public string CheckBookCode { get; set; }
    }


    public class CheckBookQueryParameters
    {
        const int maxPageSize = 50;
        private int _pageSize = 10;
        public int Page { get; set; } = 1;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value > maxPageSize ? maxPageSize : value; }
        }
        public int? UserId { get; set; }
        public string? AccountNumber { get; set; }
        public string? SortColumn { get; set; } = "StatusChangedDate";
        public string? SortOrder { get; set; } = "DESC";  // "asc" or "desc" for sorting order
    }
}
