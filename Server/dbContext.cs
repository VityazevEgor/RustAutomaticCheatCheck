using Microsoft.EntityFrameworkCore;

namespace Server
{
    public class dbContext: DbContext
    {
        public dbContext(DbContextOptions<dbContext> options) : base(options)
        {
        }
        public DbSet<Models.SuspectsModel> SuspectsModel { get; set; } = default!;
		public DbSet<Models.EvidenceModel> EvidenceModel { get; set; } = default!;
		public DbSet<Models.ModeratorModel> ModeratorModel { get; set; } = default!;
		public DbSet<Models.InviteModel> InviteModel { get; set; } = default!;
	}
}
