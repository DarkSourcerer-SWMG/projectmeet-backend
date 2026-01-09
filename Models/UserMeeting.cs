namespace ProjectMeet.Models
{
    public class UserMeeting
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MeetingId { get; set; }
        public DateTime SignUpDate { get; set; }

        public User User { get; set; }
        public Meeting Meeting { get; set; }
    }
}
