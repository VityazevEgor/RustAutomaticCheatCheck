using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium.DevTools.V112.Debugger;
using Server.BGTasks.EvidenceProcessors;
using Server.Models;
using System.Diagnostics;

namespace Server.BGTasks
{
	public class EvidenceChecker : IHostedService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly dbContext _context;
		List<IEvidenceWoker> evidenceWokers = new List<IEvidenceWoker>();
		const int tasksLimit = 10;

		private string runHistoryChachePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "runHistoryChache.txt");

		public interface IEvidenceWoker
		{
			int score { get; set; }
			int evidenceId { get; set; }
			string reasonForScore { get; set; }
			string additionalOutput { get; set; }
			bool isProccessed { get; set; }

			Task Process(Dictionary<string, string> data);
		}

		public EvidenceChecker(IServiceScopeFactory serviceScopeFactory)
		{
			_serviceScopeFactory = serviceScopeFactory;
			var scope = _serviceScopeFactory.CreateScope();
			_context = scope.ServiceProvider.GetRequiredService<dbContext>();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			if (!File.Exists(runHistoryChachePath)) File.WriteAllText(runHistoryChachePath, string.Empty);
			Task.Run(() => paralerJob());
			return Task.CompletedTask;
		}
		

		private async Task paralerJob()
		{
			while (true)
			{
				var currentEvidences = await _context.EvidenceModel.Where(e => e.isProcessed == false).OrderBy(e => e.createdAt).ToListAsync();

				// добавления новых задач
				foreach (var currentEvidence in currentEvidences)
				{
					if (evidenceWokers.Count >= tasksLimit) break;

					// Если текущее доказательство не обрабатывается
					if (evidenceWokers.FirstOrDefault(e => e.evidenceId == currentEvidence.Id) is null)
					{
						switch (currentEvidence.type)
						{
							case "RunHistory":
								Log("Found runhistory task");
								evidenceWokers.Add(CreateEvidenceWorker<RunHisoty>(currentEvidence, File.ReadAllText(runHistoryChachePath)));
								break;

							case "SteamAccounts":
								Log("Found steam acc task");
								evidenceWokers.Add(CreateEvidenceWorker<SteamAccounts>(currentEvidence));
								break;

							case "USBDevices":
								var runHistoryEvidence = await _context.EvidenceModel.FirstOrDefaultAsync(e => e.steamId == currentEvidence.steamId && e.type == "RunHistory");
								if (runHistoryEvidence is not null)
								{
									Log("Found USB task");
									evidenceWokers.Add(CreateEvidenceWorker<USBDevices>(currentEvidence, runHistoryEvidence.data));
								}
								else
								{
									Log("Can't process USB task cuz don't have RunHistory");
								}
								break;

							case "BrowserHistory":
								Log("Found browser history task");
								evidenceWokers.Add(CreateEvidenceWorker<BrowserHistory>(currentEvidence));
								break;

							case "DownloadHistory":
								Log("Found downloads hisotry task");
								var bhEvidence = await _context.EvidenceModel.FirstOrDefaultAsync(e => e.steamId == currentEvidence.steamId && e.type == "BrowserHistory");
								if (bhEvidence is not null)
								{
									evidenceWokers.Add(CreateEvidenceWorker<DownloadHistory>(currentEvidence, bhEvidence.data));
								}
								else
								{
									Log("Can't process download history task cuz there is no BrowserHisory evidence");
								}
								break;

							default:
								//Log($"Unknown task type: {currentEvidence.type}");
								break;
						}
					}
				}

				// обработка тех задач которые уже были выполнены
				var complitedTasks = evidenceWokers.Where(t=>t.isProccessed).ToList();
				foreach (var currentTask in complitedTasks)
				{
					Log($"Task with id = {currentTask.evidenceId} finished his work with score = {currentTask.score}");

					var evidence = await _context.EvidenceModel.FirstOrDefaultAsync(e => currentTask.evidenceId == e.Id);
					evidence.score = currentTask.score;
					evidence.reasonForScore = currentTask.reasonForScore;
					evidence.isProcessed = true;

					var suspectModel = await _context.SuspectsModel.FirstOrDefaultAsync(s=>s.steamId == evidence.steamId);
					suspectModel.score += evidence.score;

					await _context.SaveChangesAsync();

					if (evidence.type == "RunHistory")
					{
						await File.AppendAllTextAsync(runHistoryChachePath, currentTask.additionalOutput);
					}

					evidenceWokers.RemoveAll(w => w.evidenceId == currentTask.evidenceId);
				}

				// чистка
				complitedTasks.Clear();
				currentEvidences.Clear();

				await Task.Delay(500);
			}
		}

		IEvidenceWoker CreateEvidenceWorker<T>(EvidenceModel evidence, string aditionalData = null) where T : IEvidenceWoker, new()
		{
			var worker = new T
			{
				evidenceId = evidence.Id
			};
			var arguments = new Dictionary<string, string>
			{
				{ "raw", evidence.data }
			};
			if (aditionalData is not null)
			{
				arguments.Add("aditionalData", aditionalData);
			}
			worker.Process(arguments);
			return worker;
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
