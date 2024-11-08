using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace T2305M_API.Entities
{
    public class CheckBook
    {
        [Key]
        public int CheckBookId { get; set; }// auto-increment
        public string CheckBookCode { get; set; } //= $"CheckBook{Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8)}";
        public int AccountId { get; set; } // References the account associated with this checkbook
        public virtual Account? Account { get; set; } // Navigation property to Account
        public int UserId { get; set; } // References the account associated with this checkbook
        public virtual User? User { get; set; } // Navigation property to Account
        public int TotalChecks { get; set; } // Total number of checks in the checkbook (e.g., 50, 100)
        public int ChecksRemaining { get; set; } // Tracks how many checks are still available
        public DateTime? LastCheckClearedDate { get; set; } 
        public string? LastClearedCheckCode { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalClearedCheckAmount { get; set; } //=0;
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string DeliveryAddress { get; set; }
        public DateTime ExpiryDate { get; set; } //= DateTime.UtcNow.AddMonths(6);
        public string? Status { get; set; } //= "PENDING"; // Status of the checkbook (e.g., "Pending", "Approved", "Dispatched", "Delivered", "WORKING" ,"CANCELED", "LOCKED", "STOPPED", "EXHAUSTED", "CLOSED")
        public DateTime? StatusChangedDate { get; set; } //= DateTime.Now;    // Date the checkbook request was approved

        public virtual ICollection<Check>? Checks { get; set; } // Incoming transactions

        //public DateTime? DispatchedDate { get; set; } // Date the checkbook was dispatched
        //public DateTime? DeliveredDate { get; set; } // Date the checkbook was delivered to the customer
        //public DateTime? CancelledDate { get; set; } // Date the checkbook was delivered to the customer
        //public DateTime? LOCKEDDate { get; set; } // Date the checkbook was delivered to the customer
        //public DateTime? STOPPEDDate { get; set; } // Date the checkbook was delivered to the customer

        // Collection of checks that belong to this checkbook
    }

}
