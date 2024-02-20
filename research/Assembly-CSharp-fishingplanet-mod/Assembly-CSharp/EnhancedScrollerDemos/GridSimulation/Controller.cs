using System;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace EnhancedScrollerDemos.GridSimulation
{
	public class Controller : MonoBehaviour, IEnhancedScrollerDelegate
	{
		private void Start()
		{
			this.scroller.Delegate = this;
			this.LoadData();
		}

		private void LoadData()
		{
			this._data = new SmallList<Data>();
			for (int i = 0; i < 1000; i++)
			{
				this._data.Add(new Data
				{
					someText = i.ToString()
				});
			}
			this.scroller.ReloadData(0f);
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return Mathf.CeilToInt((float)this._data.Count / (float)this.numberOfCellsPerRow);
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return 100f;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			CellView cellView = scroller.GetCellView(this.cellViewPrefab) as CellView;
			cellView.name = "Cell " + (dataIndex * this.numberOfCellsPerRow).ToString() + " to " + (dataIndex * this.numberOfCellsPerRow + this.numberOfCellsPerRow - 1).ToString();
			cellView.SetData(ref this._data, dataIndex * this.numberOfCellsPerRow);
			return cellView;
		}

		private SmallList<Data> _data;

		public EnhancedScroller scroller;

		public EnhancedScrollerCellView cellViewPrefab;

		public int numberOfCellsPerRow = 3;
	}
}
