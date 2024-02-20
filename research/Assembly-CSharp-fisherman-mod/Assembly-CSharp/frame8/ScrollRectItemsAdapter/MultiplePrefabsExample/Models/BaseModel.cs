using System;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.Models
{
	public abstract class BaseModel
	{
		public BaseModel()
		{
			this.CachedType = base.GetType();
		}

		public Type CachedType { get; private set; }

		public int id;
	}
}
