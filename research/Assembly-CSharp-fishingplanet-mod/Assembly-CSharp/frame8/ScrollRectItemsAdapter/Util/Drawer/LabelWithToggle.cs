using System;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util.Drawer
{
	public class LabelWithToggle : MonoBehaviour
	{
		public LabelWithToggle Init(string text = "")
		{
			this.labelText.text = text;
			return this;
		}

		public Text labelText;

		public Toggle toggle;
	}
}
