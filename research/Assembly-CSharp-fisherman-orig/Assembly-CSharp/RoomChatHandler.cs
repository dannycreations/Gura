using System;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomChatHandler : MonoBehaviour
{
	public RoomPopulation CurrentRoom { get; private set; }

	private void Start()
	{
		this.Button.onClick.AddListener(delegate
		{
			this.OnClickAction();
		});
	}

	internal void Init(RoomPopulation room, GameObject backgroundPanel, int roomListNumber)
	{
		this.CurrentRoom = room;
		this._backgroundPanel = backgroundPanel;
		string text = string.Empty;
		if (room.LanguageId != PhotonConnectionFactory.Instance.Profile.LanguageId && (room.LanguageId != 1 || PhotonConnectionFactory.Instance.Profile.LanguageId != 3))
		{
			int num = room.LanguageId - 1;
			CustomLanguages customLanguages = (CustomLanguages)num;
			CustomLanguagesShort customLanguagesShort = ChangeLanguage.GetCustomLanguagesShort(customLanguages);
			text = "[" + customLanguagesShort + "]";
		}
		this.RoomName.text = string.Concat(new object[]
		{
			ScriptLocalization.Get("RoomCaption"),
			roomListNumber + 1,
			"<color=#54ac54>(",
			this.CurrentRoom.FriendList.Length.ToString(),
			")</color>",
			text
		});
	}

	public void OnPointerEnterAction()
	{
		this._menuPanel = GUITools.AddChild(this._backgroundPanel, this.RoomChatActionMenuPrefab);
		this._menuPanel.GetComponent<RectTransform>().position = new Vector3(base.transform.position.x + 80f, base.transform.position.y - 15f, 0f);
		this._menuPanel.GetComponent<RoomFriendsPopupInit>().Init(this.CurrentRoom.FriendList);
	}

	public void OnPointerExitAction()
	{
		if (this._menuPanel != null)
		{
			Object.Destroy(this._menuPanel);
		}
	}

	public void Remove()
	{
		Object.Destroy(base.gameObject);
	}

	private void OnClickAction()
	{
		if (GameFactory.Player != null && GameFactory.Player.State != typeof(PlayerIdle) && GameFactory.Player.State != typeof(PlayerIdlePitch) && GameFactory.Player.State != typeof(PlayerDrawIn) && GameFactory.Player.State != typeof(PlayerEmpty) && GameFactory.Player.State != typeof(ToolIdle))
		{
			GameFactory.Message.ShowCantChangeRoomWhenCasted();
		}
		else
		{
			this._messageBox = MenuHelpers.Instance.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("GoToRoomQuestion"), ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), false, false);
			this._messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.CompleteMessage_ActionCalled;
			this._messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CancelMessage_ActionCalled;
		}
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		this._messageBox.AlphaFade.HideFinished += this.Instance_HideFinished;
		this._messageBox.Close();
	}

	private void Instance_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this._messageBox.AlphaFade.HideFinished -= this.Instance_HideFinished;
		BlackScreenHandler.Show(false, null);
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
		PhotonConnectionFactory.Instance.MoveToRoom(StaticUserData.CurrentPond.PondId, this.CurrentRoom.RoomId);
		this.OnPointerExitAction();
	}

	private void OnMoved(TravelDestination destination)
	{
		BlackScreenHandler.Hide();
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
		PhotonConnectionFactory.Instance.Game.Resume(true);
		StaticUserData.ChatController.ClearMessages(MessageChatType.Location);
		GameFactory.ChatInGameController.RoomsToggle.isOn = false;
		PhotonConnectionFactory.Instance.GetChatPlayersCount();
	}

	private void CancelMessage_ActionCalled(object sender, EventArgs e)
	{
		if (this._messageBox != null)
		{
			this._messageBox.Close();
		}
	}

	public Text RoomName;

	public Button Button;

	public GameObject RoomChatActionMenuPrefab;

	private GameObject _menuPanel;

	private GameObject _backgroundPanel;

	private MessageBoxBase _messageBox;

	private readonly PondHelpers _pondHelpers = new PondHelpers();
}
