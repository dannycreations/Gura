using System;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.PlayerProfile;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UserChatHandler : MonoBehaviour
{
	public string UserName
	{
		get
		{
			return (this._currentPlayer == null) ? null : this._currentPlayer.UserName;
		}
	}

	public GameObject MenuPanel
	{
		get
		{
			return this._menuPanel;
		}
	}

	public void Init(Player player, GameObject backgroundPanel, bool is3DInit = false, bool isDummy = false)
	{
		this._isDummy = isDummy;
		this._voiceIco.gameObject.SetActive(false);
		this.BlockIcon.SetActive(false);
		this._currentPlayer = player;
		Player player2 = null;
		if (this._isDummy)
		{
			this.Name.text = string.Format(" {0} ", this._currentPlayer.UserName);
			return;
		}
		if (PhotonConnectionFactory.Instance.Profile.Friends != null)
		{
			player2 = PhotonConnectionFactory.Instance.Profile.Friends.FirstOrDefault((Player x) => x.UserId == player.UserId);
		}
		string playerNameLevelRank = PlayerProfileHelper.GetPlayerNameLevelRank(this._currentPlayer);
		bool flag = player2 != null && player2.Status == FriendStatus.Friend;
		this.Name.text = string.Format((!flag) ? " {0}" : "<color=green> {0}</color>", playerNameLevelRank);
		if (player2 != null && player2.Status == FriendStatus.Ignore)
		{
			this.BlockIcon.SetActive(true);
		}
	}

	public void UpdateVoice(bool isMuted, bool isTalking)
	{
		bool flag = isMuted || isTalking;
		if (flag != this._voiceIco.gameObject.activeSelf)
		{
			this._voiceIco.gameObject.SetActive(flag);
			if (this._voiceIco.gameObject.activeSelf)
			{
				this._voiceIco.text = ((!isMuted) ? "\ue777" : "\ue778");
			}
		}
		if (this._menuPanel != null && this._menuHandler != null)
		{
			this._menuHandler.UpdateVoice(isMuted, isTalking, false);
		}
	}

	public void ResetCounters()
	{
		this._enterCounter = 0;
	}

	private bool IsFreeToOpen()
	{
		return !GameFactory.Player.IsTackleThrown && !GameFactory.Player.CantOpenInventory && !GameFactory.Player.IsInteractionWithRodStand;
	}

	private void Update()
	{
		if (this._isDummy)
		{
			return;
		}
		if (this._enterCounter > 0 && this._menuPanel == null && this.IsFreeToOpen())
		{
			this._menuPanel = GUITools.AddChild(base.transform.parent.parent.parent.gameObject, this.PlayerChatActionMenuPrefab);
			this._menuPanel.GetComponent<RectTransform>().position = new Vector3(base.transform.position.x, base.transform.position.y, 0f);
			this._menuHandler = this._menuPanel.GetComponent<PlayerChatMenuHandler>();
			this._menuHandler.PointerEnter += this.UserChatHandler_PointerEnter;
			this._menuHandler.PointerExit += this.UserChatHandler_PointerExit;
			this._menuHandler.ClickShowProfile += this.UserChatHandler_ClickShowProfile;
			this._menuHandler.ClickAddToFriend += this._menuHandler_ClickAddToFriend;
			this._menuHandler.ClickToIgnore += this._menuHandler_ClickToIgnore;
			this._menuHandler.ClickReport += this._menuHandler_ClickReport;
			this._menuHandler.ClickToUnIgnore += this._menuHandler_ClickToUnIgnore;
			this._menuHandler.ClickPrivateChat += this._menuHandler_ClickPrivateChat;
			this._menuHandler.ClickMute += this._menuHandler_ClickMute;
			if (this._currentPlayer != null)
			{
				this._menuHandler.Init(this._currentPlayer);
			}
		}
		else if ((this._enterCounter <= 0 || !this.IsFreeToOpen()) && this._menuPanel != null)
		{
			this._enterCounter = 0;
			this._menuHandler.PointerEnter -= this.UserChatHandler_PointerEnter;
			this._menuHandler.PointerExit -= this.UserChatHandler_PointerExit;
			this._menuHandler.ClickShowProfile -= this.UserChatHandler_ClickShowProfile;
			this._menuHandler.ClickAddToFriend -= this._menuHandler_ClickAddToFriend;
			this._menuHandler.ClickToIgnore -= this._menuHandler_ClickToIgnore;
			this._menuHandler.ClickReport -= this._menuHandler_ClickReport;
			this._menuHandler.ClickToUnIgnore -= this._menuHandler_ClickToUnIgnore;
			this._menuHandler.ClickPrivateChat -= this._menuHandler_ClickPrivateChat;
			this._menuHandler.ClickMute -= this._menuHandler_ClickMute;
			Object.Destroy(this._menuPanel);
			this._menuPanel = null;
		}
		if (ControlsController.ControlsActions.UICancel.WasPressed && this._messageBox != null)
		{
			this._messageBox.Close();
		}
	}

	private void _menuHandler_ClickMute(object sender, EventArgs e)
	{
	}

	private void _menuHandler_ClickReport(object sender, EventArgs e)
	{
		PhotonConnectionFactory.Instance.SendChatCommand(1, this._currentPlayer.UserName);
	}

	private void _menuHandler_ClickPrivateChat(object sender, EventArgs e)
	{
		if (!StaticUserData.ChatController.OpenPrivates.Any((Player x) => x.UserId == this._currentPlayer.UserId))
		{
			StaticUserData.ChatController.OpenPrivates.Add(this._currentPlayer);
			ToggleChatController.Instance.AddPrivateChat(this._currentPlayer);
		}
		else
		{
			ToggleChatController.Instance.ActivatePrivateChat(this._currentPlayer);
		}
	}

	private void _menuHandler_ClickToUnIgnore(object sender, EventArgs e)
	{
		PhotonConnectionFactory.Instance.UnIgnorePlayer(this._currentPlayer);
		this.BlockIcon.SetActive(false);
	}

	private void _menuHandler_ClickToIgnore(object sender, EventArgs e)
	{
		this._messageBox = MenuHelpers.Instance.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("IgnoreConfirmText"), ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, false);
		this._messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.CompleteMessage_ActionCalled;
		this._messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CancelMessage_ActionCalled;
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		PhotonConnectionFactory.Instance.IgnorePlayer(this._currentPlayer);
		this._messageBox.Close();
		this.BlockIcon.SetActive(true);
	}

	private void CancelMessage_ActionCalled(object sender, EventArgs e)
	{
		this._messageBox.Close();
	}

	private void _menuHandler_ClickAddToFriend(object sender, EventArgs e)
	{
		if (PhotonConnectionFactory.Instance.FriendsCount < 100)
		{
			PhotonConnectionFactory.Instance.RequestFriendship(this._currentPlayer);
		}
	}

	private void UserChatHandler_ClickShowProfile(object sender, EventArgs e)
	{
		this.ShowPlayerProfile();
	}

	public void OnPointerEnter()
	{
		this._enterCounter++;
	}

	private void UserChatHandler_PointerExit(object sender, EventArgs e)
	{
		this._enterCounter--;
	}

	private void UserChatHandler_PointerEnter(object sender, EventArgs e)
	{
		this._enterCounter++;
	}

	public void OnPointerExit()
	{
		this._enterCounter--;
	}

	public void ShowPlayerProfile()
	{
		if (this.clicked == null)
		{
			this.clicked = this._currentPlayer.UserId;
			EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
			PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile += this.OnGotOtherPlayerProfile;
			PhotonConnectionFactory.Instance.RequestOtherPlayerProfile(this.clicked);
		}
	}

	private void OnGotOtherPlayerProfile(Profile profile)
	{
		PhotonConnectionFactory.Instance.OnGotOtherPlayerProfile -= this.OnGotOtherPlayerProfile;
		if (this.clicked == profile.UserId.ToString())
		{
			this.InitProfile(profile);
		}
	}

	private void InitProfile(Profile profile)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		this._messageBox = MenuHelpers.Instance.ShowPlayerProfile(profile, new Action(this.Cleanup));
		this._messageBox.GetComponent<InitPlayerProfile>().Init(profile, true);
	}

	private void Cleanup()
	{
		if (this._messageBox != null)
		{
			bool flag = this._messageBox.GetComponent<InitPlayerProfile>() != null;
			if (flag)
			{
			}
			this.clicked = null;
			this._messageBox = null;
		}
	}

	internal void OnDestroy()
	{
		if (this._menuHandler == null)
		{
			return;
		}
		try
		{
			this._menuHandler.PointerEnter -= this.UserChatHandler_PointerEnter;
			this._menuHandler.PointerExit -= this.UserChatHandler_PointerExit;
			this._menuHandler.ClickShowProfile -= this.UserChatHandler_ClickShowProfile;
			this._menuHandler.ClickAddToFriend -= this._menuHandler_ClickAddToFriend;
			this._menuHandler.ClickToIgnore -= this._menuHandler_ClickToIgnore;
			this._menuHandler.ClickToUnIgnore -= this._menuHandler_ClickToUnIgnore;
			this._menuHandler.ClickPrivateChat -= this._menuHandler_ClickPrivateChat;
			this._menuHandler.ClickMute -= this._menuHandler_ClickMute;
		}
		catch (Exception)
		{
		}
		Object.Destroy(this._menuPanel);
	}

	[SerializeField]
	private Text _voiceIco;

	private Player _currentPlayer;

	private GameObject _menuPanel;

	private int _enterCounter;

	private PlayerChatMenuHandler _menuHandler;

	private MessageBoxBase _messageBox;

	private string clicked;

	public Text Name;

	public GameObject BlockIcon;

	public GameObject PlayerChatActionMenuPrefab;

	public GameObject messageBoxPlayerProfilePrefab;

	private bool _isDummy;
}
