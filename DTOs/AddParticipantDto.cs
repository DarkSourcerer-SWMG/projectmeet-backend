namespace ProjectMeet.DTOs
{
    public class AddParticipantDto
    {
        public string Email { get; set; } = string.Empty;
        public int MeetingId { get; set; }
    }
}
