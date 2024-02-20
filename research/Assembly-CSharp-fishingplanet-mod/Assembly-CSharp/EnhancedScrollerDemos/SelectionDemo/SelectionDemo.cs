using System;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.SelectionDemo
{
	public class SelectionDemo : MonoBehaviour, IEnhancedScrollerDelegate
	{
		private void Awake()
		{
			Toggle component = GameObject.Find("Mask Toggle").GetComponent<Toggle>();
			this.MaskToggle_OnValueChanged(component.isOn);
			Toggle component2 = GameObject.Find("Loop Toggle").GetComponent<Toggle>();
			this.LoopToggle_OnValueChanged(component2.isOn);
			this.CellViewSelected(null);
		}

		private void Start()
		{
			this.vScroller.Delegate = this;
			this.hScroller.Delegate = this;
			this.Reload();
		}

		private void Reload()
		{
			if (this._data != null)
			{
				for (int i = 0; i < this._data.Count; i++)
				{
					this._data[i].selectedChanged = null;
				}
			}
			this._data = new SmallList<InventoryData>();
			this._data.Add(new InventoryData
			{
				itemName = "Sword",
				itemCost = 123,
				itemDamage = 50,
				itemDefense = 0,
				itemWeight = 10,
				spritePath = this.resourcePath + "/sword",
				itemDescription = "Broadsword with a double-edged blade"
			});
			this._data.Add(new InventoryData
			{
				itemName = "Shield",
				itemCost = 80,
				itemDamage = 0,
				itemDefense = 60,
				itemWeight = 50,
				spritePath = this.resourcePath + "/shield",
				itemDescription = "Steel shield to deflect your enemy's blows"
			});
			this._data.Add(new InventoryData
			{
				itemName = "Amulet",
				itemCost = 260,
				itemDamage = 0,
				itemDefense = 0,
				itemWeight = 1,
				spritePath = this.resourcePath + "/amulet",
				itemDescription = "Magic amulet restores your health points gradually over time"
			});
			this._data.Add(new InventoryData
			{
				itemName = "Helmet",
				itemCost = 50,
				itemDamage = 0,
				itemDefense = 20,
				itemWeight = 20,
				spritePath = this.resourcePath + "/helmet",
				itemDescription = "Standard helm will decrease your vulnerability"
			});
			this._data.Add(new InventoryData
			{
				itemName = "Boots",
				itemCost = 40,
				itemDamage = 0,
				itemDefense = 10,
				itemWeight = 5,
				spritePath = this.resourcePath + "/boots",
				itemDescription = "Boots of speed will double your movement points"
			});
			this._data.Add(new InventoryData
			{
				itemName = "Bracers",
				itemCost = 30,
				itemDamage = 0,
				itemDefense = 20,
				itemWeight = 10,
				spritePath = this.resourcePath + "/bracers",
				itemDescription = "Bracers will upgrade your overall armor"
			});
			this._data.Add(new InventoryData
			{
				itemName = "Crossbow",
				itemCost = 100,
				itemDamage = 40,
				itemDefense = 0,
				itemWeight = 30,
				spritePath = this.resourcePath + "/crossbow",
				itemDescription = "Crossbow can attack from long range"
			});
			this._data.Add(new InventoryData
			{
				itemName = "Fire Ring",
				itemCost = 300,
				itemDamage = 100,
				itemDefense = 0,
				itemWeight = 1,
				spritePath = this.resourcePath + "/fireRing",
				itemDescription = "Fire ring gives you the magical ability to cast fireball spells"
			});
			this._data.Add(new InventoryData
			{
				itemName = "Knapsack",
				itemCost = 22,
				itemDamage = 0,
				itemDefense = 0,
				itemWeight = 0,
				spritePath = this.resourcePath + "/knapsack",
				itemDescription = "Knapsack will increase your carrying capacity by twofold"
			});
			this.vScroller.ReloadData(0f);
			this.hScroller.ReloadData(0f);
		}

		private void CellViewSelected(EnhancedScrollerCellView cellView)
		{
			if (cellView == null)
			{
				this.selectedImage.gameObject.SetActive(false);
				this.selectedImageText.text = "None";
			}
			else
			{
				int dataIndex = (cellView as InventoryCellView).DataIndex;
				for (int i = 0; i < this._data.Count; i++)
				{
					this._data[i].Selected = dataIndex == i;
				}
				this.selectedImage.gameObject.SetActive(true);
				this.selectedImage.sprite = Resources.Load<Sprite>(this._data[dataIndex].spritePath + "_v");
				this.selectedImageText.text = this._data[dataIndex].itemName;
			}
		}

		public void MaskToggle_OnValueChanged(bool val)
		{
			this.vScroller.GetComponent<Mask>().enabled = val;
			this.hScroller.GetComponent<Mask>().enabled = val;
		}

		public void LoopToggle_OnValueChanged(bool val)
		{
			this.vScroller.Loop = val;
			this.hScroller.Loop = val;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._data.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			if (scroller == this.vScroller)
			{
				return 320f;
			}
			return 150f;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			InventoryCellView inventoryCellView = scroller.GetCellView((!(scroller == this.vScroller)) ? this.hCellViewPrefab : this.vCellViewPrefab) as InventoryCellView;
			inventoryCellView.name = ((!(scroller == this.vScroller)) ? "Horizontal" : "Vertical") + " " + this._data[dataIndex].itemName;
			inventoryCellView.selected = new SelectedDelegate(this.CellViewSelected);
			inventoryCellView.SetData(dataIndex, this._data[dataIndex], scroller == this.vScroller);
			return inventoryCellView;
		}

		private SmallList<InventoryData> _data;

		public EnhancedScroller vScroller;

		public EnhancedScroller hScroller;

		public EnhancedScrollerCellView vCellViewPrefab;

		public EnhancedScrollerCellView hCellViewPrefab;

		public Image selectedImage;

		public Text selectedImageText;

		public string resourcePath;
	}
}
