using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using FriendsSRIA.Models;
using FriendsSRIA.ViewsHolders;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FriendsSRIA
{
	public class FriendsHandlerSRIA : SRIA<MyParams, BaseVH>
	{
		protected override void Awake()
		{
			base.Awake();
			if (this.rootNavigation == null)
			{
				return;
			}
			UINavigation uinavigation = this.rootNavigation;
			uinavigation.OnBottomReached = (Action<Selectable>)Delegate.Combine(uinavigation.OnBottomReached, new Action<Selectable>(this.OnBottomReached));
			UINavigation uinavigation2 = this.rootNavigation;
			uinavigation2.OnTopReached = (Action<Selectable>)Delegate.Combine(uinavigation2.OnTopReached, new Action<Selectable>(this.OnTopReached));
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			UINavigation uinavigation = this.rootNavigation;
			uinavigation.OnBottomReached = (Action<Selectable>)Delegate.Remove(uinavigation.OnBottomReached, new Action<Selectable>(this.OnBottomReached));
			UINavigation uinavigation2 = this.rootNavigation;
			uinavigation2.OnTopReached = (Action<Selectable>)Delegate.Remove(uinavigation2.OnTopReached, new Action<Selectable>(this.OnTopReached));
		}

		private void OnTopReached(Selectable curr)
		{
			if (!base.gameObject.activeInHierarchy || (this._cg != null && !this._cg.interactable))
			{
				return;
			}
			BaseVH itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(0);
			if (curr != null && !EventSystem.current.alreadySelecting && curr.transform.IsChildOf(base.transform) && ((itemViewsHolderIfVisible != null && (itemViewsHolderIfVisible.root.gameObject == curr.gameObject || curr.transform.IsChildOf(itemViewsHolderIfVisible.root.transform))) || !curr.gameObject.activeInHierarchy))
			{
				this.rootNavigation.SetPaused(true);
				UINavigation.SetSelectedGameObject(null);
				base.StopAllCoroutines();
				int num = this._Params.data.Count - 1;
				this.ScrollTo(num, 1f, 1f);
				base.StartCoroutine(this.SetElementSelected(num, 0.06f));
			}
		}

		private void OnBottomReached(Selectable curr)
		{
			if (!base.gameObject.activeInHierarchy || (this._cg != null && !this._cg.interactable))
			{
				return;
			}
			if (curr != null && !EventSystem.current.alreadySelecting && curr.transform.IsChildOf(base.transform))
			{
				int num = this._Params.data.Count - 1;
				BaseVH itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(num);
				if ((itemViewsHolderIfVisible != null && (itemViewsHolderIfVisible.root.gameObject == curr.gameObject || curr.transform.IsChildOf(itemViewsHolderIfVisible.root.transform))) || !curr.gameObject.activeInHierarchy)
				{
					this.rootNavigation.SetPaused(true);
					UINavigation.SetSelectedGameObject(null);
					base.StopAllCoroutines();
					this.ScrollTo(0, 0f, 0f);
					base.StartCoroutine(this.SetElementSelected(1, 0.06f));
				}
			}
		}

		protected override void Start()
		{
			ActivityState parentActivityState = ActivityState.GetParentActivityState(base.transform);
			if (parentActivityState != null)
			{
				this._cg = parentActivityState.CanvasGroup;
			}
			base.Start();
		}

		protected override bool IsRecyclable(BaseVH potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, float heightOfItemThatWillBecomeVisible)
		{
			return EventSystem.current != null && potentiallyRecyclable != null && potentiallyRecyclable.root != null && EventSystem.current.currentSelectedGameObject != potentiallyRecyclable.root.gameObject && this._Params.data != null && indexOfItemThatWillBecomeVisible < this._Params.data.Count && potentiallyRecyclable.CanPresentModelType(this._Params.data[indexOfItemThatWillBecomeVisible].CachedType);
		}

		protected override BaseVH CreateViewsHolder(int itemIndex)
		{
			Type cachedType = this._Params.data[itemIndex].CachedType;
			if (cachedType == typeof(HeaderFriendGroupModel))
			{
				HeaderFriendGroupVH headerFriendGroupVH = new HeaderFriendGroupVH();
				headerFriendGroupVH.Init(this._Params.headerPrefab, itemIndex, true, true);
				return headerFriendGroupVH;
			}
			if (cachedType == typeof(FriendItemModel))
			{
				FriendItemVH friendItemVH = new FriendItemVH();
				friendItemVH.Init(this._Params.friendItemPrefab, itemIndex, true, true);
				this.rootNavigation.ForceUpdate();
				return friendItemVH;
			}
			throw new InvalidOperationException("Unrecognized model type: " + cachedType.Name);
		}

		protected override void CollectItemsSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, ItemsDescriptor itemsDesc)
		{
			base.CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, itemsDesc);
			if (changeMode == ItemCountChangeMode.REMOVE || count == 0)
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
				Type cachedType = this._Params.data[i].CachedType;
				if (cachedType == typeof(HeaderFriendGroupModel))
				{
					itemsDesc[i] = 40f;
				}
				else if (cachedType == typeof(FriendItemModel))
				{
					itemsDesc[i] = 100f;
				}
			}
			itemsDesc.EndChangingItemsSizes();
		}

		protected override void UpdateViewsHolder(BaseVH newOrRecycled)
		{
			BaseModel baseModel = this._Params.data[newOrRecycled.ItemIndex];
			newOrRecycled.root.gameObject.SetActive(true);
			newOrRecycled.UpdateViews(baseModel);
			this.rootNavigation.ForceUpdate();
			FriendItemModel friendItemModel = baseModel as FriendItemModel;
			if (friendItemModel != null)
			{
				this.ItemUpdated(newOrRecycled, new FriendEventArgs
				{
					Name = friendItemModel.item.UserName
				});
			}
		}

		private IEnumerator SetElementSelected(int itemIndex, float time = 0.1f)
		{
			yield return new WaitForSeconds(time);
			this.rootNavigation.SetPaused(false);
			BaseVH vh = base.GetItemViewsHolderIfVisible(itemIndex);
			if (vh != null)
			{
				Selectable componentInChildren = vh.root.GetComponentInChildren<Selectable>();
				UINavigation.SetSelectedGameObject((!(componentInChildren != null)) ? vh.root.gameObject : componentInChildren.gameObject);
			}
			else
			{
				this.rootNavigation.SetFirstActive();
			}
			this.rootNavigation.UpdateFromEventSystemSelected();
			yield break;
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<FriendEventArgs> IsSelectedItem;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<FriendEventArgs> OnItemUpdated;

		protected override void Update()
		{
			if (this._cg != null && !this._cg.interactable)
			{
				return;
			}
			base.Update();
			this.emptyText.SetActive(this._Params.data.Count == 0);
		}

		private void UpdateFriendCategories()
		{
			this.allFriends = PhotonConnectionFactory.Instance.Profile.Friends;
			if (this.allFriends != null)
			{
				this.activeFriends = this.allFriends.Where((Player x) => x.Status == FriendStatus.Friend).ToList<Player>();
				this.activeFriends.Sort(delegate(Player a, Player b)
				{
					int num = 0;
					if (PhotonConnectionFactory.Instance.Profile.BuoyShareRequests != null)
					{
						int count = PhotonConnectionFactory.Instance.Profile.BuoyShareRequests.Where((BuoySetting y) => y.Sender == a.UserName).ToList<BuoySetting>().Count;
						num = PhotonConnectionFactory.Instance.Profile.BuoyShareRequests.Where((BuoySetting y) => y.Sender == b.UserName).ToList<BuoySetting>().Count.CompareTo(count);
					}
					if (num == 0)
					{
						num = b.IsOnline.CompareTo(a.IsOnline);
					}
					return num;
				});
				this.friendshipRequests = this.allFriends.Where((Player x) => x.Status == FriendStatus.FriendshipRequest).ToList<Player>();
				this.ignoredFriends = this.allFriends.Where((Player x) => x.Status == FriendStatus.Ignore).ToList<Player>();
			}
			else
			{
				this.activeFriends = new List<Player>();
				this.friendshipRequests = new List<Player>();
				this.ignoredFriends = new List<Player>();
			}
		}

		public void LoadFriendsList(bool flushSelected = true)
		{
			FriendsHandlerSRIA.Instance = this;
			if (!base.Initialized)
			{
				return;
			}
			if (flushSelected && this.IsSelectedItem != null)
			{
				this.IsSelectedItem(null, new FriendEventArgs
				{
					Name = string.Empty
				});
			}
			this.UpdateFriendCategories();
			this._Params.data.Clear();
			if (this.friendshipRequests.Count != 0)
			{
				this._Params.data.Add(new HeaderFriendGroupModel
				{
					GroupName = string.Format(ScriptLocalization.Get("RequestCaption"), this.friendshipRequests.Count),
					type = BaseModel.FriendModelType.Request
				});
				for (int i = 0; i < this.friendshipRequests.Count; i++)
				{
					BaseModel baseModel = new FriendItemModel
					{
						bgColor = ((i % 2 != 0) ? this._transparentColor : this._normalGreyColor),
						type = BaseModel.FriendModelType.Request,
						item = this.friendshipRequests[i]
					};
					this._Params.data.Add(baseModel);
				}
			}
			if (this.activeFriends.Count != 0)
			{
				this._Params.data.Add(new HeaderFriendGroupModel
				{
					GroupName = ScriptLocalization.Get("FriendsButtonPopup").ToUpper() + ": " + this.activeFriends.Count,
					type = BaseModel.FriendModelType.Normal
				});
				for (int j = 0; j < this.activeFriends.Count; j++)
				{
					BaseModel baseModel2 = new FriendItemModel
					{
						bgColor = ((j % 2 != 0) ? this._lightGreyColor : this._normalGreyColor),
						type = BaseModel.FriendModelType.Normal,
						item = this.activeFriends[j]
					};
					this._Params.data.Add(baseModel2);
				}
			}
			if (this.ignoredFriends.Count != 0)
			{
				this._Params.data.Add(new HeaderFriendGroupModel
				{
					GroupName = ScriptLocalization.Get("UserIgnoredCaption").ToUpper() + ": " + this.ignoredFriends.Count,
					type = BaseModel.FriendModelType.Ignored
				});
				for (int k = 0; k < this.ignoredFriends.Count; k++)
				{
					BaseModel baseModel3 = new FriendItemModel
					{
						bgColor = ((k % 2 != 0) ? this._transparentColor : this._normalGreyColor),
						type = BaseModel.FriendModelType.Ignored,
						item = this.ignoredFriends[k]
					};
					this._Params.data.Add(baseModel3);
				}
			}
			this.ResetItems(this._Params.data.Count, false, false);
			if (this.OnListBuilt != null)
			{
				this.OnListBuilt();
			}
		}

		public void LoadFoundList(List<Player> foundPlayers)
		{
			this.UpdateFriendCategories();
			this._Params.data.Clear();
			this._Params.data.Add(new HeaderFriendGroupModel
			{
				GroupName = string.Format(ScriptLocalization.Get("SearchResultCaption"), foundPlayers.Count),
				type = BaseModel.FriendModelType.Found
			});
			for (int i = 0; i < foundPlayers.Count; i++)
			{
				BaseModel baseModel = new FriendItemModel
				{
					bgColor = ((i % 2 != 0) ? this._lightGreyColor : this._normalGreyColor),
					type = BaseModel.FriendModelType.Found,
					item = foundPlayers[i]
				};
				this._Params.data.Add(baseModel);
			}
			this.ResetItems(this._Params.data.Count, false, false);
			if (this.OnListBuilt != null)
			{
				this.OnListBuilt();
			}
		}

		public void OnPlayerSelected(object sender, FriendEventArgs e)
		{
			if (this.IsSelectedItem != null)
			{
				this.IsSelectedItem(sender, e);
			}
		}

		private void ItemUpdated(object sender, FriendEventArgs e)
		{
			if (this.OnItemUpdated != null)
			{
				this.OnItemUpdated(sender, e);
			}
		}

		public void Clear()
		{
			this._Params.data.Clear();
			this.ResetItems(0, false, false);
			if (this.IsSelectedItem != null)
			{
				this.IsSelectedItem(null, new FriendEventArgs
				{
					Name = string.Empty
				});
			}
		}

		public void GetFriendsAction(object sender, EventArgs e)
		{
			this.LoadFriendsList(true);
		}

		private CanvasGroup _cg;

		public static FriendsHandlerSRIA Instance;

		public GameObject emptyText;

		public GameObject friendsListContent;

		private Color _lightGreyColor = new Color(0.76f, 0.76f, 0.76f, 0.066f);

		private Color _normalGreyColor = new Color(1f, 1f, 1f, 0.066f);

		private Color _transparentColor = new Color(0f, 0f, 0f, 0f);

		[Space(15f)]
		private List<Player> allFriends;

		private List<Player> friendshipRequests;

		private List<Player> activeFriends;

		private List<Player> ignoredFriends;

		public UINavigation rootNavigation;

		public Action OnListBuilt;

		private int indexToSelect;
	}
}
