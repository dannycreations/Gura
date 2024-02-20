using System;

namespace ObjectModel
{
	public interface IDependencyChangeCollectionItem : IDependencyChange
	{
		string Operation { get; set; }

		bool IsAdded { get; set; }

		bool IsUpdated { get; set; }

		bool IsRemoved { get; set; }
	}
}
