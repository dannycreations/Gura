using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using LeaderboardSRIA.Models;
using LeaderboardSRIA.ViewsHolders;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leaderboard
{
	public class LeaderboardSRIA : SRIA<MyParams, BaseVH>
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
			base.Parameters.SetItemSize(base.Parameters.entryPrefab.rect.height);
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
					this.SelectFirst();
				}
			}
		}

		public void SelectFirst()
		{
			if (this._Params.data != null && this._Params.data.Count > 0)
			{
				this.SelectAndScrollToIndex(0);
			}
			else
			{
				base.StopAllCoroutines();
				this.rootNavigation.SetPaused(false);
				this.rootNavigation.SetFirstActive();
			}
		}

		public void SelectAndScrollToIndex(int index)
		{
			this.rootNavigation.SetPaused(true);
			UINavigation.SetSelectedGameObject(null);
			base.StopAllCoroutines();
			this.ScrollTo(index, 0f, 0f);
			base.StartCoroutine(this.SetElementSelected(index, 0.06f));
		}

		protected override void Start()
		{
			ActivityState parentActivityState = ActivityState.GetParentActivityState(base.transform);
			if (parentActivityState != null)
			{
				this._cg = parentActivityState.CanvasGroup;
			}
			base.Start();
			this.CreateMyVH();
		}

		protected virtual BaseVH GetViewHodler()
		{
			return new FishItemVH();
		}

		protected override BaseVH CreateViewsHolder(int itemIndex)
		{
			BaseVH viewHodler = this.GetViewHodler();
			viewHodler.Init(this._Params.entryPrefab, itemIndex, true, true);
			return viewHodler;
		}

		protected override void UpdateViewsHolder(BaseVH newOrRecycled)
		{
			BaseModel baseModel = this._Params.data[newOrRecycled.ItemIndex];
			newOrRecycled.root.gameObject.SetActive(true);
			newOrRecycled.UpdateViews(baseModel);
			this.rootNavigation.ForceUpdate();
		}

		private IEnumerator SetElementSelected(int itemIndex, float time = 0.1f)
		{
			yield return new WaitForSeconds(time);
			this.rootNavigation.SetPaused(false);
			if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
			{
				BaseVH itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(itemIndex);
				if (itemViewsHolderIfVisible != null)
				{
					Selectable componentInChildren = itemViewsHolderIfVisible.root.GetComponentInChildren<Selectable>();
					UINavigation.SetSelectedGameObject((!(componentInChildren != null)) ? itemViewsHolderIfVisible.root.gameObject : componentInChildren.gameObject);
				}
				else
				{
					this.rootNavigation.SetFirstActive();
				}
				this.rootNavigation.UpdateFromEventSystemSelected();
			}
			yield break;
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<FriendEventArgs> IsSelectedItem;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event EventHandler<FriendEventArgs> OnItemUpdated;

		public void SetMine(TopPlayerBase myData)
		{
			this._hasMine = true;
			BaseModel baseModel = this.CreateModelFromItem(myData, (!this._lastOrdered.Contains(myData)) ? 0 : this._lastOrdered.IndexOf(myData));
			this.myVH.UpdateViews(baseModel);
		}

		public virtual void LoadEntriesList(IEnumerable<TopPlayerBase> players)
		{
			if (!base.Initialized)
			{
				return;
			}
			this._hasMine = false;
			this._lastOrdered = ((players == null) ? new List<TopPlayerBase>() : new List<TopPlayerBase>(players));
			TopPlayerBase topPlayerBase = this._lastOrdered.FirstOrDefault((TopPlayerBase x) => x != null && x.UserId == PhotonConnectionFactory.Instance.Profile.UserId);
			if (topPlayerBase != null)
			{
				this.SetMine(topPlayerBase);
			}
			else
			{
				this._hasMine = false;
			}
			this._Params.data.Clear();
			for (int i = 0; i < this._lastOrdered.Count; i++)
			{
				TopPlayerBase topPlayerBase2 = this._lastOrdered[i];
				BaseModel baseModel = this.CreateModelFromItem(topPlayerBase2, i);
				this._Params.data.Add(baseModel);
			}
			this.ResetItems(this._Params.data.Count, false, false);
			if (this.OnListBuilt != null)
			{
				this.OnListBuilt();
			}
			this.SelectFirst();
		}

		protected virtual BaseModel CreateModelFromItem(TopPlayerBase entry, int index)
		{
			return null;
		}

		protected virtual void CreateMyVH()
		{
			RectTransform rectTransform = Object.Instantiate<RectTransform>(base.Parameters.entryPrefab, this.MyParent.transform as RectTransform);
			rectTransform.anchoredPosition = Vector2.zero;
			this.myEntry = rectTransform.gameObject;
			this.myVH = this.GetViewHodler();
			this.myVH.root = this.myEntry.transform as RectTransform;
			this.myVH.CollectViews();
		}

		protected override void Update()
		{
			if (this._cg != null && !this._cg.interactable)
			{
				return;
			}
			base.Update();
			if (this._lastOrdered == null)
			{
				if (this.MyParent.activeSelf)
				{
					this.MyParent.SetActive(false);
				}
			}
			else
			{
				TopPlayerBase topPlayerBase = this._lastOrdered.FirstOrDefault((TopPlayerBase x) => x != null && x.UserId == PhotonConnectionFactory.Instance.Profile.UserId);
				if (topPlayerBase != null)
				{
					if (base.GetItemViewsHolderIfVisible(this._lastOrdered.IndexOf(topPlayerBase)) == null)
					{
						if (!this.MyParent.activeSelf)
						{
							this.MyParent.SetActive(true);
						}
					}
					else if (this.MyParent.activeSelf)
					{
						this.MyParent.SetActive(false);
					}
				}
				else if (this._hasMine)
				{
					if (!this.MyParent.activeSelf)
					{
						this.MyParent.SetActive(true);
					}
				}
				else if (this.MyParent.activeSelf)
				{
					this.MyParent.SetActive(false);
				}
			}
		}

		public void OnEntrySelected(object sender, FriendEventArgs e)
		{
			if (this.IsSelectedItem != null)
			{
				this.IsSelectedItem(sender, e);
			}
		}

		public void Clear()
		{
			this._Params.data.Clear();
			this.ResetItems(0, false, false);
		}

		private CanvasGroup _cg;

		public GameObject friendsListContent;

		public UINavigation rootNavigation;

		public GameObject MyParent;

		private GameObject myEntry;

		private BaseVH myVH;

		public Action OnListBuilt;

		private List<TopPlayerBase> _lastOrdered;

		private bool _hasMine;
	}
}
