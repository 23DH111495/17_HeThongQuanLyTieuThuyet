using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNovel.Models.ApiModels
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public DateTime JoinDate { get; set; }
        public bool IsPremium { get; set; }
        public decimal CoinBalance { get; set; }
        public List<string> Roles { get; set; }
    }
}