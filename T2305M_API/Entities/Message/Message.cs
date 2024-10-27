using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T2305M_API.Entities
{
    public class Message
    {
        [Key] 
        public int MessageId { get; set; }// auto-increment
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string Content { get; set; }
        public int UserId { get; set; }
        public int ServiceRequestId { get; set; }
        public DateTime AnswerDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "SHOW";// SHOW, HIDE
        public virtual User? User { get; set; }
        public virtual ServiceRequest? ServiceRequest { get; set; }


    }
}
