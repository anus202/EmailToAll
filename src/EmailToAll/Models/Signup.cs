using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmailToAll.Models
{
    public class Signup : CommonField
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        [NotMapped]
        public string ConfirmPass { get; set; } = string.Empty;
    }
}
