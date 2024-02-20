using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class OneLinePointConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(Point3);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is Point3)
			{
				Point3 point = value as Point3;
				string text = string.Format("\"X\":{0},\"Y\": {1},\"Z\": {2}", SerializationHelper.FloatToString(point.X), SerializationHelper.FloatToString(point.Y), SerializationHelper.FloatToString(point.Z));
				writer.WriteRawValue("{ " + text + " }");
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
