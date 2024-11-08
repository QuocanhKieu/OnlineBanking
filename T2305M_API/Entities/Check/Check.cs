using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T2305M_API.Entities
{
    public class Check
    {
        [Key]
        public int CheckId { get; set; } // auto-increment
        public string CheckCode { get; set; } = $"C{Guid.NewGuid().ToString("N").ToUpper().Substring(0, 13)}";
        public int CheckBookId { get; set; }
        public virtual CheckBook? CheckBook { get; set; }
        public decimal? Amount { get; set; }
        public string? Status { get; set; } // "PENDING", "COMPLETE", "REJECTED"
        public string? DesAccountNumber { get; set; } 
        public string? CheckImageUrl { get; set; }
        public string? Note { get; set; }
        public DateTime? StatusChangedDate { get; set; } //= DateTime.Now;    // Date the checkbook request was approved
    }
}
