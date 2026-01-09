using System;

namespace ProjectMeet.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
        public Meeting Meeting { get; set; }
    }
}
