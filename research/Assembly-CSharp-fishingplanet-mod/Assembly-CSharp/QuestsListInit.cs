using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class QuestsListInit : ActivityStateControlled
{
	private void Awake()
	{
		this.DetailsPanel.SetEmptyList();
		this.DetailsPanel.RefreshApplyButton();
	}

	protected override void Start()
	{
		base.Start();
		this._icoCounterController = base.GetComponent<IcoCounterController>();
		this._icoCounterController.SetUseServerData(false);
		this.DetailsPanel.OnUntrackQuest += this.DetailsPanel_OnUntrackQuest;
		this.DetailsPanel.OnTrackQuest += this.DetailsPanel_OnTrackQuest;
		PhotonConnectionFactory.Instance.OnMissionTimedOut += this.Instance_OnMissionTimedOut;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.StopAllCoroutines();
		this.DetailsPanel.OnUntrackQuest -= this.DetailsPanel_OnUntrackQuest;
		this.DetailsPanel.OnTrackQuest -= this.DetailsPanel_OnTrackQuest;
		PhotonConnectionFactory.Instance.OnMissionTimedOut -= this.Instance_OnMissionTimedOut;
	}

	protected override void SetHelp()
	{
		base.SetHelp();
		if (StaticUserData.DISABLE_MISSIONS)
		{
			return;
		}
		if (this._icoCounterController == null)
		{
			this._icoCounterController = base.GetComponent<IcoCounterController>();
		}
		this._icoCounterController.SetUseServerData(false);
		PhotonConnectionFactory.Instance.MissionsListReceived += this.PhotonServer_OnMissionListReceived;
		PhotonConnectionFactory.Instance.ActiveMissionSet += this.OnSet;
		PhotonConnectionFactory.Instance.ActiveMissionSetFailed += this.OnSetFailed;
		if (!this._activeQuests.isOn)
		{
			this._activeQuests.isOn = true;
		}
		this.Refresh(QuestsListInit.MissionTypes.Started, false);
	}

	protected override void OnUserChanged()
	{
		this._current = QuestsListInit.MissionTypes.None;
		this.Refresh(QuestsListInit.MissionTypes.Started, true);
	}

	protected override void HideHelp()
	{
		base.HideHelp();
		this._groupedList.Clear();
		this._current = QuestsListInit.MissionTypes.None;
		if (StaticUserData.DISABLE_MISSIONS)
		{
			return;
		}
		this.UnsubscribeGetMission();
		this.UnsubscribeUntrack();
		PhotonConnectionFactory.Instance.MissionsListReceived -= this.PhotonServer_OnMissionListReceived;
		PhotonConnectionFactory.Instance.ActiveMissionSet -= this.OnSet;
		PhotonConnectionFactory.Instance.ActiveMissionSetFailed -= this.OnSetFailed;
	}

	public void GetArchived()
	{
		this.Refresh(QuestsListInit.MissionTypes.Archived, true);
	}

	public void GetStarted()
	{
		this.Refresh(QuestsListInit.MissionTypes.Started, true);
	}

	public void GetCompleted()
	{
		this.Refresh(QuestsListInit.MissionTypes.Completed, true);
	}

	public void Refresh(QuestsListInit.MissionTypes type, bool clearSelected = true)
	{
		if (this._current == type || StaticUserData.DISABLE_MISSIONS)
		{
			return;
		}
		this._currentMissionId = null;
		if (clearSelected)
		{
			this.ListToggleGroup.SetAllTogglesOff();
		}
		if (this.ContentPanel.transform.childCount == 0)
		{
			this.LoadingGizmo.SetActive(true);
		}
		this._current = type;
		if (StaticUserData.DISABLE_MISSIONS)
		{
			return;
		}
		this.DetailsPanel.SetEmptyList();
		this.DetailsPanel.RefreshApplyButton();
		LogHelper.Log("___kocha QuestsListInit::Refresh type:{0}", new object[] { type });
		if (type != QuestsListInit.MissionTypes.Started)
		{
			if (type != QuestsListInit.MissionTypes.Completed)
			{
				if (type == QuestsListInit.MissionTypes.Archived)
				{
					PhotonConnectionFactory.Instance.GetMissionsArchived();
				}
			}
			else
			{
				PhotonConnectionFactory.Instance.GetMissionsCompleted();
			}
		}
		else
		{
			PhotonConnectionFactory.Instance.GetMissionsStarted();
		}
	}

	private void SetActiveItem(QuestListItemHandler item)
	{
		item.Toggle.Select();
		this.QuestListNavigation.UpdateFromEventSystemSelected();
		this.QuestListNavigation.ForceUpdate();
		if (!item.Toggle.isOn)
		{
			item.Toggle.isOn = true;
		}
		else
		{
			this.DetailsPanel.InitAndBlink(item.Mission, true);
			this.DetailsPanel.RefreshApplyButton();
		}
	}

	private void QuestListInit_IsSelectedItem(object sender, QuestEventArgs e)
	{
		if (this._currentMissionId != null && this._currentMissionId.Value == e.Quest.MissionId)
		{
			return;
		}
		this._currentMissionId = new int?(e.Quest.MissionId);
		for (int i = 0; i < this._groupedList.Count; i++)
		{
			GroupedList groupedList = this._groupedList[i];
			if (!groupedList.Missions.Contains(e.Quest.MissionId))
			{
				groupedList.TurnOffAllToggles();
			}
		}
		if (ClientMissionsManager.IsUnreadQuest(e.Quest.MissionId))
		{
			ClientMissionsManager.ReadQuest(e.Quest.MissionId, e.Quest.Name);
		}
		MissionOnClient missionOnClient = this.FindMission(e.Quest.MissionId);
		this.UpdateStateElement(missionOnClient);
		this.UpdateStateGroup(missionOnClient);
		if (this._current == QuestsListInit.MissionTypes.Started)
		{
			ClientMissionsManager.UpdateMissionCount(this.GetNewMissionCount());
		}
		this.DetailsPanel.InitAndBlink(missionOnClient, true);
		this.DetailsPanel.RefreshApplyButton();
	}

	private void PhotonServer_OnMissionListReceived(byte subOpCode, List<MissionOnClient> missions)
	{
		this.ReInit(missions);
		this.ScrollToSelectedMission();
	}

	private void Instance_DisableMissionActivationFailed(Failure failure)
	{
		this._nextMissionId = null;
		this.UnsubscribeUntrack();
		Debug.LogErrorFormat("Mission:DisableMissionActivationFailed ErrorCode:{0} ErrorMessage:{1}", new object[] { failure.ErrorCode, failure.ErrorMessage });
		UIHelper.Waiting(false, null);
	}

	private void Instance_MissionActivationDisabled()
	{
		this.UnsubscribeUntrack();
		if (ClientMissionsManager.Instance.CurrentTrackedMission != null)
		{
			this.MissionGet(ClientMissionsManager.Instance.CurrentTrackedMission.MissionId);
		}
	}

	private void MissionGet(int missionId)
	{
		PhotonConnectionFactory.Instance.MissionGet += this.Instance_MissionGet;
		PhotonConnectionFactory.Instance.MissionGetFailed += this.Instance_MissionGetFailed;
		PhotonConnectionFactory.Instance.GetMission(missionId);
	}

	private void Instance_MissionGetFailed(Failure failure)
	{
		this._nextMissionId = null;
		this.UnsubscribeGetMission();
		Debug.LogErrorFormat("Mission:Instance_MissionGetFailed ErrorCode:{0} ErrorMessage:{1}", new object[] { failure.ErrorCode, failure.ErrorMessage });
		UIHelper.Waiting(false, null);
	}

	private void Instance_MissionGet(MissionOnClient mission)
	{
		UIHelper.Waiting(false, null);
		this.UnsubscribeGetMission();
		ClientMissionsManager.Instance.CurrentTrackedMission = ((!mission.IsActiveMission) ? null : mission);
		this.UpdateMission(mission);
		this.UpdateStateElement(mission);
		this.UpdateStateGroup(mission);
		if (this._current == QuestsListInit.MissionTypes.Started)
		{
			ClientMissionsManager.UpdateMissionCount(this.GetNewMissionCount());
		}
		this.DetailsPanel.InitAndBlink(mission, false);
		this.DetailsPanel.RefreshApplyButton();
		if (this._nextMissionId != null)
		{
			UIHelper.Waiting(true, null);
			int value = this._nextMissionId.Value;
			this._nextMissionId = null;
			PhotonConnectionFactory.Instance.SetActiveMission(new int?(value));
		}
	}

	private void UnsubscribeGetMission()
	{
		PhotonConnectionFactory.Instance.MissionGet -= this.Instance_MissionGet;
		PhotonConnectionFactory.Instance.MissionGetFailed -= this.Instance_MissionGetFailed;
	}

	private void UnsubscribeUntrack()
	{
		PhotonConnectionFactory.Instance.MissionActivationDisabled -= this.Instance_MissionActivationDisabled;
		PhotonConnectionFactory.Instance.DisableMissionActivationFailed -= this.Instance_DisableMissionActivationFailed;
	}

	private Dictionary<string, List<MissionOnClient>> GenerateGroups(List<MissionOnClient> missions)
	{
		Dictionary<string, List<MissionOnClient>> dictionary = new Dictionary<string, List<MissionOnClient>>();
		this._missionGroups.Clear();
		for (int i = 0; i < missions.Count; i++)
		{
			MissionOnClient missionOnClient = missions[i];
			string text = ((missionOnClient.Group == null) ? "Misc." : missionOnClient.Group.Name);
			if (!dictionary.ContainsKey(text))
			{
				if (missionOnClient.Group != null)
				{
					this._missionGroups.Add(missionOnClient.Group);
				}
				dictionary.Add(text, new List<MissionOnClient>());
			}
			dictionary[text].Add(missionOnClient);
		}
		return dictionary;
	}

	private void ReInit(List<MissionOnClient> missions)
	{
		this._firstToggleInList = null;
		this._groupedList.Clear();
		Dictionary<string, List<MissionOnClient>> dictionary = this.GenerateGroups(missions);
		List<string> list = (from o in this._missionGroups
			orderby o.Priority
			select o into x
			select x.Name).ToList<string>();
		if (dictionary.ContainsKey("Misc."))
		{
			list.Add("Misc.");
		}
		int i = 0;
		int j = 0;
		if (!QuestsListInit.GroupsFoldMap.ContainsKey(this._current))
		{
			QuestsListInit.GroupsFoldMap.Add(this._current, new Dictionary<string, bool>());
		}
		foreach (string text in list)
		{
			if (!QuestsListInit.GroupsFoldMap[this._current].ContainsKey(text))
			{
				QuestsListInit.GroupsFoldMap[this._current][text] = true;
			}
			GroupedList groupedList;
			if (this.allGroups.Count > j)
			{
				groupedList = this.allGroups[j];
				groupedList.transform.SetParent(this.ContentPanel.transform);
				groupedList.transform.localScale = Vector3.one;
			}
			else
			{
				groupedList = GUITools.AddChild(this.ContentPanel, this.GroupListPrefab).GetComponent<GroupedList>();
				this.allGroups.Add(groupedList);
			}
			groupedList.Init(text, this.ZeroScale, QuestListBase.State.None);
			groupedList.transform.SetSiblingIndex(j++);
			bool flag = false;
			TimeSpan timeSpan = TimeSpan.Zero;
			List<MissionOnClient> list2 = dictionary[text].OrderBy((MissionOnClient p) => p.OrderId).ToList<MissionOnClient>();
			for (int k = 0; k < list2.Count; k++)
			{
				MissionOnClient missionOnClient = list2[k];
				if (missionOnClient.TimeToComplete == null && missionOnClient.TimeRemain != null && missionOnClient.TimeRemain.Value.TotalSeconds > timeSpan.TotalSeconds)
				{
					timeSpan = missionOnClient.TimeRemain.Value;
				}
				QuestListItemHandler questListItemHandler;
				if (this.allQuestItems.Count > i)
				{
					questListItemHandler = this.allQuestItems[i];
					questListItemHandler.transform.localScale = Vector3.one;
				}
				else
				{
					questListItemHandler = Object.Instantiate<GameObject>(this.QuestListItemPrefab).GetComponent<QuestListItemHandler>();
					this.allQuestItems.Add(questListItemHandler);
				}
				i++;
				questListItemHandler.Init(missionOnClient);
				questListItemHandler.IsSelectedItem += this.QuestListInit_IsSelectedItem;
				Toggle toggle = questListItemHandler.Toggle;
				toggle.group = this.ListToggleGroup;
				groupedList.AddElement(questListItemHandler, k);
				if (this._firstToggleInList == null)
				{
					this._firstToggleInList = questListItemHandler;
				}
				if (!flag && toggle.isOn)
				{
					this._firstToggleInList = questListItemHandler;
				}
				if (missionOnClient.IsActiveMission)
				{
					flag = true;
					questListItemHandler.UpdateState(QuestListBase.State.Active);
					this._firstToggleInList = questListItemHandler;
					ClientMissionsManager.Instance.CurrentTrackedMission = missionOnClient;
				}
				else if (ClientMissionsManager.IsUnreadQuest(missionOnClient.MissionId))
				{
					questListItemHandler.UpdateState(QuestListBase.State.New);
				}
			}
			groupedList.SetTimeRemain(timeSpan);
			string group = text;
			if (!QuestsListInit.GroupsFoldMap[this._current][group])
			{
				groupedList.GroupToggle.isOn = QuestsListInit.GroupsFoldMap[this._current][group];
				groupedList.OnRollup();
			}
			groupedList.REinitListeners(delegate(bool isActive)
			{
				QuestsListInit.GroupsFoldMap[this._current][group] = isActive;
				this.QuestListNavigation.ForceUpdate();
			});
			this._groupedList.Add(groupedList);
		}
		while (i < this.allQuestItems.Count)
		{
			this.allQuestItems[i++].transform.SetParent(this.ZeroScale);
		}
		while (j < this.allGroups.Count)
		{
			this.allGroups[j++].transform.SetParent(this.ZeroScale);
		}
		if (this._groupedList.Count == 0)
		{
			this.DetailsPanel.SetEmptyList();
			this.DetailsPanel.RefreshApplyButton();
		}
		else
		{
			for (int l = 0; l < this._groupedList.Count; l++)
			{
				List<int> missions2 = this._groupedList[l].Missions;
				for (int n = 0; n < missions2.Count; n++)
				{
					this.UpdateStateGroup(this._groupedList[l].Find(missions2[n]));
				}
			}
		}
		if (this._current == QuestsListInit.MissionTypes.Started)
		{
			ClientMissionsManager.UpdateMissionCount(this.GetNewMissionCount());
		}
		this.LoadingGizmo.SetActive(false);
		MissionOnClient m = ClientMissionsManager.Instance.CurrentTrackedMission;
		if (m == null)
		{
			m = (from p in this.allQuestItems
				where p.Toggle.isOn
				select p.Mission).FirstOrDefault<MissionOnClient>();
		}
		if (m != null)
		{
			foreach (GroupedList groupedList2 in this._groupedList)
			{
				if (groupedList2.Missions.Contains(m.MissionId))
				{
					QuestListItemHandler questListItemHandler2 = groupedList2.MissionItems.FirstOrDefault((QuestListItemHandler x) => x.Mission.MissionId == m.MissionId);
					if (questListItemHandler2 != null)
					{
						this.SetActiveItem(questListItemHandler2);
						break;
					}
				}
			}
		}
		else if (this._groupedList.Count > 0 && this._firstToggleInList != null)
		{
			this.SetActiveItem(this._firstToggleInList);
		}
	}

	private void OnSet(int? activeMissionId)
	{
		MissionOnClient currentTrackedMission = ClientMissionsManager.Instance.CurrentTrackedMission;
		if (currentTrackedMission != null)
		{
			currentTrackedMission.IsActiveMission = false;
			this.UpdateStateElement(currentTrackedMission);
			this.UpdateStateGroup(currentTrackedMission);
			if (this._current == QuestsListInit.MissionTypes.Started)
			{
				ClientMissionsManager.UpdateMissionCount(this.GetNewMissionCount());
			}
			this.DetailsPanel.InitAndBlink(currentTrackedMission, false);
			this.DetailsPanel.RefreshApplyButton();
		}
		if (activeMissionId != null)
		{
			MissionOnClient missionOnClient = this.FindMission(activeMissionId.Value);
			if (missionOnClient != null)
			{
				this.MissionGet(activeMissionId.Value);
			}
			else
			{
				this.GetStarted();
			}
		}
		else
		{
			ClientMissionsManager.Instance.CurrentTrackedMission = null;
			UIHelper.Waiting(false, null);
		}
	}

	private void OnSetFailed(Failure failure)
	{
		UIHelper.Waiting(false, null);
		this.LoadingGizmo.SetActive(false);
		Debug.LogWarning(failure.FullErrorInfo);
		this.GetStarted();
	}

	private void DetailsPanel_OnTrackQuest(int missionId)
	{
		if (ClientMissionsManager.Instance.CurrentTrackedMission == null)
		{
			UIHelper.Waiting(true, null);
			PhotonConnectionFactory.Instance.SetActiveMission(new int?(missionId));
		}
		else if (ClientMissionsManager.Instance.CurrentTrackedMission.MissionId != missionId)
		{
			this._nextMissionId = new int?(missionId);
			this.MissionGet(ClientMissionsManager.Instance.CurrentTrackedMission.MissionId);
		}
	}

	private void DetailsPanel_OnUntrackQuest()
	{
		UIHelper.Waiting(true, null);
		PhotonConnectionFactory.Instance.SetActiveMission(null);
	}

	private MissionOnClient FindMission(int missionId)
	{
		for (int i = 0; i < this._groupedList.Count; i++)
		{
			GroupedList groupedList = this._groupedList[i];
			MissionOnClient missionOnClient = groupedList.Find(missionId);
			if (missionOnClient != null)
			{
				return missionOnClient;
			}
		}
		return null;
	}

	private void UpdateMission(MissionOnClient m)
	{
		for (int i = 0; i < this._groupedList.Count; i++)
		{
			GroupedList groupedList = this._groupedList[i];
			if (groupedList.UpdateMission(m))
			{
				break;
			}
		}
	}

	private void UpdateStateElement(MissionOnClient mission)
	{
		GroupedList groupedList = null;
		for (int i = 0; i < this._groupedList.Count; i++)
		{
			groupedList = this._groupedList[i];
			MissionOnClient missionOnClient = groupedList.Find(mission.MissionId);
			if (missionOnClient != null)
			{
				break;
			}
		}
		if (groupedList != null)
		{
			QuestListBase.State state = QuestListBase.State.None;
			if (mission.IsActiveMission)
			{
				state = QuestListBase.State.Active;
			}
			else if (ClientMissionsManager.IsUnreadQuest(mission.MissionId))
			{
				state = QuestListBase.State.New;
			}
			groupedList.UpdateMission(mission);
			groupedList.UpdateStateElement(mission.MissionId, state);
		}
	}

	private void UpdateStateGroup(MissionOnClient mission)
	{
		int num = -1;
		for (int i = 0; i < this._groupedList.Count; i++)
		{
			if (this._groupedList[i].Find(mission.MissionId) != null)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			QuestListBase.State state = QuestListBase.State.None;
			GroupedList groupedList = this._groupedList[num];
			List<int> missions = groupedList.Missions;
			for (int j = 0; j < missions.Count; j++)
			{
				MissionOnClient missionOnClient = groupedList.Find(missions[j]);
				if (missionOnClient.IsActiveMission)
				{
					state = QuestListBase.State.Active;
				}
				else if (ClientMissionsManager.IsUnreadQuest(missionOnClient.MissionId) && state == QuestListBase.State.None)
				{
					state = QuestListBase.State.New;
				}
			}
			groupedList.UpdateState(state);
		}
	}

	private int GetNewMissionCount()
	{
		int num = 0;
		for (int i = 0; i < this._groupedList.Count; i++)
		{
			List<int> missions = this._groupedList[i].Missions;
			for (int j = 0; j < missions.Count; j++)
			{
				MissionOnClient missionOnClient = this._groupedList[i].Find(missions[j]);
				if (!missionOnClient.IsActiveMission && ClientMissionsManager.IsUnreadQuest(missionOnClient.MissionId))
				{
					num++;
				}
			}
		}
		return num;
	}

	private void Instance_OnMissionTimedOut(int missionId, string missionName, int? missionImage)
	{
		LogHelper.Log("___kocha QuestsListInit::OnMissionTimedOut missionId:{0}", new object[] { missionId });
		QuestListItemHandler questListItemHandler = this.allQuestItems.FirstOrDefault((QuestListItemHandler p) => p.Mission.MissionId == missionId);
		if (questListItemHandler != null)
		{
			questListItemHandler.ClearPulsator();
		}
		if (this._current == QuestsListInit.MissionTypes.Started)
		{
			this._current = QuestsListInit.MissionTypes.None;
			this.Refresh(QuestsListInit.MissionTypes.Started, this._currentMissionId != null && this._currentMissionId == missionId);
		}
	}

	private IEnumerator Scrolling(ScrollRect sr, float v)
	{
		yield return new WaitForEndOfFrame();
		sr.verticalNormalizedPosition = v;
		yield break;
	}

	private void ScrollToSelectedMission()
	{
		base.StopAllCoroutines();
		int? num = null;
		int? num2 = null;
		int num3 = 0;
		for (int i = 0; i < this._groupedList.Count; i++)
		{
			num3++;
			GroupedList groupedList = this._groupedList[i];
			for (int j = 0; j < groupedList.MissionItems.Count; j++)
			{
				num3++;
				QuestListItemHandler questListItemHandler = groupedList.MissionItems[j];
				if (questListItemHandler.CurrentState == QuestListBase.State.Active)
				{
					num = new int?(num3);
				}
				else if (questListItemHandler.CurrentState == QuestListBase.State.New && num2 == null)
				{
					num2 = new int?(num3);
				}
			}
		}
		if (num != null || num2 != null)
		{
			float num4 = (float)((num == null) ? num2 : num).Value / (float)num3;
			if (this._scroll.direction == 2)
			{
				num4 = 1f - num4;
			}
			base.StartCoroutine(this.Scrolling(this._scrollRect, Mathf.Clamp01(num4)));
		}
	}

	[SerializeField]
	private Toggle _activeQuests;

	private const string NoGroupTitle = "Misc.";

	public GameObject QuestListItemPrefab;

	public GameObject GroupListPrefab;

	public GameObject ContentPanel;

	public ToggleGroup ListToggleGroup;

	public ActiveQuestDetails DetailsPanel;

	public GameObject LoadingGizmo;

	private QuestListItemHandler _firstToggleInList;

	private QuestsListInit.MissionTypes _current;

	private List<GroupedList> _groupedList = new List<GroupedList>();

	private int? _nextMissionId;

	private int? _currentMissionId;

	private IcoCounterController _icoCounterController;

	[SerializeField]
	private Transform ZeroScale;

	[SerializeField]
	private UINavigation QuestListNavigation;

	[SerializeField]
	private Scrollbar _scroll;

	[SerializeField]
	private ScrollRect _scrollRect;

	private static Dictionary<QuestsListInit.MissionTypes, Dictionary<string, bool>> GroupsFoldMap = new Dictionary<QuestsListInit.MissionTypes, Dictionary<string, bool>>();

	private List<MissionGroupOnClient> _missionGroups = new List<MissionGroupOnClient>();

	private List<QuestListItemHandler> allQuestItems = new List<QuestListItemHandler>();

	private List<GroupedList> allGroups = new List<GroupedList>();

	public enum MissionTypes : byte
	{
		None,
		Started,
		Completed,
		Archived
	}
}
