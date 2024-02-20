using System;
using System.Diagnostics;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomListItemInit : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> OnJoinRoom;

	public void Init(RoomPopulation room, int roomListNumber)
	{
		this._room = room;
		this.RoomName.text = string.Concat(new object[]
		{
			ScriptLocalization.Get("RoomCaption"),
			roomListNumber + 1,
			"<color=#54ac54>(",
			this._room.FriendList.Length.ToString(),
			")</color>"
		});
		if (room.LanguageId != PhotonConnectionFactory.Instance.Profile.LanguageId && (room.LanguageId != 1 || PhotonConnectionFactory.Instance.Profile.LanguageId != 3))
		{
			int num = room.LanguageId - 1;
			CustomLanguages customLanguages = (CustomLanguages)num;
			CustomLanguagesShort customLanguagesShort = ChangeLanguage.GetCustomLanguagesShort(customLanguages);
			this.LanguageText.text = "[" + customLanguagesShort + "]";
			this.LanguageText.gameObject.SetActive(true);
		}
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
	}

	public void JoinRoom()
	{
		if (this._isMoving)
		{
			return;
		}
		this._isMoving = true;
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Loading);
		StaticUserData.CurrentLocation = ShowLocationInfo.Instance.CurrentLocation;
		PhotonConnectionFactory.Instance.MoveToRoom(PhotonConnectionFactory.Instance.CurrentPondId.Value, this._room.RoomId);
		this.CloseFriendsPopup();
		this.OnJoinRoom(this, new EventArgs());
	}

	private void OnMoved(TravelDestination destination)
	{
		EventSystem.current.GetComponent<CursorManager>().SetCursor(CursorType.Standart);
		this._isMoving = false;
	}

	public void ShowFriendsPopup()
	{
		this.roomFriendsPopup = GUITools.AddChild(base.gameObject, this.roomFriendsPopupPrefab);
		this.roomFriendsPopup.GetComponent<RectTransform>().anchoredPosition = new Vector3(250f, 9f, 0f);
		this.roomFriendsPopup.GetComponent<RoomFriendsPopupInit>().Init(this._room.FriendList);
	}

	public void CloseFriendsPopup()
	{
		Object.Destroy(this.roomFriendsPopup);
	}

	public Text RoomName;

	public Text RoomFriendsCount;

	public GameObject JoinButton;

	public GameObject Background;

	public GameObject messageBoxPrefab;

	public GameObject roomFriendsPopupPrefab;

	public Text LanguageText;

	private GameObject roomFriendsPopup;

	private GameObject messageBox;

	private RoomPopulation _room;

	private bool _isMoving;
}
