using System;
using frame8.ScrollRectItemsAdapter.Util;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.IncrementalItemFetchExample2
{
	[Serializable]
	public class MyParams : BaseParamsWithPrefabAndData<ExampleItemModel>
	{
		public Text statusText;

		public int preFetchedItemsCount;

		[Tooltip("Set to -1 if while fetching <preFetchedItemsCount> items, the adapter shouldn't check for a capacity limit")]
		public int totalCapacity;
	}
}
