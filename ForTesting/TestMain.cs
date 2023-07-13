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

			string queryString = "SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB' AND MediaType='Removable Media'";

			ManagementObjectSearcher searcher = new ManagementObjectSearcher(queryString);

			foreach (ManagementObject drive in searcher.Get())
			{
				string deviceId = (string)drive.GetPropertyValue("DeviceID");
				object installDateObject = drive.GetPropertyValue("InstallDate");
				DateTime installDate;

				if (installDateObject == null)
				{
					installDate = new DateTime(1970, 1, 1);
				}
				else
				{
					installDate = ManagementDateTimeConverter.ToDateTime((string)installDateObject);
				}

				Console.WriteLine("USB Mass Storage device found: {0}, installed on {1}", deviceId, installDate.ToString("yyyy-MM-dd HH:mm:ss"));
			}
		}

		public class USBDeviceInfo
		{
			public string DeviceID { get; set; }
			public string PNPDeviceID { get; set; }
			public string Description { get; set; }
			public string InstallDate { get; set; }
		}

		public static List<USBDeviceInfo> GetUSBDevices()
		{
			List<USBDeviceInfo> devices = new List<USBDeviceInfo>();
			using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
			{
				foreach (var device in searcher.Get())
				{
					if ((string)device.GetPropertyValue("ClassGuid") == "{4d36e967-e325-11ce-bfc1-08002be10318}")
					{
						USBDeviceInfo deviceInfo = new USBDeviceInfo();
						deviceInfo.DeviceID = (string)device.GetPropertyValue("DeviceID");
						deviceInfo.PNPDeviceID = (string)device.GetPropertyValue("PNPDeviceID");
						deviceInfo.Description = (string)device.GetPropertyValue("Description");
						deviceInfo.InstallDate = (string)device.GetPropertyValue("InstallDate");
						devices.Add(deviceInfo);
					}
				}
			}
			return devices;
		}
	}
}