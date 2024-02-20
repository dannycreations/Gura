using System;

namespace ObjectModel
{
	public class MissionItem : InventoryItem
	{
		[JsonConfig]
		public float Buoyancy { get; set; }

		[JsonConfig]
		public float SpeedIncBuoyancy { get; set; }

		[JsonConfig]
		public float MinSpeedBuoyancy { get; set; }

		[JsonConfig]
		public float ResistanceForce { get; set; }

		[JsonConfig]
		public float Windage { get; set; }

		[JsonConfig]
		public float MinReelSpeed { get; set; }

		[JsonConfig]
		public float MaxReelSpeed { get; set; }

		public override bool IsGiftable
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public override bool IsOccupyInventorySpace
		{
			get
			{
				return false;
			}
		}
	}
}
