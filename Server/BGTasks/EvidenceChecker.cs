using Microsoft.EntityFrameworkCore;

namespace Server.BGTasks
{
	public class EvidenceChecker : IHostedService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly dbContext _context;

		public EvidenceChecker(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
			var scope = _serviceScopeFactory.CreateScope();
			_context = scope.ServiceProvider.GetRequiredService<dbContext>();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			Task.Run(() => Job());
			return Task.CompletedTask;
		}

		public async Task Job()
		{
			while (true)
			{
				var currentEvidence = await _context.EvidenceModel.Where(e=>e.isProcessed == false).OrderBy(e=>e.createdAt).FirstOrDefaultAsync();
                if (currentEvidence is not null)
                {
                    if (currentEvidence.type == "RunHistory")
					{
						Log("Found runhistory task");
						await EvidenceProcessors.RunHisoty.Process(currentEvidence.data);
					}
                }
				await Task.Delay(30000);
            }
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public void Log(string message)
		{
			Console.WriteLine($"[EvidenceChecker] {message}");
		}
	}
}
