using System;
using frame8.Logic.Misc.Other.Extensions;
using FriendsSRIA.Models;
using UnityEngine.UI;

namespace FriendsSRIA.ViewsHolders
{
	public class FriendItemVH : BaseVH
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.root.GetComponentAtPath("request", out this.requestContent);
			this.root.GetComponentAtPath("normal", out this.friendContent);
			this.root.GetComponentAtPath("found", out this.searchContent);
			this.root.GetComponentAtPath("ignored", out this.ignoredContent);
			this.root.GetComponentAtPath("Bg", out this._background);
			this.current = this.friendContent;
		}

		public override bool CanPresentModelType(Type modelType)
		{
			return modelType == typeof(FriendItemModel);
		}

		private void PlayerSelected(object sender, FriendEventArgs e)
		{
			FriendsHandlerSRIA.Instance.OnPlayerSelected(this, e);
		}

		internal override void UpdateViews(BaseModel model)
		{
			this.model = model;
			base.UpdateViews(model);
			FriendItemModel friendItemModel = model as FriendItemModel;
			this.friendContent.gameObject.SetActive(friendItemModel.type == BaseModel.FriendModelType.Normal);
			this.requestContent.gameObject.SetActive(friendItemModel.type == BaseModel.FriendModelType.Request);
			this.searchContent.gameObject.SetActive(friendItemModel.type == BaseModel.FriendModelType.Found);
			this.ignoredContent.gameObject.SetActive(friendItemModel.type == BaseModel.FriendModelType.Ignored);
			this._background.color = friendItemModel.bgColor;
			switch (friendItemModel.type)
			{
			case BaseModel.FriendModelType.Normal:
				this.current = this.friendContent;
				this.friendContent.Init(friendItemModel.item, FriendsHandlerSRIA.Instance.friendsListContent, FriendsHandlerSRIA.Instance.rootNavigation);
				this.friendContent.IsSelectedItem += this.PlayerSelected;
				break;
			case BaseModel.FriendModelType.Request:
				this.current = this.requestContent;
				this.requestContent.Init(friendItemModel.item, FriendsHandlerSRIA.Instance.friendsListContent);
				this.requestContent.IsSelectedItem += this.PlayerSelected;
				this.requestContent.OnConfirmFriendship += FriendsHandlerSRIA.Instance.GetFriendsAction;
				this.requestContent.OnDeclineFriendship += FriendsHandlerSRIA.Instance.GetFriendsAction;
				break;
			case BaseModel.FriendModelType.Found:
				this.current = this.searchContent;
				this.searchContent.Init(friendItemModel.item, FriendsHandlerSRIA.Instance.friendsListContent);
				this.searchContent.IsSelectedItem += this.PlayerSelected;
				break;
			case BaseModel.FriendModelType.Ignored:
				this.current = this.ignoredContent;
				this.ignoredContent.Init(friendItemModel.item, FriendsHandlerSRIA.Instance.friendsListContent);
				this.ignoredContent.IsSelectedItem += this.PlayerSelected;
				break;
			}
		}

		public FriendshipRequestListItemInit requestContent;

		public FriendsListItemInit friendContent;

		public SearchFriendsListItemInit searchContent;

		public IgonoredFriendsListItemInit ignoredContent;

		public FriendListItemBase current;

		public Image _background;

		public BaseModel model;
	}
}
