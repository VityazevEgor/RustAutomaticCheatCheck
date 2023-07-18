using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using Usb.Net;

namespace ForTesting
{
	internal class TestMain
	{
        [DllImport("ntdll.dll")]
        public static extern int NtQuerySystemInformation(int SystemInformationClass, IntPtr SystemInformation, int SystemInformationLength, ref int ReturnLength);

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_MODULE_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr Reserved2;
            public IntPtr ImageBaseAddress;
            public uint ImageSize;
            public uint Flags;
            public ushort Index;
            public ushort NameLength;
            public ushort LoadCount;
            public ushort PathLength;
            public char Name;
        }

        static void Main(string[] args)
        {
            // Открываем раздел реестра Windows для чтения
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU");
            if (key != null)
            {
                byte[] values = (byte[])key.GetValue(null);
                if (values != null)
                {
                    List<string> folders = new List<string>();
                    for (int i = 0; i < values.Length; i += 2)
                    {
                        if (values[i] == 0 && values[i + 1] == 0)
                        {
                            i++;
                            continue;
                        }
                        StringBuilder sb = new StringBuilder();
                        while (i < values.Length && values[i] != 0)
                        {
                            sb.Append((char)values[i]);
                            i += 2;
                        }
                        folders.Add(sb.ToString());
                    }
                    Console.WriteLine("Last Visited Folders:");
                    foreach (string folder in folders)
                    {
                        Console.WriteLine(folder);
                    }
                }
                key.Close();
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}