using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;

namespace ScrollSelectorSRIA
{
	[Serializable]
	public class MyParams<T> : BaseParamsWithPrefab where T : IScrollSelectorElement
	{
		[NonSerialized]
		public List<IScrollSelectorElement> Data = new List<IScrollSelectorElement>();
	}
}
