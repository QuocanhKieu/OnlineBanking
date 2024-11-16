using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace T2305M_API.Entities
{
    public class Transaction 
    {
        [Key]
        public int TransactionId { get; set; } // auto-increment
        public string? TransactionCode { get; set; }  // Adjust the type if needed
        public int? SourceAccountId { get; set; }
        public int? DesAccountId { get; set; }
        public string TransactionType{ get; set; } // BANKTRANSFER, CHECKPAYMENT
        public string? SourceAccountNumber { get; set; }
        public string? DesAccountNumber { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SourceAccountBalanceAfter { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DesAccountBalanceAfter { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string? TransactionDescription { get; set; }
        public string? TransactionMessage { get; set; }


        // Navigation property
        public virtual Account? SourceAccount { get; set; }
        public virtual Account? DesAccount { get; set; }
    }
}
