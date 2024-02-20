using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public abstract class BaseParamsWithPrefabAndLazyData<TData> : BaseParamsWithPrefab
	{
		public LazyList<TData> Data { get; set; }

		public Func<int, TData> NewModelCreator { get; set; }

		public override void InitIfNeeded(ISRIA sria)
		{
			base.InitIfNeeded(sria);
			if (this.Data == null)
			{
				this.Data = new LazyList<TData>(this.NewModelCreator, 0);
			}
		}
	}
}
