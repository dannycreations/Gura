using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectModel
{
	[JsonObject(1)]
	public class InventoryCondition : BaseCondition, IInventoryCondition
	{
		[JsonProperty]
		[JsonConverter(typeof(StringEnumConverter))]
		public ItemTypes? ItemType { get; set; }

		[JsonProperty]
		[JsonConverter(typeof(StringEnumConverter))]
		public ItemSubTypes? ItemSubType { get; set; }

		[JsonProperty]
		public int? CategoryId { get; set; }

		[JsonProperty]
		public int? ItemId { get; set; }

		[JsonProperty]
		public int[] ItemIds { get; set; }

		[JsonProperty]
		public int? Slot { get; set; }

		[JsonProperty]
		[JsonConverter(typeof(StringEnumConverter))]
		public ItemTypes? ParentItemType { get; set; }

		[JsonProperty]
		[JsonConverter(typeof(StringEnumConverter))]
		public ItemSubTypes? ParentItemSubType { get; set; }

		[JsonProperty]
		public int? ParentCategoryId { get; set; }

		[JsonProperty]
		public int? ParentItemId { get; set; }

		[JsonProperty]
		public int[] ParentItemIds { get; set; }

		[JsonProperty]
		public int? ParentSlot { get; set; }

		[JsonProperty]
		[JsonConverter(typeof(StringEnumConverter))]
		public StoragePlaces? ParentStorage { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public StoragePlaces[] ParentStorages { get; set; }

		[JsonProperty]
		[JsonConverter(typeof(StringEnumConverter))]
		public StoragePlaces? Storage { get; set; }

		[JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
		public StoragePlaces[] Storages { get; set; }

		[JsonProperty]
		public int? Durability { get; set; }

		[JsonProperty]
		public int? Count { get; set; }

		[JsonProperty]
		public float? Amount { get; set; }

		[JsonProperty]
		public bool TestChumIngredient { get; set; }

		[JsonProperty]
		public int? ItemsCount { get; set; }

		[JsonProperty]
		public int? Length { get; set; }

		[JsonProperty]
		public int? MaxLength { get; set; }

		[JsonProperty]
		public float? Thickness { get; set; }

		[JsonProperty]
		public float? MaxThickness { get; set; }

		[JsonProperty]
		public float? MinLoad { get; set; }

		[JsonProperty]
		public float? MaxLoad { get; set; }

		[JsonProperty]
		public float? FishCageTotalWeight { get; set; }

		protected override string[] MonitoringDependencies
		{
			get
			{
				List<string> list = new List<string>();
				list.Add("MoveInventory");
				if (this.Count != null)
				{
					list.Add("Inventory");
				}
				if (this.Durability != null)
				{
					list.Add("WearInventory");
					list.Add("RepairInventory");
				}
				return list.ToArray();
			}
		}

		public override bool Check(MissionsContext context)
		{
			List<InventoryItem> list = context.GetProfile().Inventory.Where(delegate(InventoryItem i)
			{
				if (!this.TestChumIngredient)
				{
					return this.Match(i, false);
				}
				Chum chum = i as Chum;
				if (chum == null)
				{
					return false;
				}
				foreach (ChumIngredient chumIngredient in chum.Ingredients)
				{
					if (this.Match(chumIngredient, false))
					{
						return true;
					}
				}
				return false;
			}).ToList<InventoryItem>();
			if (this.ItemsCount != null)
			{
				base.LastProgress = list.Count<InventoryItem>();
			}
			else if (this.Amount != null)
			{
				base.LastProgressFloat = list.Sum((InventoryItem i) => i.Amount);
			}
			else
			{
				base.LastProgress = list.SumInt((InventoryItem i) => i.Count);
			}
			if (this.ItemsCount != null)
			{
				int? itemsCount = this.ItemsCount;
				return base.LastProgress >= itemsCount;
			}
			if (this.Amount != null)
			{
				return base.LastProgressFloat >= this.Amount.Value;
			}
			int lastProgress = base.LastProgress;
			int? count = this.Count;
			return lastProgress >= ((count == null) ? 1 : count.Value);
		}

		public override string GetProgress(MissionsContext context)
		{
			if (!base.ShowProgress)
			{
				return null;
			}
			if (this.ItemsCount != null)
			{
				return string.Format("{0}/{1}", base.LastProgress, this.ItemsCount);
			}
			if (this.Amount != null)
			{
				return string.Format("{0}/{1}", base.LastProgressFloat, this.Amount.Value);
			}
			string text = "{0}/{1}";
			object obj = base.LastProgress;
			int? count = this.Count;
			return string.Format(text, obj, (count == null) ? 1 : count.Value);
		}
	}
}
