using System;

namespace ObjectModel
{
	public interface IInventoryCondition
	{
		ItemTypes? ItemType { get; }

		ItemSubTypes? ItemSubType { get; }

		int? CategoryId { get; }

		int? ItemId { get; }

		int[] ItemIds { get; }

		int? Slot { get; }

		ItemTypes? ParentItemType { get; }

		ItemSubTypes? ParentItemSubType { get; }

		int? ParentCategoryId { get; }

		int? ParentItemId { get; }

		int[] ParentItemIds { get; }

		int? ParentSlot { get; }

		StoragePlaces? ParentStorage { get; }

		StoragePlaces[] ParentStorages { get; }

		StoragePlaces? Storage { get; }

		StoragePlaces[] Storages { get; }

		int? Durability { get; }

		int? Count { get; }

		float? Amount { get; }

		int? ItemsCount { get; }

		int? Length { get; }

		int? MaxLength { get; }

		float? Thickness { get; set; }

		float? MaxThickness { get; set; }

		float? MinLoad { get; }

		float? MaxLoad { get; }

		float? FishCageTotalWeight { get; set; }
	}
}
