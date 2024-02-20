using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.Models;
using frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.ViewsHolders;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample
{
	public class MultiplePrefabsExample : SRIA<MyParams, BaseVH>, ExpandCollapseOnClick.ISizeChangesHandler
	{
		protected override void Start()
		{
			base.Start();
			DrawerCommandPanel.Instance.Init(this, true, true, true, false, true);
			DrawerCommandPanel.Instance.galleryEffectSetting.slider.value = 0f;
			DrawerCommandPanel.Instance.ItemCountChangeRequested += this.OnItemCountChangeRequested;
			DrawerCommandPanel.Instance.AddItemRequested += this.OnAddItemRequested;
			DrawerCommandPanel.Instance.RemoveItemRequested += this.OnRemoveItemRequested;
			DrawerCommandPanel.Instance.RequestChangeItemCountToSpecified();
		}

		protected override void Update()
		{
			base.Update();
			if (this._Params == null || this._Params.data == null)
			{
				this.averageValuesInModelsText.text = "0";
				return;
			}
			if (this._Params.data.Count > 10000)
			{
				this.averageValuesInModelsText.text = "too much data";
				this.averageValuesInModelsText.color = Color.red * 0.5f;
				return;
			}
			this.averageValuesInModelsText.color = Color.white;
			float num = (float)this._Params.data.Count / 100000f;
			int num2 = Mathf.Min(60, Mathf.Max(1, (int)(num * 60f)));
			if (Time.frameCount % num2 == 0)
			{
				float num3 = 0f;
				int num4 = 0;
				foreach (BaseModel baseModel in this._Params.data)
				{
					BidirectionalModel bidirectionalModel = baseModel as BidirectionalModel;
					if (bidirectionalModel != null)
					{
						num4++;
						num3 += bidirectionalModel.value;
					}
				}
				this.averageValuesInModelsText.text = (num3 / (float)num4).ToString("0.000");
			}
		}

		public override void ChangeItemsCount(ItemCountChangeMode changeMode, int itemsCount, int indexIfInsertingOrRemoving = -1, bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
		{
			this._IndexOfCurrentlyExpandedItem = -1;
			base.ChangeItemsCount(changeMode, itemsCount, indexIfInsertingOrRemoving, contentPanelEndEdgeStationary, keepVelocity);
		}

		protected override BaseVH CreateViewsHolder(int itemIndex)
		{
			Type cachedType = this._Params.data[itemIndex].CachedType;
			if (cachedType == typeof(BidirectionalModel))
			{
				BidirectionalVH bidirectionalVH = new BidirectionalVH();
				bidirectionalVH.Init(this._Params.bidirectionalPrefab, itemIndex, true, true);
				return bidirectionalVH;
			}
			if (cachedType == typeof(ExpandableModel))
			{
				ExpandableVH expandableVH = new ExpandableVH();
				expandableVH.Init(this._Params.expandablePrefab, itemIndex, true, true);
				expandableVH.expandCollapseOnClickBehaviour.sizeChangesHandler = this;
				expandableVH.expandCollapseOnClickBehaviour.expandFactor = this._Params.expandableItemExpandFactor;
				return expandableVH;
			}
			throw new InvalidOperationException("Unrecognized model type: " + cachedType.Name);
		}

		protected override void UpdateViewsHolder(BaseVH newOrRecycled)
		{
			BaseModel baseModel = this._Params.data[newOrRecycled.ItemIndex];
			newOrRecycled.UpdateViews(baseModel);
		}

		protected override bool IsRecyclable(BaseVH potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, float heightOfItemThatWillBecomeVisible)
		{
			return potentiallyRecyclable.CanPresentModelType(this._Params.data[indexOfItemThatWillBecomeVisible].CachedType);
		}

		bool ExpandCollapseOnClick.ISizeChangesHandler.HandleSizeChangeRequest(RectTransform rt, float newRequestedSize)
		{
			BaseVH itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(rt);
			if (itemViewsHolderIfVisible != null)
			{
				ExpandableModel expandableModel = this._Params.data[itemViewsHolderIfVisible.ItemIndex] as ExpandableModel;
				base.RequestChangeItemSizeAndUpdateLayout(itemViewsHolderIfVisible, newRequestedSize, DrawerCommandPanel.Instance.freezeItemEndEdgeToggle.isOn, true);
				if (this._IndexOfCurrentlyExpandedItem != -1)
				{
					ExpandableVH expandableVH = itemViewsHolderIfVisible as ExpandableVH;
					if (!expandableVH.expandCollapseOnClickBehaviour.expanded)
					{
						float num = newRequestedSize / (expandableModel.nonExpandedSize * this._Params.expandableItemExpandFactor);
						ExpandableModel expandableModel2 = this._Params.data[this._IndexOfCurrentlyExpandedItem] as ExpandableModel;
						float num2 = Mathf.Lerp(expandableModel2.nonExpandedSize, expandableModel2.nonExpandedSize * this._Params.expandableItemExpandFactor, 1f - num);
						base.RequestChangeItemSizeAndUpdateLayout(this._IndexOfCurrentlyExpandedItem, num2, DrawerCommandPanel.Instance.freezeItemEndEdgeToggle.isOn, true);
					}
				}
				return true;
			}
			return false;
		}

		public void OnExpandedStateChanged(RectTransform rt, bool expanded)
		{
			BaseVH itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(rt);
			if (itemViewsHolderIfVisible != null)
			{
				ExpandableModel expandableModel = this._Params.data[itemViewsHolderIfVisible.ItemIndex] as ExpandableModel;
				if (expandableModel == null)
				{
					throw new UnityException(string.Concat(new object[]
					{
						"MultiplePrefabsExample.MyScrollRectAdapter.OnExpandedStateChanged: item model at index ",
						itemViewsHolderIfVisible.ItemIndex,
						" is not of type ",
						typeof(ExpandableModel).Name,
						", as expected by the views holder having this itemIndex. Happy debugging :)"
					}));
				}
				expandableModel.expanded = expanded;
				if (expanded)
				{
					if (this._IndexOfCurrentlyExpandedItem != -1)
					{
						ExpandableModel expandableModel2 = this._Params.data[this._IndexOfCurrentlyExpandedItem] as ExpandableModel;
						expandableModel2.expanded = false;
						BaseVH itemViewsHolderIfVisible2 = base.GetItemViewsHolderIfVisible(this._IndexOfCurrentlyExpandedItem);
						if (itemViewsHolderIfVisible2 != null)
						{
							itemViewsHolderIfVisible2.UpdateViews(expandableModel2);
						}
					}
					this._IndexOfCurrentlyExpandedItem = itemViewsHolderIfVisible.ItemIndex;
				}
				else if (itemViewsHolderIfVisible.ItemIndex == this._IndexOfCurrentlyExpandedItem)
				{
					this._IndexOfCurrentlyExpandedItem = -1;
				}
			}
		}

		private void OnAddItemRequested(bool atEnd)
		{
			int count = this._Params.data.Count;
			int num = ((!atEnd) ? 0 : count);
			int num2 = 0;
			if (count > 0)
			{
				if (atEnd)
				{
					num2 = this._Params.data[count - 1].id + 1;
				}
				else
				{
					num2 = this._Params.data[0].id - 1;
				}
			}
			this._Params.data.Insert(num, this.CreateNewModel(num2));
			this.InsertItems(num, 1, DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn, false);
		}

		private void OnRemoveItemRequested(bool fromEnd)
		{
			if (this._Params.data.Count == 0)
			{
				return;
			}
			int num = ((!fromEnd) ? 0 : (this._Params.data.Count - 1));
			this._Params.data.RemoveAt(num);
			this.RemoveItems(num, 1, DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn, false);
		}

		private void OnItemCountChangeRequested(int newCount)
		{
			BaseModel[] array = new BaseModel[newCount];
			for (int i = 0; i < newCount; i++)
			{
				array[i] = this.CreateNewModel(i);
			}
			this._Params.data.Clear();
			this._Params.data.AddRange(array);
			this.ResetItems(array.Length, DrawerCommandPanel.Instance.freezeContentEndEdgeToggle.isOn, false);
		}

		private BaseModel CreateNewModel(int id)
		{
			BaseModel baseModel;
			if (Random.Range(0, 2) == 0)
			{
				Rect rect = this._Params.expandablePrefab.rect;
				float num = ((!this._Params.scrollRect.horizontal) ? rect.height : rect.width);
				baseModel = new ExpandableModel
				{
					imageURL = C.GetRandomSmallImageURL(),
					nonExpandedSize = num
				};
			}
			else
			{
				Rect rect = this._Params.bidirectionalPrefab.rect;
				float num2 = ((!this._Params.scrollRect.horizontal) ? rect.height : rect.width);
				baseModel = new BidirectionalModel
				{
					value = Random.Range(-5f, 5f)
				};
			}
			baseModel.id = id;
			return baseModel;
		}

		public Text averageValuesInModelsText;

		private int _IndexOfCurrentlyExpandedItem;
	}
}
