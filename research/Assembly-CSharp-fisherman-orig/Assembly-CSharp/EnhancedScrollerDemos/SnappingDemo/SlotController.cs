using System;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace EnhancedScrollerDemos.SnappingDemo
{
	public class SlotController : MonoBehaviour, IEnhancedScrollerDelegate
	{
		private void Awake()
		{
			this._data = new SmallList<SlotData>();
		}

		private void Start()
		{
			this.scroller.Delegate = this;
		}

		public void Reload(Sprite[] sprites)
		{
			this._data.Clear();
			foreach (Sprite sprite in sprites)
			{
				this._data.Add(new SlotData
				{
					sprite = sprite
				});
			}
			this.scroller.ReloadData(0f);
		}

		public void AddVelocity(float amount)
		{
			this.scroller.LinearVelocity = amount;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._data.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return 150f;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			SlotCellView slotCellView = scroller.GetCellView(this.slotCellViewPrefab) as SlotCellView;
			slotCellView.SetData(this._data[dataIndex]);
			return slotCellView;
		}

		private SmallList<SlotData> _data;

		public EnhancedScroller scroller;

		public EnhancedScrollerCellView slotCellViewPrefab;
	}
}
