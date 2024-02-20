using System;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Common.Managers;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FriendOptionsPanelInit : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnActive = delegate(bool b)
	{
	};

	public void Init(Player player)
	{
		this._player = player;
	}

	private void Unfriend(object sender, EventArgs e)
	{
		PhotonConnectionFactory.Instance.UnFriend(this._player, false);
		this.CloseMessageBox(this, new EventArgs());
	}

	public void AddToChat()
	{
		if (!StaticUserData.ChatController.OpenPrivates.Any((Player x) => x.UserId == this._player.UserId))
		{
			StaticUserData.ChatController.OpenPrivates.Add(this._player);
		}
		if (StaticUserData.CurrentLocation != null)
		{
			KeysHandlerAction.EscapeHandler(false);
			if (GameFactory.ChatInGameController != null)
			{
				GameFactory.ChatInGameController.OpenChat();
			}
		}
		else
		{
			MessageBox messageBox = this.helpers.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("ChatOnLocationText"), false, false, false, null);
			messageBox.GetComponent<EventAction>().ActionCalled += delegate(object sender, EventArgs args)
			{
				messageBox.Close();
			};
		}
	}

	private void UnfriendIgnore(object sender, EventArgs e)
	{
		PhotonConnectionFactory.Instance.UnFriend(this._player, true);
		this.CloseMessageBox(this, new EventArgs());
	}

	private void CloseMessageBox(object sender, EventArgs e)
	{
		if (this.messageBox != null)
		{
			this.messageBox.GetComponent<MessageBox>().Close();
		}
	}

	public void ShowUnfriendMessage()
	{
		GameObject gameObject = InfoMessageController.Instance.gameObject;
		this.messageBox = MenuHelpers.Instance.ShowMessageThreeSelectable(gameObject, ScriptLocalization.Get("UnfriendCaption"), string.Format(ScriptLocalization.Get("UnfriendConfirmCaption"), this._player.UserName), ScriptLocalization.Get("UnfriendCaption"), ScriptLocalization.Get("UnfriendIgnoreCaption"), ScriptLocalization.Get("CancelButton"));
		this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.Unfriend;
		this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.UnfriendIgnore;
		this.messageBox.GetComponent<EventConfirmAction>().ThirdButtonActionCalled += this.CloseMessageBox;
	}

	public void SendGift()
	{
		GameObject gameObject = InfoMessageController.Instance.gameObject;
		this.messageBox = GUITools.AddChild(gameObject, this.GiftsPopupPrefab);
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this.messageBox.GetComponent<GiftsPopupInit>().Init(this._player);
		this.messageBox.GetComponent<AlphaFade>().FastHidePanel();
		this.messageBox.GetComponent<AlphaFade>().ShowPanel();
	}

	public void ShowInviteFriend()
	{
	}

	public void GoFishing()
	{
	}

	public void ShowOptionsPanel()
	{
		this.OnActive(true);
	}

	public void HideOptionsPanel()
	{
		this.OnActive(false);
	}

	private Player _player;

	public GameObject UnfriendButton;

	public GameObject ChatButton;

	public GameObject MessageBoxUnfriendPrefab;

	public GameObject GiftsPopupPrefab;

	public Button InviteButton;

	private GameObject messageBox;

	private MenuHelpers helpers = new MenuHelpers();
}
