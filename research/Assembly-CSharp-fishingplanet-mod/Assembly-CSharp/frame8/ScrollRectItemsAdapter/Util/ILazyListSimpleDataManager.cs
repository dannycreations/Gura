using System;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public interface ILazyListSimpleDataManager<TItem>
	{
		void Refresh();

		LazyList<TItem> Data { get; }
	}
}
