using System;
using ObjectModel;

namespace InventorySRIA.Models
{
	public class HeaderGroupModel : BaseModel
	{
		public string GroupName;

		public string GroupIcon;

		public bool Opened;

		public InventoryItem[] Items;

		public GroupCategoryType GCT;
	}
}
