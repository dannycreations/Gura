using System;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.Models
{
	public class ExpandableModel : BaseModel
	{
		public string imageURL;

		[NonSerialized]
		public bool expanded;

		[NonSerialized]
		public float nonExpandedSize;
	}
}
