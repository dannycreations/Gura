using System;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util.Drawer
{
	public class WithInputPanel : MonoBehaviour
	{
		public float InputFieldValueAsFloat
		{
			get
			{
				return float.Parse(this.inputField.text);
			}
		}

		public int InputFieldValueAsInt
		{
			get
			{
				return int.Parse(this.inputField.text);
			}
		}

		public InputField inputField;
	}
}
