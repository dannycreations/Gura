using System;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util.Drawer
{
	public class ButtonsPanel : MonoBehaviour
	{
		public bool Interactable
		{
			set
			{
				Selectable selectable = this.button1;
				this.button4.interactable = value;
				this.button3.interactable = value;
				this.button2.interactable = value;
				selectable.interactable = value;
			}
		}

		internal void Init(string label1, string label2, string label3, string label4)
		{
			this.button1.transform.GetComponentAtPath("Text").text = label1;
			this.button2.transform.GetComponentAtPath("Text").text = label2;
			this.button3.transform.GetComponentAtPath("Text").text = label3;
			this.button4.transform.GetComponentAtPath("Text").text = label4;
			this.button1.gameObject.SetActive(!string.IsNullOrEmpty(label1));
			this.button2.gameObject.SetActive(!string.IsNullOrEmpty(label2));
			this.button3.gameObject.SetActive(!string.IsNullOrEmpty(label3));
			this.button4.gameObject.SetActive(!string.IsNullOrEmpty(label4));
		}

		public Button button1;

		public Button button2;

		public Button button3;

		public Button button4;
	}
}
