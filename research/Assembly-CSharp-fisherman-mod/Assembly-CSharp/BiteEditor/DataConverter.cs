using System;
using BiteEditor.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiteEditor
{
	internal class DataConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(IAttractor) || objectType == typeof(ProbabilityMap);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject jobject = JObject.Load(reader);
			if (jobject["AttractionMinValue"] != null)
			{
				return jobject.ToObject<Attractor>(serializer);
			}
			if (jobject["Type"] != null && jobject["Type"].ToString() == "BiteMap")
			{
				return jobject.ToObject<BiteMap>(serializer);
			}
			if (jobject["Type"] != null && jobject["Type"].ToString() == "MapProcessor")
			{
				return jobject.ToObject<MapProcessor>(serializer);
			}
			if (jobject["Type"] != null && jobject["Type"].ToString() == "TimedMaps")
			{
				return jobject.ToObject<TimedMaps>(serializer);
			}
			if (jobject["Type"] != null && jobject["Type"].ToString() == "WindyMaps")
			{
				return jobject.ToObject<WindyMaps>(serializer);
			}
			return jobject;
		}

		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
