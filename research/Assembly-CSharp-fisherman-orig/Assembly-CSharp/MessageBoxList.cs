using System;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class MessageBoxList : MonoBehaviour
{
	public bool IsActive
	{
		get
		{
			return this.currentMessage != null;
		}
	}

	public InfoServerMessagesHandler ServerMessagesHandler
	{
		get
		{
			if (this._serverMessagesHandler == null)
			{
				this._serverMessagesHandler = base.GetComponent<InfoServerMessagesHandler>();
			}
			return this._serverMessagesHandler;
		}
	}

	internal void Awake()
	{
		MessageBoxList.Instance = this;
		PhotonConnectionFactory.Instance.OnDisconnect += this.OnDisconnect;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnDisconnect -= this.OnDisconnect;
	}

	internal void Update()
	{
		if (!StaticUserData.IS_IN_TUTORIAL && (ScreenManager.Instance == null || ScreenManager.Instance.GameScreen == GameScreenType.Undefined))
		{
			return;
		}
		if (MessageFactory.MessageBoxQueue.Count > 0)
		{
			MessageBoxBase messageBoxBase = MessageFactory.MessageBoxQueue.FirstOrDefault((MessageBoxBase x) => x.OnPriority);
			if (messageBoxBase != null)
			{
				this.ReadyForMessage(messageBoxBase);
			}
			else if (GameFactory.Player == null || GameFactory.Player.State == null || GameFactory.Player.State == typeof(PlayerIdle) || GameFactory.Player.State == typeof(PlayerIdlePitch) || GameFactory.Player.State == typeof(PlayerDrawIn) || GameFactory.Player.State == typeof(PlayerEmpty) || GameFactory.Player.State == typeof(ToolIdle) || GameFactory.Player.State == typeof(RodPodIdle) || GameFactory.Player.State == typeof(HandIdle) || GameFactory.Player.State == typeof(PlayerOnBoat))
			{
				this.ReadyForMessage(null);
			}
		}
	}

	public void Show()
	{
		this.Update();
	}

	public void VerifyMessageSetToCurrent(MessageBoxBase msg)
	{
		if (this.currentMessage == null)
		{
			this.currentMessage = msg;
		}
	}

	private void ReadyForMessage(MessageBoxBase result)
	{
		if (result != null)
		{
			this.ShowMessage(result, true);
			MessageFactory.MessageBoxQueue.Remove(result);
		}
		if (!InfoMessageController.Instance.IsActive && this.currentMessage == null && MessageFactory.MessageBoxQueue.Count > 0 && (!(TutorialSlidesController.Instance != null) || !TutorialSlidesController.Instance.IsShowing))
		{
			this.ShowMessage(MessageFactory.MessageBoxQueue.Dequeue(), false);
		}
	}

	private void ShowMessage(MessageBoxBase message, bool priority = false)
	{
		message.gameObject.SetActive(true);
		AlphaFade alphaFade = message.AlphaFade;
		if (alphaFade != null)
		{
			alphaFade.FastHidePanel();
			alphaFade.ShowPanel();
		}
		if (!priority || this.currentMessage == null)
		{
			this.currentMessage = message;
		}
	}

	private void OnDisconnect()
	{
		MessageFactory.MessageBoxQueue = new ListQueue<MessageBoxBase>();
		if (this.currentMessage != null)
		{
			this.currentMessage.GetComponent<AlphaFade>().FastHidePanel();
			Object.Destroy(this.currentMessage);
		}
	}

	public GameObject blackScreen;

	public GameObject messageBoxPrefab;

	public GameObject messageBoxSelectablePrefab;

	public GameObject messageBoxSelectableWithoutButtonsPrefab;

	public GameObject messageBoxThreeSelectablePrefab;

	public GameObject messageBoxBuyMoneyPrefab;

	public GameObject messageBoxLicenseMissingPrefab;

	public GameObject messageBoxPlayerProfilePrefab;

	public GameObject messageReleaseNotesPrefab;

	public GameObject MessageBoxWithCurrencyPrefab;

	public GameObject tournamentDetailMessagePrefab;

	public GameObject tournamentDetailMessageNewPrefab;

	public GameObject tournamentSerieDetailMessagePrefab;

	public GameObject eulaMessagePrefab;

	public GameObject wearTacklePrefab;

	public GameObject tournamentTimeEndedPrefab;

	public GameObject tournamentTimeEndedTeamPrefab;

	public GameObject fishSoldPrefab;

	public GameObject cutLinesPrefab;

	public GameObject exitMessage;

	public GameObject lineLeashLengthController;

	public GameObject cutLeaderLengthController;

	public GameObject converterPrefab;

	public GameObject buyPassSelectionPrefab;

	public GameObject CheckPenaltyPanel;

	public GameObject PenaltyPanel;

	public GameObject NonPenaltyPanel;

	public GameObject WaitingPanel;

	public GameObject ShowFullDescriptionPanel;

	public GameObject RentPopup;

	public GameObject ExpandStorageWindow;

	public GameObject SelectFriendsWindow;

	public GameObject SelectFriendsBuoy;

	public GameObject BuoyDelivered;

	public MessageBoxBase currentMessage;

	public GameObject Waiting;

	public GameObject HelpCanvasPrefab;

	public GameObject HelpConsoleCanvasPrefab;

	public GameObject ProgressMessage;

	public GameObject ScrollSearchSelector;

	public InfoMessage ChumMixPrefab;

	public GameObject CutChumComponentPrefab;

	public GameObject RenameChumPrefab;

	public GameObject ChumMixingProgressPrefab;

	[Space(5f)]
	public GameObject WindowListPrefab;

	public GameObject WindowListCompetitionsPrefab;

	public GameObject WindowLevelMinMaxPrefab;

	public GameObject WindowEntryFeePrefab;

	public GameObject WindowInitialPrizePoolPrefab;

	public GameObject WindowScheduleAndDurationPrefab;

	public GameObject WindowTimeAndWeatherPrefab;

	public GameObject WindowSearchPrefab;

	public GameObject InputAreaWndPrefab;

	public GameObject SoldFishTimeIsUpMessagePrefab;

	public GameObject TournamentResultWindowPrefab;

	public GameObject HelpF1MousePrefab;

	public GameObject PremShopItemInfoWindowPrefab;

	public GameObject converterMoneyPrefab;

	public Sprite LogoRetail;

	public static MessageBoxList Instance;

	private InfoServerMessagesHandler _serverMessagesHandler;
}
