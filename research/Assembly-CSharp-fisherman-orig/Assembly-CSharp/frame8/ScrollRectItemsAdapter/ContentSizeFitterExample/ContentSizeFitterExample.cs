using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.ContentSizeFitterExample
{
	public class ContentSizeFitterExample : SRIA<MyParams, MyItemViewsHolder>
	{
		protected override void Start()
		{
			this._Params.NewModelCreator = (int index) => this.CreateNewModel(index);
			base.Start();
			DrawerCommandPanel.Instance.Init(this, true, false, true, false, true);
			DrawerCommandPanel.Instance.galleryEffectSetting.slider.value = 0f;
			DrawerCommandPanel.Instance.ItemCountChangeRequested += this.OnItemCountChangeRequested;
			DrawerCommandPanel.Instance.AddItemRequested += this.OnAddItemRequested;
			DrawerCommandPanel.Instance.RemoveItemRequested += this.OnRemoveItemRequested;
			DrawerCommandPanel.Instance.RequestChangeItemCountToSpecified();
		}

		protected override MyItemViewsHolder CreateViewsHolder(int itemIndex)
		{
			MyItemViewsHolder myItemViewsHolder = new MyItemViewsHolder();
			myItemViewsHolder.Init(this._Params.itemPrefab, itemIndex, true, true);
			return myItemViewsHolder;
		}

		protected override void OnItemHeightChangedPreTwinPass(MyItemViewsHolder vh)
		{
			base.OnItemHeightChangedPreTwinPass(vh);
			this._Params.Data[vh.ItemIndex].HasPendingSizeChange = false;
			vh.contentSizeFitter.enabled = false;
		}

		protected override void UpdateViewsHolder(MyItemViewsHolder newOrRecycled)
		{
			ExampleItemModel exampleItemModel = this._Params.Data[newOrRecycled.ItemIndex];
			newOrRecycled.UpdateFromModel(exampleItemModel, this._Params.availableIcons);
			if (newOrRecycled.contentSizeFitter.enabled)
			{
				newOrRecycled.contentSizeFitter.enabled = false;
			}
			if (exampleItemModel.HasPendingSizeChange)
			{
				newOrRecycled.MarkForRebuild();
				base.ScheduleComputeVisibilityTwinPass(DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn);
			}
		}

		protected override void RebuildLayoutDueToScrollViewSizeChange()
		{
			foreach (ExampleItemModel exampleItemModel in this._Params.Data.AsEnumerableForExistingItems)
			{
				exampleItemModel.HasPendingSizeChange = true;
			}
			base.RebuildLayoutDueToScrollViewSizeChange();
		}

		private void OnAddItemRequested(bool atEnd)
		{
			int num = ((!atEnd) ? 0 : this._Params.Data.Count);
			this._Params.Data.Insert(num, 1);
			this.InsertItems(num, 1, DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn, false);
		}

		private void OnRemoveItemRequested(bool fromEnd)
		{
			if (this._Params.Data.Count == 0)
			{
				return;
			}
			int num = ((!fromEnd) ? 0 : (this._Params.Data.Count - 1));
			this._Params.Data.RemoveAt(num);
			this.RemoveItems(num, 1, DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn, false);
		}

		private void OnItemCountChangeRequested(int newCount)
		{
			this._Params.Data.InitWithNewCount(newCount);
			this.ResetItems(this._Params.Data.Count, DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn, false);
		}

		public ExampleItemModel CreateNewModel(int itemIdex)
		{
			return new ExampleItemModel
			{
				Title = C.GetRandomTextBody("Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.".Length / 50 + 1, "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.".Length / 2),
				IconIndex = Random.Range(0, this._Params.availableIcons.Length)
			};
		}

		public RectTransformEdgeDragger edgeDragger;
	}
}
