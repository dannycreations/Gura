using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.SimpleLoopingSpinnerExample
{
	[Serializable]
	public class MyParams : BaseParamsWithPrefab
	{
		public int GetItemValueAtIndex(int index)
		{
			return this.startItemNumber + this.increment * index;
		}

		public int startItemNumber;

		public int increment = 1;

		public Color selectedColor;

		public Color nonSelectedColor;

		public Text currentSelectedIndicatorText;
	}
}
