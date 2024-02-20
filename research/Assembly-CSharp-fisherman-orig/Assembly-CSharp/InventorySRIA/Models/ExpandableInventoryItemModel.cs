using System;
using ObjectModel;

namespace InventorySRIA.Models
{
	public class ExpandableInventoryItemModel : BaseModel
	{
		public InventoryItem item;

		public DragNDropType TypeObjectOfDragItem;

		public StoragePlaces Places;

		public InitRod ActiveRod;

		[NonSerialized]
		public bool expanded;

		[NonSerialized]
		public float nonExpandedSize = 80f;

		[NonSerialized]
		public float ExpandedSize = 380f;
	}
}
