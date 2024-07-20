using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandyHero.Data;
using HandyHero.Models;
using HandyHero.DTO;



namespace HandyHero.Services
{
    public class ChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateNewChat(List<int> participantIds)
        {
            // Create a new Chat entity
            var newChat = new Chat
            {
                Participants = participantIds.Select(userId => new ChatParticipant { UserId = userId }).ToList()
            };

            // Add the new chat to the database context
            _context.Chat.Add(newChat);

            // Save changes to the database to generate the ChatId
            await _context.SaveChangesAsync();

            // Return the generated ChatId
            return newChat.Id;
        }
    }
}
