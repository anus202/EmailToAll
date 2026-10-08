using System.ComponentModel.DataAnnotations;

namespace EmailToAll.Models
{
    /// <summary>
    /// The mailbox a user sends from. One row per user.
    /// </summary>
    public class EmailSetup : CommonField
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;

        /// <summary>Encrypted with <see cref="Service.EncryptionService"/>.</summary>
        public string Password { get; set; } = string.Empty;
    }
}
