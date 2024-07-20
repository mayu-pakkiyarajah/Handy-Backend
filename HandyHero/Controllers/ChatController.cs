using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HandyHero.DTO;
using HandyHero.Models;
using HandyHero.Services;
using HandyHero.Data;

namespace HandyHero.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ChatService _chatService; // Add a reference to the ChatService

        public ChatController(ApplicationDbContext context, ChatService chatService)
        {
            _context = context;
            _chatService = chatService;
        }

        // Get Chat Messages between Sender and Receiver within a Chat
        [HttpGet("chats/{chatId}/users/{senderId}/receivers/{receiverId}/messages")]
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetChatMessagesBetweenUsers(int chatId, int senderId, int receiverId)
        {
            try
            {
                var messages = await _context.ChatMessages
                    .Where(m => m.ChatId == chatId && ((m.SenderId == senderId && m.ReceiverId == receiverId) || (m.SenderId == receiverId && m.ReceiverId == senderId)))
                    .Select(m => new ChatMessageDto
                    {
                        Id = m.Id,
                        SenderId = m.SenderId,
                        ReceiverId = m.ReceiverId,
                        Content = m.Content,
                        Timestamp = m.Timestamp,
                        ChatId = m.ChatId
                    })
                    .ToListAsync();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                // Log and handle exceptions appropriately
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        // Get Chat Messages
        [HttpGet("{chatId}/messages")]
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetChatMessages(int chatId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ChatId == chatId)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    ChatId = m.ChatId
                })
                .ToListAsync();

            return Ok(messages);
        }

        // Get Chat Messages by User ID and Chat ID
        [HttpGet("{chatId}/users/{userId}/messages")]
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetUserChatMessages(int chatId, int userId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ChatId == chatId && (m.SenderId == userId || m.ReceiverId == userId))
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    ChatId = m.ChatId
                })
                .ToListAsync();

            return Ok(messages);
        }


        // Send Message
        [HttpPost("send")]
        public async Task<ActionResult> SendMessage([FromBody] ChatMessageDto messageDto)
        {
            try
            {
                // Check if Sender and Receiver exist
                var sender = await _context.Users.FindAsync(messageDto.SenderId);
                var receiver = await _context.Users.FindAsync(messageDto.ReceiverId);

                if (sender == null)
                {
                    return BadRequest("Sender does not exist.");
                }

                if (receiver == null)
                {
                    return BadRequest("Receiver does not exist.");
                }

                // Check if both sender and receiver are clients
                if (sender.Role == "Client" && receiver.Role == "Client")
                {
                    return BadRequest("Client-to-client chat is not allowed.");
                }

                // Check if a chat exists between the sender and receiver
                var chat = await _context.Chat
                    .Where(c => c.Participants.Any(p => p.UserId == messageDto.SenderId))
                    .Where(c => c.Participants.Any(p => p.UserId == messageDto.ReceiverId))
                    .FirstOrDefaultAsync();

                // If chat doesn't exist, create a new one
                if (chat == null)
                {
                    var participantIds = new List<int> { messageDto.SenderId, messageDto.ReceiverId };
                    var newChatId = await _chatService.CreateNewChat(participantIds);
                    messageDto.ChatId = newChatId; // Set the chatId in the messageDto
                }
                else
                {
                    // If chat exists, set the chatId in the messageDto
                    messageDto.ChatId = chat.Id;
                }

                // Create new ChatMessage
                var chatMessage = new ChatMessage
                {
                    SenderId = messageDto.SenderId,
                    ReceiverId = messageDto.ReceiverId,
                    Content = messageDto.Content,
                    Timestamp = messageDto.Timestamp,
                    ChatId = messageDto.ChatId
                };

                _context.ChatMessages.Add(chatMessage);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    return StatusCode(500, "An error occurred while saving the message. See the inner exception for details.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Get Chat Messages between Sender and Receiver
        [HttpGet("messages")]
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetMessagesBetweenUsers(int senderId, int receiverId)
        {
            try
            {
                var messages = await _context.ChatMessages
                    .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) || (m.SenderId == receiverId && m.ReceiverId == senderId))
                    .Select(m => new ChatMessageDto
                    {
                        Id = m.Id,
                        SenderId = m.SenderId,
                        ReceiverId = m.ReceiverId,
                        Content = m.Content,
                        Timestamp = m.Timestamp,
                        ChatId = m.ChatId
                    })
                    .ToListAsync();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Get the latest message for a user in a specific chat
        [HttpGet("latestMessage")]
        public async Task<ActionResult<ChatMessageDto>> GetLatestMessageBetweenUsers(int senderId, int receiverId)
        {
            try
            {
                // Fetch the latest message where the user is either the sender or the receiver
                var latestMessage = await _context.ChatMessages
                    .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) || (m.SenderId == receiverId && m.ReceiverId == senderId))
                    .OrderByDescending(m => m.Timestamp)
                    .Select(m => new ChatMessageDto
                    {
                        Id = m.Id,
                        SenderId = m.SenderId,
                        ReceiverId = m.ReceiverId,
                        Content = m.Content,
                        Timestamp = m.Timestamp,
                        ChatId = m.ChatId
                    })
                    .FirstOrDefaultAsync();

                if (latestMessage == null)
                {
                    return NotFound();
                }

                return Ok(latestMessage);
            }
            catch (Exception ex)
            {
                // Log the exception (using a logger, for example)
                // _logger.LogError(ex, "Error retrieving latest message between users {SenderId} and {ReceiverId}", senderId, receiverId);

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}



