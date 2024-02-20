using System;
using frame8.ScrollRectItemsAdapter.Util;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.ContentSizeFitterExample
{
	[Serializable]
	public class MyParams : BaseParamsWithPrefabAndLazyData<ExampleItemModel>
	{
		public Texture2D[] availableIcons;
	}
}
