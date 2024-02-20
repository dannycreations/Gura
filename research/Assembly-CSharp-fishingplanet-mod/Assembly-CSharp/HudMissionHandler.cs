using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Quests;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudMissionHandler : MonoBehaviour
{
	private void Awake()
	{
		this._state = HudMissionHandler.States.None;
		ColorUtility.TryParseHtmlString("#F7F7F7FF", ref this._normalColor);
		ColorUtility.TryParseHtmlString("#7ED321FF", ref this._doneColor);
		ScreenManager.Instance.OnScreenChanged += this.Instance_OnScreenChanged;
	}

	private void OnEnable()
	{
		if (StaticUserData.DISABLE_MISSIONS)
		{
			base.gameObject.SetActive(false);
			return;
		}
		ScreenManager.Instance.OnTransfer += this.Instance_OnTransfer;
		ClientMissionsManager.Instance.TrackedMissionUpdated += this.Populate;
		InfoMessageController.Instance.OnActivate += this.Instance_OnActivate;
		MissionOnClient currentTrackedMission = ClientMissionsManager.Instance.CurrentTrackedMission;
		if (currentTrackedMission != null && currentTrackedMission.MissionId == this._missionId)
		{
			this._completedCount = 0;
			this.Populate(currentTrackedMission);
		}
	}

	private void OnDisable()
	{
		ScreenManager.Instance.OnTransfer -= this.Instance_OnTransfer;
		InfoMessageController.Instance.OnActivate -= this.Instance_OnActivate;
		ClientMissionsManager.Instance.TrackedMissionUpdated -= this.Populate;
	}

	private void OnDestroy()
	{
		this._state = HudMissionHandler.States.Destroyed;
		ClientMissionsManager.Instance.TrackedMissionUpdated -= this.Populate;
		ScreenManager.Instance.OnScreenChanged -= this.Instance_OnScreenChanged;
		base.StopAllCoroutines();
		this._current.FastHidePanel();
		this._done.FastHidePanel();
		this.Clear();
	}

	private void Update()
	{
		if (this._timePulsator != null)
		{
			this._timePulsator.Update();
		}
	}

	public bool IsFinishing
	{
		get
		{
			return this._state == HudMissionHandler.States.Finishing || (ScreenManager.Game3DScreens.Contains(ScreenManager.Instance.GameScreen) && this._tasksForMoveLeft.Count > 0);
		}
	}

	public void Populate(MissionOnClient mission)
	{
		if (base.gameObject == null || this._current == null || this._state == HudMissionHandler.States.Destroyed)
		{
			return;
		}
		if (!ScreenManager.Game3DScreens.Contains(ScreenManager.Instance.GameScreen))
		{
			return;
		}
		if (mission == null)
		{
			if (this._missionId != 0 && !this.IsInfoMsgActive(InfoMessageTypes.MissionAccomplished))
			{
				this.Clear();
				base.gameObject.SetActive(false);
			}
			return;
		}
		string text = "MissionHUD:Populate _missionId:{0} MissionId:{1} tasksForMoveLeft:{2} IsStarted:{3} IsCompleted:{4} TasksCompleted:{5}";
		object[] array = new object[6];
		array[0] = this._missionId;
		array[1] = mission.MissionId;
		array[2] = this._tasksForMoveLeft.Count;
		array[3] = mission.IsStarted;
		array[4] = mission.IsCompleted;
		array[5] = mission.Tasks.Count((MissionTaskOnClient p) => p.IsCompleted);
		LogHelper.Log(string.Format(text, array));
		bool flag = this._missionId != mission.MissionId;
		List<MissionTaskOnClient> list = mission.Tasks.OrderBy((MissionTaskOnClient p) => p.OrderId).ToList<MissionTaskOnClient>();
		if (flag)
		{
			if (this._tasksForMoveLeft.Count > 0 || this._state == HudMissionHandler.States.Finishing || this.IsInfoMsgActive(InfoMessageTypes.MissionAccomplished))
			{
				this._nextMission = mission;
				return;
			}
			this.Clear();
			if (mission.TimeToComplete != null)
			{
				this._timePulsator = new QuestTimePulsator(mission, this._missionTimer);
			}
			this._missionId = mission.MissionId;
			TMP_Text missionName = this._missionName;
			string name = mission.Name;
			this._missionNameDone.text = name;
			missionName.text = name;
		}
		else
		{
			list = list.OrderBy((MissionTaskOnClient p) => p.IsCompleted).ToList<MissionTaskOnClient>();
		}
		bool flag2 = false;
		int num = 0;
		int i = 0;
		while (i < list.Count)
		{
			MissionTaskOnClient t = list[i];
			if (t.IsCompleted)
			{
				num++;
			}
			string text2;
			string text3;
			ClientMissionsManager.ParseTaskProgress(t.Progress, out text2, out text3);
			HudMissionTask hudMissionTask = null;
			if (flag)
			{
				goto IL_31B;
			}
			hudMissionTask = this._tasks.FirstOrDefault((HudMissionTask p) => p.TaskId == t.TaskId);
			if (!(hudMissionTask != null))
			{
				goto IL_31B;
			}
			if (t.IsHidden)
			{
				this.RemoveHidden(hudMissionTask);
			}
			else
			{
				hudMissionTask.UpdateProgress(text2, text3);
				if (t.IsCompleted && this._tasksForMoveLeft.FindIndex((HudMissionTask p) => p.TaskId == t.TaskId) == -1)
				{
					if (this._taskDone != null)
					{
						UIAudioSourceListener.Instance.Audio.PlayOneShot(this._taskDone, SettingsManager.InterfaceVolume);
					}
					this._tasksForMoveLeft.Add(hudMissionTask);
					hudMissionTask.Done();
					goto IL_31B;
				}
				goto IL_31B;
			}
			IL_386:
			i++;
			continue;
			IL_31B:
			if (!t.IsCompleted && !t.IsHidden && (flag || hudMissionTask == null))
			{
				if (!flag)
				{
					flag2 = this.IsNeedReOrdering(t.OrderId);
				}
				this.AddTask(this._tasks.Count, t, text3, text2);
				goto IL_386;
			}
			goto IL_386;
		}
		if (flag2 && this._tasksForMoveLeft.Count == 0)
		{
			this.Clear();
			this.Populate(mission);
			return;
		}
		this.UpdateTextProgress(this._missionTasksProgressDone, this._missioTasksCountDone, list.Count, num);
		this.UpdateTextProgress(this._missionTasksProgress, this._missioTasksCount, list.Count, num);
		if (this._state == HudMissionHandler.States.Finished)
		{
			this.MissionFinish();
		}
		else
		{
			if (!this._progress.gameObject.activeSelf)
			{
				this._progress.gameObject.SetActive(true);
			}
			if (this._progress.gameObject.activeInHierarchy && this._completedCount == num && num == 0)
			{
				this._progress.InitProgressDisplay(0f, 0f);
			}
			if (this._completedCount != num)
			{
				if (this._progress.gameObject.activeInHierarchy && num > 0)
				{
					if (!this._isProgressActive)
					{
						this._isProgressActive = true;
						this._progress.OnFinish += this._progress_OnFinish;
					}
					this._progress.InitProgressDisplay((float)this._completedCount / (float)list.Count, (float)num / (float)list.Count);
				}
				this._completedCount = num;
				if (num == list.Count)
				{
					this._state = HudMissionHandler.States.Finishing;
				}
			}
			else if (this._tasksForMoveLeft.Count > 0)
			{
				this._progress_OnFinish();
			}
		}
	}

	private void _progress_OnFinish()
	{
		this._isProgressActive = false;
		if (this._state == HudMissionHandler.States.Destroyed)
		{
			return;
		}
		if (this._nextMission != null && this._state != HudMissionHandler.States.Finishing)
		{
			this._tasksForMoveLeft.Clear();
			this.Populate(this._nextMission);
			return;
		}
		if (this._state == HudMissionHandler.States.Finishing)
		{
			this._current.HideFinished += this._current_HideFinished;
			this._current.HidePanel();
		}
		this._progress.OnFinish -= this._progress_OnFinish;
		for (int i = 0; i < this._tasksForMoveLeft.Count; i++)
		{
			this._tasksForMoveLeft[i].OnHide += this.TaskView_OnHide;
			this._tasksForMoveLeft[i].MoveLeft();
		}
	}

	private void AddTask(int index, MissionTaskOnClient t, string count, string progress)
	{
		HudMissionTask hudMissionTask = Object.Instantiate<HudMissionTask>(this.taskPrefab, this.tasksParent);
		float num = this.FindPrevY(this._tasks, index);
		float num2 = this.FindPrevH(this._tasks, index);
		hudMissionTask.Init(t.TaskId, t.OrderId, t.Name, count, progress, num, num2, index);
		this._tasks.Add(hudMissionTask);
	}

	private void TaskView_OnHide(int taskId)
	{
		if (this._state == HudMissionHandler.States.Destroyed)
		{
			return;
		}
		int num = this._tasks.FindIndex((HudMissionTask p) => p.TaskId == taskId);
		if (num == -1)
		{
			return;
		}
		int num2 = this._tasksForMoveLeft.FindIndex((HudMissionTask p) => p.TaskId == taskId);
		if (num2 != -1)
		{
			this._tasksForMoveLeft.RemoveAt(num2);
		}
		HudMissionTask hudMissionTask = this._tasks[num];
		hudMissionTask.OnHide -= this.TaskView_OnHide;
		if (this._tasksForMoveLeft.Count > 0)
		{
			return;
		}
		Dictionary<HudMissionTask, float> dictionary = this.CalcAllPosY(0);
		List<HudMissionTask> list = this._tasks.FindAll((HudMissionTask p) => p.IsDone);
		list.ForEach(delegate(HudMissionTask p)
		{
			Object.Destroy(p.gameObject);
		});
		this._tasks.RemoveAll((HudMissionTask p) => p.IsDone);
		float num3 = 0f;
		foreach (KeyValuePair<HudMissionTask, float> keyValuePair in dictionary)
		{
			base.StartCoroutine(this.MoveUp(num3, keyValuePair.Key, keyValuePair.Value));
			num3 += 0.2f;
		}
	}

	private IEnumerator MoveUp(float time, HudMissionTask t, float posY)
	{
		yield return new WaitForSeconds(time);
		t.MoveUp(posY);
		yield break;
	}

	private Dictionary<HudMissionTask, float> CalcAllPosY(int startIndex = 0)
	{
		float num = 0f;
		List<HudMissionTask> list = new List<HudMissionTask>();
		Dictionary<HudMissionTask, float> dictionary = new Dictionary<HudMissionTask, float>();
		for (int i = startIndex; i < this._tasks.Count; i++)
		{
			HudMissionTask hudMissionTask = this._tasks[i];
			if (hudMissionTask.gameObject.activeSelf && !hudMissionTask.IsDone)
			{
				int count = list.Count;
				float num2 = this.FindPrevH(list, count);
				num = HudMissionTask.GetY(num, num2, count);
				dictionary.Add(hudMissionTask, num);
				list.Add(hudMissionTask);
			}
		}
		return dictionary;
	}

	private void _current_HideFinished(object sender, EventArgsAlphaFade e)
	{
		if (this._state == HudMissionHandler.States.Destroyed)
		{
			return;
		}
		this._current.HideFinished -= this._current_HideFinished;
		this._done.ShowFinished += this._done_ShowFinished;
		this._done.ShowPanel();
	}

	private void _done_ShowFinished(object sender, EventArgs e)
	{
		if (this._state == HudMissionHandler.States.Destroyed)
		{
			return;
		}
		this._done.ShowFinished -= this._done_ShowFinished;
		if (this._missionDone != null)
		{
			UIAudioSourceListener.Instance.Audio.PlayOneShot(this._missionDone, SettingsManager.InterfaceVolume);
		}
		this.MissionFinish();
	}

	private void MissionFinish()
	{
		this._missionNameDone.color = this._doneColor;
		AlphaFade alphaFade = this._imageDone.GetComponent<AlphaFade>();
		alphaFade.ShowFinished += this.Af_ShowFinished;
		alphaFade.ShowPanel();
		alphaFade = this._imageDoneProgress.GetComponent<AlphaFade>();
		alphaFade.FastShowPanel();
		this._missioTasksCountDone.gameObject.SetActive(false);
		this._missionTasksProgressDone.gameObject.SetActive(false);
	}

	private void Af_ShowFinished(object sender, EventArgs e)
	{
		this._imageDoneProgress.GetComponent<AlphaFade>().ShowFinished -= this.Af_ShowFinished;
		this._state = HudMissionHandler.States.Finished;
	}

	private void UpdateTextProgress(TextMeshProUGUI pText, TextMeshProUGUI countText, int count, int completedCount)
	{
		if (!this._isProgressTextsActive)
		{
			countText.gameObject.SetActive(false);
			pText.gameObject.SetActive(false);
			return;
		}
		countText.text = string.Format("/ {0}", count);
		pText.text = completedCount.ToString();
	}

	private void Clear()
	{
		this._current.FastShowPanel();
		this._done.FastHidePanel();
		this._isProgressActive = false;
		this._nextMission = null;
		this._completedCount = 0;
		this._state = HudMissionHandler.States.None;
		if (this._timePulsator != null)
		{
			this._timePulsator.Dispose();
			this._timePulsator = null;
		}
		this._missionId = 0;
		this._imageDone.GetComponent<AlphaFade>().FastHidePanel();
		this._imageDoneProgress.GetComponent<AlphaFade>().FastHidePanel();
		this._missioTasksCountDone.gameObject.SetActive(true);
		this._missionTasksProgressDone.gameObject.SetActive(true);
		this._missionNameDone.color = this._normalColor;
		for (int i = 0; i < this._tasks.Count; i++)
		{
			Object.Destroy(this._tasks[i].gameObject);
		}
		this._tasks.Clear();
		this._tasksForMoveLeft.Clear();
		this._progress.gameObject.SetActive(false);
		TMP_Text missionName = this._missionName;
		string empty = string.Empty;
		this._missionNameDone.text = empty;
		missionName.text = empty;
	}

	private void Instance_OnActivate(InfoMessageTypes messageType, bool isActive)
	{
		if (messageType == InfoMessageTypes.MissionAccomplished && !isActive && this._nextMission == null)
		{
			this.Clear();
		}
		else if (messageType == InfoMessageTypes.MissionNew && !isActive && this._nextMission != null)
		{
			this._tasksForMoveLeft.Clear();
			this.Populate(this._nextMission);
		}
	}

	private void RemoveHidden(HudMissionTask taskView)
	{
		int num = this._tasks.FindIndex((HudMissionTask p) => p.TaskId == taskView.TaskId);
		if (num != -1)
		{
			Dictionary<HudMissionTask, float> dictionary = this.CalcAllPosY(num + 1);
			Object.Destroy(taskView.gameObject);
			this._tasks.RemoveAt(num);
			foreach (KeyValuePair<HudMissionTask, float> keyValuePair in dictionary)
			{
				keyValuePair.Key.MoveUp(keyValuePair.Value);
			}
		}
	}

	private float FindPrevY(List<HudMissionTask> tasks, int i)
	{
		HudMissionTask hudMissionTask = null;
		try
		{
			hudMissionTask = tasks.ElementAt(i - 1);
		}
		catch (Exception)
		{
		}
		return (!(hudMissionTask == null)) ? hudMissionTask.Y : 0f;
	}

	private float FindPrevH(List<HudMissionTask> tasks, int i)
	{
		HudMissionTask hudMissionTask = null;
		try
		{
			hudMissionTask = tasks.ElementAt(i - 1);
		}
		catch (Exception)
		{
		}
		return (!(hudMissionTask == null)) ? hudMissionTask.H : 21f;
	}

	private bool IsNeedReOrdering(int orderId)
	{
		return this._tasks.Any((HudMissionTask p) => p.OrderId > orderId);
	}

	private void Instance_OnScreenChanged(GameScreenType obj)
	{
		if (ScreenManager.Game3DScreens.Contains(ScreenManager.Instance.GameScreen) && !ScreenManager.Game3DScreens.Contains(ScreenManager.Instance.GameScreenPrev))
		{
			this.Populate(ClientMissionsManager.Instance.CurrentTrackedMission);
		}
		else if (!ScreenManager.Game3DScreens.Contains(ScreenManager.Instance.GameScreen) && ScreenManager.Game3DScreens.Contains(ScreenManager.Instance.GameScreenPrev))
		{
			this.Clear();
		}
	}

	private void Instance_OnTransfer(bool isTransfer)
	{
		if (isTransfer)
		{
			this.Clear();
		}
	}

	private bool IsInfoMsgActive(InfoMessageTypes messageType)
	{
		return MessageFactory.InfoMessagesQueue.Any((InfoMessage p) => p.MessageType == messageType) || (InfoMessageController.Instance.currentMessage != null && InfoMessageController.Instance.currentMessage.MessageType == messageType);
	}

	[SerializeField]
	private bool _isProgressTextsActive;

	[SerializeField]
	private AudioClip _missionDone;

	[SerializeField]
	private AudioClip _taskDone;

	[SerializeField]
	private TextMeshProUGUI _imageDone;

	[SerializeField]
	private Image _imageDoneProgress;

	[SerializeField]
	private HudMissionTask taskPrefab;

	[SerializeField]
	private Transform tasksParent;

	[SerializeField]
	private TextMeshProUGUI _missioTasksCount;

	[SerializeField]
	private TextMeshProUGUI _missionName;

	[SerializeField]
	private TextMeshProUGUI _missionTasksProgress;

	[SerializeField]
	private TextMeshProUGUI _missioTasksCountDone;

	[SerializeField]
	private TextMeshProUGUI _missionTasksProgressDone;

	[SerializeField]
	private TextMeshProUGUI _missionNameDone;

	[SerializeField]
	private AlphaFade _current;

	[SerializeField]
	private AlphaFade _done;

	[SerializeField]
	private ProgressDisplayer _progress;

	[Space(5f)]
	[SerializeField]
	private TextMeshProUGUI _missionTimer;

	private QuestTimePulsator _timePulsator;

	private int _missionId;

	private MissionOnClient _nextMission;

	private List<HudMissionTask> _tasks = new List<HudMissionTask>();

	private int _completedCount;

	private List<HudMissionTask> _tasksForMoveLeft = new List<HudMissionTask>();

	private Color _normalColor;

	private Color _doneColor;

	private HudMissionHandler.States _state;

	private bool _isProgressActive;

	private enum States : byte
	{
		None,
		Destroyed,
		Finished,
		Finishing
	}
}
