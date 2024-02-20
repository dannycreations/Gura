using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class MissionInteractiveObject : InteractiveObject
	{
		public MissionInteractiveObject()
		{
			this.InteractionsHistory = new Dictionary<string, MissionInteractiveObject.AllowedInteraction>();
			this.AllowedInteractions = new Dictionary<string, MissionInteractiveObject.AllowedInteraction>();
		}

		[JsonProperty]
		public int MissionId { get; set; }

		[JsonProperty]
		public string ResourceKey { get; set; }

		[JsonProperty]
		public int Version { get; set; }

		[JsonIgnore]
		public Dictionary<string, MissionInteractiveObject.AllowedInteraction> InteractionsHistory { get; private set; }

		[JsonIgnore]
		public Dictionary<string, MissionInteractiveObject.AllowedInteraction> AllowedInteractions { get; private set; }

		[JsonIgnore]
		public MissionInteractiveObject.AllowedInteraction CurInteraction { get; private set; }

		public string CurrentState { get; set; }

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event InteractiveObjectEventHandler ActiveStateChanged;

		private void OnActiveStateChanged()
		{
			if (this.ActiveStateChanged != null)
			{
				this.ActiveStateChanged(this);
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event InteractiveObjectInteractionChangedEventHandler InteractionChanged;

		private void OnInteractionChanged(string interaction, bool added)
		{
			if (this.InteractionChanged != null)
			{
				this.InteractionChanged(this, interaction, added);
			}
		}

		public void ProcessMessage(HintMessage message)
		{
			this.ProcessMessage(message.EventType, message);
		}

		public void ProcessMessage(MissionEventType eventType, HintMessage message)
		{
			if (eventType != MissionEventType.MissionTaskHintMessageAdded)
			{
				if (eventType == MissionEventType.MissionTaskHintMessageRemoved)
				{
					if (message.ActiveState != null)
					{
						this.CurrentState = null;
						this.OnActiveStateChanged();
					}
					if (message.AllowInteraction != null)
					{
						this.AllowedInteractions.Remove(message.AllowInteraction);
						if (this.AllowedInteractions.Count == 0)
						{
							this.CurInteraction = null;
						}
						else if (this.CurInteraction == null || !this.AllowedInteractions.ContainsKey(this.CurInteraction.Name))
						{
							this.CurInteraction = this.AllowedInteractions.ElementAt(0).Value;
						}
						this.OnInteractionChanged(message.AllowInteraction, false);
					}
				}
			}
			else
			{
				if (message.ActiveState != null)
				{
					this.CurrentState = message.ActiveState;
					this.OnActiveStateChanged();
				}
				if (message.AllowInteraction != null)
				{
					this.CurInteraction = new MissionInteractiveObject.AllowedInteraction
					{
						Name = message.AllowInteraction,
						BeforeMessage = message.Title,
						AlternateMessage = message.Tooltip,
						InteractedMessage = message.Description,
						ExecutionNextTime = message.ExecutionNextTime,
						IsDisabled = message.IsDisabled
					};
					this.InteractionsHistory[message.AllowInteraction] = this.CurInteraction;
					this.AllowedInteractions[message.AllowInteraction] = this.CurInteraction;
					this.OnInteractionChanged(message.AllowInteraction, true);
				}
			}
		}

		public class AllowedInteraction : ICloneable
		{
			public string Name { get; set; }

			public string BeforeMessage { get; set; }

			public string AlternateMessage { get; set; }

			public string InteractedMessage { get; set; }

			public DateTime? ExecutionNextTime { get; set; }

			public bool IsDisabled { get; set; }

			public object Clone()
			{
				return base.MemberwiseClone();
			}
		}
	}
}
