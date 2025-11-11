using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DarkNovel.Models
{
    [Table("PromoCodes")]
    public class PromoCode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required, MaxLength(50)]
        public string PromoType { get; set; }

        [Required]
        public int Value { get; set; }  

        public int? MaxUses { get; set; }
        public int? UsedCount { get; set; }

        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }

        public bool? IsActive { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
