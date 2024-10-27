using Castle.Core.Resource;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace T2305M_API.Entities

{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Account
    {
        [Key]
        public int AccountId { get; set; } // auto-increment
        public int UserId { get; set; }
        public bool isDefault { get; set; } = false;
        public string AccountNumber { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Status { get; set; } = "ACTIVE";// ACTIVE, LOCKED
        public virtual User? User{ get; set; }
        public virtual ICollection<Transaction>? Transactions { get; set; }  // Navigation property for many-to-many
        public virtual ICollection<CheckBook>? CheckBooks { get; set; }  // Navigation property for many-to-many
        public virtual ICollection<Check>? Checks { get; set; }  // Navigation property for many-to-many
    }
}