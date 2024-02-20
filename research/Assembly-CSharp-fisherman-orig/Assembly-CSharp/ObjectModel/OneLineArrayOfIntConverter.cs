using System;
using System.Text;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class OneLineArrayOfIntConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(int[]);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			int[] array = value as int[];
			if (array != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (int num in array)
				{
					if (stringBuilder.Length == 0)
					{
						stringBuilder.Append(num);
					}
					else
					{
						stringBuilder.Append(",");
						stringBuilder.Append(num);
					}
				}
				writer.WriteRawValue(string.Format("[{0}]", stringBuilder));
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
