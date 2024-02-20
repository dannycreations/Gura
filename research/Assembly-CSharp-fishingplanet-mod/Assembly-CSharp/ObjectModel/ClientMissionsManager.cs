using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ObjectModel
{
	public class ClientMissionsManager
	{
		public static ClientMissionsManager.GetProfileHandler ChannelGetProfile { get; set; }

		public static ClientMissionsManager.InteractiveObjectConfigurationRequest ChannelConfigurationRequest { get; set; }

		public static ClientMissionsManager.HintsReceivedHandler ChannelHintsReceived { get; set; }

		public static ClientMissionsManager.InteractHandler ChannelInteract { get; set; }

		public static ClientMissionsManager.CompleteConditionHandler ChannelCompleteCondition { get; set; }

		public static ClientMissionsManager Instance
		{
			get
			{
				if (ClientMissionsManager.instance == null)
				{
					ClientMissionsManager.instance = new ClientMissionsManager();
				}
				return ClientMissionsManager.instance;
			}
		}

		public Profile Profile
		{
			get
			{
				return (ClientMissionsManager.ChannelGetProfile != null) ? ClientMissionsManager.ChannelGetProfile() : null;
			}
		}

		public static void Clear()
		{
			ClientMissionsManager.instance = null;
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action<int> OnUpdateMissionCount;

		public static void UpdateMissionCount(int count)
		{
			ClientMissionsManager.OnUpdateMissionCount(count);
		}

		public static int? CurrentWidgetTaskId { get; set; }

		public int CurrentTrackedMissionId
		{
			get
			{
				return (this.currentTrackedMission == null) ? 0 : this.currentTrackedMission.MissionId;
			}
		}

		public MissionOnClient CurrentTrackedMission
		{
			get
			{
				return this.currentTrackedMission;
			}
			set
			{
				MissionOnClient missionOnClient = this.currentTrackedMission;
				this.currentTrackedMission = value;
				if (this.TrackedMissionUpdated != null)
				{
					this.TrackedMissionUpdated(this.currentTrackedMission);
				}
				this.UpdateClientTaskHints(missionOnClient, value);
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event ClientMissionsManager.TrackedMissionUpdatedHandler TrackedMissionUpdated;

		public void UpdateCurrentMissionTasks(List<MissionTaskOnClient> updatedTasks)
		{
			if (this.CurrentTrackedMission == null)
			{
				return;
			}
			updatedTasks = updatedTasks.FindAll((MissionTaskOnClient p) => p.MissionId == this.CurrentTrackedMissionId);
			if (updatedTasks.Count == 0)
			{
				return;
			}
			DebugUtility.Missions.Important("Tasks updated", new object[0]);
			for (int i = 0; i < updatedTasks.Count; i++)
			{
				for (int j = 0; j < this.CurrentTrackedMission.Tasks.Count; j++)
				{
					if (this.CurrentTrackedMission.Tasks[j].TaskId == updatedTasks[i].TaskId)
					{
						this.CurrentTrackedMission.Tasks[j].Progress = updatedTasks[i].Progress;
						this.CurrentTrackedMission.Tasks[j].IsCompleted = updatedTasks[i].IsCompleted;
						this.CurrentTrackedMission.Tasks[j].IsHidden = updatedTasks[i].IsHidden;
						break;
					}
				}
			}
			List<MissionTaskOnClient> list = this.CurrentTrackedMission.Tasks.FindAll((MissionTaskOnClient p) => !p.IsCompleted);
			if (list.Count == 0 && !ClientMissionsManager.IsUnreadQuest(this.CurrentTrackedMission.MissionId))
			{
				ClientMissionsManager.UnreadQuest(this.CurrentTrackedMission.MissionId);
			}
			if (this.TrackedMissionUpdated != null)
			{
				this.TrackedMissionUpdated(this.CurrentTrackedMission);
			}
		}

		public static void ParseTaskProgress(string progress, out string progressNum, out string count)
		{
			if (string.IsNullOrEmpty(progress))
			{
				string empty;
				count = (empty = string.Empty);
				progressNum = empty;
				return;
			}
			string[] array = progress.Split(new char[] { '/' });
			progressNum = array[0];
			count = array[1];
		}

		public void ArrivedToPond(int pondId)
		{
			this.pondId = pondId;
			foreach (KeyValuePair<int, ClientMissionsManager.MissionState> keyValuePair in this.missionStateHash.ToList<KeyValuePair<int, ClientMissionsManager.MissionState>>())
			{
				ClientMissionsManager.MissionState value = keyValuePair.Value;
				foreach (MissionInteractiveObject missionInteractiveObject in value.interactiveObjects.ToList<MissionInteractiveObject>())
				{
					if (missionInteractiveObject.PondId != pondId)
					{
						this.UnloadInteractiveObject(value, missionInteractiveObject);
					}
				}
			}
		}

		public List<MissionOnClient> GetTimeTrackedMissions()
		{
			return (from s in this.missionStateHash
				where !s.Value.IsUnloaded && s.Value.TimeToComplete != null
				select s into m
				select new MissionOnClient
				{
					MissionId = m.Key,
					StartTime = m.Value.StartTime,
					TimeToComplete = m.Value.TimeToComplete
				}).ToList<MissionOnClient>();
		}

		private void TrackMission(HintMessage message)
		{
			int missionId = message.MissionId;
			ClientMissionsManager.MissionState missionState;
			if (!this.missionStateHash.TryGetValue(missionId, out missionState))
			{
				this.missionStateHash.Add(missionId, missionState = new ClientMissionsManager.MissionState());
			}
			missionState.IsUnloaded = false;
			missionState.StartTime = message.StartTime.Value;
			missionState.TimeToComplete = message.TimeToComplete;
		}

		private void CancelMission(int missionId)
		{
			foreach (KeyValuePair<int, ClientMissionsManager.MissionState> keyValuePair in this.missionStateHash.ToList<KeyValuePair<int, ClientMissionsManager.MissionState>>())
			{
				if (keyValuePair.Key == missionId)
				{
					ClientMissionsManager.MissionState value = keyValuePair.Value;
					foreach (MissionInteractiveObject missionInteractiveObject in value.interactiveObjects.ToList<MissionInteractiveObject>())
					{
						this.UnloadInteractiveObject(value, missionInteractiveObject);
					}
					foreach (MissionTaskTrackedOnClient missionTaskTrackedOnClient in value.trackedTasks.ToList<MissionTaskTrackedOnClient>())
					{
						this.UnloadMissionTaskTrackedOnClient(value, missionTaskTrackedOnClient);
					}
					value.IsUnloaded = true;
				}
			}
		}

		public List<HintMessage> ProcessMessages(List<HintMessage> messages)
		{
			List<HintMessage> list = (from m in messages
				where m.InteractiveObjectId != null
				group m by m.InteractiveObjectId).SelectMany(delegate(IGrouping<string, HintMessage> g)
			{
				List<HintMessage> list3 = g.ToList<HintMessage>();
				HintMessage lastActiveState = list3.LastOrDefault((HintMessage m) => m.ActiveState != null && m.EventType == MissionEventType.MissionTaskHintMessageAdded);
				if (lastActiveState != null)
				{
					list3.RemoveAll((HintMessage m) => m.ActiveState != null && !object.ReferenceEquals(m, lastActiveState));
				}
				return list3;
			}).ToList<HintMessage>();
			foreach (HintMessage hintMessage in list)
			{
				int missionId = hintMessage.MissionId;
				string resourceKey = hintMessage.InteractiveObjectId;
				if (!this.QueryInteractiveObject(hintMessage))
				{
					if (hintMessage.EventType != MissionEventType.MissionTaskHintMessageRemoved)
					{
						if (!this.interactiveObjectConfigurationRequestList.Any((ClientMissionsManager.InteractiveObjectKey r) => r.MissionId == missionId && r.ResourceKey == resourceKey))
						{
							this.interactiveObjectConfigurationRequestList.Add(new ClientMissionsManager.InteractiveObjectKey
							{
								MissionId = missionId,
								ResourceKey = resourceKey
							});
							ClientMissionsManager.ChannelConfigurationRequest(missionId, resourceKey);
						}
					}
				}
				else
				{
					hintMessage.InteractiveObject.ProcessMessage(hintMessage);
				}
			}
			List<HintMessage> list2 = messages.Where((HintMessage m) => m.InteractiveObjectId == null).ToList<HintMessage>();
			foreach (HintMessage hintMessage2 in list2)
			{
				int missionId2 = hintMessage2.MissionId;
				if (hintMessage2.IsStartMissionEvent())
				{
					this.TrackMission(hintMessage2);
				}
				if (hintMessage2.IsCancelMissionEvent())
				{
					this.CancelMission(missionId2);
				}
			}
			if (list2.Any<HintMessage>())
			{
				ClientMissionsManager.ChannelHintsReceived(list2);
			}
			return list2;
		}

		public void CompleteTestCondition(bool completed, string progress = null)
		{
			foreach (KeyValuePair<int, ClientMissionsManager.MissionState> keyValuePair in this.missionStateHash.ToList<KeyValuePair<int, ClientMissionsManager.MissionState>>())
			{
				ClientMissionsManager.MissionState value = keyValuePair.Value;
				foreach (MissionTaskTrackedOnClient missionTaskTrackedOnClient in value.trackedTasks)
				{
					if (missionTaskTrackedOnClient.Condition != null && missionTaskTrackedOnClient.Exception == null && missionTaskTrackedOnClient.Asset == "Test")
					{
						ClientMissionsManager.ChannelCompleteCondition(missionTaskTrackedOnClient.MissionId, missionTaskTrackedOnClient.TaskId, missionTaskTrackedOnClient.ResourceKey, completed, progress);
					}
				}
			}
		}

		public void StartTracking(MissionTaskTrackedOnClient trackedTask)
		{
			int missionId = trackedTask.MissionId;
			ClientMissionsManager.MissionState missionState;
			if (!this.missionStateHash.TryGetValue(missionId, out missionState))
			{
				this.missionStateHash.Add(missionId, missionState = new ClientMissionsManager.MissionState());
			}
			try
			{
				trackedTask.Condition = this.CreateClientCondition(trackedTask);
				trackedTask.Condition.Task = trackedTask;
				trackedTask.Condition.Start();
				trackedTask.Condition.HintsChanged += this.Condition_HintsChanged;
				trackedTask.Condition.Completed += this.Condition_Completed;
			}
			catch (Exception ex)
			{
				trackedTask.Exception = ex;
				DebugUtility.Missions.Warn("MissionClientCondition StartTracking exception {0}. {1}", new object[] { trackedTask, ex });
			}
			missionState.trackedTasks.Add(trackedTask);
		}

		public void StopTracking(MissionTaskTrackedOnClient trackedTask)
		{
			int missionId = trackedTask.MissionId;
			ClientMissionsManager.MissionState missionState;
			if (!this.missionStateHash.TryGetValue(missionId, out missionState))
			{
				this.missionStateHash.Add(missionId, missionState = new ClientMissionsManager.MissionState());
			}
			foreach (MissionTaskTrackedOnClient missionTaskTrackedOnClient in missionState.trackedTasks.ToList<MissionTaskTrackedOnClient>())
			{
				if (missionTaskTrackedOnClient.TaskId == trackedTask.TaskId && missionTaskTrackedOnClient.ResourceKey == trackedTask.ResourceKey)
				{
					this.UnloadMissionTaskTrackedOnClient(missionState, missionTaskTrackedOnClient);
				}
			}
		}

		private void UnloadMissionTaskTrackedOnClient(ClientMissionsManager.MissionState state, MissionTaskTrackedOnClient trackedTask)
		{
			this.UpdateConditionHints(trackedTask.Condition, new List<HintMessage>());
			state.trackedTasks.Remove(trackedTask);
			trackedTask.Condition.Destroy();
			trackedTask.Condition.HintsChanged -= this.Condition_HintsChanged;
			trackedTask.Condition.Completed -= this.Condition_Completed;
			trackedTask.Condition.Task = null;
			trackedTask.Condition = null;
		}

		public bool CheckMonitoredDependencies(string dependency, bool affectProcessing)
		{
			bool flag = false;
			foreach (KeyValuePair<int, ClientMissionsManager.MissionState> keyValuePair in this.missionStateHash.ToList<KeyValuePair<int, ClientMissionsManager.MissionState>>())
			{
				ClientMissionsManager.MissionState value = keyValuePair.Value;
				foreach (MissionTaskTrackedOnClient missionTaskTrackedOnClient in value.trackedTasks)
				{
					if (missionTaskTrackedOnClient.Condition != null && missionTaskTrackedOnClient.Exception == null)
					{
						flag |= missionTaskTrackedOnClient.Condition.CheckMonitoredDependencies(dependency);
					}
				}
			}
			return flag;
		}

		public void UnityUpdateTrackedTasks()
		{
			if (this.Profile == null || this.Profile.MissionsContext == null)
			{
				return;
			}
			this.Profile.MissionsContext.CaptureDependencies();
			foreach (KeyValuePair<int, ClientMissionsManager.MissionState> keyValuePair in this.missionStateHash.ToList<KeyValuePair<int, ClientMissionsManager.MissionState>>())
			{
				ClientMissionsManager.MissionState value = keyValuePair.Value;
				foreach (MissionTaskTrackedOnClient missionTaskTrackedOnClient in value.trackedTasks)
				{
					if (missionTaskTrackedOnClient.Condition != null && missionTaskTrackedOnClient.Exception == null)
					{
						try
						{
							missionTaskTrackedOnClient.Condition.Update(this.Profile.MissionsContext);
						}
						catch (Exception ex)
						{
							missionTaskTrackedOnClient.Exception = ex;
							DebugUtility.Missions.Warn("MissionClientCondition Update exception {0}. {1}", new object[] { missionTaskTrackedOnClient, ex });
						}
					}
				}
			}
		}

		private IMissionClientCondition CreateClientCondition(MissionTaskTrackedOnClient task)
		{
			if (task.Asset == "Test")
			{
				return new TestClientCondition();
			}
			if (task.Asset == "AssembleRodCondition")
			{
				return JsonConvert.DeserializeObject<AssembleRodCondition>(task.ConfigJson, SerializationHelper.JsonSerializerSettings);
			}
			throw new NotImplementedException(string.Format("Can't create ClientCondition for task {0}", task));
		}

		private void Condition_HintsChanged(IMissionClientCondition sender, List<HintMessage> newMessages)
		{
			if (sender.Task.MissionId == this.CurrentTrackedMissionId)
			{
				this.UpdateConditionHints(sender, newMessages);
			}
		}

		private void Condition_Completed(IMissionClientCondition sender, bool completed, string progress)
		{
			ClientMissionsManager.ChannelCompleteCondition(sender.Task.MissionId, sender.Task.TaskId, sender.Task.ResourceKey, completed, progress);
		}

		public void MissionClientConditionCompleted(int missionId, int taskId, string resourceKey, bool completed, string progress)
		{
			ClientMissionsManager.MissionState missionState;
			if (!this.missionStateHash.TryGetValue(missionId, out missionState))
			{
				this.missionStateHash.Add(missionId, missionState = new ClientMissionsManager.MissionState());
			}
			foreach (MissionTaskTrackedOnClient missionTaskTrackedOnClient in missionState.trackedTasks)
			{
				if (missionTaskTrackedOnClient.TaskId == taskId && missionTaskTrackedOnClient.ResourceKey == resourceKey)
				{
					missionTaskTrackedOnClient.IsClientCompleted = completed;
					missionTaskTrackedOnClient.ClientProgress = progress;
					if (completed)
					{
						this.UpdateConditionHints(missionTaskTrackedOnClient.Condition, new List<HintMessage>());
					}
				}
			}
		}

		public void UpdateConditionHints(IMissionClientCondition condition, List<HintMessage> newMessages)
		{
			List<HintMessage> prevMessages;
			if (!this.hashConditionHints.TryGetValue(condition, out prevMessages))
			{
				prevMessages = new List<HintMessage>();
			}
			if (newMessages.Count == 0 && prevMessages.Count == 0)
			{
				return;
			}
			List<HintMessage> list = prevMessages.Where((HintMessage m) => !newMessages.Any((HintMessage mm) => mm.AreSame(m))).ToList<HintMessage>();
			list.ForEach(delegate(HintMessage m)
			{
				m.EventType = MissionEventType.MissionTaskHintMessageRemoved;
			});
			List<HintMessage> list2 = newMessages.Where((HintMessage m) => !prevMessages.Any((HintMessage mm) => mm.AreSame(m))).ToList<HintMessage>();
			list2.ForEach(delegate(HintMessage m)
			{
				m.EventType = MissionEventType.MissionTaskHintMessageAdded;
				m.TranslateBeforeSend(this.Profile.MissionsContext);
			});
			this.ProcessMessages(list.Union(list2).ToList<HintMessage>());
			this.hashConditionHints[condition] = prevMessages.Except(list).Union(list2).ToList<HintMessage>();
		}

		private void UpdateClientTaskHints(MissionOnClient prevMission, MissionOnClient newMission)
		{
			int num = ((prevMission == null) ? 0 : prevMission.MissionId);
			int num2 = ((newMission == null) ? 0 : newMission.MissionId);
			if (num2 != num)
			{
				foreach (KeyValuePair<int, ClientMissionsManager.MissionState> keyValuePair in this.missionStateHash.ToList<KeyValuePair<int, ClientMissionsManager.MissionState>>())
				{
					int key = keyValuePair.Key;
					ClientMissionsManager.MissionState value = keyValuePair.Value;
					if (key != num2)
					{
						foreach (MissionTaskTrackedOnClient missionTaskTrackedOnClient in value.trackedTasks)
						{
							if (missionTaskTrackedOnClient.Condition != null && missionTaskTrackedOnClient.Exception == null)
							{
								this.UpdateConditionHints(missionTaskTrackedOnClient.Condition, new List<HintMessage>());
							}
						}
					}
					if (key == num2)
					{
						foreach (MissionTaskTrackedOnClient missionTaskTrackedOnClient2 in value.trackedTasks)
						{
							if (missionTaskTrackedOnClient2.Condition != null && missionTaskTrackedOnClient2.Exception == null)
							{
								missionTaskTrackedOnClient2.Condition.ForceGenerateHints(this.Profile.MissionsContext);
							}
						}
					}
				}
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event InteractiveObjectEventHandler Added;

		private void OnAdded(MissionInteractiveObject interactiveObject)
		{
			DebugUtility.Missions.Important("InteractiveObject OnAdded MissionId: {0}, ResourceKey: {1}", new object[] { interactiveObject.MissionId, interactiveObject.ResourceKey });
			if (this.Added != null)
			{
				this.Added(interactiveObject);
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event InteractiveObjectEventHandler Removed;

		private void OnRemoved(MissionInteractiveObject interactiveObject)
		{
			DebugUtility.Missions.Important("InteractiveObject OnRemoved MissionId: {0}, ResourceKey: {1}", new object[] { interactiveObject.MissionId, interactiveObject.ResourceKey });
			if (this.Removed != null)
			{
				this.Removed(interactiveObject);
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event InteractiveObjectEventHandler ActiveStateChanged;

		private void OnActiveStateChanged(MissionInteractiveObject interactiveObject)
		{
			if (this.ActiveStateChanged != null)
			{
				this.ActiveStateChanged(interactiveObject);
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event InteractiveObjectInteractionChangedEventHandler InteractionChanged;

		private void OnInteractionChanged(MissionInteractiveObject interactiveObject, string interaction, bool added)
		{
			if (this.InteractionChanged != null)
			{
				this.InteractionChanged(interactiveObject, interaction, added);
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event InteractiveObjectInteractedEventHandler Interacted;

		private void OnInteracted(MissionInteractiveObject interactiveObject, MissionInteractiveObject.AllowedInteraction interaction)
		{
			if (this.Interacted != null)
			{
				this.Interacted(interactiveObject, interaction);
			}
		}

		private bool QueryInteractiveObject(HintMessage message)
		{
			int missionId = message.MissionId;
			ClientMissionsManager.MissionState missionState;
			if (!this.missionStateHash.TryGetValue(missionId, out missionState))
			{
				this.missionStateHash.Add(missionId, missionState = new ClientMissionsManager.MissionState());
				missionState.interactiveObjectsPendingMessages.Add(message);
				return false;
			}
			MissionInteractiveObject missionInteractiveObject = missionState.interactiveObjects.FirstOrDefault((MissionInteractiveObject r) => r.ResourceKey == message.InteractiveObjectId);
			if (missionInteractiveObject == null)
			{
				missionState.interactiveObjectsPendingMessages.Add(message);
				return false;
			}
			if (missionInteractiveObject.Version < message.InteractiveVersion)
			{
				missionState.interactiveObjects.Remove(missionInteractiveObject);
				missionState.interactiveObjectsPendingMessages.Add(message);
				return false;
			}
			message.InteractiveObject = missionInteractiveObject;
			return true;
		}

		public void InteractiveObjectGot(int missionId, string resourceKey, MissionInteractiveObject interactiveObject)
		{
			this.interactiveObjectConfigurationRequestList.RemoveAll((ClientMissionsManager.InteractiveObjectKey r) => r.MissionId == missionId && r.ResourceKey == resourceKey);
			ClientMissionsManager.MissionState state;
			if (!this.missionStateHash.TryGetValue(missionId, out state))
			{
				this.missionStateHash.Add(missionId, state = new ClientMissionsManager.MissionState());
			}
			state.interactiveObjects.RemoveAll((MissionInteractiveObject r) => r.ResourceKey == resourceKey && r.Version <= interactiveObject.Version);
			if (interactiveObject.PondId != this.pondId)
			{
				return;
			}
			if (!state.interactiveObjects.Any((MissionInteractiveObject r) => r.ResourceKey == resourceKey))
			{
				state.interactiveObjects.Add(interactiveObject);
				interactiveObject.ActiveStateChanged += this.OnActiveStateChanged;
				interactiveObject.InteractionChanged += this.OnInteractionChanged;
				this.OnAdded(interactiveObject);
			}
			state.interactiveObjectsPendingMessages.ToList<HintMessage>().ForEach(delegate(HintMessage m)
			{
				if (m.InteractiveObjectId == resourceKey)
				{
					m.InteractiveObject = interactiveObject;
					m.InteractiveObject.ProcessMessage(m);
					state.interactiveObjectsPendingMessages.Remove(m);
				}
			});
		}

		public bool CanInteract(int missionId, string resourceKey, string interaction)
		{
			MissionInteractiveObject interactiveObject = this.GetInteractiveObject(missionId, resourceKey);
			return interactiveObject != null && interactiveObject.AllowedInteractions.ContainsKey(interaction);
		}

		public void Interact(int missionId, string resourceKey, string interaction, object argument)
		{
			if (!this.CanInteract(missionId, resourceKey, interaction))
			{
				throw new InvalidOperationException("Intreraction denied");
			}
			ClientMissionsManager.ChannelInteract(missionId, resourceKey, interaction, argument);
		}

		public void OnInteracted(int missionId, string resourceKey, string interaction)
		{
			MissionInteractiveObject interactiveObject = this.GetInteractiveObject(missionId, resourceKey);
			MissionInteractiveObject.AllowedInteraction allowedInteraction = interactiveObject.InteractionsHistory[interaction];
			this.OnInteracted(interactiveObject, allowedInteraction);
		}

		public MissionInteractiveObject GetInteractiveObject(int missionId, string resourceKey)
		{
			ClientMissionsManager.MissionState missionState;
			if (!this.missionStateHash.TryGetValue(missionId, out missionState))
			{
				return null;
			}
			return missionState.interactiveObjects.FirstOrDefault((MissionInteractiveObject r) => r.ResourceKey == resourceKey);
		}

		private void UnloadInteractiveObject(ClientMissionsManager.MissionState missionState, MissionInteractiveObject interactiveObject)
		{
			missionState.interactiveObjects.Remove(interactiveObject);
			foreach (HintMessage hintMessage in missionState.interactiveObjectsPendingMessages.ToList<HintMessage>())
			{
				if (hintMessage.InteractiveObjectId == interactiveObject.ResourceKey)
				{
					missionState.interactiveObjectsPendingMessages.Remove(hintMessage);
				}
			}
			interactiveObject.ActiveStateChanged -= this.OnActiveStateChanged;
			interactiveObject.InteractionChanged -= this.OnInteractionChanged;
			this.OnRemoved(interactiveObject);
		}

		public static bool IsUnreadQuest(int missionId)
		{
			return !PlayerPrefs.HasKey(string.Format("Mission_{0}", missionId));
		}

		public static void ReadQuest(int missionId, string value)
		{
			PlayerPrefs.SetString(string.Format("Mission_{0}", missionId), value);
		}

		public static void UnreadQuest(int missionId)
		{
			PlayerPrefs.DeleteKey(string.Format("Mission_{0}", missionId));
		}

		// Note: this type is marked as 'beforefieldinit'.
		static ClientMissionsManager()
		{
			ClientMissionsManager.OnUpdateMissionCount = delegate
			{
			};
		}

		private static ClientMissionsManager instance;

		private readonly Dictionary<int, ClientMissionsManager.MissionState> missionStateHash = new Dictionary<int, ClientMissionsManager.MissionState>();

		private readonly List<ClientMissionsManager.InteractiveObjectKey> interactiveObjectConfigurationRequestList = new List<ClientMissionsManager.InteractiveObjectKey>();

		private int pondId;

		private MissionOnClient currentTrackedMission;

		public const string TestAssetName = "Test";

		private readonly Dictionary<IMissionClientCondition, List<HintMessage>> hashConditionHints = new Dictionary<IMissionClientCondition, List<HintMessage>>();

		private const string PlayerPrefsPrefix = "Mission_{0}";

		public delegate Profile GetProfileHandler();

		public delegate void InteractiveObjectConfigurationRequest(int missionId, string resourceKey);

		public delegate void HintsReceivedHandler(List<HintMessage> messages);

		public delegate void InteractHandler(int missionId, string resourceKey, string interaction, object argument);

		public delegate void CompleteConditionHandler(int missionId, int taskId, string resourceKey, bool completed, string progress);

		public delegate void TrackedMissionUpdatedHandler(MissionOnClient mission);

		private class MissionState
		{
			public DateTime StartTime { get; set; }

			public int? TimeToComplete { get; set; }

			public bool IsUnloaded { get; set; }

			public readonly List<HintMessage> interactiveObjectsPendingMessages = new List<HintMessage>();

			public readonly List<MissionInteractiveObject> interactiveObjects = new List<MissionInteractiveObject>();

			public readonly List<MissionTaskTrackedOnClient> trackedTasks = new List<MissionTaskTrackedOnClient>();
		}

		private class InteractiveObjectKey
		{
			public int MissionId { get; set; }

			public string ResourceKey { get; set; }
		}
	}
}
