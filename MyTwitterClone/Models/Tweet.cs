using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyTwitterClone.Models
{
    public class Tweet
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "TweetId is required")]        
        public int TweetId { get; set; }

        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; }

        //[Required(ErrorMessage = "Email is required")]
        //[EmailAddress(ErrorMessage = "Email is invalid")]
        public DateTime Created { get; set; }
    }
}