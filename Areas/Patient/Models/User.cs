using System.ComponentModel.DataAnnotations.Schema;

namespace Wellora.Areas.Patient.Models
{
    [Table("users")] // map to table name
    public class User
    {
        [Column("user_id")] // map property to column
        public int UserId { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("user_name")]
        public string? Username { get; set; }

        [Column("email")] 
        public string Email { get; set; }

        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [Column("role")] 
        public string Role { get; set; }

        [Column("status")] 
        public string Status { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}
