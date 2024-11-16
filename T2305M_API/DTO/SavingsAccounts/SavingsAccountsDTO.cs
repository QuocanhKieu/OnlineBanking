using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using T2305M_API.Entities;


namespace T2305M_API.DTO.SavingsAccount
{
    public class GetDetailSavingsAccountDTO
    {
        public int SavingsAccountId { get; set; }  // ID of the savings account
        public string SavingsAccountCode { get; set; }  // Code for the savings account (e.g., unique identifier)
        public decimal Balance { get; set; }  // Amount in the savings account
        public decimal InterestRate { get; set; }  // Annual interest rate (as a percentage)

        public int Term { get; set; }  // Term of the savings (in months or years)
        public DateTime StartDate { get; set; }  // Date the savings account was created

        public DateTime? MaturityDate { get; set; }  // Date the account will mature
        public DateTime? WithdrawnDate { get; set; }  // Date the account will mature

        public string Status { get; set; }  // Status of the account (Active, Matured, Withdrawn, etc.)

        // Navigation property
    } 

    public class CreateSavingAccountDTO
    {
        [Required]
        [Range(500, (double)decimal.MaxValue, ErrorMessage = "Deposit amount must be at least 500 USD.")]
        public decimal DepositAmount { get; set; }
        [Required]
        public string SourceAccountNumber { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Term must be at least 1 month.")]
        public int Term { get; set; }  // Term of the savings (in months or years)
        [Required]
        public string Otp { get; set; } // Customer message
    }

public class CalculateInterestDTO
{
    [Required]
    [Range(500, (double)decimal.MaxValue, ErrorMessage = "Deposit amount must be at least 500 USD.")]
    public decimal DepositAmount { get; set; } // The principal deposit amount
    [Range(1, int.MaxValue, ErrorMessage = "Term in months must be at least 1.")]
    public int? Months { get; set; } // Optional: Term in months

    public DateTime StartDate { get; set; } = DateTime.Now; // Default to current date

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Term must be at least 1 month.")]
        public int Term { get; set; }  // Term of the savings (in months or years)
    }


    public class WithdrawFromSavingsDTO
    {
        [Required]
        public string SavingsAccountCode { get; set; }  // Code for the savings account (e.g., unique identifier)      
        [Required]
        public string DesAccountNumber { get; set; }
        [Required]
        public string Otp { get; set; } // Customer message
    }
    public class InterestRateRange
    {
        public int StartMonth { get; set; }  // The starting month of the range
        public int EndMonth { get; set; }    // The ending month of the range
        public decimal InterestRate { get; set; } // Interest rate for this range
    }


    public class SavingsAccountQueryParameters
    {
        public int? Userid { get; set; }
        const int maxPageSize = 50;
        public int Page { get; set; } = 1;
        private int _pageSize = 10;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value > maxPageSize ? maxPageSize : value; }
        }
        
        public string? SearchTerm { get; set; }
        public string? SortColumn { get; set; } = "Balance";
        public string? SortOrder { get; set; } = "desc";  // "asc" or "desc" for sorting order
    }
}
