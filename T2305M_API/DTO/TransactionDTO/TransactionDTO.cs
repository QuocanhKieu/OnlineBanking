using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using T2305M_API.DTO.Event;
using T2305M_API.Entities;


namespace T2305M_API.DTO.Transaction
{
    //Basic/Detail with the same DTO
    public class GetBasicTransactionDTO
    {
        public int TransactionId { get; set; } // auto-increment
        public string TransactionType { get; set; } // INFLOW, OUTFLOW//tùy vào context
        public string FromAccountNumber { get; set; }
        public string? ToAccountNumber { get; set; }
        public string FromUserName { get; set; } 
        public string? ToUserName { get; set; } 
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
        public string Status { get; set; } //SUCESS, FAILED
    }

    public class CreateTransactionDTO
    {
        public int UserId { get; set; } // for correctly associating transaction
        public string TransactionType { get; set; } // INFLOW, OUTFLOW//tùy vào context//khác nhau
        public string FromAccountNumber { get; set; }
        public string? ToAccountNumber { get; set; }
        public int FromAccountId { get; set; }
        public int? ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }// khác nhau// nhớ cho từng account thụ hưởng
        public string? Description { get; set; }
        public string Status { get; set; } //SUCESS, FAILED
    }

    public class TransactionQueryParameters
    {
        public int? Userid { get; set; }
        public string? AccountNumber { get; set; }
        const int maxPageSize = 50;
        public int Page { get; set; } = 1;
        private int _pageSize = 10;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value > maxPageSize ? maxPageSize : value; }
        }
        public string? SearchTerm { get; set; }
        public string? TransactionType { get; set; } // INFLOW, OUTFLOW//tùy vào context//khác nhau
        public string? Period { get; set; } // 7DAYS, 1MONTH, 3MONTH, 6MONTH, 9MONTH, 12MONTH
        public DateTime? StartDate {  get; set; }
        public DateTime? EndDate {  get; set; }
        public string? SortColumn { get; set; } = "TransactionDate";
        public string? SortOrder { get; set; } = "desc";  // "asc" or "desc" for sorting order
    }
}
