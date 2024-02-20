using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class Weather
	{
		public string Name { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public SkyType Sky
		{
			get
			{
				return this.sky;
			}
			set
			{
				this.sky = value;
			}
		}

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public TemperatureType Temperature
		{
			get
			{
				return this.temperature;
			}
			set
			{
				this.temperature = value;
			}
		}

		[JsonConfig]
		public int WaterTemperature
		{
			get
			{
				return this.waterTemperature;
			}
			set
			{
				this.waterTemperature = value;
			}
		}

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public WindType Wind
		{
			get
			{
				return this.wind;
			}
			set
			{
				this.wind = value;
			}
		}

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public PrecipitationType Precipitation
		{
			get
			{
				return this.precipitation;
			}
			set
			{
				this.precipitation = value;
			}
		}

		[JsonConfig]
		public int Icon { get; set; }

		public override string ToString()
		{
			if (this.sky == (SkyType)0 && this.temperature == (TemperatureType)0 && this.wind == (WindType)0 && this.precipitation == PrecipitationType.None)
			{
				return string.Empty;
			}
			string[] array = new string[]
			{
				Enum.GetName(typeof(SkyType), this.sky),
				Enum.GetName(typeof(TemperatureType), this.temperature),
				Enum.GetName(typeof(WindType), this.wind),
				Enum.GetName(typeof(PrecipitationType), this.precipitation)
			};
			string text = string.Empty;
			foreach (string text2 in array)
			{
				if (!string.IsNullOrEmpty(text2))
				{
					if (string.IsNullOrEmpty(text))
					{
						text += text2;
					}
					else
					{
						text = text + "&" + text2;
					}
				}
			}
			return text;
		}

		public bool Match(Weather weatherMask)
		{
			return (weatherMask.Sky == (SkyType)0 || this.Sky == weatherMask.Sky) && (weatherMask.Temperature == (TemperatureType)0 || this.Temperature == weatherMask.Temperature) && (weatherMask.Wind == (WindType)0 || this.Wind == weatherMask.Wind) && (weatherMask.Precipitation == PrecipitationType.None || this.Precipitation == weatherMask.Precipitation);
		}

		public static Weather Parse(string weather)
		{
			byte b = Weather.MatchedEnum(typeof(SkyType), weather);
			byte b2 = Weather.MatchedEnum(typeof(TemperatureType), weather);
			byte b3 = Weather.MatchedEnum(typeof(WindType), weather);
			byte b4 = Weather.MatchedEnum(typeof(PrecipitationType), weather);
			Weather weather2 = new Weather();
			if (b != 0)
			{
				weather2.Sky = (SkyType)b;
			}
			if (b2 != 0)
			{
				weather2.Temperature = (TemperatureType)b2;
			}
			if (b3 != 0)
			{
				weather2.Wind = (WindType)b3;
			}
			if (b4 != 0)
			{
				weather2.Precipitation = (PrecipitationType)b4;
			}
			return weather2;
		}

		private static byte MatchedEnum(Type enumType, string s)
		{
			foreach (string text in Enum.GetNames(enumType))
			{
				if (s.Contains(text))
				{
					return (byte)Enum.Parse(enumType, text);
				}
			}
			return 0;
		}

		private SkyType sky;

		private TemperatureType temperature;

		private int waterTemperature;

		private WindType wind;

		private PrecipitationType precipitation;
	}
}
