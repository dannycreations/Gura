using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public static class TravelSerializationHelper
	{
		public static List<PondBrief> DeserializePondBriefs(Dictionary<byte, object> parameters)
		{
			object obj;
			List<PondBrief> list;
			if (parameters.TryGetValue(192, out obj))
			{
				string text = CompressHelper.DecompressString((byte[])obj);
				list = JsonConvert.DeserializeObject<List<PondBrief>>(text);
			}
			else
			{
				list = new List<PondBrief>();
			}
			return list;
		}

		public static List<LocationBrief> DeserializeLocationBriefs(Dictionary<byte, object> parameters)
		{
			object obj;
			List<LocationBrief> list;
			if (parameters.TryGetValue(192, out obj))
			{
				string text = CompressHelper.DecompressString((byte[])obj);
				list = JsonConvert.DeserializeObject<List<LocationBrief>>(text);
			}
			else
			{
				list = new List<LocationBrief>();
			}
			return list;
		}

		public static Pond DeserializePond(Dictionary<byte, object> parameters)
		{
			Pond pond = null;
			object obj;
			if (parameters.TryGetValue(192, out obj))
			{
				string text = CompressHelper.DecompressString((byte[])obj);
				pond = JsonConvert.DeserializeObject<Pond>(text);
			}
			return pond;
		}

		public static List<Pond> DeserializePonds(Dictionary<byte, object> parameters)
		{
			List<Pond> list = null;
			object obj;
			if (parameters.TryGetValue(192, out obj))
			{
				string text = CompressHelper.DecompressString((byte[])obj);
				list = JsonConvert.DeserializeObject<List<Pond>>(text);
			}
			return list;
		}

		public static List<PondLevelInfo> DeserializePondLevels(Dictionary<byte, object> parameters)
		{
			List<PondLevelInfo> list = null;
			object obj;
			if (parameters.TryGetValue(192, out obj))
			{
				list = JsonConvert.DeserializeObject<List<PondLevelInfo>>((string)obj);
			}
			return list;
		}

		public static Location DeserializeLocation(Dictionary<byte, object> parameters)
		{
			Location location = null;
			object obj;
			if (parameters.TryGetValue(192, out obj))
			{
				string text = CompressHelper.DecompressString((byte[])obj);
				location = JsonConvert.DeserializeObject<Location>(text);
			}
			return location;
		}

		public static WeatherDesc[] DeserializePondWeather(Dictionary<byte, object> parameters)
		{
			WeatherDesc[] array = null;
			object obj;
			if (parameters.TryGetValue(192, out obj))
			{
				string text = CompressHelper.DecompressString((byte[])obj);
				array = JsonConvert.DeserializeObject<WeatherDesc[]>(text);
			}
			return array;
		}

		public static Dictionary<int, WeatherDesc[]> DeserializeAllPondWeather(Dictionary<byte, object> parameters)
		{
			Dictionary<int, WeatherDesc[]> dictionary = null;
			object obj;
			if (parameters.TryGetValue(192, out obj))
			{
				string text = CompressHelper.DecompressString((byte[])obj);
				dictionary = JsonConvert.DeserializeObject<Dictionary<int, WeatherDesc[]>>(text);
			}
			return dictionary;
		}
	}
}
