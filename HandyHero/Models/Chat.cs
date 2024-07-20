using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HandyHero.Models;  // Add this line

namespace HandyHero.Models
{

    public class Chat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public List<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }

    public class ChatParticipant
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        public int ChatId { get; set; }
        public User User { get; set; }
        public Chat Chat { get; set; }
    }


    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int ChatId { get; set; } // Add this property to link to Chat
        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public User Sender { get; set; }
        public User Receiver { get; set; }
        public Chat Chat { get; set; } // Navigation property to Chat
    }

    

    
}
