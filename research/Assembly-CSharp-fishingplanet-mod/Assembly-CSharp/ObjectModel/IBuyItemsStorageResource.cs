using System;

namespace ObjectModel
{
	public interface IBuyItemsStorageResource
	{
		StoragePlaces? Storage { get; set; }

		StoragePlaces[] Storages { get; set; }
	}
}
