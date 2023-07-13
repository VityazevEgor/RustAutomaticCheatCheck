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
						(int score, string reason) = await EvidenceProcessors.RunHisoty.Process(currentEvidence.data);
						currentEvidence.score = score;
						currentEvidence.reasonForScore = reason;
					}
					if (currentEvidence.type == "SteamAccounts")
					{ 
						Log("Found steam acc task");
						(int score, string reason) = await EvidenceProcessors.SteamAccounts.Process(currentEvidence.data);
						currentEvidence.score = score;
						currentEvidence.reasonForScore = reason;
					}
					if (currentEvidence.type == "USBDevices")
					{
						Log("Found USB task");
						var runHistoryEvidence = await _context.EvidenceModel.FirstOrDefaultAsync(e=>e.steamId == currentEvidence.steamId && e.type== "RunHistory");
						if (runHistoryEvidence is not null)
						{
							(int score, string reason) = await EvidenceProcessors.USBDevices.Process(currentEvidence.data, runHistoryEvidence.data);
							currentEvidence.score = score;
							currentEvidence.reasonForScore = reason;
						}
						else
						{
							Log("Can't process USB task cuz don't have RunHistory");
							continue; // из-за этого может быть проблема
						}
					}

					currentEvidence.isProcessed = true;
					(await _context.SuspectsModel.FirstOrDefaultAsync(s => s.steamId == currentEvidence.steamId)).score += currentEvidence.score;
					await _context.SaveChangesAsync();
                }
				await Task.Delay(1000);
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
