using System;
using System.Collections;
using System.Text;
using Newtonsoft.Json;

namespace ObjectModel
{
	public abstract class ArrayOfEnumsToStringConverterBase : JsonConverter
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
			return this.GetFromReader(reader);
		}

		protected abstract object GetFromReader(JsonReader reader);
	}
}
