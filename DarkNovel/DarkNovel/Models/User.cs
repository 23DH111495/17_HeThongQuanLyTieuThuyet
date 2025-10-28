using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DarkNovel.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("Username")]
        public string? Username { get; set; }

        [Column("Email")]
        public string? Email { get; set; }

        [Column("PasswordHash")]
        public string? PasswordHash { get; set; }

        [Column("FirstName")]
        public string? FirstName { get; set; }

        [Column("LastName")]
        public string? LastName { get; set; }
        [JsonIgnore]
        [Column("ProfilePicture")]
        public string? ProfilePicture { get; set; }

        [Column("JoinDate")]
        public DateTime? JoinDate { get; set; }

        [Column("LastLoginDate")]
        public DateTime? LastLoginDate { get; set; }

        [Column("IsActive")]
        public bool? IsActive { get; set; }

        [Column("EmailVerified")]
        public bool? EmailVerified { get; set; }

        [Column("CreatedAt")]
        public DateTime? CreatedAt { get; set; }

        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }
        [JsonIgnore]

        [Column("ProfilePictureData")]
        public byte[]? ProfilePictureData { get; set; }
        [JsonIgnore]

        [Column("ProfilePictureContentType")]
        public string? ProfilePictureContentType { get; set; }
        [JsonIgnore]

        [Column("ProfilePictureFileName")]
        public string? ProfilePictureFileName { get; set; }
    }
}
