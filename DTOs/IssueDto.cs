namespace ProjectMeet.DTOs
{
    public class IssueDto
    {
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string RepoLink { get; set; } = null!;
    }
}
