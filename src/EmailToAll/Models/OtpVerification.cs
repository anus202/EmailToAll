using System.ComponentModel.DataAnnotations;

namespace EmailToAll.Models
{
    public class OtpVerification : CommonField
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public DateTime ExpiryTime { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}
