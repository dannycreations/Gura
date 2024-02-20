using System;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.PullDownRefresh
{
	public class Controller : MonoBehaviour, IEnhancedScrollerDelegate, IBeginDragHandler, IEndDragHandler, IEventSystemHandler
	{
		private void Start()
		{
			this.scroller.Delegate = this;
			this.scroller.scrollerScrolled = new ScrollerScrolledDelegate(this.ScrollerScrolled);
			this.LoadLargeData();
		}

		private void LoadLargeData()
		{
			this._data = new SmallList<Data>();
			for (int i = 0; i < 100; i++)
			{
				this._data.Add(new Data
				{
					someText = "Cell Data Index " + i.ToString()
				});
			}
			this.scroller.ReloadData(0f);
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

		private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
		{
			bool flag = scrollPosition <= -this.pullDownThreshold || scrollPosition == 0f;
			if (this._dragging && flag)
			{
				this._pullToRefresh = true;
				this.releaseToRefreshText.gameObject.SetActive(true);
			}
			this.pullDownToRefreshText.gameObject.SetActive(scrollPosition <= 0f);
		}

		public void OnBeginDrag(PointerEventData data)
		{
			this._dragging = true;
		}

		public void OnEndDrag(PointerEventData data)
		{
			this._dragging = false;
			if (this._pullToRefresh)
			{
				for (int i = 0; i < 3; i++)
				{
					this._data.Insert(new Data
					{
						someText = "Brand New Data " + i.ToString() + "!!!"
					}, 0);
				}
				this.scroller.ReloadData(0f);
				this._pullToRefresh = false;
				this.releaseToRefreshText.gameObject.SetActive(false);
			}
		}

		private SmallList<Data> _data;

		private bool _dragging = true;

		private bool _pullToRefresh;

		public EnhancedScroller scroller;

		public EnhancedScrollerCellView cellViewPrefab;

		public float pullDownThreshold;

		public Text pullDownToRefreshText;

		public Text releaseToRefreshText;
	}
}
