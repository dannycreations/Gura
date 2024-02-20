using System;
using System.Collections;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.IncrementalItemFetchExample2
{
	public class IncrementalItemFetchExample : SRIA<MyParams, MyItemViewsHolder>
	{
		protected override void Start()
		{
			base.Start();
			DrawerCommandPanel.Instance.Init(this, false, false, true, true, false);
			DrawerCommandPanel.Instance.galleryEffectSetting.slider.value = 0f;
			DrawerCommandPanel.Instance.ItemCountChangeRequested += this.UpdateCapacity;
			this._FetchCountSetting = DrawerCommandPanel.Instance.AddLabelWithInputPanel("Max items to fetch:");
			this._FetchCountSetting.inputField.text = this._Params.preFetchedItemsCount + string.Empty;
			this._FetchCountSetting.inputField.keyboardType = 4;
			this._FetchCountSetting.inputField.characterLimit = 2;
			this._FetchCountSetting.inputField.onEndEdit.AddListener(delegate(string _)
			{
				this._Params.preFetchedItemsCount = this._FetchCountSetting.InputFieldValueAsInt;
			});
			this._RandomSizesForNewItemsSetting = DrawerCommandPanel.Instance.AddLabelWithTogglePanel("Random sizes for new items");
		}

		protected override void Update()
		{
			base.Update();
			if (this._Fetching)
			{
				return;
			}
			int num = -1;
			if (this._VisibleItemsCount > 0)
			{
				num = this._VisibleItems[this._VisibleItemsCount - 1].ItemIndex;
			}
			int num2 = this._Params.Data.Count - (num + 1);
			if (num2 < this._Params.preFetchedItemsCount)
			{
				int num3 = this._Params.Data.Count + this._Params.preFetchedItemsCount;
				if (this._Params.totalCapacity > -1)
				{
					num3 = Mathf.Min(num3, this._Params.totalCapacity);
				}
				if (num3 > this._Params.Data.Count)
				{
					this.StartPreFetching(num3 - this._Params.Data.Count);
				}
			}
		}

		protected override void CollectItemsSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, ItemsDescriptor itemsDesc)
		{
			base.CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, itemsDesc);
			if (changeMode == ItemCountChangeMode.REMOVE || count == 0)
			{
				return;
			}
			if (!this._RandomSizesForNewItemsSetting.toggle.isOn)
			{
				return;
			}
			int num;
			if (changeMode == ItemCountChangeMode.RESET)
			{
				num = 0;
			}
			else
			{
				num = indexIfInsertingOrRemoving;
			}
			int num2 = num + count;
			itemsDesc.BeginChangingItemsSizes(num);
			for (int i = num; i < num2; i++)
			{
				itemsDesc[i] = Random.Range(this._Params.DefaultItemSize / 3f, this._Params.DefaultItemSize * 3f);
			}
			itemsDesc.EndChangingItemsSizes();
		}

		protected override MyItemViewsHolder CreateViewsHolder(int itemIndex)
		{
			MyItemViewsHolder myItemViewsHolder = new MyItemViewsHolder();
			myItemViewsHolder.Init(this._Params.itemPrefab, itemIndex, true, true);
			return myItemViewsHolder;
		}

		protected override void UpdateViewsHolder(MyItemViewsHolder newOrRecycled)
		{
			ExampleItemModel exampleItemModel = this._Params.Data[newOrRecycled.ItemIndex];
			newOrRecycled.titleText.text = string.Concat(new object[] { "[", newOrRecycled.ItemIndex, "] ", exampleItemModel.title });
		}

		public void UpdateCapacity(int newCapacity)
		{
			this._Params.totalCapacity = newCapacity;
			if (newCapacity < this._Params.Data.Count)
			{
				int num = this._Params.Data.Count - newCapacity;
				this.RemoveItems(newCapacity, num, DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn, false);
				this._Params.statusText.text = this._Params.Data.Count + " items";
			}
		}

		private void StartPreFetching(int additionalItems)
		{
			this._Fetching = true;
			DrawerCommandPanel.Instance.setCountPanel.button.interactable = false;
			base.StartCoroutine(this.FetchItemModelsFromServer(additionalItems, new Action<ExampleItemModel[]>(this.OnPreFetchingFinished)));
		}

		private void OnPreFetchingFinished(ExampleItemModel[] models)
		{
			int count = this._Params.Data.Count;
			this._Params.Data.AddRange(models);
			this.InsertItems(count, models.Length, DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn, true);
			this._Params.statusText.text = this._Params.Data.Count + " items";
			this._Fetching = false;
			DrawerCommandPanel.Instance.setCountPanel.button.interactable = true;
		}

		private IEnumerator FetchItemModelsFromServer(int count, Action<ExampleItemModel[]> onDone)
		{
			this._Params.statusText.text = "Fetching " + count + " from server...";
			yield return new WaitForSeconds((float)DrawerCommandPanel.Instance.serverDelaySetting.InputFieldValueAsInt);
			ExampleItemModel[] results = new ExampleItemModel[count];
			for (int i = 0; i < count; i++)
			{
				results[i] = new ExampleItemModel();
				results[i].title = "Item got at " + DateTime.Now.ToString("hh:mm:ss");
			}
			onDone(results);
			yield break;
		}

		private bool _Fetching;

		private LabelWithInputPanel _FetchCountSetting;

		private LabelWithToggle _RandomSizesForNewItemsSetting;
	}
}
