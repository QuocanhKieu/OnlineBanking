using Castle.Core.Resource;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace T2305M_API.Entities
{
    public class ServiceRequest
    {
        [Key]
        public int ServiceRequestId { get; set; }// auto-increment
        public int UserId { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string Description { get; set; }
        [Required]
        public DateTime RequestDate { get; set; }
        [Required]
        public string Status { get; set; } // PENDING, WORKING, COMPLETE
        public virtual User? User { get; set; }
        public virtual ICollection<Message>? Messages { get; set; }  // Navigation property for many-to-many

    }

}

