using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
	}
}
