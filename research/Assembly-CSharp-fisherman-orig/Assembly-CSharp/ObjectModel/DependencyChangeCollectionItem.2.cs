using System;

namespace ObjectModel
{
	public class DependencyChangeCollectionItem<T, E> : DependencyChangeCollectionItem<T>
	{
		public E Event { get; set; }

		public override string ToString()
		{
			return string.Format("CollectionDependencyChange '{0}': item = {1}, operation = {2}, event = {3}", new object[] { base.Name, base.Item, base.Operation, this.Event });
		}
	}
}
