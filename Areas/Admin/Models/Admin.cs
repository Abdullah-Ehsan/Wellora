using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wellora.Models;

namespace Wellora.Areas.Admin.Models
{
    [Table("admins")]
    public class Admin
    {
        [Key]
        [Column("admin_id")]
        public int AdminId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [StringLength(150)]
        [Column("full_name")]
        public string FullName { get; set; }

        // Navigation Property

        // Assuming you have User model
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}