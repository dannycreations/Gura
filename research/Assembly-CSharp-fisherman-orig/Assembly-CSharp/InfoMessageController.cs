using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.Scripts.Common.Managers.Helpers;
using InControl;
using ObjectModel;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InfoMessageController : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<InfoMessageTypes, bool> OnActivate = delegate
	{
	};

	public bool IsActive
	{
		get
		{
			return this.currentMessage != null || this._currentMessageBox != null;
		}
	}

	internal void Start()
	{
		if (InfoMessageController.Instance != null)
		{
			Object.Destroy(InfoMessageController.Instance.gameObject);
		}
		InfoMessageController.Instance = this;
		Object.DontDestroyOnLoad(this);
		this._messageBoxList = base.gameObject.GetComponent<MessageBoxList>();
		PhotonConnectionFactory.Instance.OnDisconnect += this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnConnectionFailed += this.OnConnectionFailed;
	}

	internal void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnDisconnect -= this.OnDisconnect;
		PhotonConnectionFactory.Instance.OnConnectionFailed -= this.OnConnectionFailed;
	}

	internal void Update()
	{
		if (CacheLibrary.MapCache == null || CacheLibrary.MapCache.CachedPonds == null || this._helpers.MenuPrefabsList == null)
		{
			return;
		}
		if (this.CanShow && !this._helpers.MenuPrefabsList.IsLoadingOrTransfer() && ((GameFactory.Player != null && !GameFactory.Player.IsTackleThrown && !GameFactory.Player.CantOpenInventory && !GameFactory.Player.IsInteractionWithRodStand && (ShowHudElements.Instance == null || ShowHudElements.Instance.AdditionalInfo == null || !ShowHudElements.Instance.AdditionalInfo.IsMissionFinishing) && !this._helpers.MenuPrefabsList.loadingFormAS.isActive) || ((StaticUserData.CurrentPond == null && this._helpers.MenuPrefabsList.globalMapForm != null && !this._helpers.MenuPrefabsList.loadingFormAS.isActive && (this._helpers.MenuPrefabsList.travelingForm == null || !this._helpers.MenuPrefabsList.travelingFormAS.isActive)) || (GameFactory.Player == null && StaticUserData.CurrentPond != null && !this._helpers.MenuPrefabsList.loadingFormAS.isActive && (this._helpers.MenuPrefabsList.travelingForm == null || !this._helpers.MenuPrefabsList.travelingFormAS.isActive) && (this._helpers.MenuPrefabsList.startFormAS == null || !this._helpers.MenuPrefabsList.startFormAS.isActive))) || this.ShowMandatory) && (!(TutorialSlidesController.Instance != null) || !TutorialSlidesController.Instance.IsShowing) && (ScreenManager.Instance == null || !this.IsGame3DWaitingWindow()) && MessageFactory.InfoMessagesQueue.Count > 0)
		{
			InfoMessage infoMessage = MessageFactory.InfoMessagesQueue.Peek();
			if (infoMessage != null && (infoMessage.MessageType != InfoMessageTypes.SoldFishTimeIsUp || GameFactory.Player == null || GameFactory.Player.CanBreakStateMachine))
			{
				this.ReadyForMessage();
			}
		}
		if (this.currentMessage != null && (ControlsController.ControlsActions.Space.WasPressedMandatory || ControlsController.ControlsActions.GetMouseButtonDownMandatory(0)) && (TutorialSlidesController.Instance == null || !TutorialSlidesController.Instance.IsShowing) && this.currentMessage.CloseByAnyClick)
		{
			this.CloseMessage();
		}
		if (this.currentMessage != null && (ControlsController.ControlsActions.Space.WasPressedMandatory || InputManager.ActiveDevice.Action2.WasPressed) && (TutorialSlidesController.Instance == null || !TutorialSlidesController.Instance.IsShowing) && this.currentMessage.CloseBySpace)
		{
			this.CloseMessage();
		}
		this._showTime += Time.deltaTime;
	}

	public void ResetShowTime()
	{
		this._showTime = 0f;
	}

	public T GetClassManager<T>() where T : WindowBase
	{
		return base.GetComponentInChildren<T>();
	}

	public void ForceShow(InfoMessage message)
	{
		this.ShowMessage(message);
	}

	private void CloseMessage()
	{
		AlphaFade component = this.currentMessage.GetComponent<AlphaFade>();
		if (!component.IsHiding)
		{
			component.HidePanel();
		}
	}

	private void ReadyForMessage()
	{
		this._currentMessageBox = this._messageBoxList.currentMessage;
		if (this._currentMessageBox == null && this.currentMessage == null && MessageFactory.InfoMessagesQueue.Count > 0 && MessageFactory.MessageBoxQueue.Count == 0)
		{
			InfoMessage infoMessage = null;
			if (StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId == 2)
			{
				Queue<InfoMessage> queue = new Queue<InfoMessage>();
				while (MessageFactory.InfoMessagesQueue.Count != 0)
				{
					InfoMessage infoMessage2 = MessageFactory.InfoMessagesQueue.Dequeue();
					if (infoMessage != null || !infoMessage2.ShowInTutorial || (infoMessage2.ShowOnGlobalMap && SceneManager.GetActiveScene().name != "GlobalMapScene"))
					{
						queue.Enqueue(infoMessage2);
					}
					else
					{
						infoMessage = infoMessage2;
					}
				}
				MessageFactory.InfoMessagesQueue = queue;
			}
			else
			{
				infoMessage = MessageFactory.InfoMessagesQueue.Dequeue();
				if ((infoMessage.ShowOnGlobalMap && SceneManager.GetActiveScene().name != "GlobalMapScene") || this._showTime < infoMessage.DelayTime)
				{
					MessageFactory.InfoMessagesQueue.Enqueue(infoMessage);
					infoMessage = null;
				}
			}
			if (infoMessage != null)
			{
				this.ShowMessage(infoMessage);
			}
		}
	}

	private void ShowMessage(InfoMessage message)
	{
		this.OnActivate(message.MessageType, true);
		this.ResetShowTime();
		CursorManager.ShowCursor();
		ControlsController.ControlsActions.BlockInput(null);
		this.currentMessage = message;
		message.transform.localScale = new Vector3(1f, 1f, 1f);
		message.gameObject.SetActive(true);
		RectTransform component = message.GetComponent<RectTransform>();
		component.anchoredPosition = new Vector3(0f, 0f, 0f);
		component.sizeDelta = new Vector2(0f, 0f);
		if (message.ShowDelayTime > 0f)
		{
			base.StartCoroutine(this.Show(message.ShowDelayTime));
		}
		else
		{
			this.Show();
		}
	}

	private IEnumerator Show(float t)
	{
		yield return new WaitForSeconds(t);
		this.Show();
		yield break;
	}

	private void Show()
	{
		AlphaFade component = this.currentMessage.GetComponent<AlphaFade>();
		component.ShowPanel();
		component.HideFinished += new EventHandler<EventArgsAlphaFade>(this.InfoMessageController_HideFinished);
		if (this.currentMessage.ExecuteAfterShow != null)
		{
			this.currentMessage.ExecuteAfterShow();
		}
	}

	private void InfoMessageController_HideFinished(object sender, EventArgs e)
	{
		if (ScreenManager.Instance != null && ScreenManager.Instance.GameScreen == GameScreenType.Game && !CursorManager.IsModalWindow())
		{
			CursorManager.ResetCursor();
		}
		ControlsController.ControlsActions.UnBlockInput();
		CursorManager.HideCursor();
		GameObject gameObject = (GameObject)sender;
		this.OnActivate(gameObject.GetComponent<InfoMessage>().MessageType, false);
		Object.Destroy(gameObject);
	}

	private void OnDisconnect()
	{
		MessageFactory.InfoMessagesQueue = new Queue<InfoMessage>();
		if (this.currentMessage != null)
		{
			this.currentMessage.GetComponent<AlphaFade>().FastHidePanel();
		}
	}

	private void OnConnectionFailed()
	{
	}

	private bool IsGame3DWaitingWindow()
	{
		return ScreenManager.Game3DScreens.Contains(ScreenManager.Instance.GameScreen) && this.IsCatchedFish;
	}

	private bool IsCatchedFish
	{
		get
		{
			return GameFactory.Player != null && GameFactory.Player.IsCatchedSomething;
		}
	}

	public InfoMessage currentMessage;

	public static InfoMessageController Instance;

	private MenuHelpers _helpers = new MenuHelpers();

	private MessageBoxList _messageBoxList;

	private MessageBoxBase _currentMessageBox;

	internal bool CanShow;

	internal bool ShowMandatory;

	private float _showTime;
}
