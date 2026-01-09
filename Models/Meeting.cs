namespace ProjectMeet.Models
{
    public class Meeting
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int MaxParticipants { get; set; }
    }
}
