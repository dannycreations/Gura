using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using DG.Tweening;
using I2.Loc;
using InControl;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionWidgetManager : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnHide = delegate
	{
	};

	public int MissionId { get; private set; }

	private void Start()
	{
		base.GetComponent<RectTransform>().anchoredPosition = this._position;
		this._taskNameText.gameObject.SetActive(false);
		this._taskPressBtnText.gameObject.SetActive(false);
		this._taskPressBtnControllerText.gameObject.SetActive(false);
		this._nameText.gameObject.SetActive(false);
		this._progressText.gameObject.SetActive(false);
		this._image.gameObject.SetActive(false);
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		InfoMessageController.Instance.OnActivate += this.Instance_OnActivate;
		PhotonConnectionFactory.Instance.MissionsListReceived += this.PhotonServer_OnMissionListReceived;
	}

	private void OnDestroy()
	{
		InfoMessageController.Instance.OnActivate -= this.Instance_OnActivate;
		PhotonConnectionFactory.Instance.MissionsListReceived -= this.PhotonServer_OnMissionListReceived;
		ShortcutExtensions.DOKill(this._contentRt, false);
		base.StopCoroutine(this.DelayShow());
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	private void Update()
	{
		if (this._tw == null)
		{
			this.CheckDeactivation(false);
		}
		if (this._state != MissionWidgetManager.WidgetStates.None && SettingsManager.InputType != InputModuleManager.InputType.Mouse && InputManager.ActiveDevice.GetControl(InputControlType.Action2).WasLongPressed)
		{
			this.OpenMissions();
		}
		this.CheckActivation();
		if (this._tw != null)
		{
			TimeSpan timeSpan = this._tw.Time - TimeHelper.UtcTime();
			string text = ((timeSpan.Minutes <= 0) ? timeSpan.GetFormatedSecondsOnly() : timeSpan.GetFormated(false, true));
			this._valueTimer.text = string.Format("{0} {1}{2}", ScriptLocalization.Get("UGC_Countdown"), "\ue70c", text);
			this._imageProgressTimer.fillAmount = (float)(timeSpan.TotalSeconds / this._tw.Duration.TotalSeconds);
			if (timeSpan.TotalSeconds < 0.0)
			{
				this.ClearTimer();
			}
		}
	}

	public void ClearTimer()
	{
		if (this._tw == null)
		{
			return;
		}
		this._tw = null;
		ShortcutExtensions.DOKill(this._contentTimerRt, false);
		TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOAnchorPos(this._contentTimerRt, new Vector2(this._contentTimerRt.rect.width, this._contentTimerRt.anchoredPosition.y), this._deactivationTime, false), delegate
		{
			this._state = MissionWidgetManager.WidgetStates.None;
			this._contentTimerRt.gameObject.SetActive(false);
		});
	}

	public void PushTimer(MissionWidgetManager.TimerWidget tw)
	{
		this._tw = tw;
		this._tw.Duration = this._tw.Time - TimeHelper.UtcTime();
		LogHelper.Log("___kocha PushTimer Duration:{0} Time:{1}", new object[]
		{
			this._tw.Duration,
			this._tw.Time
		});
		this._imageLdbl.Load(tw.Image, this._imageTimer, "Textures/Inventory/{0}");
		this._titleTimer.text = tw.Name;
		this._imageProgressTimer.fillAmount = 1f;
		this._state = MissionWidgetManager.WidgetStates.Showing;
		this._contentRt.gameObject.SetActive(false);
		this._contentTimerRt.gameObject.SetActive(true);
		ShortcutExtensions.DOKill(this._contentTimerRt, false);
		TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOAnchorPos(this._contentTimerRt, new Vector2(0f, this._contentTimerRt.anchoredPosition.y), this._activationTime, false), delegate
		{
			this._state = MissionWidgetManager.WidgetStates.Show;
		});
	}

	public void Push(MissionOnClient m, int? taskId, bool isForceShow = false)
	{
		if (this._state == MissionWidgetManager.WidgetStates.Show && m.MissionId == this.MissionId && this._screensSkipedTasks.Contains(this._currenGameScreen))
		{
			this.UpdateProgressText(m, m.Tasks.Count);
		}
		else if (this._widgetsPaused.Count > 0)
		{
			for (int i = 0; i < this._widgetsPaused.Count; i++)
			{
				if (this._widgetsPaused[i].Mission.MissionId == m.MissionId)
				{
					this._widgetsPaused[i].Mission.Tasks = m.Tasks;
				}
			}
		}
		else
		{
			this._widgets.Enqueue(new MissionWidgetManager.WidgetContent
			{
				Mission = m,
				TaskId = taskId,
				IsForceShow = isForceShow
			});
			this.Show(false);
		}
	}

	public void Clear(bool fastHide = false)
	{
		if (this._state == MissionWidgetManager.WidgetStates.Showing)
		{
			this._state = MissionWidgetManager.WidgetStates.Show;
		}
		if (this._state == MissionWidgetManager.WidgetStates.Show)
		{
			this._curTime = this._showTime;
			if (fastHide)
			{
				this.CheckDeactivation(true);
			}
		}
		else
		{
			this.OnHide();
		}
	}

	public void ClearAll(bool fastHide = false)
	{
		this._widgets.Clear();
		this.Clear(fastHide);
	}

	public void OpenMissions()
	{
		ScreenManager.OpenMissions();
	}

	public void Pause(bool flag)
	{
		if (flag)
		{
			if (this.MissionId != 0 && ClientMissionsManager.Instance.CurrentTrackedMission != null)
			{
				this._widgetsPaused = this._widgets.ToList<MissionWidgetManager.WidgetContent>();
				this._widgetsPaused.Insert(0, new MissionWidgetManager.WidgetContent
				{
					Mission = ClientMissionsManager.Instance.CurrentTrackedMission,
					TaskId = ClientMissionsManager.CurrentWidgetTaskId,
					IsForceShow = false
				});
			}
			this.ClearAll(true);
		}
		else
		{
			this._widgets = new Queue<MissionWidgetManager.WidgetContent>(this._widgetsPaused);
			this._widgetsPaused.Clear();
			this.Show(false);
		}
	}

	public void SetCurrentGameScreen(GameScreenType gs)
	{
		if (this._currenGameScreen != gs)
		{
			this._currenGameScreen = gs;
			if (this._widgetsPaused.Count > 0)
			{
				this.Pause(false);
			}
			else if ((this._state == MissionWidgetManager.WidgetStates.Show || this._state == MissionWidgetManager.WidgetStates.Showing) && ClientMissionsManager.CurrentWidgetTaskId != null && this._screensSkipedTasks.Contains(this._currenGameScreen))
			{
				this.Clear(this._currenGameScreen != GameScreenType.Fishkeeper);
			}
			if (ScreenManager.Game3DScreens.Contains(gs))
			{
				Vector2 position = this._position;
				position.x += 64f;
				base.GetComponent<RectTransform>().anchoredPosition = position;
			}
			else
			{
				base.GetComponent<RectTransform>().anchoredPosition = this._position;
			}
		}
	}

	public static void SetPlayerPrefs(int missionId)
	{
		PlayerPrefs.SetString(string.Format("MissionUntracked_{0}", missionId), missionId.ToString());
	}

	public static void ClearPlayerPrefs()
	{
		for (int i = 0; i < 300; i++)
		{
			PlayerPrefs.DeleteKey(string.Format("MissionUntracked_{0}", i));
		}
	}

	private void Show(bool skipInfoMsg = false)
	{
		if (this._state == MissionWidgetManager.WidgetStates.None && this._widgets.Count > 0 && (!ScreenManager.Instance.IsTransfer || this._widgets.ToList<MissionWidgetManager.WidgetContent>()[0].IsForceShow) && (skipInfoMsg || (!this.IsInfoMsgActive(InfoMessageTypes.MissionNew) && !this.IsInfoMsgActive(InfoMessageTypes.MissionAccomplished))))
		{
			MissionWidgetManager.WidgetContent mc = this._widgets.Dequeue();
			if (mc.TaskId != null && this._screensSkipedTasks.Contains(this._currenGameScreen))
			{
				this.Show(false);
				return;
			}
			this._state = MissionWidgetManager.WidgetStates.Showing;
			MissionOnClient mission = mc.Mission;
			this.MissionId = mission.MissionId;
			ClientMissionsManager.CurrentWidgetTaskId = mc.TaskId;
			int count = mission.Tasks.Count;
			this._taskPressBtnText.gameObject.SetActive(false);
			this._taskPressBtnControllerText.gameObject.SetActive(false);
			if (mc.TaskId != null)
			{
				MissionTaskOnClient missionTaskOnClient = mission.Tasks.FirstOrDefault((MissionTaskOnClient p) => p.TaskId == mc.TaskId.Value);
				if (missionTaskOnClient == null)
				{
					Debug.LogErrorFormat("MissionWidgetManager:Show - cant'found TaskId:{0} MissionId:{1}", new object[]
					{
						mc.TaskId.Value,
						mission.MissionId
					});
					this.Show(false);
					return;
				}
				this._taskNameText.text = missionTaskOnClient.Name;
				this.CheckTextLen(this._taskNameText);
				string text = HotkeyIcons.KeyMappings[InputControlType.Action2];
				string text2 = ScriptLocalization.Get("MissionShowDetailsCaptionController");
				this._taskPressBtnControllerText.text = string.Format(text2, string.Format("<color=#E1DD77FF>{0}</color>", text));
				this._taskPressBtnControllerText.gameObject.SetActive(true);
				if (SettingsManager.InputType == InputModuleManager.InputType.Mouse)
				{
					text = ScriptLocalization.Get("MouseLeftButton");
					text2 = ScriptLocalization.Get("MissionShowDetailsCaption");
					this._taskPressBtnText.text = string.Format(text2, string.Format("<color=#E1DD77FF>{0}</color>", text));
					this._taskPressBtnText.gameObject.SetActive(true);
					this._taskPressBtnControllerText.gameObject.SetActive(false);
				}
			}
			else
			{
				this.UpdateProgressText(mission, count);
				this._nameText.text = mission.Name;
				this.CheckTextLen(this._nameText);
			}
			this._taskNameText.gameObject.SetActive(mc.TaskId != null);
			this._nameText.gameObject.SetActive(mc.TaskId == null);
			this._progressText.gameObject.SetActive(mc.TaskId == null);
			if (mission.ThumbnailBID != null)
			{
				this._imageLdbl.Image = this._image;
				this._imageLdbl.Load(string.Format("Textures/Inventory/{0}", mission.ThumbnailBID.Value));
				this._image.gameObject.SetActive(true);
			}
			else
			{
				this._image.gameObject.SetActive(false);
			}
			if (!this._contentRt.gameObject.activeSelf)
			{
				this._contentRt.gameObject.SetActive(true);
			}
			ShortcutExtensions.DOKill(this._contentRt, false);
			TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOAnchorPos(this._contentRt, new Vector2(0f, this._contentRt.anchoredPosition.y), this._activationTime, false), delegate
			{
				this._state = MissionWidgetManager.WidgetStates.Show;
			});
		}
	}

	private void CheckActivation()
	{
		if (this._state == MissionWidgetManager.WidgetStates.None && this._widgets.Count > 0)
		{
			List<MissionWidgetManager.WidgetContent> list = this._widgets.ToList<MissionWidgetManager.WidgetContent>();
			if (list[0].IsForceShow)
			{
				this.Show(false);
			}
		}
	}

	private void CheckDeactivation(bool fastHide)
	{
		if (this._state == MissionWidgetManager.WidgetStates.Show)
		{
			this._contentRt.anchoredPosition = new Vector2(0f, this._contentRt.anchoredPosition.y);
			this._curTime += Time.deltaTime;
			if (this._curTime >= this._showTime)
			{
				this._state = MissionWidgetManager.WidgetStates.Hide;
				this._curTime = 0f;
				ShortcutExtensions.DOKill(this._contentRt, false);
				TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOAnchorPos(this._contentRt, new Vector2(this._contentRt.rect.width, this._contentRt.anchoredPosition.y), (!fastHide) ? this._deactivationTime : 0f, false), delegate
				{
					this._state = MissionWidgetManager.WidgetStates.None;
					this.MissionId = 0;
					ClientMissionsManager.CurrentWidgetTaskId = null;
					this.OnHide();
					base.StartCoroutine(this.DelayShow());
				});
			}
		}
		else if (this._state == MissionWidgetManager.WidgetStates.None)
		{
			this._contentRt.anchoredPosition = new Vector2(this._contentRt.rect.width, this._contentRt.anchoredPosition.y);
		}
	}

	private IEnumerator DelayShow()
	{
		yield return new WaitForSeconds(this._delayTime);
		this.Show(false);
		yield break;
	}

	private void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		if (this._taskPressBtnText.gameObject != null && this._taskPressBtnText.gameObject.activeInHierarchy)
		{
			this._taskPressBtnText.gameObject.SetActive(type == InputModuleManager.InputType.Mouse);
			this._taskPressBtnControllerText.gameObject.SetActive(!this._taskPressBtnText.gameObject.activeSelf);
		}
	}

	private void UpdateProgressText(MissionOnClient m, int count)
	{
		ushort num = 0;
		for (int i = 0; i < count; i++)
		{
			if (m.Tasks[i].IsCompleted)
			{
				num += 1;
			}
		}
		this._progressText.text = string.Format("{0} {1}/{2}", ScriptLocalization.Get("CompletedQuestsLabel"), num, count);
	}

	private void Instance_OnActivate(InfoMessageTypes messageType, bool isActive)
	{
		if (messageType == InfoMessageTypes.MissionNew && !isActive)
		{
			this.Show(true);
		}
		else if (isActive)
		{
			this.Clear(true);
		}
		if (!isActive && (messageType == InfoMessageTypes.MissionNew || messageType == InfoMessageTypes.LevelUp))
		{
			PhotonConnectionFactory.Instance.GetMissionsStarted();
		}
	}

	private bool IsInfoMsgActive(InfoMessageTypes messageType = InfoMessageTypes.MissionNew)
	{
		return MessageFactory.InfoMessagesQueue.Any((InfoMessage p) => p.MessageType == messageType) || (InfoMessageController.Instance.currentMessage != null && InfoMessageController.Instance.currentMessage.MessageType == messageType);
	}

	private void CheckTextLen(Text t)
	{
		RectTransform component = t.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(component.rect.width, (t.text.Length <= 24) ? 30f : 60f);
	}

	private void PhotonServer_OnMissionListReceived(byte operationcode, List<MissionOnClient> missions)
	{
		if (operationcode == 1)
		{
			for (int i = 0; i < missions.Count; i++)
			{
				MissionOnClient missionOnClient = missions[i];
				if (ClientMissionsManager.IsUnreadQuest(missionOnClient.MissionId) && !missionOnClient.IsActiveMission && !PlayerPrefs.HasKey(string.Format("MissionUntracked_{0}", missionOnClient.MissionId)))
				{
					LogHelper.Log("___kocha added missionId:{0}", new object[] { missionOnClient.MissionId });
					MissionWidgetManager.SetPlayerPrefs(missionOnClient.MissionId);
					List<MissionWidgetManager.WidgetContent> list = this._widgets.ToList<MissionWidgetManager.WidgetContent>();
					list.Insert(0, new MissionWidgetManager.WidgetContent
					{
						Mission = missionOnClient,
						IsForceShow = true
					});
					this._widgets = new Queue<MissionWidgetManager.WidgetContent>(list);
					this.Show(false);
				}
			}
		}
	}

	[SerializeField]
	private RectTransform _contentTimerRt;

	[SerializeField]
	private Image _imageTimer;

	[SerializeField]
	private TextMeshProUGUI _titleTimer;

	[SerializeField]
	private TextMeshProUGUI _valueTimer;

	[SerializeField]
	private Image _imageProgressTimer;

	[SerializeField]
	private RectTransform _contentRt;

	[SerializeField]
	private float _showTime = 6f;

	[SerializeField]
	private float _delayTime = 1f;

	[SerializeField]
	private float _activationTime = 1.5f;

	[SerializeField]
	private float _deactivationTime = 1f;

	[SerializeField]
	private Text _taskNameText;

	[SerializeField]
	private Text _taskPressBtnText;

	[SerializeField]
	private Text _taskPressBtnControllerText;

	[SerializeField]
	private Text _nameText;

	[SerializeField]
	private Text _progressText;

	[SerializeField]
	private Image _image;

	private ResourcesHelpers.AsyncLoadableImage _imageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	private const float PosOffsetGame3D = 64f;

	private readonly Vector2 _position = new Vector2(1118f, 302f);

	private const float TextHeightOffset = 30f;

	private const float TextHeight = 30f;

	private const int TextLenForResize = 24;

	private const string PlayerPrefsPrefix = "MissionUntracked_{0}";

	private float _curTime;

	private Queue<MissionWidgetManager.WidgetContent> _widgets = new Queue<MissionWidgetManager.WidgetContent>();

	private List<MissionWidgetManager.WidgetContent> _widgetsPaused = new List<MissionWidgetManager.WidgetContent>();

	private MissionWidgetManager.WidgetStates _state;

	private GameScreenType _currenGameScreen;

	private readonly List<GameScreenType> _screensSkipedTasks = new List<GameScreenType>(ScreenManager.Game3DScreens) { GameScreenType.Fishkeeper };

	private MissionWidgetManager.TimerWidget _tw;

	private enum WidgetStates : byte
	{
		None,
		Show,
		Showing,
		Hide
	}

	private class WidgetContent
	{
		public bool IsForceShow { get; set; }

		public MissionOnClient Mission { get; set; }

		public int? TaskId { get; set; }
	}

	public class TimerWidget
	{
		public int? Image { get; set; }

		public string Name { get; set; }

		public DateTime Time { get; set; }

		public TimeSpan Duration { get; set; }
	}
}
