using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.JumpToDemo
{
	public class Controller : MonoBehaviour, IEnhancedScrollerDelegate
	{
		private void Start()
		{
			this.vScroller.Delegate = this;
			this.hScroller.Delegate = this;
			this._data = new List<Data>();
			for (int i = 0; i < 100; i++)
			{
				this._data.Add(new Data
				{
					cellText = "Cell Data Index " + i.ToString()
				});
			}
			this.vScroller.ReloadData(0f);
			this.hScroller.ReloadData(0f);
		}

		public void JumpButton_OnClick()
		{
			int num;
			if (int.TryParse(this.jumpIndexInput.text, out num))
			{
				this.vScroller.JumpToDataIndex(num, this.scrollerOffsetSlider.value, this.cellOffsetSlider.value, this.useSpacingToggle.isOn, this.vScrollerTweenType, this.vScrollerTweenTime, null, EnhancedScroller.LoopJumpDirectionEnum.Down);
				this.hScroller.JumpToDataIndex(num, this.scrollerOffsetSlider.value, this.cellOffsetSlider.value, this.useSpacingToggle.isOn, this.hScrollerTweenType, this.hScrollerTweenTime, null, EnhancedScroller.LoopJumpDirectionEnum.Closest);
			}
			else
			{
				Debug.LogWarning("The jump value you entered is not a number.");
			}
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._data.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			if (scroller == this.vScroller)
			{
				return (dataIndex % 2 != 0) ? 100f : 30f;
			}
			return 200f;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			CellView cellView = scroller.GetCellView(this.cellViewPrefab) as CellView;
			cellView.name = "Cell " + dataIndex.ToString();
			cellView.SetData(this._data[dataIndex]);
			return cellView;
		}

		private List<Data> _data;

		public EnhancedScroller vScroller;

		public EnhancedScroller hScroller;

		public InputField jumpIndexInput;

		public Toggle useSpacingToggle;

		public Slider scrollerOffsetSlider;

		public Slider cellOffsetSlider;

		public EnhancedScrollerCellView cellViewPrefab;

		public EnhancedScroller.TweenType vScrollerTweenType;

		public float vScrollerTweenTime;

		public EnhancedScroller.TweenType hScrollerTweenType;

		public float hScrollerTweenTime;
	}
}
