using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.Models;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample
{
	[Serializable]
	public class MyParams : BaseParams
	{
		public RectTransform bidirectionalPrefab;

		public RectTransform expandablePrefab;

		public float expandableItemExpandFactor = 2f;

		[NonSerialized]
		public List<BaseModel> data = new List<BaseModel>();
	}
}
