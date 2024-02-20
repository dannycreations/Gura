using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.SimpleLoopingSpinnerExample
{
	public class SimpleLoopingSpinnerExample : SRIA<MyParams, MyItemViewsHolder>
	{
		protected override void Start()
		{
			base.Start();
			DrawerCommandPanel.Instance.Init(this, false, false, false, false, true);
			DrawerCommandPanel.Instance.galleryEffectSetting.slider.value = 0.2f;
			DrawerCommandPanel.Instance.addRemoveOnePanel.button2.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.addRemoveOnePanel.button4.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.ItemCountChangeRequested += this.ChangeItemsCountWithChecks;
			DrawerCommandPanel.Instance.AddItemRequested += delegate(bool _)
			{
				this.ChangeItemsCountWithChecks(this.GetItemsCount() + 1);
			};
			DrawerCommandPanel.Instance.RemoveItemRequested += delegate(bool _)
			{
				this.ChangeItemsCountWithChecks(this.GetItemsCount() - 1);
			};
			DrawerCommandPanel.Instance.RequestChangeItemCountToSpecified();
		}

		protected override void Update()
		{
			base.Update();
			this._Params.currentSelectedIndicatorText.text = "Selected: ";
			if (this._VisibleItemsCount == 0)
			{
				return;
			}
			int num = this._VisibleItemsCount / 2;
			MyItemViewsHolder myItemViewsHolder = this._VisibleItems[num];
			Text currentSelectedIndicatorText = this._Params.currentSelectedIndicatorText;
			currentSelectedIndicatorText.text += this._Params.GetItemValueAtIndex(myItemViewsHolder.ItemIndex);
			myItemViewsHolder.background.color = this._Params.selectedColor;
			for (int i = 0; i < this._VisibleItemsCount; i++)
			{
				if (i != num)
				{
					this._VisibleItems[i].background.color = this._Params.nonSelectedColor;
				}
			}
		}

		protected override MyItemViewsHolder CreateViewsHolder(int itemIndex)
		{
			MyItemViewsHolder myItemViewsHolder = new MyItemViewsHolder();
			myItemViewsHolder.Init(this._Params.itemPrefab, itemIndex, true, true);
			return myItemViewsHolder;
		}

		protected override void UpdateViewsHolder(MyItemViewsHolder newOrRecycled)
		{
			newOrRecycled.titleText.text = this._Params.GetItemValueAtIndex(newOrRecycled.ItemIndex) + string.Empty;
		}

		private void ChangeItemsCountWithChecks(int newCount)
		{
			int num = 4;
			if (newCount < num)
			{
				newCount = num;
			}
			DrawerCommandPanel.Instance.setCountPanel.inputField.text = newCount + string.Empty;
			DrawerCommandPanel.Instance.addRemoveOnePanel.button3.interactable = newCount > num;
			this.ResetItems(newCount, false, false);
		}
	}
}
