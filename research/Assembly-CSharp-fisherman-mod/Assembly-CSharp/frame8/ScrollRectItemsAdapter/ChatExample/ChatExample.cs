using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.ChatExample
{
	public class ChatExample : SRIA<MyParams, ChatMessageViewsHolder>
	{
		protected override void Start()
		{
			base.Start();
			DrawerCommandPanel.Instance.Init(this, false, false, false, false, true);
			DrawerCommandPanel.Instance.galleryEffectSetting.slider.value = 0.04f;
			DrawerCommandPanel.Instance.addRemoveOnePanel.button2.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.addRemoveOnePanel.button4.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.addRemoveOnePanel.button3.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.AddItemRequested += this.OnAddItemRequested;
			this.OnItemCountChangeRequested(3);
		}

		protected override void Update()
		{
			base.Update();
			foreach (ChatMessageViewsHolder chatMessageViewsHolder in this._VisibleItems)
			{
				if (chatMessageViewsHolder.IsPopupAnimationActive)
				{
					chatMessageViewsHolder.UpdatePopupAnimation();
				}
			}
		}

		protected override ChatMessageViewsHolder CreateViewsHolder(int itemIndex)
		{
			ChatMessageViewsHolder chatMessageViewsHolder = new ChatMessageViewsHolder();
			chatMessageViewsHolder.Init(this._Params.itemPrefab, itemIndex, true, true);
			return chatMessageViewsHolder;
		}

		protected override void OnItemHeightChangedPreTwinPass(ChatMessageViewsHolder vh)
		{
			base.OnItemHeightChangedPreTwinPass(vh);
			this._Params.Data[vh.ItemIndex].HasPendingVisualSizeChange = false;
			vh.contentSizeFitter.enabled = false;
		}

		protected override void UpdateViewsHolder(ChatMessageViewsHolder newOrRecycled)
		{
			ChatMessageModel chatMessageModel = this._Params.Data[newOrRecycled.ItemIndex];
			newOrRecycled.UpdateFromModel(chatMessageModel, this._Params);
			if (newOrRecycled.contentSizeFitter.enabled)
			{
				newOrRecycled.contentSizeFitter.enabled = false;
			}
			if (chatMessageModel.HasPendingVisualSizeChange)
			{
				newOrRecycled.MarkForRebuild();
				base.ScheduleComputeVisibilityTwinPass(true);
			}
			if (!newOrRecycled.IsPopupAnimationActive && newOrRecycled.itemIndexInView == this.GetItemsCount() - 1)
			{
				newOrRecycled.IsPopupAnimationActive = true;
			}
		}

		protected override void OnBeforeRecycleOrDisableViewsHolder(ChatMessageViewsHolder inRecycleBinOrVisible, int newItemIndex)
		{
			inRecycleBinOrVisible.IsPopupAnimationActive = false;
			base.OnBeforeRecycleOrDisableViewsHolder(inRecycleBinOrVisible, newItemIndex);
		}

		protected override void RebuildLayoutDueToScrollViewSizeChange()
		{
			foreach (ChatMessageModel chatMessageModel in this._Params.Data)
			{
				chatMessageModel.HasPendingVisualSizeChange = true;
			}
			base.RebuildLayoutDueToScrollViewSizeChange();
		}

		private void OnAddItemRequested(bool atEnd)
		{
			int num = ((!atEnd) ? 0 : this._Params.Data.Count);
			this._Params.Data.Insert(num, this.CreateNewModel(num, true));
			this.InsertItems(num, 1, true, false);
		}

		private void OnItemCountChangeRequested(int newCount)
		{
			ChatMessageModel[] array = new ChatMessageModel[newCount];
			for (int i = 0; i < newCount; i++)
			{
				array[i] = this.CreateNewModel(i, i != 1);
			}
			this._Params.Data.Clear();
			this._Params.Data.AddRange(array);
			this.ResetItems(this._Params.Data.Count, true, false);
		}

		private ChatMessageModel CreateNewModel(int itemIdex, bool addImageOnlyRandomly = true)
		{
			return new ChatMessageModel
			{
				timestampSec = (int)DateTime.UtcNow.Subtract(ChatMessageModel.EPOCH_START_TIME).TotalSeconds,
				Text = ChatExample.GetRandomContent(),
				IsMine = (Random.Range(0, 2) == 0),
				ImageIndex = ((!addImageOnlyRandomly) ? 0 : Random.Range(-2 * this._Params.availableChatImages.Length, this._Params.availableChatImages.Length))
			};
		}

		private static string GetRandomContent()
		{
			return "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.".Substring(0, Random.Range("Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.".Length / 50 + 1, "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.".Length / 2));
		}

		private const string LOREM_IPSUM = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
	}
}
