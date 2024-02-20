using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace EnhancedScrollerDemos.ViewDrivenCellSizes
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
			for (int i = 0; i < 7; i++)
			{
				this._data.Add(new Data
				{
					cellSize = 0f,
					someText = (i * 11).ToString() + " Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam augue enim, scelerisque ac diam nec, efficitur aliquam orci. Vivamus laoreet, libero ut aliquet convallis, dolor elit auctor purus, eget dapibus elit libero at lacus. Aliquam imperdiet sem ultricies ultrices vestibulum. Proin feugiat et dui sit amet ultrices. Quisque porta lacus justo, non ornare nulla eleifend at. Nunc malesuada eget neque sit amet viverra. Donec et lectus ac lorem elementum porttitor. Praesent urna felis, dapibus eu nunc varius, varius tincidunt ante. Vestibulum vitae nulla malesuada, consequat justo eu, dapibus elit. Nulla tristique enim et convallis facilisis."
				});
				this._data.Add(new Data
				{
					cellSize = 0f,
					someText = (i * 11 + 1).ToString() + " Nunc convallis, ipsum a porta viverra, tortor velit feugiat est, eget consectetur ex metus vel diam."
				});
				this._data.Add(new Data
				{
					cellSize = 0f,
					someText = (i * 11 + 2).ToString() + " Phasellus laoreet vitae lectus sit amet venenatis. Duis scelerisque ultricies tincidunt. Cras ullamcorper lectus sed risus porttitor, id viverra urna venenatis. Maecenas in odio sed mi tempus porta et a justo. Nullam non ullamcorper est. Nam rhoncus nulla quis commodo aliquam. Maecenas pulvinar est sed ex iaculis, eu pretium tellus placerat. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Praesent in ipsum faucibus, fringilla lectus id, congue est. "
				});
				this._data.Add(new Data
				{
					cellSize = 0f,
					someText = (i * 11 + 3).ToString() + " Fusce ex lectus."
				});
				this._data.Add(new Data
				{
					cellSize = 0f,
					someText = (i * 11 + 4).ToString() + " Fusce mollis elementum sem euismod malesuada. Aenean et convallis turpis. Suspendisse potenti."
				});
				this._data.Add(new Data
				{
					cellSize = 0f,
					someText = (i * 11 + 5).ToString() + " Fusce nec sapien orci. Pellentesque mollis ligula vitae interdum imperdiet. Aenean ultricies velit at turpis luctus, nec lacinia ligula malesuada. Nulla facilisi. Donec at nisi lorem. Aenean vestibulum velit velit, sed eleifend dui sodales in. Nunc vulputate, nulla non facilisis hendrerit, neque dolor lacinia orci, et fermentum nunc quam vel purus. Donec gravida massa non ullamcorper consectetur. Sed pellentesque leo ac ornare egestas. "
				});
				this._data.Add(new Data
				{
					cellSize = 0f,
					someText = (i * 11 + 6).ToString() + " Curabitur non dignissim turpis, vel viverra elit. Cras in sem rhoncus, gravida velit ut, consectetur erat. Proin ac aliquet nulla. Mauris quis augue nisi. Sed purus magna, mollis sed massa ac, scelerisque lobortis leo. Nullam at facilisis ex. Nullam ut accumsan orci. Integer vitae dictum felis, quis tristique sem. Suspendisse potenti. Curabitur bibendum eleifend eros at porta. Ut malesuada consectetur arcu nec lacinia. "
				});
				this._data.Add(new Data
				{
					cellSize = 0f,
					someText = (i * 11 + 7).ToString() + " Pellentesque pulvinar ac arcu fermentum interdum. Pellentesque gravida faucibus ipsum at blandit. Vestibulum pharetra erat sit amet feugiat sodales. Nunc et dui viverra tellus efficitur egestas. Sed ex mauris, eleifend in nisi sed, consequat tincidunt elit. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Proin vel bibendum enim. Etiam feugiat nulla ac dui commodo, eget vehicula est scelerisque. In metus neque, congue a justo ac, consequat lacinia neque. Vivamus non velit vitae ex dictum pharetra. Aliquam blandit nisi eget libero feugiat porta. "
				});
				this._data.Add(new Data
				{
					cellSize = 0f,
					someText = (i * 11 + 8).ToString() + " Proin bibendum ligula a pulvinar convallis. Mauris tincidunt tempor ipsum id viverra. Vivamus congue ipsum venenatis tellus semper, vel venenatis mauris finibus. Vivamus a nisl in lacus fermentum varius. Mauris bibendum magna placerat risus interdum, vitae facilisis nulla pellentesque. Curabitur vehicula odio quis magna pulvinar, et lacinia ante bibendum. Morbi laoreet eleifend ante, quis luctus augue luctus sit amet. Sed consectetur enim et orci posuere euismod. Curabitur sollicitudin metus eu nisl dictum suscipit. "
				});
				this._data.Add(new Data
				{
					cellSize = 0f,
					someText = (i * 11 + 9).ToString() + " Sed gravida augue ligula, tempus auctor ante rutrum sit amet. Vestibulum finibus magna ut viverra rhoncus. Vestibulum rutrum eu nibh interdum imperdiet. Curabitur ac nunc a turpis ultricies dictum. Phasellus in molestie eros. Morbi porta imperdiet odio sed pharetra. Cras blandit tincidunt ultricies. "
				});
				this._data.Add(new Data
				{
					cellSize = 0f,
					someText = (i * 11 + 10).ToString() + " Integer pellentesque viverra orci, sollicitudin luctus dui rhoncus sed. Duis placerat at felis vel placerat. Mauris massa urna, scelerisque vitae posuere vitae, ultrices in nibh. Mauris posuere hendrerit viverra. In lacinia urna nibh, ut lobortis lectus finibus et. Aliquam arcu dolor, suscipit eget massa id, eleifend dapibus est. Quisque eget bibendum urna. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed condimentum pulvinar ornare. Aliquam venenatis eget nunc et euismod. "
				});
			}
			this.ResizeScroller();
		}

		public void AddNewRow()
		{
			this.scroller.ClearAll();
			this.scroller.ScrollPosition = 0f;
			foreach (Data data in this._data)
			{
				data.cellSize = 0f;
			}
			this._data.Add(new Data
			{
				cellSize = 0f,
				someText = this._data.Count.ToString() + " New Row Added!"
			});
			this.ResizeScroller();
			this.scroller.JumpToDataIndex(this._data.Count - 1, 1f, 1f, true, EnhancedScroller.TweenType.immediate, 0f, null, EnhancedScroller.LoopJumpDirectionEnum.Closest);
		}

		private void ResizeScroller()
		{
			RectTransform component = this.scroller.GetComponent<RectTransform>();
			Vector2 sizeDelta = component.sizeDelta;
			component.sizeDelta = new Vector2(sizeDelta.x, float.MaxValue);
			this.scroller.ReloadData(0f);
			component.sizeDelta = sizeDelta;
			this.scroller.ReloadData(0f);
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._data.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return this._data[dataIndex].cellSize;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			CellView cellView = scroller.GetCellView(this.cellViewPrefab) as CellView;
			cellView.SetData(this._data[dataIndex]);
			return cellView;
		}

		private List<Data> _data;

		public EnhancedScroller scroller;

		public EnhancedScrollerCellView cellViewPrefab;
	}
}
