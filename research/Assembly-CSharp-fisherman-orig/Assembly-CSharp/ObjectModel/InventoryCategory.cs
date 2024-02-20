using System;
using Newtonsoft.Json;

namespace ObjectModel
{
	[JsonObject(0)]
	public class InventoryCategory
	{
		public int CategoryId { get; set; }

		public int? ParentCategoryId { get; set; }

		public string Name { get; set; }

		[JsonIgnore]
		public InventoryCategory ParentCategory { get; set; }

		[JsonIgnore]
		public ItemTypes ItemType
		{
			get
			{
				if (this.ParentCategoryId == null)
				{
					return (ItemTypes)this.CategoryId;
				}
				return (ItemTypes)this.ParentCategoryId.Value;
			}
		}

		[JsonIgnore]
		public ItemSubTypes? ItemSubType
		{
			get
			{
				return new ItemSubTypes?((ItemSubTypes)this.CategoryId);
			}
		}
	}
}
