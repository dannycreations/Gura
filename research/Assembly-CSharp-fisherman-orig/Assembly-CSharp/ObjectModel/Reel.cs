using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	public class Reel : InventoryItem, IReel
	{
		public ReelTypes ReelType
		{
			get
			{
				ItemSubTypes itemSubType = base.ItemSubType;
				if (itemSubType == ItemSubTypes.CastReel)
				{
					return ReelTypes.Baitcasting;
				}
				if (itemSubType == ItemSubTypes.SpinReel)
				{
					return ReelTypes.Spinning;
				}
				if (itemSubType != ItemSubTypes.FlyReel)
				{
					return ReelTypes.Invalid;
				}
				return ReelTypes.Fly;
			}
		}

		[JsonConfig]
		public float Ratio { get; set; }

		[JsonConfig]
		public float Speed { get; set; }

		[JsonConfig]
		public float CastFriction { get; set; }

		[JsonConfig]
		public float BacklashProbability { get; set; }

		[JsonConfig]
		public float CastWeightMin { get; set; }

		[JsonConfig]
		public float CastWeightMax { get; set; }

		[JsonConfig]
		public int LineCapacity { get; set; }

		[JsonConfig]
		public float MaxLoad { get; set; }

		[JsonIgnore]
		public float MaxDrag
		{
			get
			{
				return this.MaxLoad;
			}
		}

		[JsonConfig]
		public bool HasBaitRunner { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public ReelBrakeType BrakeType { get; set; }

		[JsonConfig]
		public int BearingCount { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public ReelSpoolType SpoolType { get; set; }

		[JsonConfig]
		[JsonConverter(typeof(StringEnumConverter))]
		public ReelDragType DragType { get; set; }

		[JsonConfig]
		public int? FrictionSectionsCount { get; set; }

		[JsonIgnore]
		protected override bool IsSellable
		{
			get
			{
				return base.Rent == null;
			}
		}
	}
}
