namespace Server.Models
{
	public class EvidenceModel
	{
		public int Id { get; set; }
		public string? steamId { get; set; }
		public string? type { get; set; }
		public int score { get; set; } = 0;
		public string? data { get; set; }
		public string? reasonForScore { get; set; }

		public DateTime createdAt { get; set; } = DateTime.Now;
		public bool isProcessed { get; set; } = false;
	}
}
