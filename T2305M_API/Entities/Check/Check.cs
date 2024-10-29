using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T2305M_API.Entities
{
    public class Check
    {
        [Key]
        public int CheckId { get; set; } // auto-increment
        public string CheckNumber { get; set; }
        public int CheckBookId { get; set; }
        public virtual CheckBook? CheckBook { get; set; }
        public int AccountId { get; set; }
        public virtual Account? Account { get; set; }
        public int UserId { get; set; }
        public virtual User? User { get; set; }
        public decimal? Amount { get; set; }
        public string? PayeeName { get; set; }
        public string? PayeeCitizenId { get; set; }
        public string? PayeeAccountNumber { get; set; } // for money transfer to
        public int? StaffId { get; set; }
        public string? Status { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ClearedDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}
