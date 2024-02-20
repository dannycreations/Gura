using System;
using System.Collections;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using UnityEngine;
using UnityEngine.Events;

namespace frame8.ScrollRectItemsAdapter.SimpleTutorialExample
{
	public class SimpleTutorial : SRIA<MyParams, MyItemViewsHolder>, ExpandCollapseOnClick.ISizeChangesHandler
	{
		protected override void Start()
		{
			this._Params.NewModelCreator = (int index) => this.CreateNewModel(index);
			base.Start();
			DrawerCommandPanel.Instance.Init(this, true, true, true, true, true);
			DrawerCommandPanel.Instance.serverDelaySetting.inputField.text = Mathf.Max(0, this._Params.initialSimulatedServerDelay) + string.Empty;
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
			myItemViewsHolder.expandOnCollapseComponent.sizeChangesHandler = this;
			return myItemViewsHolder;
		}

		protected override void UpdateViewsHolder(MyItemViewsHolder newOrRecycled)
		{
			ExampleItemModel exampleItemModel = this._Params.Data[newOrRecycled.ItemIndex];
			newOrRecycled.backgroundImage.color = exampleItemModel.color;
			newOrRecycled.titleText.text = exampleItemModel.title + " #" + newOrRecycled.ItemIndex;
			newOrRecycled.icon1Image.texture = this._Params.availableIcons[exampleItemModel.icon1Index];
			newOrRecycled.icon2Image.texture = this._Params.availableIcons[exampleItemModel.icon2Index];
			if (newOrRecycled.expandOnCollapseComponent)
			{
				newOrRecycled.expandOnCollapseComponent.expanded = exampleItemModel.expanded;
				newOrRecycled.expandOnCollapseComponent.nonExpandedSize = exampleItemModel.nonExpandedSize;
			}
		}

		bool ExpandCollapseOnClick.ISizeChangesHandler.HandleSizeChangeRequest(RectTransform rt, float newSize)
		{
			MyItemViewsHolder itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(rt);
			if (itemViewsHolderIfVisible == null)
			{
				return false;
			}
			base.RequestChangeItemSizeAndUpdateLayout(itemViewsHolderIfVisible, newSize, DrawerCommandPanel.Instance.freezeItemEndEdgeToggle.isOn, true);
			return true;
		}

		public void OnExpandedStateChanged(RectTransform rt, bool expanded)
		{
			MyItemViewsHolder itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(rt);
			if (itemViewsHolderIfVisible != null)
			{
				this._Params.Data[itemViewsHolderIfVisible.ItemIndex].expanded = expanded;
			}
		}

		private void OnAddItemRequested(bool atEnd)
		{
			int num = ((!atEnd) ? 0 : this._Params.Data.Count);
			this._Params.Data.Insert(num, 1);
			this.InsertItems(num, 1, DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn, false);
			if (this.OnItemsUpdated != null)
			{
				this.OnItemsUpdated.Invoke();
			}
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
			if (this.OnItemsUpdated != null)
			{
				this.OnItemsUpdated.Invoke();
			}
		}

		private void OnItemCountChangeRequested(int newCount)
		{
			base.StartCoroutine(this.FetchItemModelsFromServer(newCount, delegate
			{
				this.OnReceivedNewModels(newCount);
			}));
		}

		private void OnReceivedNewModels(int newCount)
		{
			this._Params.Data.Clear();
			this._Params.Data.InitWithNewCount(newCount);
			this.ResetItems(this._Params.Data.Count, DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn, false);
			if (this.OnItemsUpdated != null)
			{
				this.OnItemsUpdated.Invoke();
			}
		}

		private IEnumerator FetchItemModelsFromServer(int count, Action onDone)
		{
			yield return new WaitForSeconds((float)DrawerCommandPanel.Instance.serverDelaySetting.InputFieldValueAsInt);
			onDone();
			yield break;
		}

		private ExampleItemModel CreateNewModel(int itemIdex)
		{
			return new ExampleItemModel
			{
				title = "Item ",
				icon1Index = Random.Range(0, this._Params.availableIcons.Length),
				icon2Index = Random.Range(0, this._Params.availableIcons.Length),
				nonExpandedSize = this._Params.ItemPrefabSize
			};
		}

		public UnityEvent OnItemsUpdated;
	}
}
