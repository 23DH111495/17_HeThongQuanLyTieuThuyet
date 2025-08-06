using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    public class AuthorViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Pen Name")]
        public string PenName { get; set; }

        [Display(Name = "Biography")]
        [DataType(DataType.MultilineText)]
        public string Biography { get; set; }

        [StringLength(255)]
        [Url]
        [Display(Name = "Website")]
        public string Website { get; set; }

        [Display(Name = "Social Links")]
        public string SocialLinks { get; set; }

        [Display(Name = "Is Verified")]
        public bool IsVerified { get; set; }

        [Display(Name = "Verification Date")]
        public DateTime? VerificationDate { get; set; }

        [Display(Name = "Author Rank")]
        public string AuthorRank { get; set; }

        [Display(Name = "Total Novels")]
        public int TotalNovels { get; set; }

        [Display(Name = "Total Views")]
        public long TotalViews { get; set; }

        [Display(Name = "Total Followers")]
        public int TotalFollowers { get; set; }

        // Related user info
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}