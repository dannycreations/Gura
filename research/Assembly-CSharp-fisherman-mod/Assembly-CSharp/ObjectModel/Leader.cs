using System;

namespace ObjectModel
{
	public class Leader : TerminalTackleItem, ILeader
	{
		public float Thickness { get; set; }

		public float Visibility { get; set; }

		public float MaxLoad { get; set; }

		public float LeaderLength { get; set; }

		public float Flexibility { get; set; }

		public float Toughness { get; set; }

		public string Color { get; set; }

		[NoClone(true)]
		public bool? WasCut { get; set; }

		public override bool CanCombineWith(InventoryItem targetItem)
		{
			return base.CanCombineWith(targetItem) && this.CanCombine() && ((Leader)targetItem).CanCombine();
		}

		public bool CanCombine()
		{
			if (base.ItemSubType.IsManyTimeLeader())
			{
				return base.MaxDurability != null && base.Durability == base.MaxDurability;
			}
			return this.WasCut != true && base.MaxDurability != null && base.Durability == base.MaxDurability;
		}

		public override bool IsBroken()
		{
			if (base.ItemSubType.IsManyTimeLeader())
			{
				return base.Durability == 0;
			}
			bool flag;
			if (!(this.WasCut == true) && base.MaxDurability != null)
			{
				int? maxDurability = base.MaxDurability;
				flag = base.Durability < maxDurability;
			}
			else
			{
				flag = true;
			}
			return flag;
		}
	}
}
