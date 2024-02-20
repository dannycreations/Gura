using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class BaseParamsWithPrefabAndData<TData> : BaseParamsWithPrefab
	{
		[NonSerialized]
		public List<TData> Data = new List<TData>();
	}
}
