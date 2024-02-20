using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.SimpleTutorialExample
{
	public class ExampleItemModel
	{
		public string title;

		public int icon1Index;

		public int icon2Index;

		public bool expanded;

		public float nonExpandedSize;

		public readonly Color color = Utils.GetRandomColor(false);
	}
}
