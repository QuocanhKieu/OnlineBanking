using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using T2305M_API.Entities;


namespace T2305M_API.DTO.Transaction
{
    //Basic/Detail with the same DTO
    public class GetBasicTransactionDTO
    {
        public string TransactionType { get; set; } // BANKTRANSFER, CHECKPAYMENT
        public string? SourceAccountNumber { get; set; }
        public string? DesAccountNumber { get; set; }
        public int? SourceAccountId { get; set; }
        public int? DesAccountId { get; set; }
        public string? SourceUserName { get; set; } 
        public string? DesUserName { get; set; } 
        public decimal Amount { get; set; }
        public decimal? BalanceAfter { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? TransactionDescription { get; set; }
        public string? TransactionMesage { get; set; }
        public string? TransactionCode { get; set; }
    }

    public class CreateTransactionDTO
    {
        public string TransactionCode { get; set; } = $"{Guid.NewGuid().ToString("N").ToUpper().Substring(0, 16)}";
        public string TransactionType { get; set; } // BANKTRANSFER, CHECKPAYMENT
        public string? SourceAccountNumber { get; set; }
        public string? DesAccountNumber { get; set; }
        public int? SourceAccountId { get; set; }
        public int? DesAccountId { get; set; }
        public decimal Amount { get; set; }
        public decimal? SourceAccountBalanceAfter { get; set; }
        public decimal? DesAccountBalanceAfter { get; set; }
        public string? TransactionDescription { get; set; }
        public string? TransactionMessage { get; set; }
    }
    public class AfterSuccessTransactionDTO
    {
        public string? TransactionCode { get; set; }  // Adjust the type if needed
        public string SourceAccountNumber { get; set; }
        public string TransactionType { get; set; } // BANKTRANSFER, CHECKPAYMENT
        public string? DesAccountNumber { get; set; }
        public string? DesAccountOwnerName { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; } = 0;
        public string? TransactionMessage { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
    public class TransactionQueryParameters
    {
        public string AccountNumber { get; set; }
        public int UserId { get; set; }
        const int maxPageSize = 50;
        public int Page { get; set; } = 1;
        private int _pageSize = 10;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value > maxPageSize ? maxPageSize : value; }
        }
        public string? SearchTerm { get; set; }
        public string? MoneyFlow { get; set; } // IN , OUT
        public string? TransactionType { get; set; } // BANKTRANSFER, CHECKPAYMENT
        public string? Period { get; set; } // 7DAY, 1MONTH, 3MONTH, 6MONTH, 9MONTH, 12MONTH
        public DateTime? StartDate {  get; set; }
        public DateTime? EndDate {  get; set; }
        public string? SortColumn { get; set; } = "TransactionDate";
        public string? SortOrder { get; set; } = "desc";  // "asc" or "desc" for sorting order
    }
}
