using System;
using UnityEngine;

namespace Kender.uGUI
{
	public class TestComboBox : MonoBehaviour
	{
		private void Start()
		{
			ComboBoxItem itemMakeBig = new ComboBoxItem("Make me big!");
			ComboBoxItem itemMakeNormal = new ComboBoxItem("Normal", this.image, true);
			ComboBoxItem itemMakeSmall = new ComboBoxItem("Make me small!");
			ComboBoxItem itemMakeBig2 = itemMakeBig;
			itemMakeBig2.OnSelect = (Action)Delegate.Combine(itemMakeBig2.OnSelect, new Action(delegate
			{
				this.comboBox.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(0, 180f);
				this.comboBox.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(1, 40f);
				this.comboBox.UpdateGraphics();
				itemMakeBig.Caption = "Big";
				itemMakeBig.IsDisabled = true;
				itemMakeNormal.Caption = "Make me normal!";
				itemMakeNormal.IsDisabled = false;
				itemMakeSmall.Caption = "Make me small!";
				itemMakeSmall.IsDisabled = false;
			}));
			ComboBoxItem itemMakeNormal2 = itemMakeNormal;
			itemMakeNormal2.OnSelect = (Action)Delegate.Combine(itemMakeNormal2.OnSelect, new Action(delegate
			{
				this.comboBox.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(0, 160f);
				this.comboBox.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(1, 30f);
				this.comboBox.UpdateGraphics();
				itemMakeBig.Caption = "Make me big!";
				itemMakeBig.IsDisabled = false;
				itemMakeNormal.Caption = "Normal";
				itemMakeNormal.IsDisabled = true;
				itemMakeSmall.Caption = "Make me small!";
				itemMakeSmall.IsDisabled = false;
			}));
			ComboBoxItem itemMakeSmall2 = itemMakeSmall;
			itemMakeSmall2.OnSelect = (Action)Delegate.Combine(itemMakeSmall2.OnSelect, new Action(delegate
			{
				this.comboBox.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(0, 160f);
				this.comboBox.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(1, 20f);
				this.comboBox.UpdateGraphics();
				itemMakeBig.Caption = "Make me big!";
				itemMakeBig.IsDisabled = false;
				itemMakeNormal.Caption = "Make me normal!";
				itemMakeNormal.IsDisabled = false;
				itemMakeSmall.Caption = "Small";
				itemMakeSmall.IsDisabled = true;
			}));
			this.comboBox.AddItems(new object[] { itemMakeBig, itemMakeNormal, itemMakeSmall });
			this.comboBox.SelectedIndex = 1;
			ComboBox comboBox = this.comboBox;
			comboBox.OnSelectionChanged = (Action<int>)Delegate.Combine(comboBox.OnSelectionChanged, new Action<int>(delegate(int index)
			{
				Camera.main.backgroundColor = new Color32((byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)Random.Range(0, 256), byte.MaxValue);
			}));
		}

		public ComboBox comboBox;

		public Sprite image;
	}
}
