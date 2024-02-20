using System;

namespace ObjectModel
{
	public class TerminalTackleItem : InventoryItem
	{
		[JsonConfig]
		public float Buoyancy { get; set; }

		[JsonConfig]
		public float ResistanceForce { get; set; }

		[JsonConfig]
		public float Windage { get; set; }
	}
}
