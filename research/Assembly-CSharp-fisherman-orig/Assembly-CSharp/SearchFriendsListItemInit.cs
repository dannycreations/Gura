using System;
using System.Diagnostics;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class SearchFriendsListItemInit : FriendListItemBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> OnConfirmFriendship;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> OnDeclineFriendship;

	protected override void Awake()
	{
		base.Awake();
		PhotonConnectionFactory.Instance.OnFriendshipAdded += this.OnFriendshipAdded;
		PhotonConnectionFactory.Instance.OnFriendshipRequest += this.OnFriendshipRequest;
		PhotonConnectionFactory.Instance.OnFriendshipRemoved += this.OnFriendshipRemoved;
	}

	public override void Init(Player player, GameObject friendsContentPanel)
	{
		base.Init(player, friendsContentPanel);
		if (PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.Friends != null)
		{
			if (PhotonConnectionFactory.Instance.Profile.Friends.Exists((Player x) => x.Status == FriendStatus.FriendshipRequested && x.UserId.Equals(player.UserId, StringComparison.InvariantCultureIgnoreCase)))
			{
				this.MarkAsRequestSent();
			}
			if (PhotonConnectionFactory.Instance.Profile.Friends.Exists((Player x) => x.Status == FriendStatus.FriendshipRequest && x.UserId.Equals(player.UserId, StringComparison.InvariantCultureIgnoreCase)))
			{
				this.MarkAsRequestReceived();
			}
			if (PhotonConnectionFactory.Instance.Profile.Friends.Exists((Player x) => x.Status == FriendStatus.Friend && x.UserId.Equals(player.UserId, StringComparison.InvariantCultureIgnoreCase)))
			{
				this.MarkAsFriends();
			}
			if (PhotonConnectionFactory.Instance.Profile.Friends.Exists((Player x) => x.Status == FriendStatus.Ignore && x.UserId.Equals(player.UserId, StringComparison.InvariantCultureIgnoreCase)))
			{
				this.MarkAsIgnored();
			}
		}
	}

	private void OnDisable()
	{
		PhotonConnectionFactory.Instance.OnFriendshipAdded -= this.OnFriendshipAdded;
		PhotonConnectionFactory.Instance.OnFriendshipRequest -= this.OnFriendshipRequest;
		PhotonConnectionFactory.Instance.OnFriendshipRemoved -= this.OnFriendshipRemoved;
	}

	public void RequestFriendship()
	{
		if (PhotonConnectionFactory.Instance.FriendsCount < 100)
		{
			PhotonConnectionFactory.Instance.RequestFriendship(this._player);
			this.MarkAsRequestSent();
		}
		else
		{
			this.messageBox = MenuHelpers.Instance.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("FriendsLimitText"), false, false, false, null);
			this.messageBox.GetComponent<MessageBox>().ConfirmButtonText = ScriptLocalization.Get("CloseButton");
			this.messageBox.GetComponent<EventAction>().ActionCalled += this.CloseMessageBox;
			this.messageBox.Open();
		}
	}

	public void MarkAsRequestSent()
	{
		this.requestFriendshipButton.gameObject.SetActive(false);
		this.ConfirmFriendshipButton.SetActive(false);
		this.DeclineFriendshipButton.SetActive(false);
		this.MessageText2.gameObject.SetActive(false);
		this.MessageText.gameObject.SetActive(true);
		this.MessageText.text = ScriptLocalization.Get("RequestSentText");
	}

	public void MarkAsRequestReceived()
	{
		this.requestFriendshipButton.gameObject.SetActive(false);
		this.ConfirmFriendshipButton.SetActive(true);
		this.DeclineFriendshipButton.SetActive(true);
		this.MessageText2.gameObject.SetActive(true);
		this.MessageText.gameObject.SetActive(false);
		this.MessageText2.text = ScriptLocalization.Get("RequestReceivedText");
	}

	public void MarkAsFriends()
	{
		this.requestFriendshipButton.gameObject.SetActive(false);
		this.ConfirmFriendshipButton.SetActive(false);
		this.DeclineFriendshipButton.SetActive(false);
		this.MessageText2.gameObject.SetActive(false);
		this.MessageText.gameObject.SetActive(true);
		this.MessageText.text = ScriptLocalization.Get("YouAreFriendsText");
	}

	public void MarkAsNotFriends()
	{
		this.requestFriendshipButton.gameObject.SetActive(true);
		this.ConfirmFriendshipButton.SetActive(false);
		this.DeclineFriendshipButton.SetActive(false);
		this.MessageText2.gameObject.SetActive(false);
		this.MessageText.gameObject.SetActive(false);
	}

	public void MarkAsIgnored()
	{
		this.requestFriendshipButton.gameObject.SetActive(false);
		this.ConfirmFriendshipButton.SetActive(false);
		this.DeclineFriendshipButton.SetActive(false);
		this.MessageText2.gameObject.SetActive(false);
		this.MessageText.gameObject.SetActive(true);
		this.MessageText.text = ScriptLocalization.Get("UserIsIgnoreMessage");
	}

	private void CloseMessageBox(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	private void OnFriendshipAdded(Player player)
	{
		if (player.UserId == this.UserId)
		{
			this.MarkAsFriends();
		}
	}

	private void OnFriendshipRequest(Player player)
	{
		if (player.UserId == this.UserId)
		{
			this._player = player;
			this.MarkAsRequestReceived();
		}
	}

	private void OnFriendshipRemoved(Player player)
	{
		if (player.UserId == this.UserId)
		{
			this._player = player;
			this.MarkAsNotFriends();
		}
	}

	public void ConfirmFriendship()
	{
		MonoBehaviour.print(this._player);
		PhotonConnectionFactory.Instance.ReplyToFriendshipRequest(this._player, FriendshipAction.Confirm);
		this.MarkAsFriends();
	}

	public void DeclineFriendship(object sender, EventArgs e)
	{
		PhotonConnectionFactory.Instance.ReplyToFriendshipRequest(this._player, FriendshipAction.Cancel);
		this.CloseDeclineMessage(this, new EventArgs());
		this.OnDeclineFriendship(this, new EventArgs());
	}

	private void CloseDeclineMessage(object sender, EventArgs e)
	{
		this.messageBox.Close();
	}

	public void ShowFrienshipDeclineMessage()
	{
		string text = "<b>" + this._player.UserName + "</b> ?";
		this.messageBox = MenuHelpers.Instance.ShowMessageThreeSelectable(null, ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("ApproveDeclineFriendMessage"), text), ScriptLocalization.Get("DeclineCaption"), ScriptLocalization.Get("CancelButton"), ScriptLocalization.Get("DeclineAndIgnoreCaption")).GetComponent<MessageBoxBase>();
		this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.DeclineFriendship;
		this.messageBox.GetComponent<EventConfirmAction>().ThirdButtonActionCalled += this.DeclineFriendshipIgnore;
		this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CloseDeclineMessage;
		this.messageBox.Open();
	}

	public void DeclineFriendshipIgnore(object sender, EventArgs e)
	{
		PhotonConnectionFactory.Instance.ReplyToFriendshipRequest(this._player, FriendshipAction.Ignore);
		this.CloseDeclineMessage(this, new EventArgs());
		this.OnDeclineFriendship(this, new EventArgs());
	}

	public GameObject requestFriendshipButton;

	public Text MessageText;

	public Text MessageText2;

	public GameObject ConfirmFriendshipButton;

	public GameObject DeclineFriendshipButton;

	private MessageBoxBase messageBox;

	private MenuHelpers helpers = new MenuHelpers();

	public bool isInGame;
}
