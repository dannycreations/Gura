using System;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace EnhancedScrollerDemos.RefreshDemo
{
	public class Controller : MonoBehaviour, IEnhancedScrollerDelegate
	{
		private void Start()
		{
			this.scroller.Delegate = this;
			this.LoadLargeData();
		}

		private void Update()
		{
			if (Input.GetKeyDown(114))
			{
				this._data[0].someText = "This cell was updated";
				this._data[1].someText = "---";
				this._data[2].someText = "---";
				this._data[3].someText = "---";
				this._data[4].someText = "---";
				this._data[5].someText = "This cell was also updated";
				this.scroller.RefreshActiveCellViews();
			}
		}

		private void LoadLargeData()
		{
			this._data = new SmallList<Data>();
			for (int i = 0; i < 1000; i++)
			{
				this._data.Add(new Data
				{
					someText = "Cell Data Index " + i.ToString()
				});
			}
			this.scroller.ReloadData(0f);
		}

		private void LoadSmallData()
		{
			this._data = new SmallList<Data>();
			this._data.Add(new Data
			{
				someText = "A"
			});
			this._data.Add(new Data
			{
				someText = "B"
			});
			this._data.Add(new Data
			{
				someText = "C"
			});
			this.scroller.ReloadData(0f);
		}

		public void LoadLargeDataButton_OnClick()
		{
			this.LoadLargeData();
		}

		public void LoadSmallDataButton_OnClick()
		{
			this.LoadSmallData();
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._data.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return (dataIndex % 2 != 0) ? 100f : 30f;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			CellView cellView = scroller.GetCellView(this.cellViewPrefab) as CellView;
			cellView.name = "Cell " + dataIndex.ToString();
			cellView.SetData(this._data[dataIndex]);
			return cellView;
		}

		private SmallList<Data> _data;

		public EnhancedScroller scroller;

		public EnhancedScrollerCellView cellViewPrefab;
	}
}
