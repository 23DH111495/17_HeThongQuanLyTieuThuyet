using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [StringLength(255)]
        [Display(Name = "Profile Picture URL")]
        public string ProfilePicture { get; set; }

        [Display(Name = "Join Date")]
        public DateTime JoinDate { get; set; }

        [Display(Name = "Last Login")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Email Verified")]
        public bool EmailVerified { get; set; } = false;

        [Display(Name = "Role")]
        public string RoleName { get; set; }

        [Display(Name = "Role")]
        public int SelectedRoleId { get; set; }

        // List of available roles for dropdown/selection
        public List<RoleViewModel> AvailableRoles { get; set; } = new List<RoleViewModel>();

        // Computed properties
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [Display(Name = "Status")]
        public string Status => IsActive ? "Active" : "Inactive";

        // These two properties fix your error
        public string StatusBadge => IsActive ? "Active" : "Inactive";
        public string EmailStatusBadge => EmailVerified ? "Verified" : "Unverified";

        // Helper property to get the selected role name
        public string SelectedRoleName
        {
            get
            {
                if (AvailableRoles != null && SelectedRoleId > 0)
                {
                    var role = AvailableRoles.FirstOrDefault(r => r.Id == SelectedRoleId);
                    return role?.Name ?? "Unknown Role";
                }
                return "No Role";
            }
        }
    }
}