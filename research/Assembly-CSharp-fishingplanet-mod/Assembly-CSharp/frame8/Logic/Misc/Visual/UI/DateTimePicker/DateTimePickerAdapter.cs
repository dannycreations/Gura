using System;
using System.Diagnostics;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;

namespace frame8.Logic.Misc.Visual.UI.DateTimePicker
{
	public class DateTimePickerAdapter : SRIA<MyParams, MyItemViewsHolder>
	{
		public int SelectedValue { get; private set; }

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<int> OnSelectedValueChanged;

		protected override void Start()
		{
			base.Start();
		}

		protected override void Update()
		{
			base.Update();
			if (this._VisibleItemsCount == 0)
			{
				return;
			}
			int num = this._VisibleItemsCount / 2;
			MyItemViewsHolder myItemViewsHolder = this._VisibleItems[num];
			int selectedValue = this.SelectedValue;
			this.SelectedValue = this._Params.GetItemValueAtIndex(myItemViewsHolder.ItemIndex);
			myItemViewsHolder.background.color = this._Params.selectedColor;
			for (int i = 0; i < this._VisibleItemsCount; i++)
			{
				if (i != num)
				{
					this._VisibleItems[i].background.color = this._Params.nonSelectedColor;
				}
			}
			if (selectedValue != this.SelectedValue && this.OnSelectedValueChanged != null)
			{
				this.OnSelectedValueChanged(this.SelectedValue);
			}
		}

		protected override MyItemViewsHolder CreateViewsHolder(int itemIndex)
		{
			MyItemViewsHolder myItemViewsHolder = new MyItemViewsHolder();
			myItemViewsHolder.Init(this._Params.itemPrefab, itemIndex, true, true);
			return myItemViewsHolder;
		}

		protected override void UpdateViewsHolder(MyItemViewsHolder newOrRecycled)
		{
			newOrRecycled.titleText.text = this._Params.GetItemValueAtIndex(newOrRecycled.ItemIndex) + string.Empty;
		}

		private void ChangeItemsCountWithChecks(int newCount)
		{
			int num = 4;
			if (newCount < num)
			{
				newCount = num;
			}
			this.ResetItems(newCount, false, false);
		}
	}
}
