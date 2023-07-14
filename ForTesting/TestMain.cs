using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Management;
using Usb.Net;

namespace ForTesting
{
	internal class TestMain
	{
		static void Main(string[] args)
		{
			List<IWorker> tasks = new List<IWorker>();
			tasks.Add(new Worker());
			foreach (var task in tasks)
			{
				task.Process("kek");
			}
			while (true)
			{
				for (int i=0; i<tasks.Count; i++)
				{
					if (tasks[i].finsihed)
					{
						Console.WriteLine(i);
						break;
					}
				}
			}
			tasks.RemoveAll(x => x.finsihed);
			
		}
		public interface IWorker
		{
			string result { get; }
			bool finsihed { get; }
			Task Process(string content);

		}
		public class Worker : IWorker
		{
			public string result { get; set; } = string.Empty;
			public bool finsihed { get; set; } = false;
			public async Task Process(string content)
			{
				await Task.Delay(1000);
				result = content;
				finsihed = true;
			}
		}
	}
}