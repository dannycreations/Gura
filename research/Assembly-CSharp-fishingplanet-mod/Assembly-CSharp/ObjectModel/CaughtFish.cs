using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class CaughtFish : ICloneable
	{
		public Fish Fish { get; set; }

		public int[] BaitIds { get; set; }

		public int? BaitId { get; set; }

		[JsonIgnore]
		public int[] AllBaitIds
		{
			get
			{
				if (this.BaitIds != null)
				{
					return this.BaitIds;
				}
				if (this.BaitId != null)
				{
					return new int[] { this.BaitId.Value };
				}
				return new int[0];
			}
			set
			{
				this.BaitIds = value;
			}
		}

		public int LineId { get; set; }

		public float LineMaxLoad { get; set; }

		public TimeOfDay Time { get; set; }

		public int WeatherIcon { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public RodTemplate RodTemplate { get; set; }

		public bool IsOnBoat { get; set; }

		public object Clone()
		{
			return base.MemberwiseClone();
		}
	}
}
