using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FriendsSRIA;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FriendsHandlerNew : ActivityStateControlled
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<FriendEventArgs> IsSelectedItem;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<FriendEventArgs> UpdatedItem;

	private void Update()
	{
		if (!base.ShouldUpdate())
		{
			return;
		}
		if (this.IsSearchingPossible && ControlsController.ControlsActions.UISubmit.WasReleased)
		{
			this.SearchPlayers();
		}
	}

	private void Awake()
	{
		if (this.InviteFriendButton != null)
		{
			this.InviteFriendButton.SetActive(false);
		}
	}

	protected override void Start()
	{
		base.Start();
		this.searchFriendsInput.placeholder.GetComponent<Text>().text = ScriptLocalization.Get("FindFriendsCaption");
		this.searchFriendsInput.onEndEdit.AddListener(delegate(string val)
		{
			if (ControlsController.ControlsActions.UISubmit.WasPressed)
			{
				this.SearchPlayers();
			}
		});
	}

	protected override void SetHelp()
	{
		FriendsHandlerSRIA friendsHandlerSRIA = this.listBuilder;
		friendsHandlerSRIA.OnListBuilt = (Action)Delegate.Combine(friendsHandlerSRIA.OnListBuilt, this.OnListBuilt);
		this.listBuilder.IsSelectedItem += this.IsSelectedItem;
		this.listBuilder.OnItemUpdated += this.UpdatedItem;
		this.GetFriends();
		PhotonConnectionFactory.Instance.OnFindPlayers += this.OnFindPlayers;
		PhotonConnectionFactory.Instance.OnFriendshipRequest += this.OnFriendshipRequest;
		PhotonConnectionFactory.Instance.OnFriendshipAdded += this.OnFriendshipAdded;
		PhotonConnectionFactory.Instance.OnFriendshipRemoved += this.OnFriendshipRemoved;
		PhotonConnectionFactory.Instance.OnFriendsDetailsRefreshed += this.OnFriendsDetailsRefreshed;
		PhotonConnectionFactory.Instance.OnChatCommandSucceeded += this.OnChatCommandSucceeded;
		if (FriendsHandlerNew.refferalReward == null)
		{
			PhotonConnectionFactory.Instance.OnGotReferralReward += this.OnGotReferralReward;
			PhotonConnectionFactory.Instance.GetReferralReward();
		}
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
	}

	protected override void HideHelp()
	{
		this.DesactivateInputField();
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
		FriendsHandlerSRIA friendsHandlerSRIA = this.listBuilder;
		friendsHandlerSRIA.OnListBuilt = (Action)Delegate.Remove(friendsHandlerSRIA.OnListBuilt, this.OnListBuilt);
		this.listBuilder.IsSelectedItem -= this.IsSelectedItem;
		this.listBuilder.OnItemUpdated -= this.UpdatedItem;
		base.CancelInvoke("RefreshFriendsDetails");
		this.ClearSearch();
		PhotonConnectionFactory.Instance.OnFindPlayers -= this.OnFindPlayers;
		PhotonConnectionFactory.Instance.OnFriendshipRequest -= this.OnFriendshipRequest;
		PhotonConnectionFactory.Instance.OnFriendshipAdded -= this.OnFriendshipAdded;
		PhotonConnectionFactory.Instance.OnFriendshipRemoved -= this.OnFriendshipRemoved;
		PhotonConnectionFactory.Instance.OnGotReferralReward -= this.OnGotReferralReward;
		PhotonConnectionFactory.Instance.OnFriendsDetailsRefreshed -= this.OnFriendsDetailsRefreshed;
		PhotonConnectionFactory.Instance.OnChatCommandSucceeded -= this.OnChatCommandSucceeded;
	}

	public void OnGotReferralReward(ReferralReward reward)
	{
		PhotonConnectionFactory.Instance.OnGotReferralReward -= this.OnGotReferralReward;
		FriendsHandlerNew.refferalReward = reward;
	}

	public void GetFriendsAction(object sender, EventArgs e)
	{
		this.GetFriends();
	}

	public void GetFriends()
	{
		this.listBuilder.LoadFriendsList(true);
		base.InvokeRepeating("RefreshFriendsDetails", 0f, 60f);
		this.indexToSelect = 0;
		base.StartCoroutine(this.WaitAndSelect());
	}

	public void ActivateInputField()
	{
		UINavigation.SetSelectedGameObject(this.searchFriendsInput.gameObject);
		this._isEditMode = true;
	}

	public void DesactivateInputField()
	{
		this._isEditMode = false;
		UINavigation.SetSelectedGameObject(null);
	}

	public void InviteFriend()
	{
		GameObject gameObject = GUITools.AddChild(InfoMessageController.Instance.gameObject, this.InviteMessage);
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		gameObject.GetComponent<InviteFriendMessage>().InitWithReward(FriendsHandlerNew.refferalReward);
	}

	public void ShowInviteFriend()
	{
	}

	private void RefreshFriendsDetails()
	{
		if (base.gameObject.activeInHierarchy)
		{
			PhotonConnectionFactory.Instance.RefreshFriendsDetails();
		}
	}

	private void OnFriendsDetailsRefreshed()
	{
		MonoBehaviour.print("OnFriendsDetailsRefreshed");
		if (this.clearSearchButton.gameObject.activeSelf)
		{
			return;
		}
		this.listBuilder.LoadFriendsList(false);
	}

	private IEnumerator SetScrollPosition()
	{
		yield return null;
		this.listBuilder.SmoothScrollTo(0, 0.1f, 0f, 0f, null, false);
		yield break;
	}

	private void OnPlayerSelected(object sender, FriendEventArgs e)
	{
		if (this.IsSelectedItem != null)
		{
			this.IsSelectedItem(sender, e);
		}
	}

	private void OnChatCommandSucceeded(ChatCommandInfo info)
	{
		MonoBehaviour.print(string.Concat(new object[] { "OnChatCommandSucceeded: ", info.Username, " ", info.Command }));
		switch (info.Command)
		{
		case 1:
			break;
		case 2:
		case 3:
			this.listBuilder.LoadFriendsList(true);
			break;
		case 4:
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void SearchPlayers()
	{
		if (this.IsSelectedItem != null)
		{
			this.IsSelectedItem(null, new FriendEventArgs
			{
				Name = string.Empty
			});
		}
		if (this.IsSearchingPossible)
		{
			this.listBuilder.Clear();
			string text = this.searchFriendsInput.text;
			if (text.Contains('@'))
			{
				PhotonConnectionFactory.Instance.FindPlayersByEmail(text);
			}
			else
			{
				PhotonConnectionFactory.Instance.FindPlayersByName(text);
			}
		}
		else
		{
			this.SearchFailedText.gameObject.SetActive(true);
			this.SearchFailedText.text = ScriptLocalization.Get("SearchFriendClarificationText");
		}
	}

	private bool IsSearchingPossible
	{
		get
		{
			string text = this.searchFriendsInput.text;
			return text != null && text.Length >= 3;
		}
	}

	public void ClearSearch()
	{
		this.searchFriendsInput.text = string.Empty;
		this.clearSearchButton.SetActive(false);
		this.SearchFailedText.gameObject.SetActive(false);
		this.listBuilder.Clear();
		if (this.IsSelectedItem != null)
		{
			this.IsSelectedItem(null, new FriendEventArgs
			{
				Name = string.Empty
			});
		}
	}

	private IEnumerator WaitAndSelect()
	{
		yield return new WaitForEndOfFrame();
		this.rootNavigation.ForceUpdateImmediately();
		List<Selectable> sorted = this.rootNavigation.Selectables.ToList<Selectable>();
		sorted.Sort((Selectable x, Selectable y) => y.transform.position.y.CompareTo(x.transform.position.y));
		Selectable next = ((this.indexToSelect >= sorted.Count) ? ((sorted.Count <= 0) ? null : sorted[sorted.Count - 1]) : sorted[this.indexToSelect]);
		MonoBehaviour.print("Sorted:");
		foreach (Selectable selectable in sorted)
		{
			MonoBehaviour.print(selectable.gameObject.name);
		}
		if (next != null)
		{
			if (BlockableRegion.Current != null)
			{
				BlockableRegion.Current.OverrideLastSelected(next.gameObject);
			}
			else
			{
				UINavigation.SetSelectedGameObject(next.gameObject);
				this.rootNavigation.UpdateFromEventSystemSelected();
			}
		}
		yield break;
	}

	private void OnFriendshipRequest(Player player)
	{
		this.listBuilder.LoadFriendsList(true);
	}

	private void OnFriendshipAdded(Player player)
	{
		this.listBuilder.LoadFriendsList(true);
	}

	private void OnFriendshipRemoved(Player player)
	{
		MonoBehaviour.print("on friendship removed");
		this.listBuilder.LoadFriendsList(true);
	}

	private void OnFindPlayers(IEnumerable<Player> players)
	{
		this.SearchFailedText.gameObject.SetActive(false);
		if (players != null)
		{
			List<Player> list = players.Where((Player x) => x.UserId != PhotonConnectionFactory.Instance.Profile.UserId.ToString() && x.Level > 1).ToList<Player>();
			this.listBuilder.LoadFoundList(list);
		}
		else
		{
			this.listBuilder.LoadFoundList(new List<Player>());
		}
		this.rootNavigation.SetFirstActive();
		if (this.OnListBuilt != null)
		{
			this.OnListBuilt();
		}
		this.clearSearchButton.SetActive(true);
	}

	private void OnFindPlayersFailed()
	{
	}

	public void InviteFriends()
	{
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		if (this._isEditMode)
		{
			base.StartCoroutine(this.StartEdit());
		}
	}

	private IEnumerator StartEdit()
	{
		yield return new WaitForEndOfFrame();
		this.ActivateInputField();
		this.searchFriendsInput.Select();
		this.searchFriendsInput.ActivateInputField();
		this.searchFriendsInput.MoveTextEnd(true);
		yield break;
	}

	public FriendsHandlerSRIA listBuilder;

	[Space(15f)]
	public InputField searchFriendsInput;

	public GameObject clearSearchButton;

	public GameObject searchButton;

	public Text SearchFailedText;

	[Space(15f)]
	private List<Player> allFriends;

	private List<Player> friendshipRequested;

	private List<Player> friendshipRequests;

	private List<Player> activeFriends;

	private List<Player> ignoredFriends;

	public UINavigation rootNavigation;

	public GameObject InviteMessage;

	public bool hideSearchButtonsOnPS4 = true;

	public GameObject InviteFriendButton;

	public Action OnListBuilt;

	private static ReferralReward refferalReward;

	private bool _isEditMode;

	private bool isLoadingList;

	private int _currIndex;

	private int _pageSize = 10;

	private int indexToSelect;
}
