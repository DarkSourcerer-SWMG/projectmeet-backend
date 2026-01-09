using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProjectMeet.Data;
using ProjectMeet.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ProjectMeet.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                Console.WriteLine($"[ChatHub] Client connected: {Context.ConnectionId}");
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatHub] ERROR in OnConnectedAsync: {ex.Message}");
                Console.WriteLine($"[ChatHub] Stack: {ex.StackTrace}");
                throw;
            }
        }

        public async Task JoinRoom(int meetingId)
    {
        try
        {
            Console.WriteLine($"[ChatHub] JoinRoom called with meetingId: {meetingId}");
            
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"meeting_{meetingId}"
            );
            
            Console.WriteLine($"[ChatHub] Added to group, fetching history...");

            var history = await (
                from m in _context.ChatMessages
                join u in _context.Users on m.UserId equals u.Id
                where m.MeetingId == meetingId
                orderby m.SentAt
                select new
                {
                    user = u.Name,
                    message = m.Message,
                    sentAt = m.SentAt
                }
            ).ToListAsync();

            Console.WriteLine($"[ChatHub] Fetched {history.Count} messages");
            await Clients.Caller.SendAsync("ReceiveHistory", history);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ChatHub] ERROR in JoinRoom: {ex.Message}");
            Console.WriteLine($"[ChatHub] Stack Trace: {ex.StackTrace}");
            throw;
        }
    }



        public async Task SendMessage(int meetingId, string message)
{

    if (string.IsNullOrWhiteSpace(message))
        return;

    var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
    var userName = Context.User?.Identity?.Name ?? "Anon";

    if (userIdClaim == null)
        return;

    var userId = int.Parse(userIdClaim.Value);

    var msg = new ChatMessage
    {
        MeetingId = meetingId,
        UserId = userId,
        Message = message,
        SentAt = DateTime.UtcNow
    };

    _context.ChatMessages.Add(msg);
    await _context.SaveChangesAsync();

    await Clients.Group($"meeting_{meetingId}")
        .SendAsync("ReceiveMessage", userName, message, msg.SentAt);
}

    }
}
