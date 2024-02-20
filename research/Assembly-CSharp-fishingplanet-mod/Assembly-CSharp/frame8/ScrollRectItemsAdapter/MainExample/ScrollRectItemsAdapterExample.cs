using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.MainExample
{
	public class ScrollRectItemsAdapterExample : SRIA<MyParams, ClientItemViewsHolder>, ExpandCollapseOnClick.ISizeChangesHandler
	{
		protected override void Start()
		{
			this._Params.InitTextures();
			this._Params.NewModelCreator = (int index) => this.CreateNewModel(index);
			base.Start();
			DrawerCommandPanel.Instance.ItemCountChangeRequested += this.OnItemCountChangeRequested;
			DrawerCommandPanel.Instance.AddItemRequested += this.OnAddItemRequested;
			DrawerCommandPanel.Instance.RemoveItemRequested += this.OnRemoveItemRequested;
			MainExampleHelper.Instance.OnAdapterInitialized();
		}

		protected override void OnDestroy()
		{
			this._Params.ReleaseTextures();
		}

		protected override ClientItemViewsHolder CreateViewsHolder(int itemIndex)
		{
			ClientItemViewsHolder clientItemViewsHolder = new ClientItemViewsHolder();
			clientItemViewsHolder.Init(this._Params.itemPrefab, itemIndex, true, true);
			if (this._Params.itemsAreExpandable)
			{
				clientItemViewsHolder.expandCollapseComponent.sizeChangesHandler = this;
			}
			return clientItemViewsHolder;
		}

		protected override void UpdateViewsHolder(ClientItemViewsHolder newOrRecycled)
		{
			newOrRecycled.UpdateViews(this._Params);
		}

		bool ExpandCollapseOnClick.ISizeChangesHandler.HandleSizeChangeRequest(RectTransform rt, float newSize)
		{
			ClientItemViewsHolder itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(rt);
			if (itemViewsHolderIfVisible != null)
			{
				base.RequestChangeItemSizeAndUpdateLayout(itemViewsHolderIfVisible, newSize, DrawerCommandPanel.Instance.freezeItemEndEdgeToggle.isOn, true);
				return true;
			}
			return false;
		}

		public void OnExpandedStateChanged(RectTransform rt, bool expanded)
		{
			ClientItemViewsHolder itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(rt);
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

		private ClientModel CreateNewModel(int index)
		{
			ClientModel clientModel = new ClientModel
			{
				avatarImageId = this.Rand(this._Params.sampleAvatars.Count),
				clientName = this._Params.sampleFirstNames[this.Rand(this._Params.sampleFirstNames.Length)],
				location = this._Params.sampleLocations[this.Rand(this._Params.sampleLocations.Length)],
				availability01 = this.RandF(1f),
				contractChance01 = this.RandF(1f),
				longTermClient01 = this.RandF(1f),
				isOnline = (this.Rand(2) == 0),
				nonExpandedSize = this._Params.ItemPrefabSize
			};
			int num = this.Rand(10);
			clientModel.friendsAvatarIds = new int[num];
			for (int i = 0; i < num; i++)
			{
				clientModel.friendsAvatarIds[i] = this.Rand(this._Params.sampleAvatarsDownsized.Count);
			}
			return clientModel;
		}

		private int Rand(int maxExcl)
		{
			return Random.Range(0, maxExcl);
		}

		private float RandF(float maxExcl = 1f)
		{
			return Random.Range(0f, maxExcl);
		}
	}
}
