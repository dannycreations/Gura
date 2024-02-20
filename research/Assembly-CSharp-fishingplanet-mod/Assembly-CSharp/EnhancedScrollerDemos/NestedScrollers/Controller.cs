using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace EnhancedScrollerDemos.NestedScrollers
{
	public class Controller : MonoBehaviour, IEnhancedScrollerDelegate
	{
		private void Start()
		{
			this.masterScroller.Delegate = this;
			this.LoadData();
		}

		private void LoadData()
		{
			this._data = new List<MasterData>();
			for (int i = 0; i < 1000; i++)
			{
				MasterData masterData = new MasterData
				{
					normalizedScrollPosition = 0f,
					childData = new List<DetailData>()
				};
				this._data.Add(masterData);
				for (int j = 0; j < 20; j++)
				{
					masterData.childData.Add(new DetailData
					{
						someText = i.ToString() + "," + j.ToString()
					});
				}
			}
			this.masterScroller.ReloadData(0f);
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._data.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return 100f;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			MasterCellView masterCellView = scroller.GetCellView(this.masterCellViewPrefab) as MasterCellView;
			masterCellView.name = "Master Cell " + dataIndex.ToString();
			masterCellView.SetData(this._data[dataIndex]);
			return masterCellView;
		}

		private List<MasterData> _data;

		public EnhancedScroller masterScroller;

		public EnhancedScrollerCellView masterCellViewPrefab;
	}
}
