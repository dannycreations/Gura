using System;
using System.Linq;

namespace ObjectModel
{
	public static class ConditionExtensions
	{
		public static bool Match(this IInventoryCondition condition, InventoryItem item, bool checkCount = false)
		{
			if (item.ItemId == 0)
			{
				return false;
			}
			if (condition.ItemType != null && item.ItemType != condition.ItemType)
			{
				return false;
			}
			if (condition.ItemSubType != null && item.ItemSubType != condition.ItemSubType)
			{
				return false;
			}
			if (condition.CategoryId != null && item.ItemSubType != (ItemSubTypes)condition.CategoryId.Value)
			{
				return false;
			}
			if (condition.ItemId != null && item.ItemId != condition.ItemId)
			{
				return false;
			}
			if (condition.ItemIds != null && condition.ItemIds.Length > 0 && !condition.ItemIds.Contains(item.ItemId))
			{
				return false;
			}
			if (condition.Slot != null && item.Slot != condition.Slot)
			{
				return false;
			}
			if (item.ParentItem != null)
			{
				if (condition.ParentItemType != null && item.ParentItem.ItemType != condition.ParentItemType)
				{
					return false;
				}
				if (condition.ParentItemSubType != null && item.ParentItem.ItemSubType != condition.ParentItemSubType)
				{
					return false;
				}
				if (condition.ParentCategoryId != null && item.ParentItem.ItemSubType != (ItemSubTypes)condition.ParentCategoryId.Value)
				{
					return false;
				}
				if (condition.ParentItemId != null && item.ParentItem.ItemId != condition.ParentItemId)
				{
					return false;
				}
				if (condition.ParentItemIds != null && condition.ParentItemIds.Length > 0 && !condition.ParentItemIds.Contains(item.ParentItem.ItemId))
				{
					return false;
				}
				if (condition.ParentSlot != null && item.ParentItem.Slot != condition.ParentSlot)
				{
					return false;
				}
				if (condition.ParentStorage != null && item.ParentItem.Storage != condition.ParentStorage)
				{
					return false;
				}
				if (condition.ParentStorages != null && !condition.ParentStorages.Contains(item.ParentItem.Storage))
				{
					return false;
				}
			}
			if (condition.Storage != null && item.Storage != condition.Storage)
			{
				return false;
			}
			if (condition.Storages != null && !condition.Storages.Contains(item.Storage))
			{
				return false;
			}
			if (condition.Durability != null)
			{
				int? durability = condition.Durability;
				if (item.Durability < durability)
				{
					return false;
				}
			}
			if (checkCount && condition.Count != null)
			{
				int? count = condition.Count;
				if (item.Count < count)
				{
					return false;
				}
			}
			if (checkCount && condition.Amount != null)
			{
				float? amount = condition.Amount;
				if (item.Amount < amount)
				{
					return false;
				}
			}
			if (condition.Length != null)
			{
				double? length = item.Length;
				bool flag = length != null;
				int? length2 = condition.Length;
				double? num = ((length2 == null) ? null : new double?((double)length2.Value));
				if ((flag & (num != null)) && length.GetValueOrDefault() < num.GetValueOrDefault())
				{
					return false;
				}
			}
			if (condition.MaxLength != null)
			{
				double? length3 = item.Length;
				bool flag2 = length3 != null;
				int? maxLength = condition.MaxLength;
				double? num2 = ((maxLength == null) ? null : new double?((double)maxLength.Value));
				if ((flag2 & (num2 != null)) && length3.GetValueOrDefault() > num2.GetValueOrDefault())
				{
					return false;
				}
			}
			if (condition.Thickness != null || condition.MaxThickness != null)
			{
				Line line = item as Line;
				if (line == null)
				{
					return false;
				}
				if (condition.Thickness != null)
				{
					float? thickness = condition.Thickness;
					if (line.Thickness < thickness)
					{
						return false;
					}
				}
				if (condition.MaxThickness != null)
				{
					float? maxThickness = condition.MaxThickness;
					if (line.Thickness > maxThickness)
					{
						return false;
					}
				}
			}
			if (condition.MinLoad != null || condition.MaxLoad != null)
			{
				if (item.ItemType != ItemTypes.Rod && item.ItemType != ItemTypes.Reel && item.ItemType != ItemTypes.Line && item.ItemType != ItemTypes.Leader)
				{
					return false;
				}
				float itemMaxLoad = MissionInventoryContext.GetItemMaxLoad(item);
				if (condition.MinLoad != null && itemMaxLoad < condition.MinLoad)
				{
					return false;
				}
				if (condition.MaxLoad != null && itemMaxLoad > condition.MaxLoad)
				{
					return false;
				}
			}
			if (condition.FishCageTotalWeight != null)
			{
				FishCage fishCage = item as FishCage;
				if (fishCage == null)
				{
					return false;
				}
				float? fishCageTotalWeight = condition.FishCageTotalWeight;
				if (fishCage.TotalWeight < fishCageTotalWeight)
				{
					return false;
				}
			}
			return true;
		}
	}
}
