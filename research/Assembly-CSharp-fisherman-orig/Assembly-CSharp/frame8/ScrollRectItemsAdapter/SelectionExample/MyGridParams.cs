using System;
using frame8.ScrollRectItemsAdapter.Util.GridView;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.SelectionExample
{
	[Serializable]
	public class MyGridParams : GridParams
	{
		public Button deleteButton;

		public Button cancelButton;

		public bool autoSelectFirstOnSelectionMode = true;

		public bool keepSelectionModeAfterDeletion = true;
	}
}
