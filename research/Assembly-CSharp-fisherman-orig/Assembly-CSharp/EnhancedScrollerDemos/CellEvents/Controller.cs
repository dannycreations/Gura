using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace EnhancedScrollerDemos.CellEvents
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
			this._data = new List<Data>();
			for (int i = 0; i < 24; i++)
			{
				this._data.Add(new Data
				{
					hour = i
				});
			}
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._data.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return this.cellSize;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			CellView cellView = scroller.GetCellView(this.cellViewPrefab) as CellView;
			cellView.cellButtonTextClicked = new CellButtonTextClickedDelegate(this.CellButtonTextClicked);
			cellView.cellButtonFixedIntegerClicked = new CellButtonIntegerClickedDelegate(this.CellButtonFixedIntegerClicked);
			cellView.cellButtonDataIntegerClicked = new CellButtonIntegerClickedDelegate(this.CellButtonDataIntegerClicked);
			cellView.SetData(this._data[dataIndex]);
			return cellView;
		}

		private void CellButtonTextClicked(string value)
		{
			Debug.Log("Cell Text Button Clicked! Value = " + value);
		}

		private void CellButtonFixedIntegerClicked(int value)
		{
			Debug.Log("Cell Fixed Integer Button Clicked! Value = " + value);
		}

		private void CellButtonDataIntegerClicked(int value)
		{
			Debug.Log("Cell Data Integer Button Clicked! Value = " + value);
		}

		private List<Data> _data;

		public EnhancedScroller scroller;

		public EnhancedScrollerCellView cellViewPrefab;

		public float cellSize;
	}
}
