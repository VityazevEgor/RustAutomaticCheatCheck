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
		static async Task Main(string[] args)
		{

			string logName = "Microsoft-Windows-DriverFrameworks-UserMode/Operational";
			string source = "Microsoft-Windows-DriverFrameworks-UserMode";
			string query = "*[System/EventID=2003 or System/EventID=2100 or System/EventID=2102]";

			var eventLog = new EventLogQuery(logName, PathType.LogName, query);
			var eventLogReader = new EventLogReader(eventLog);

			for (EventRecord eventInstance = eventLogReader.ReadEvent(); eventInstance != null; eventInstance = eventLogReader.ReadEvent())
			{
				Console.WriteLine("Event ID: {0}", eventInstance.Id);
				Console.WriteLine("Time Created: {0}", eventInstance.TimeCreated);
				Console.WriteLine("Message: {0}", eventInstance.FormatDescription());
				Console.WriteLine();
			}
		}
	}
}