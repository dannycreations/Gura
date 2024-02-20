using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class EchoSounder : ToolItem
	{
		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public EchoSounderKind Kind { get; set; }

		[JsonConfig]
		public float MaxFishLegth { get; set; }

		[JsonConfig]
		public float Convenience { get; set; }

		[JsonConfig]
		public int BatteryCapacity
		{
			get
			{
				return this.batteryCapacity;
			}
			set
			{
				this.batteryCapacity = value;
				this.AllowedEquipment.First((AllowedEquipment i) => i.ItemSubType == ItemSubTypes.SounderBattery).MaxItems = value;
			}
		}

		[JsonConfig]
		public float BatteryDischargeRate { get; set; }

		[JsonIgnore]
		public override AllowedEquipment[] AllowedEquipment
		{
			get
			{
				return new AllowedEquipment[]
				{
					new AllowedEquipment
					{
						ItemSubType = new ItemSubTypes?(ItemSubTypes.SounderBattery),
						MaxItems = this.BatteryCapacity
					}
				};
			}
		}

		[JsonConfig]
		public float? MaxSpeed { get; set; }

		private int batteryCapacity;
	}
}
