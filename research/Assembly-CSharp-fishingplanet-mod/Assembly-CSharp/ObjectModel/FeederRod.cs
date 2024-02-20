using System;
using System.Linq;

namespace ObjectModel
{
	public class FeederRod : BottomRod
	{
		[JsonConfig]
		[NoClone(true)]
		public QuiverTip[] QuiverTips { get; set; }

		[NoClone(true)]
		public int? QuiverId { get; set; }

		public override void SetProperty(string propertyName, string propertyValue)
		{
			if (propertyName == "QuiverId")
			{
				this.QuiverId = new int?(int.Parse(propertyValue));
			}
			else if (propertyName == "QuiverIsBroken")
			{
				QuiverTip quiverTip = this.QuiverTips.First((QuiverTip q) => q.ItemId == this.QuiverId);
				quiverTip.IsBroken = bool.Parse(propertyValue);
			}
			else
			{
				base.SetProperty(propertyName, propertyValue);
			}
		}

		public override void MakeCloneOfItem(InventoryItem source, bool fullClone = false)
		{
			base.MakeCloneOfItem(source, fullClone);
			FeederRod feederRod = (FeederRod)source;
			if (feederRod.QuiverTips != null)
			{
				QuiverTip[] array = feederRod.QuiverTips.Select(delegate(QuiverTip q)
				{
					QuiverTip quiverTip = ((this.QuiverTips == null) ? null : this.QuiverTips.FirstOrDefault((QuiverTip qq) => qq.ItemId == q.ItemId)) ?? new QuiverTip();
					quiverTip.ItemId = q.ItemId;
					quiverTip.MakeCloneOf(q, fullClone);
					return quiverTip;
				}).ToArray<QuiverTip>();
				this.QuiverTips = array;
			}
		}

		public int RodDurabilityLossForQuiver(QuiverTip quiver)
		{
			float rodDurabilityLoss = quiver.RodDurabilityLoss;
			int? maxDurability = base.MaxDurability;
			return (int)(rodDurabilityLoss * (float)((maxDurability == null) ? 0 : maxDurability.Value));
		}

		public override void RepairItem(int damageRestored)
		{
			base.RepairItem(damageRestored);
			if (this.QuiverId != null)
			{
				QuiverTip quiverTip = this.QuiverTips.First((QuiverTip q) => q.ItemId == this.QuiverId);
				damageRestored = this.RepairQuiver(quiverTip, damageRestored);
			}
			foreach (QuiverTip quiverTip2 in this.QuiverTips)
			{
				damageRestored = this.RepairQuiver(quiverTip2, damageRestored);
			}
		}

		private int RepairQuiver(QuiverTip quiver, int damageRestored)
		{
			if (damageRestored <= 0)
			{
				return damageRestored;
			}
			if (quiver.IsBroken)
			{
				int num = this.RodDurabilityLossForQuiver(quiver);
				if (damageRestored < num)
				{
					return damageRestored;
				}
				damageRestored -= num;
				quiver.IsBroken = false;
			}
			return damageRestored;
		}
	}
}
