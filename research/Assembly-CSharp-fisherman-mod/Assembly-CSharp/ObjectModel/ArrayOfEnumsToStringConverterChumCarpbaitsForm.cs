using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ArrayOfEnumsToStringConverterChumCarpbaitsForm : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.IsArray && objectType.GetElementType().IsEnum;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is IEnumerable)
			{
				StringBuilder stringBuilder = new StringBuilder();
				IEnumerator enumerator = ((IEnumerable)value).GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						object obj = enumerator.Current;
						if (obj is Enum)
						{
							string name = Enum.GetName(obj.GetType(), obj);
							if (stringBuilder.Length == 0)
							{
								stringBuilder.AppendFormat("\"{0}\"", name);
							}
							else
							{
								stringBuilder.AppendFormat(",\"{0}\"", name);
							}
						}
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = enumerator as IDisposable) != null)
					{
						disposable.Dispose();
					}
				}
				writer.WriteRawValue("[" + stringBuilder + "]");
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != 2)
			{
				throw new Exception(string.Format("Unexpected token parsing array of enums. Expected array start, got {0}.", reader.TokenType));
			}
			List<ChumCarpbaitsForm> list = new List<ChumCarpbaitsForm>();
			Type elementType = objectType.GetElementType();
			while (reader.TokenType != 14)
			{
				if (!reader.Read())
				{
					break;
				}
				if (reader.TokenType == 9)
				{
					list.Add((ChumCarpbaitsForm)Enum.Parse(typeof(ChumCarpbaitsForm), reader.Value.ToString()));
				}
				else if (reader.TokenType == 7)
				{
					list.Add((ChumCarpbaitsForm)Enum.ToObject(typeof(ChumCarpbaitsForm), (long)reader.Value));
				}
			}
			return list.ToArray();
		}
	}
}
