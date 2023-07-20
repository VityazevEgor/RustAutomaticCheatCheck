namespace Server.Models
{
	public class InviteModel
	{
		public int Id { get; set; }
		public string Code { get; set; }
		public bool isUsed { get; set; }
	}
}
