namespace Server.Models
{
    public class SuspectsModel
    {
        public int Id { get; set; }
        public string? steamId { get; set; }
        public int score { get; set; } = 0;
        public DateTime? createdAt { get; set; } = DateTime.Now;
    }
}
