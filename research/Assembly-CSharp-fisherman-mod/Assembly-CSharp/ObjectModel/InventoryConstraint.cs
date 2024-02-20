using System;

namespace ObjectModel
{
	public class InventoryConstraint
	{
		public InventoryConstraint(int count, bool perType = true, ItemTypes[] altTypes = null, ItemSubTypes[] altSubTypes = null)
		{
			this.count = count;
			this.AltTypes = altTypes;
			this.AltSubTypes = altSubTypes;
			if (this.AltTypes == null || this.AltTypes.Length == 0)
			{
				this.PerType = perType;
			}
			else
			{
				this.PerType = true;
			}
		}

		public int Count
		{
			get
			{
				return this.count + this.increment;
			}
		}

		public bool PerType { get; private set; }

		public ItemTypes[] AltTypes { get; private set; }

		public ItemSubTypes[] AltSubTypes { get; private set; }

		public void Increment(int value)
		{
			this.increment += value;
		}

		public void Reset()
		{
			this.increment = 0;
		}

		public InventoryConstraint Clone()
		{
			return new InventoryConstraint(this.Count, this.PerType, this.AltTypes, this.AltSubTypes);
		}

		public const int RodCount = 1;

		public const int TackleCount = 1;

		public const int TerminalTackleCount = 10;

		public const int ChumCount = 10;

		public const int MiscCount = 10;

		public const int RodStandCount = 1;

		public const int ToolCount = 1;

		public const int OutfitCount = 1;

		public const int BoatsCount = 1;

		public const int DollChumCount = 1;

		private readonly int count;

		private int increment;
	}
}
