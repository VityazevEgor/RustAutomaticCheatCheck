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
            int length = 0;
            NtQuerySystemInformation(11, IntPtr.Zero, 0, ref length);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            NtQuerySystemInformation(11, ptr, length, ref length);
            int offset = Marshal.SizeOf(typeof(IntPtr));
            for (int i = 0; i < length / offset; i++)
            {
                IntPtr pModule = Marshal.ReadIntPtr(ptr, i * offset);
                SYSTEM_MODULE_INFORMATION module = (SYSTEM_MODULE_INFORMATION)Marshal.PtrToStructure(pModule, typeof(SYSTEM_MODULE_INFORMATION));
                Console.WriteLine(module.Name);
            }
        }
    }
}