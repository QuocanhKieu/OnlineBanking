using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace T2305M_API.Entities
{
    public class Transaction // nếu chuyển tiền thất bại thì chỉ có 1 transaction đc tạo phía người gửi
    {
        [Key]
        public int TransactionId { get; set; } // auto-increment
        public int UserId { get; set; }// transaction của người nào
        public string TransactionType { get; set; } // INFLOW, OUTFLOW//tùy vào context
        public int FromAccountId { get; set; }
        public int? ToAccountId { get; set; }
        public string FromAccountNumber { get; set; }
        public string? ToAccountNumber { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
        public string Status { get; set; } //SUCESS, FAILED

        // Navigation property
        public virtual Account? FromAccount { get; set; }
        public virtual Account? ToAccount { get; set; }
        public virtual User? User { get; set; }// chủ sở hữu// xác định transaction này nhất định phải có dù thất bại hay thành công
    }
}
