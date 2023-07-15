using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Text;
using Usb.Net;

namespace ForTesting
{
	internal class TestMain
	{
		static void Main(string[] args)
		{
			string bigString = "dshfkhaskdhfkasdhklfhklsaheruyieyiq36586herkfhkdshklfhdkslahfklhlsdahfieyriewyiryioywqeoiryiowqeyioruyqo3478658yihfdkjhsklahflkhsalifhieuyroiy438dshfkhaskdhfkasdhklfhklsaheruyieyiq36586herkfhkdshklfhdkslahfklhlsdahfieyriewyiryioywqeoiryiowqeyioruyqo3478658yihfdkjhsklahflkhsalifhieuyroiy438dshfkhaskdhfkasdhklfhklsaheruyieyiq36586herkfhkdshklfhdkslahfklhlsdahfieyriewyiryioywqeoiryiowqeyioruyqo3478658yihfdkjhsklahflkhsalifhieuyroiy438dshfkhaskdhfkasdhklfhklsaheruyieyiq36586herkfhkdshklfhdkslahfklhlsdahfieyriewyiryioywqeoiryiowqeyioruyqo3478658yihfdkjhsklahflkhsalifhieuyroiy438dshfkhaskdhfkasdhklfhklsaheruyieyiq36586herkfhkdshklfhdkslahfklhlsdahfieyriewyiryioywqeoiryiowqeyioruyqo3478658yihfdkjhsklahflkhsalifhieuyroiy438dshfkhaskdhfkasdhklfhklsaheruyieyiq36586herkfhkdshklfhdkslahfklhlsdahfieyriewyiryioywqeoiryiowqeyioruyqo3478658yihfdkjhsklahflkhsalifhieuyroiy438";
			string realyBigString = string.Empty;
			for (int i=0; i<100*100; i++)
			{
				realyBigString+= bigString;
			}
			string compressed = Compress(realyBigString);
			string decompressed = Decompress(compressed);
			Console.WriteLine($"Compressed: {compressed} \nSize = {compressed.Length} OrigSize = {realyBigString.Length}");
			Console.WriteLine($"Decompressed == Original: {decompressed == realyBigString}");
			
		}
		public static string Compress(string originalString)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(originalString);
			using (MemoryStream ms = new MemoryStream())
			{
				using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress))
				{
					gzip.Write(bytes, 0, bytes.Length);
				}
				byte[] compressedBytes = ms.ToArray();
				return Convert.ToBase64String(compressedBytes);
			}
		}

		public static string Decompress(string compressedString)
		{
			byte[] compressedBytes = Convert.FromBase64String(compressedString);
			using (MemoryStream ms = new MemoryStream(compressedBytes))
			{
				using (GZipStream gzip = new GZipStream(ms, CompressionMode.Decompress))
				{
					byte[] decompressedBytes = new byte[4096];
					using (MemoryStream msOut = new MemoryStream())
					{
						int bytesRead;
						while ((bytesRead = gzip.Read(decompressedBytes, 0, decompressedBytes.Length)) > 0)
						{
							msOut.Write(decompressedBytes, 0, bytesRead);
						}
						byte[] decompressedBytesFinal = msOut.ToArray();
						return Encoding.UTF8.GetString(decompressedBytesFinal);
					}
				}
			}
		}
	}
}