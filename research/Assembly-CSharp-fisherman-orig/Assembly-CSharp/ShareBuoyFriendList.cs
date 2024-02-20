using System;
using System.Collections.Generic;
using System.Linq;
using FriendsSRIA.ViewsHolders;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShareBuoyFriendList : MessageBoxBase
{
	protected override void Awake()
	{
		base.Awake();
		if (this.caption != null)
		{
			this.caption.text = string.Empty;
		}
		if (this.cancelButtonText != null)
		{
			this.cancelButtonText.text = ScriptLocalization.Get("CancelButton");
		}
		List<Player> friends = PhotonConnectionFactory.Instance.Profile.Friends;
		this._alphaFade.FastHidePanel();
		base.gameObject.SetActive(false);
		this.ConfirmButton.interactable = false;
		MessageFactory.MessageBoxQueue.Enqueue(this);
		if (this.FriendsView == null)
		{
			this.FriendsView = base.GetComponentInChildren<FriendsHandlerNew>();
		}
		this.FriendsView.IsSelectedItem += this.ShareBuoyFriendSelected;
		this.FriendsView.UpdatedItem += this.ItemUpdated;
	}

	public void AcceptPressed()
	{
		if (!string.IsNullOrEmpty(this._currentSelectedFriendName))
		{
			this.ConfirmAction(this._currentSelectedFriendName);
		}
		else
		{
			MonoBehaviour.print("no friend selected");
		}
		this.Close();
	}

	public void Init(BuoySetting buoy)
	{
		if (buoy.Fish != null)
		{
			ConcreteTrophyInit fishPanel = this.FishPanel;
			CaughtFish fish = buoy.Fish;
			DateTime? createdTime = buoy.CreatedTime;
			fishPanel.Refresh(fish, (createdTime == null) ? DateTime.UtcNow : createdTime.Value, buoy.PondId);
		}
		else
		{
			this.FishPanel.SetPanelEmpty();
		}
	}

	public new void Open()
	{
		this._alphaFade.ShowPanel();
	}

	public new void Close()
	{
		this.closing = true;
		this._alphaFade.HidePanel();
	}

	public string Caption
	{
		set
		{
			this.caption.text = value;
		}
	}

	public string CancelButtonText
	{
		set
		{
			this.cancelButtonText.text = value.ToUpper();
		}
	}

	public string AcceptButtonText
	{
		set
		{
			this.confirmButtonText.text = value.ToUpper();
		}
	}

	public void Update()
	{
		this.CancelButton.enabled = !this.ClearSearchButton.isActiveAndEnabled;
	}

	private void ItemUpdated(object sender, FriendEventArgs e)
	{
		if (sender == null)
		{
			return;
		}
		FriendItemVH friendItemVH = sender as FriendItemVH;
		if (friendItemVH.current.Username.text.Length > 10)
		{
			friendItemVH.current.Username.text = friendItemVH.current.Username.text.Substring(0, 10) + "...";
		}
		if (!string.IsNullOrEmpty(this._currentSelectedFriendName) && e.Name == this._currentSelectedFriendName)
		{
			(sender as FriendItemVH)._background.color = this.HighlightColor;
		}
	}

	private void ShareBuoyFriendSelected(object sender, FriendEventArgs e)
	{
		if (this._lastSelected != null && this._lastSelected.current._player.UserName == this._currentSelectedFriendName)
		{
			this._lastSelected.UpdateViews(this._lastSelected.model);
		}
		this._lastSelected = sender as FriendItemVH;
		this._currentSelectedFriendName = e.Name;
		this.ItemUpdated(sender, e);
		if (PhotonConnectionFactory.Instance.Profile.Friends == null)
		{
			this.ConfirmButton.interactable = false;
			return;
		}
		if (!PhotonConnectionFactory.Instance.Profile.Friends.Any((Player x) => x.UserName == this._currentSelectedFriendName && x.Status == FriendStatus.Friend))
		{
			this.ConfirmButton.interactable = false;
			if (!this.closing && !string.IsNullOrEmpty(e.Name))
			{
				GameFactory.Message.ShowMessage(ScriptLocalization.Get("ShareMarkOnlyFriends"), null, 2f, false);
			}
		}
		else
		{
			this.ConfirmButton.interactable = true;
		}
	}

	public TextMeshProUGUI caption;

	public Text cancelButtonText;

	public Text confirmButtonText;

	public Button ConfirmButton;

	public HotkeyPressRedirect CancelButton;

	public Button ClearSearchButton;

	public Action<string> ConfirmAction;

	public ConcreteTrophyInit FishPanel;

	public FriendsHandlerNew FriendsView;

	private BuoySetting _buoy;

	public Color HighlightColor;

	private string _currentSelectedFriendName;

	private bool closing;

	private FriendItemVH _lastSelected;
}
