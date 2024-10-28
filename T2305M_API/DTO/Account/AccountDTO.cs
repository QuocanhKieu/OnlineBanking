using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using T2305M_API.DTO.Event;
using T2305M_API.Entities;


namespace T2305M_API.DTO.Account
{
    public class GetBasicAccountDTO
    {
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; }
        public bool isDefault { get; set; }
    }

    public class GetDetailAccountDTO
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public string Status { get; set; }
        public bool isDefault { get; set; }
    }
 
    public class CheckToAccountNumber
    {
        [Required]
        [MaxLength(14)]
        public string ToAccountNumber { get; set; }

    }

    public class CheckBalance
    {
        [Required]
        [MaxLength(14)]
        public string AccountNumber { get; set; }
        [Required]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Money Amount must be greater than zero.")]
        public decimal MoneyAmount { get; set; }

    }

    public class MoneyTransfer
    {
        [Required]
        [MaxLength(14)]
        public string FromAccountNumber { get; set; }
        [Required]
        [MaxLength(14)]
        public string ToAccountNumber { get; set; }

        [Required]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Money Amount must be greater than zero.")]
        public decimal MoneyAmount { get; set; }

        [Required]
        [MaxLength (8)]
        public string TransPassword { get; set; } // 8 pin password
    }
    public class UpdateAccountAddressDTO
    {
        [Required]
        public string Address { get; set; }
    }
    public class CreateAccountDTO
    {
        [Required]
        [MaxLength(14)]
        public string AccountNumber { get; set; }
    }
    public class CheckDuplicateAccountDTO 
    {
        [Required]
        [MaxLength(14)]
        public string AccountNumber { get; set; }
    }
    public class AccountQueryParameters
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
