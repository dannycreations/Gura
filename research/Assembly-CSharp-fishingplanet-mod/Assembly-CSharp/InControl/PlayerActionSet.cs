using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace InControl
{
	public abstract class PlayerActionSet
	{
		protected PlayerActionSet()
		{
			this.Enabled = true;
			this.Device = null;
			this.IncludeDevices = new List<InputDevice>();
			this.ExcludeDevices = new List<InputDevice>();
			this.Actions = new ReadOnlyCollection<PlayerAction>(this.actions);
			InputManager.AttachPlayerActionSet(this);
		}

		public InputDevice Device { get; set; }

		public List<InputDevice> IncludeDevices { get; private set; }

		public List<InputDevice> ExcludeDevices { get; private set; }

		public ReadOnlyCollection<PlayerAction> Actions { get; private set; }

		public ulong UpdateTick { get; protected set; }

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<BindingSourceType> OnLastInputTypeChanged;

		public bool Enabled { get; set; }

		public object UserData { get; set; }

		public void Destroy()
		{
			this.OnLastInputTypeChanged = null;
			InputManager.DetachPlayerActionSet(this);
		}

		protected PlayerAction CreatePlayerAction(string name)
		{
			PlayerAction playerAction = new PlayerAction(name, this);
			this.AddPlayerAction(playerAction);
			return playerAction;
		}

		internal void AddPlayerAction(PlayerAction action)
		{
			action.Device = this.FindActiveDevice();
			if (this.actionsByName.ContainsKey(action.Name))
			{
				throw new InControlException("Action '" + action.Name + "' already exists in this set.");
			}
			this.actions.Add(action);
			this.actionsByName.Add(action.Name, action);
		}

		protected PlayerOneAxisAction CreateOneAxisPlayerAction(PlayerAction negativeAction, PlayerAction positiveAction)
		{
			PlayerOneAxisAction playerOneAxisAction = new PlayerOneAxisAction(negativeAction, positiveAction);
			this.oneAxisActions.Add(playerOneAxisAction);
			return playerOneAxisAction;
		}

		protected PlayerTwoAxisAction CreateTwoAxisPlayerAction(PlayerAction negativeXAction, PlayerAction positiveXAction, PlayerAction negativeYAction, PlayerAction positiveYAction)
		{
			PlayerTwoAxisAction playerTwoAxisAction = new PlayerTwoAxisAction(negativeXAction, positiveXAction, negativeYAction, positiveYAction);
			this.twoAxisActions.Add(playerTwoAxisAction);
			return playerTwoAxisAction;
		}

		public PlayerAction this[string actionName]
		{
			get
			{
				PlayerAction playerAction;
				if (this.actionsByName.TryGetValue(actionName, out playerAction))
				{
					return playerAction;
				}
				throw new KeyNotFoundException("Action '" + actionName + "' does not exist in this action set.");
			}
		}

		public PlayerAction GetPlayerActionByName(string actionName)
		{
			PlayerAction playerAction;
			if (this.actionsByName.TryGetValue(actionName, out playerAction))
			{
				return playerAction;
			}
			return null;
		}

		internal void Update(ulong updateTick, float deltaTime)
		{
			InputDevice inputDevice = this.Device ?? this.FindActiveDevice();
			BindingSourceType bindingSourceType = this.LastInputType;
			ulong num = this.LastInputTypeChangedTick;
			int count = this.actions.Count;
			for (int i = 0; i < count; i++)
			{
				PlayerAction playerAction = this.actions[i];
				playerAction.Update(updateTick, deltaTime, inputDevice);
				if (playerAction.UpdateTick > this.UpdateTick)
				{
					this.UpdateTick = playerAction.UpdateTick;
					this.activeDevice = playerAction.ActiveDevice;
				}
				if (playerAction.LastInputTypeChangedTick > num)
				{
					bindingSourceType = playerAction.LastInputType;
					num = playerAction.LastInputTypeChangedTick;
				}
			}
			int count2 = this.oneAxisActions.Count;
			for (int j = 0; j < count2; j++)
			{
				this.oneAxisActions[j].Update(updateTick, deltaTime);
			}
			int count3 = this.twoAxisActions.Count;
			for (int k = 0; k < count3; k++)
			{
				this.twoAxisActions[k].Update(updateTick, deltaTime);
			}
			if (num > this.LastInputTypeChangedTick)
			{
				bool flag = bindingSourceType != this.LastInputType;
				this.LastInputType = bindingSourceType;
				this.LastInputTypeChangedTick = num;
				if (this.OnLastInputTypeChanged != null && flag)
				{
					this.OnLastInputTypeChanged(bindingSourceType);
				}
			}
		}

		public void Reset()
		{
			int count = this.actions.Count;
			for (int i = 0; i < count; i++)
			{
				this.actions[i].ResetBindings();
			}
		}

		private InputDevice FindActiveDevice()
		{
			bool flag = this.IncludeDevices.Count > 0;
			bool flag2 = this.ExcludeDevices.Count > 0;
			if (flag || flag2)
			{
				InputDevice inputDevice = InputDevice.Null;
				int count = InputManager.Devices.Count;
				for (int i = 0; i < count; i++)
				{
					InputDevice inputDevice2 = InputManager.Devices[i];
					if (inputDevice2 != inputDevice && inputDevice2.LastChangedAfter(inputDevice))
					{
						if (!flag2 || !this.ExcludeDevices.Contains(inputDevice2))
						{
							if (!flag || this.IncludeDevices.Contains(inputDevice2))
							{
								inputDevice = inputDevice2;
							}
						}
					}
				}
				return inputDevice;
			}
			return InputManager.ActiveDevice;
		}

		public void ClearInputState()
		{
			int count = this.actions.Count;
			for (int i = 0; i < count; i++)
			{
				this.actions[i].ClearInputState();
			}
			int count2 = this.oneAxisActions.Count;
			for (int j = 0; j < count2; j++)
			{
				this.oneAxisActions[j].ClearInputState();
			}
			int count3 = this.twoAxisActions.Count;
			for (int k = 0; k < count3; k++)
			{
				this.twoAxisActions[k].ClearInputState();
			}
		}

		internal bool HasBinding(BindingSource binding)
		{
			if (binding == null)
			{
				return false;
			}
			int count = this.actions.Count;
			for (int i = 0; i < count; i++)
			{
				if (this.actions[i].HasBinding(binding))
				{
					return true;
				}
			}
			return false;
		}

		internal void RemoveBinding(BindingSource binding)
		{
			if (binding == null)
			{
				return;
			}
			int count = this.actions.Count;
			for (int i = 0; i < count; i++)
			{
				this.actions[i].FindAndRemoveBinding(binding);
			}
		}

		public BindingListenOptions ListenOptions
		{
			get
			{
				return this.listenOptions;
			}
			set
			{
				this.listenOptions = value ?? new BindingListenOptions();
			}
		}

		public InputDevice ActiveDevice
		{
			get
			{
				return (this.activeDevice != null) ? this.activeDevice : InputDevice.Null;
			}
		}

		public string Save()
		{
			string text;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8))
				{
					binaryWriter.Write(66);
					binaryWriter.Write(73);
					binaryWriter.Write(78);
					binaryWriter.Write(68);
					binaryWriter.Write(1);
					int count = this.actions.Count;
					binaryWriter.Write(count);
					for (int i = 0; i < count; i++)
					{
						this.actions[i].Save(binaryWriter);
					}
				}
				text = Convert.ToBase64String(memoryStream.ToArray());
			}
			return text;
		}

		public void Load(string data)
		{
			if (data == null)
			{
				return;
			}
			try
			{
				using (MemoryStream memoryStream = new MemoryStream(Convert.FromBase64String(data)))
				{
					using (BinaryReader binaryReader = new BinaryReader(memoryStream))
					{
						if (binaryReader.ReadUInt32() != 1145981250U)
						{
							throw new Exception("Unknown data format.");
						}
						if (binaryReader.ReadUInt16() != 1)
						{
							throw new Exception("Unknown data version.");
						}
						int num = binaryReader.ReadInt32();
						for (int i = 0; i < num; i++)
						{
							PlayerAction playerAction;
							if (this.actionsByName.TryGetValue(binaryReader.ReadString(), out playerAction))
							{
								playerAction.Load(binaryReader);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Provided state could not be loaded:\n" + ex.Message);
				this.Reset();
			}
		}

		public BindingSourceType LastInputType;

		public ulong LastInputTypeChangedTick;

		protected List<PlayerAction> actions = new List<PlayerAction>();

		protected List<PlayerOneAxisAction> oneAxisActions = new List<PlayerOneAxisAction>();

		protected List<PlayerTwoAxisAction> twoAxisActions = new List<PlayerTwoAxisAction>();

		protected Dictionary<string, PlayerAction> actionsByName = new Dictionary<string, PlayerAction>();

		private BindingListenOptions listenOptions = new BindingListenOptions();

		internal PlayerAction listenWithAction;

		private InputDevice activeDevice;
	}
}
