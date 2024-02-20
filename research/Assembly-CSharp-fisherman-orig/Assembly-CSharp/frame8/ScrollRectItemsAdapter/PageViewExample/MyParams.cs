using System;
using frame8.ScrollRectItemsAdapter.Util;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.PageViewExample
{
	[Serializable]
	public class MyParams : BaseParamsWithPrefabAndData<PageModel>
	{
		public Sprite[] availableImages;
	}
}
