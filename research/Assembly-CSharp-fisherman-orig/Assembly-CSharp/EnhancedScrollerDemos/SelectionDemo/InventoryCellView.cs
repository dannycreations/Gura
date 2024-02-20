using System;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.SelectionDemo
{
	public class InventoryCellView : EnhancedScrollerCellView
	{
		public int DataIndex { get; private set; }

		private void OnDestroy()
		{
			if (this._data != null)
			{
				InventoryData data = this._data;
				data.selectedChanged = (SelectedChangedDelegate)Delegate.Remove(data.selectedChanged, new SelectedChangedDelegate(this.SelectedChanged));
			}
		}

		public void SetData(int dataIndex, InventoryData data, bool isVertical)
		{
			if (this._data != null)
			{
				InventoryData data2 = this._data;
				data2.selectedChanged = (SelectedChangedDelegate)Delegate.Remove(data2.selectedChanged, new SelectedChangedDelegate(this.SelectedChanged));
			}
			this.DataIndex = dataIndex;
			this._data = data;
			this.itemNameText.text = data.itemName;
			if (this.itemCostText != null)
			{
				this.itemCostText.text = ((data.itemCost <= 0) ? "-" : data.itemCost.ToString());
			}
			if (this.itemDamageText != null)
			{
				this.itemDamageText.text = ((data.itemDamage <= 0) ? "-" : data.itemDamage.ToString());
			}
			if (this.itemDefenseText != null)
			{
				this.itemDefenseText.text = ((data.itemDefense <= 0) ? "-" : data.itemDefense.ToString());
			}
			if (this.itemWeightText != null)
			{
				this.itemWeightText.text = ((data.itemWeight <= 0) ? "-" : data.itemWeight.ToString());
			}
			if (isVertical)
			{
				this.itemDescriptionText.text = data.itemDescription;
			}
			this.image.sprite = Resources.Load<Sprite>(data.spritePath + ((!isVertical) ? "_h" : "_v"));
			InventoryData data3 = this._data;
			data3.selectedChanged = (SelectedChangedDelegate)Delegate.Remove(data3.selectedChanged, new SelectedChangedDelegate(this.SelectedChanged));
			InventoryData data4 = this._data;
			data4.selectedChanged = (SelectedChangedDelegate)Delegate.Combine(data4.selectedChanged, new SelectedChangedDelegate(this.SelectedChanged));
			this.SelectedChanged(data.Selected);
		}

		private void SelectedChanged(bool selected)
		{
			this.selectionPanel.color = ((!selected) ? this.unSelectedColor : this.selectedColor);
		}

		public void OnSelected()
		{
			if (this.selected != null)
			{
				this.selected(this);
			}
		}

		private InventoryData _data;

		public Image selectionPanel;

		public Text itemNameText;

		public Text itemCostText;

		public Text itemDamageText;

		public Text itemDefenseText;

		public Text itemWeightText;

		public Text itemDescriptionText;

		public Image image;

		public Color selectedColor;

		public Color unSelectedColor;

		public SelectedDelegate selected;
	}
}
