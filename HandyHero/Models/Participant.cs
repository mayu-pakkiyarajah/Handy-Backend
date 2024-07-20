using Microsoft.Extensions.Logging;

namespace HandyHero.Models
{
    public class Participant
    {
        // Foreign key for User
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
