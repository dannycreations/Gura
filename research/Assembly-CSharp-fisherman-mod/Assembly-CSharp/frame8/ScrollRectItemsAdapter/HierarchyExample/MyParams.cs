using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.HierarchyExample
{
	[Serializable]
	public class MyParams : BaseParamsWithPrefab
	{
		[Range(0f, 10f)]
		public int maxHierarchyDepth;

		public bool animatedFoldOut = true;

		public FSEntryNodeModel hierarchyRootNode;

		public List<FSEntryNodeModel> flattenedVisibleHierarchy;
	}
}
