using System;
using System.Diagnostics;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;

public class FriendshipRequestListItemInit : FriendListItemBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> OnConfirmFriendship;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> OnDeclineFriendship;

	protected override void Awake()
	{
		base.Awake();
		this.dashboardFriendsButton = this._pondHelpers.PondPrefabsList.topMenuForm.transform.Find("Image/TopMenuRight/Friends").gameObject;
	}

	public override void Init(Player player, GameObject friendsContentPanel)
	{
		base.Init(player, friendsContentPanel);
		this.OnConfirmFriendship = null;
		this.OnDeclineFriendship = null;
	}

	public void ConfirmFriendship()
	{
		PhotonConnectionFactory.Instance.ReplyToFriendshipRequest(this._player, FriendshipAction.Confirm);
		this.OnConfirmFriendship(this, new EventArgs());
		this.dashboardFriendsButton.GetComponent<UpdateFriendRequestsCounter>().UpdateCounter();
	}

	public void DeclineFriendship(object sender, EventArgs e)
	{
		PhotonConnectionFactory.Instance.ReplyToFriendshipRequest(this._player, FriendshipAction.Cancel);
		this.CloseDeclineMessage(this, new EventArgs());
		this.OnDeclineFriendship(this, new EventArgs());
		this.dashboardFriendsButton.GetComponent<UpdateFriendRequestsCounter>().UpdateCounter();
	}

	public void DeclineFriendshipIgnore(object sender, EventArgs e)
	{
		PhotonConnectionFactory.Instance.ReplyToFriendshipRequest(this._player, FriendshipAction.Ignore);
		this.CloseDeclineMessage(this, new EventArgs());
		this.OnDeclineFriendship(this, new EventArgs());
	}

	private void CloseDeclineMessage(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	public void ShowFrienshipDeclineMessage()
	{
		GameObject gameObject = InfoMessageController.Instance.gameObject;
		string text = "<b>" + this._player.UserName + "</b> ?";
		this.messageBox = MenuHelpers.Instance.ShowMessageThreeSelectable(gameObject, ScriptLocalization.Get("UnfriendCaption"), string.Format(ScriptLocalization.Get("ApproveDeclineFriendMessage"), text), ScriptLocalization.Get("DeclineCaption"), ScriptLocalization.Get("DeclineAndIgnoreCaption"), ScriptLocalization.Get("CancelButton")).GetComponent<MessageBoxBase>();
		this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.DeclineFriendship;
		this.messageBox.GetComponent<EventConfirmAction>().ThirdButtonActionCalled += this.CloseDeclineMessage;
		this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.DeclineFriendshipIgnore;
		if (this.isInGame && MessageFactory.MessageBoxQueue.Contains(this.messageBox.GetComponent<MessageBox>()))
		{
			MessageFactory.MessageBoxQueue.Remove(this.messageBox.GetComponent<MessageBox>());
			this.messageBox.Open();
		}
	}

	public GameObject MessageBoxFrienshipDeclinePrefab;

	public GameObject DeclineButton;

	public GameObject ConfirmButton;

	private MessageBoxBase messageBox;

	private MenuHelpers helpers = new MenuHelpers();

	private PondHelpers _pondHelpers = new PondHelpers();

	private GameObject dashboardFriendsButton;

	public bool isInGame;
}
