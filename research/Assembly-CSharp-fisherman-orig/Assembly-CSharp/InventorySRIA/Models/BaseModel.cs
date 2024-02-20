using System;

namespace InventorySRIA.Models
{
	public abstract class BaseModel
	{
		public BaseModel()
		{
			this.CachedType = base.GetType();
		}

		public Type CachedType { get; private set; }

		public int id;

		public DropMeStorage Storage;
	}
}
