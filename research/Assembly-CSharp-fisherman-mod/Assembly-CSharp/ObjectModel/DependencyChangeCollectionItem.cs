using System;
using System.Collections.Generic;

namespace ObjectModel
{
	public class DependencyChangeCollectionItem<T> : DependencyChange, IDependencyChangeCollectionItem, IDependencyChange
	{
		public string Operation { get; set; }

		public IEnumerable<T> Collection { get; set; }

		public T Item { get; set; }

		public T OldItem { get; set; }

		public bool IsAdded { get; set; }

		public bool IsUpdated { get; set; }

		public bool IsRemoved { get; set; }

		object IDependencyChange.CurrentValue
		{
			get
			{
				return this.Collection;
			}
		}

		bool IDependencyChange.IsChanged
		{
			get
			{
				return true;
			}
		}

		public override string ToString()
		{
			return string.Format("CollectionDependencyChange '{0}': item = {1}, operation = {2}", base.Name, this.Item, this.Operation);
		}
	}
}
