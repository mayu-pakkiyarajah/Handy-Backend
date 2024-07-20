using System.ComponentModel.DataAnnotations.Schema;

namespace HandyHero.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public ICollection<ChatParticipant> ChatParticipants { get; set; }

        public User()
        {
            
            ChatParticipants = new List<ChatParticipant>();
            
        }
        // Other properties
    }
}
