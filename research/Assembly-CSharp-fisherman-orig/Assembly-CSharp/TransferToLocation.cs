using System;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using ObjectModel;
using TPM;
using UnityEngine;

public class TransferToLocation : MonoBehaviour
{
	private CharactersCreator CharactersCreator
	{
		get
		{
			if (this._charactersCreator == null)
			{
				GameObject gameObject = GameObject.Find("CharacterCustomizationFactory");
				this._charactersCreator = gameObject.GetComponent<CharactersCreator>();
			}
			return this._charactersCreator;
		}
	}

	public static TransferToLocation Instance
	{
		get
		{
			return TransferToLocation._instance;
		}
	}

	public string RoomToJoin
	{
		get
		{
			return this._roomToJoin;
		}
	}

	internal void Awake()
	{
		TransferToLocation._instance = this;
		PhotonConnectionFactory.Instance.OnMoved += this.OnMoved;
		PhotonConnectionFactory.Instance.OnMoveFailed += this.OnMoveFailed;
		PhotonConnectionFactory.Instance.OnTournamentStarted += this.OnTournamentStarted;
		PhotonConnectionFactory.Instance.OnUserCompetitionStarted += this.OnUGCStarted;
		this._competitionController = this.helpers.MenuPrefabsList.globalMapForm.GetComponent<CompetitionMapController>();
	}

	private void OnUGCStarted(UserCompetitionStartMessage info)
	{
		this.OnSomeCompetitionStarted();
	}

	private void OnTournamentStarted(TournamentStartInfo info)
	{
		this.OnSomeCompetitionStarted();
	}

	private void OnSomeCompetitionStarted()
	{
		PhotonConnectionFactory.Instance.Profile.PersistentData = null;
		PhotonConnectionFactory.Instance.SetLastPin(0);
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnMoved -= this.OnMoved;
		PhotonConnectionFactory.Instance.OnMoveFailed -= this.OnMoveFailed;
		PhotonConnectionFactory.Instance.OnTournamentStarted -= this.OnTournamentStarted;
		PhotonConnectionFactory.Instance.OnUserCompetitionStarted -= this.OnUGCStarted;
		if (this.subscribedToFishSold)
		{
			this.subscribedToFishSold = false;
			TournamentHelper.FishSold = (Action)Delegate.Remove(TournamentHelper.FishSold, new Action(this.FishSold));
		}
	}

	public void TournamentTimeEnded(TournamentIndividualResults result)
	{
	}

	public void SetActivePondUi(bool flag)
	{
		LogHelper.Log("___kocha SetActivePondUi flag:{0}", new object[] { flag });
		if (flag)
		{
			MenuHelpers.Instance.ShowMenu(false, false);
		}
		else
		{
			MenuHelpers.Instance.HideMenu(false, false, false);
		}
	}

	public void SetRoomID(string roomID)
	{
		Debug.LogWarning("Setted " + roomID);
		this._roomToJoin = roomID;
	}

	public void GoFishingPressed()
	{
		LogHelper.Log("GoFishingPressed");
		this.GoFishing(false);
	}

	public void GoFishing(bool doubleClick = false)
	{
		if (ManagerScenes.InTransition || TransferToLocation.IsMoving || BackToLobbyClick.IsLeaving)
		{
			return;
		}
		TransferToLocation.IsMoving = true;
		ManagerScenes.ProgressOfLoad = 0f;
		if (!this.CheckLicense())
		{
			this.ShowLicenceMissingMessage(doubleClick);
		}
		else if (doubleClick)
		{
			this.DoubleClickTransfer_ActionCalled(null, null);
		}
		else
		{
			TransferToLocation.IsMoving = false;
			this.TransferToRoom(this._roomToJoin);
		}
	}

	private bool CheckLicense()
	{
		return PhotonConnectionFactory.Instance.Profile.Licenses.Where((PlayerLicense x) => x.StateId == StaticUserData.CurrentPond.State.StateId && (!x.CanExpire || x.End > TimeHelper.UtcTime())).ToArray<PlayerLicense>().Length > 0;
	}

	public void DoubleClickTransfer()
	{
		this.GoFishing(true);
	}

	private void ShowLicenceMissingMessage(bool doubleClick)
	{
		MonoBehaviour.print("ShowLicenseMissing");
		GameObject gameObject = InfoMessageController.Instance.gameObject;
		this.messageBox = GUITools.AddChild(gameObject, this.helpers.MessageBoxList.messageBoxThreeSelectablePrefab).GetComponent<MessageBox>();
		this.messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
		this.messageBox.confirmButton.AddComponent<HintElementId>().SetElementId("BuyLicenseOnTravel", null, null);
		this.messageBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this.messageBox.Caption = ScriptLocalization.Get("MessageCaption");
		this.messageBox.Message = ScriptLocalization.Get("LicenceAbsentText");
		this.messageBox.ConfirmButtonText = ScriptLocalization.Get("ButtonBuyLicensesLower");
		string text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ScriptLocalization.Get("GoFishingButton").ToLower());
		if (!TournamentHelper.TOURNAMENT_ENDED_STILL_IN_ROOM && (TournamentHelper.IS_IN_TOURNAMENT || (CompetitionMapController.Instance != null && CompetitionMapController.Instance.IsJoinedToTournament)))
		{
			text = ScriptLocalization.Get(TournamentHelper.IsCompetition ? "EnterCompetitionCaption" : "EnterTournamentCaption").ToUpper(CultureInfo.InvariantCulture);
		}
		this.messageBox.CancelButtonText = text;
		this.messageBox.ThirdButtonText = ScriptLocalization.Get("CancelButton");
		this.messageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.LoadShop_ActionCalled;
		if (doubleClick)
		{
			this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.DoubleClickTransfer_ActionCalled;
		}
		else
		{
			this.messageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.ProceedFishing_ActionCalled;
		}
		this.messageBox.GetComponent<EventConfirmAction>().ThirdButtonActionCalled += this.CompleteMessage_ActionCalled;
	}

	private void LoadShop_ActionCalled(object sender, EventArgs e)
	{
		this.CompleteMessage_ActionCalled(sender, e);
		ShowLocationInfo.Instance.GotoLicenseShop();
	}

	private void ProceedFishing_ActionCalled(object sender, EventArgs e)
	{
		this.CompleteMessage_ActionCalled(sender, e);
		this.TransferToRoom(this._roomToJoin);
	}

	private void DoubleClickTransfer_ActionCalled(object sender, EventArgs e)
	{
		this.CompleteMessage_ActionCalled(sender, e);
		if ((PhotonConnectionFactory.Instance.CurrentTournamentId == null || PhotonConnectionFactory.Instance.Profile.Tournament == null) && (CompetitionMapController.Instance == null || !CompetitionMapController.Instance.IsJoinedToTournament))
		{
			this.TransferToRoom(this._roomToJoin);
		}
		else
		{
			this._competitionController.EnterToTournament();
		}
	}

	private void CompleteMessage_ActionCalled(object sender, EventArgs e)
	{
		TransferToLocation.IsMoving = false;
		if (this.messageBox != null)
		{
			this.messageBox.Close();
		}
	}

	public void Transfer()
	{
		this.TransferToRoom("top");
	}

	public void Transfer(string roomId)
	{
		this.TransferToRoom(roomId);
	}

	public void TransferIn3DGame(string roomId)
	{
		if (this._pondHelpers.PondControllerList.IsInMenu)
		{
			this.TransferToRoom(roomId);
			return;
		}
		this.SetRoomID(roomId);
		this.CalcTeleportation();
		if (this._isTeleportation || this.WillBeRoomChanged)
		{
			this.ChangingRoomAcceptedIn3D();
		}
		else
		{
			this.ActivateGameView();
		}
	}

	private void TransferToRoom(string roomId)
	{
		if (ShowLocationInfo.Instance == null || ShowLocationInfo.Instance.CurrentLocation == null)
		{
			return;
		}
		if (PhotonConnectionFactory.Instance.Profile.Tournament != null && !PhotonConnectionFactory.Instance.Profile.Tournament.IsEnded)
		{
			this._confirmMessageBox = MenuHelpers.Instance.ShowMessageSelectable(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("TournamentIsNotOverText"), true);
			this._confirmMessageBox.GetComponent<EventConfirmAction>().ConfirmActionCalled += this.TransferToLocation_ConfirmActionCalled;
			this._confirmMessageBox.GetComponent<EventConfirmAction>().CancelActionCalled += this.CloseMessageBox;
		}
		else
		{
			this.GotoFishingAction(roomId);
		}
	}

	private bool CheckRodPods()
	{
		if (RodHelper.HasAnyRonOnStand())
		{
			this._roomToJoin = this._currentJoined;
			this._confirmMessageBox = MenuHelpers.Instance.ShowMessage(null, ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("CantMoveRodPodsActive"), false, false, false, null);
			this._confirmMessageBox.GetComponent<EventAction>().ActionCalled += this.CloseMessageBox;
			return false;
		}
		return true;
	}

	private void GotoFishingAction(string roomName)
	{
		Debug.Log("GotoFishingAction");
		if (TransferToLocation.IsMoving)
		{
			return;
		}
		this.SetRoomID(roomName);
		this.CalcTeleportation();
		if (this._isTeleportation || this.WillBeRoomChanged)
		{
			if (this.WillBeRoomChanged && !this.CheckRodPods())
			{
				return;
			}
			this.ChangingRoomAccepted();
		}
		else
		{
			this.ActivateGameView();
		}
	}

	private void CalcTeleportation()
	{
		PersistentData persistentData = PhotonConnectionFactory.Instance.Profile.PersistentData;
		Location currentLocation = ShowLocationInfo.Instance.CurrentLocation;
		if (PhotonConnectionFactory.Instance.CurrentTournamentId != null && (persistentData == null || persistentData.LastPinId == 0))
		{
			this._isTeleportation = true;
			return;
		}
		this._isTeleportation = (this._lastPin == null || currentLocation.LocationId != this._lastPin.LocationId) && (persistentData == null || persistentData.LastPinId == 0 || currentLocation.LocationId != persistentData.LastPinId);
	}

	private bool WillBeRoomChanged
	{
		get
		{
			return StaticUserData.CurrentLocation == null || PhotonConnectionFactory.Instance.CurrentTournamentId != null || (this._roomToJoin != this._currentJoined && (this._roomToJoin == null || TransferToLocation.basicRoomNames.Contains(this._roomToJoin) || this._roomToJoin != PhotonConnectionFactory.Instance.Room.RoomId));
		}
	}

	private void ChangingRoomAccepted()
	{
		Debug.Log("ChangingRoomAccepted");
		this._pondHelpers.PondPrefabsList.currentActiveFormAS.Hide(false);
		this._pondHelpers.PondPrefabsList.topMenuFormAS.Hide(false);
		if (this._pondHelpers.PondPrefabsList.helpPanelAS != null)
		{
			this._pondHelpers.PondPrefabsList.helpPanelAS.Hide(false);
		}
		this._pondHelpers.PondPrefabsList.loadingFormAS.Show(false);
		this.InitializeMoveToNewLocation();
	}

	private void ChangingRoomAcceptedIn3D()
	{
		Debug.Log("ChangingRoomAcceptedIn3D");
		ShowHudElements.Instance.ShowChangeRoomPanel();
		this._pondHelpers.PondControllerList.HidePlayer();
		this.InitializeMoveToNewLocation();
	}

	private void InitializeMoveToNewLocation()
	{
		TransferToLocation.IsMoving = true;
		CursorManager.HideCursor();
		if (this._roomToJoin != this._currentJoined)
		{
			GameFactory.Player.StartMoveToNewLocation();
		}
		PhotonConnectionFactory.Instance.MoveToRoom(StaticUserData.CurrentPond.PondId, this._roomToJoin);
		if (!PhotonConnectionFactory.Instance.IsInRoom)
		{
			StaticUserData.ChatController.ClearMessages(MessageChatType.Location);
		}
	}

	private void CloseMessageBox(object sender, EventArgs e)
	{
		if (this._confirmMessageBox != null)
		{
			this._confirmMessageBox.Close();
		}
	}

	private void TransferToLocation_ConfirmActionCalled(object sender, EventArgs e)
	{
		this.CloseMessageBox(sender, e);
		this.LeaveTournament(true);
	}

	public void LeaveTournament(bool goToRoom = true)
	{
		this.shouldGoToPondOnFishSold = goToRoom;
		if (!this.subscribedToFishSold)
		{
			this.subscribedToFishSold = true;
			TournamentHelper.FishSold = (Action)Delegate.Combine(TournamentHelper.FishSold, new Action(this.FishSold));
		}
		PhotonConnectionFactory.Instance.EndTournament(true);
	}

	private void FishSold()
	{
		this.subscribedToFishSold = false;
		TournamentHelper.FishSold = (Action)Delegate.Remove(TournamentHelper.FishSold, new Action(this.FishSold));
		if (this.shouldGoToPondOnFishSold)
		{
			this.GotoFishingAction("top");
		}
	}

	private void OnMoved(TravelDestination destination)
	{
		if (PhotonConnectionFactory.Instance.CurrentTournamentId != null)
		{
			this.CalcTeleportation();
		}
		LogHelper.Log("OnMoved()");
		TransferToLocation.IsMoving = false;
		this._currentJoined = this._roomToJoin;
		PhotonConnectionFactory.Instance.GetChatPlayersCount();
		if (this._roomToJoin != null && !TransferToLocation.basicRoomNames.Contains(this._roomToJoin) && PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Room != null && PhotonConnectionFactory.Instance.Room.RoomId != null)
		{
			this._currentJoined = PhotonConnectionFactory.Instance.Room.RoomId;
		}
		if (GameFactory.Player == null)
		{
			return;
		}
		if (!StaticUserData.IS_TPM_VISIBLE || GameFactory.Player.IsTPMInitialized)
		{
			LogHelper.Log("FinalizeMove()");
			this.FinalizeMove();
		}
		else
		{
			LogHelper.Log("OnMoved with characters preloading");
			this.CharactersCreator.EAllCharactersLoaded += this.OnTPMLoaded;
			this.CharactersCreator.StartServerListening();
		}
	}

	private void OnTPMLoaded()
	{
		LogHelper.Log("OnTPMLoaded");
		this.CharactersCreator.EAllCharactersLoaded -= this.OnTPMLoaded;
		this.FinalizeMove();
	}

	private void FinalizeMove()
	{
		if (ShowLocationInfo.Instance.CurrentLocation != null)
		{
			this._lastPin = ShowLocationInfo.Instance.CurrentLocation;
			StaticUserData.CurrentLocation = this._lastPin;
			LogHelper.Log("FinishMoveToNewLocation");
			if (ShowHudElements.Instance != null)
			{
				ShowHudElements.Instance.HideChangeRoomPanel();
			}
			PhotonConnectionFactory.Instance.SetLastPin(this._lastPin.LocationId);
			PhotonConnectionFactory.Instance.Game.Resume(true);
			GameFactory.Player.FinishMoveToNewLocation(this._isTeleportation);
			TimeAndWeatherManager.ResetTimeAndWeather();
			if (string.IsNullOrEmpty(GameFactory.SkyControllerInstance.CurrentSky.AssetBundleName))
			{
				GameFactory.SkyControllerInstance.ChangedSky += this.TransferToLocation_ChangedSky;
				this._pondHelpers.PondControllerList.StartCoroutine(GameFactory.SkyControllerInstance.SetFirstSky());
			}
			else
			{
				this.TransferToLocation_ChangedSky(null, null);
			}
		}
	}

	private void OnMoveFailed(Failure failure)
	{
		Debug.Log("OnMoveFailed");
		TransferToLocation.IsMoving = false;
	}

	private void TransferToLocation_ChangedSky(object sender, EventArgs e)
	{
		GameFactory.SkyControllerInstance.ChangedSky -= this.TransferToLocation_ChangedSky;
		this.ActivateGameView();
	}

	public void ActivateGameView()
	{
		if (StaticUserData.CurrentLocation == null)
		{
			return;
		}
		CursorManager.ResetCursor();
		this._pondHelpers.PondControllerList.HideMenu(true, true, true);
		PhotonConnectionFactory.Instance.Game.Resume(true);
		this._pondHelpers.PondControllerList.ShowGame();
		TransferToLocation.IsMoving = false;
	}

	private CompetitionMapController _competitionController;

	private Location _lastPin;

	private PondHelpers _pondHelpers = new PondHelpers();

	private MessageBox _confirmMessageBox;

	private GameObject _messageFishSold;

	private GameObject _messageEndTournamentTime;

	private MessageBox messageBox;

	private string _currentJoined;

	private string _roomToJoin;

	public static bool IsMoving = false;

	private MenuHelpers helpers = new MenuHelpers();

	private static TransferToLocation _instance;

	private CharactersCreator _charactersCreator;

	private bool _isTeleportation;

	private int _lastPindId;

	private bool subscribedToFishSold;

	private bool shouldGoToPondOnFishSold = true;

	private static readonly string[] basicRoomNames = new string[] { "private", "friends" };
}
