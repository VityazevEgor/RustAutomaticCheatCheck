using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inspector
{
    internal class SharedMethods
    {
        public static void PrintList<T>(List<T> list)
        {
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }

		public static string ToJson<T>(T value)
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				//Converters = { new ConcurrentBagConverter<T>() }
			};

			return JsonSerializer.Serialize(value, options);
		}

		public class ConcurrentBagConverter<T> : JsonConverter<ConcurrentBag<T>>
		{
			public override ConcurrentBag<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				throw new NotImplementedException();
			}

			public override void Write(Utf8JsonWriter writer, ConcurrentBag<T> value, JsonSerializerOptions options)
			{
				JsonSerializer.Serialize(writer, value.ToArray(), options);
			}
		}


		public static async Task<string> NirSoftEx(string url, string exeName)
		{
			string exePath = Path.Combine(Path.GetTempPath(), exeName);
            string zipPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()+".zip");
			if (!File.Exists(exePath))
			{

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        using var fileStream = await response.Content.ReadAsStreamAsync();
                        using var file = new FileStream(zipPath, FileMode.CreateNew);
                        await fileStream.CopyToAsync(file);
                    }
                    else
                    {
                        throw new Exception("Can't download tool from " + url);
                    }
                }
                ZipFile.ExtractToDirectory(zipPath, Path.GetTempPath(), true);
                File.Delete(zipPath);
            }

            string outputFileName = Path.GetRandomFileName() + ".xml";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"/sxml {outputFileName}",
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetTempPath()
                }
            };
            process.Start();
            await process.WaitForExitAsync();

            return File.ReadAllText(Path.Combine(Path.GetTempPath(), outputFileName));
        }

	}
}
