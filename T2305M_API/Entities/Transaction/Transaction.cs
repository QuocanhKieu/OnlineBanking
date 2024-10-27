using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace T2305M_API.Entities
{
   

    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; } // auto-increment
        public int AccountId { get; set; }
        public int UserId { get; set; }

        [Required]
        public string TransactionType { get; set; } // CREDIT, DEBIT

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; }

        [MaxLength(50)]
        public string? RecipientAccount { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }
        public string Status { get; set; } //SUCESS, FAILED

        // Navigation property
        public virtual Account Account { get; set; }
        public virtual User User { get; set; }
    }
}
