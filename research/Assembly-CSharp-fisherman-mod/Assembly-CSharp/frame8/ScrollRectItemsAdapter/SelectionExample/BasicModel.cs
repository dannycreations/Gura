using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.SelectionExample
{
	public class BasicModel
	{
		public int id;

		public readonly Color color = Utils.GetRandomColor(true);

		public bool isSelected;
	}
}
