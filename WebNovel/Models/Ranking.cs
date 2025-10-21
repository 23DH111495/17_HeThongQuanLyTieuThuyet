using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebNovel.Models
{
    public class Ranking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NovelId { get; set; }

        [Required]
        [StringLength(50)]
        public string RankingType { get; set; }

        [Required]
        [StringLength(20)]
        public string RankingPeriod { get; set; }

        [Required]
        public int Rank { get; set; }

        [Required]
        public decimal Score { get; set; }

        public DateTime CalculatedDate { get; set; } = DateTime.Now;

        public int? CalculatedBy { get; set; }

        // Navigation properties
        [ForeignKey("NovelId")]
        public virtual Novel Novel { get; set; }

        [ForeignKey("CalculatedBy")]
        public virtual User Calculator { get; set; }
    }
}