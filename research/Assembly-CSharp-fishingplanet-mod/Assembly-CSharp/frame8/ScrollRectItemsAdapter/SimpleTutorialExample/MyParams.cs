using System;
using frame8.ScrollRectItemsAdapter.Util;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.SimpleTutorialExample
{
	[Serializable]
	public class MyParams : BaseParamsWithPrefabAndLazyData<ExampleItemModel>
	{
		public Texture2D[] availableIcons;

		public int initialSimulatedServerDelay;
	}
}
