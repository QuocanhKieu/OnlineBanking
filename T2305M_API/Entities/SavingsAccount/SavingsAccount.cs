using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace T2305M_API.Entities
{
    public class SavingsAccount
    {
        [Key]
        public int SavingsAccountId { get; set; }  // ID of the savings account

        public int UserId { get; set; }  // ID of the user who owns the account

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }  // Amount in the savings account

        [Required]
        [Range(0, 100)]
        public decimal InterestRate { get; set; }  // Annual interest rate (as a percentage)

        [Required]
        public int Term { get; set; }  // Term of the savings (in months or years)

        [Required]
        public DateTime StartDate { get; set; }  // Date the savings account was created

        public DateTime? MaturityDate { get; set; }  // Date the account will mature
        public DateTime? WithdrawnDate { get; set; }  // Date the account will mature

        [Required]
        public string Status { get; set; }  // Status of the account (Active, Matured, Withdrawn, etc.)
        
        [Required]
        [MaxLength(20)]
        public string SavingsAccountCode { get; set; }  // Code for the savings account (e.g., unique identifier)
        // Navigation property
        public User ?  User { get; set; }  // Navigation property for the user who owns the savings account
    }

}
