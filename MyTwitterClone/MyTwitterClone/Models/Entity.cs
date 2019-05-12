using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyTwitterClone.Models
{
    public class Entity
    {
        public string TweetMessage { get; set; }
        public List<Tweet> TweetList { get; set; }
        public List<Following> FollowingList { get; set; }

        public List<RegisteredUser> SearchUserList { get; set; }

        public string UserName { get; set; }
        
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        public string FullName { get; set; }
       
        [EmailAddress(ErrorMessage = "Email is invalid")]
        public string Email { get; set; }
    }
}