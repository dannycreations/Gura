using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace InControl
{
	public class PlayerAction : OneAxisInputControl
	{
		public PlayerAction(string name, PlayerActionSet owner)
		{
			this.Raw = true;
			this.Name = name;
			this.Owner = owner;
			this.bindings = new ReadOnlyCollection<BindingSource>(this.visibleBindings);
			this.unfilteredBindings = new ReadOnlyCollection<BindingSource>(this.regularBindings);
		}

		public string Name { get; private set; }

		public PlayerActionSet Owner { get; private set; }

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<BindingSourceType> OnLastInputTypeChanged;

		public object UserData { get; set; }

		public void AddDefaultBinding(BindingSource binding, int bindingNumber = 1)
		{
			if (binding == null)
			{
				return;
			}
			if (binding.BoundTo != null)
			{
				throw new InControlException("Binding source is already bound to action " + binding.BoundTo.Name);
			}
			if (!this.defaultBindings.Contains(binding))
			{
				this.defaultBindings.Add(binding);
				binding.BoundTo = this;
			}
			if (!this.regularBindings.Contains(binding))
			{
				this.regularBindings.Add(binding);
				binding.BoundTo = this;
				binding.Number = bindingNumber;
				if (binding.IsValid)
				{
					this.visibleBindings.Add(binding);
				}
			}
		}

		public void AddDefaultBinding(Key key, int bindingNumber = 1)
		{
			this.AddDefaultBinding(new KeyBindingSource(new Key[] { key }), bindingNumber);
		}

		public void AddDefaultBinding(Mouse control)
		{
			this.AddDefaultBinding(new MouseBindingSource(control), 1);
		}

		public void AddDefaultBinding(InputControlType control)
		{
			this.AddDefaultBinding(new DeviceBindingSource(control), 1);
		}

		public bool AddBinding(BindingSource binding)
		{
			if (binding == null)
			{
				return false;
			}
			if (binding.BoundTo != null)
			{
				Debug.LogWarning("Binding source is already bound to action " + binding.BoundTo.Name);
				return false;
			}
			if (this.regularBindings.Contains(binding))
			{
				return false;
			}
			this.regularBindings.Add(binding);
			binding.BoundTo = this;
			if (binding.IsValid)
			{
				this.visibleBindings.Add(binding);
			}
			return true;
		}

		public bool InsertBindingAt(int index, BindingSource binding)
		{
			if (index < 0 || index > this.visibleBindings.Count)
			{
				throw new InControlException("Index is out of range for bindings on this action.");
			}
			if (index == this.visibleBindings.Count)
			{
				return this.AddBinding(binding);
			}
			if (binding == null)
			{
				return false;
			}
			if (binding.BoundTo != null)
			{
				Debug.LogWarning("Binding source is already bound to action " + binding.BoundTo.Name);
				return false;
			}
			if (this.regularBindings.Contains(binding))
			{
				return false;
			}
			int num = ((index != 0) ? this.regularBindings.IndexOf(this.visibleBindings[index]) : 0);
			this.regularBindings.Insert(num, binding);
			binding.BoundTo = this;
			if (binding.IsValid)
			{
				this.visibleBindings.Insert(index, binding);
			}
			return true;
		}

		public bool ReplaceBinding(BindingSource findBinding, BindingSource withBinding)
		{
			if (findBinding == null || withBinding == null)
			{
				return false;
			}
			if (withBinding.BoundTo != null)
			{
				Debug.LogWarning("Binding source is already bound to action " + withBinding.BoundTo.Name);
				return false;
			}
			int num = this.regularBindings.IndexOf(findBinding);
			if (num < 0)
			{
				Debug.LogWarning("Binding source to replace is not present in this action.");
				return false;
			}
			findBinding.BoundTo = null;
			this.regularBindings[num] = withBinding;
			withBinding.BoundTo = this;
			num = this.visibleBindings.IndexOf(findBinding);
			if (num >= 0)
			{
				this.visibleBindings[num] = withBinding;
			}
			return true;
		}

		public bool HasBinding(BindingSource binding)
		{
			if (binding == null)
			{
				return false;
			}
			BindingSource bindingSource = this.FindBinding(binding);
			return !(bindingSource == null) && bindingSource.BoundTo == this;
		}

		internal BindingSource FindBinding(BindingSource binding)
		{
			if (binding == null)
			{
				return null;
			}
			int num = this.regularBindings.IndexOf(binding);
			if (num >= 0)
			{
				return this.regularBindings[num];
			}
			return null;
		}

		internal void FindAndRemoveBinding(BindingSource binding)
		{
			if (binding == null)
			{
				return;
			}
			int num = this.regularBindings.IndexOf(binding);
			if (num >= 0)
			{
				BindingSource bindingSource = this.regularBindings[num];
				if (bindingSource.BoundTo == this)
				{
					bindingSource.BoundTo = null;
					this.regularBindings.RemoveAt(num);
					this.UpdateVisibleBindings();
				}
			}
		}

		internal int CountBindingsOfType(BindingSourceType bindingSourceType)
		{
			int num = 0;
			int count = this.regularBindings.Count;
			for (int i = 0; i < count; i++)
			{
				BindingSource bindingSource = this.regularBindings[i];
				if (bindingSource.BoundTo == this && bindingSource.BindingSourceType == bindingSourceType)
				{
					num++;
				}
			}
			return num;
		}

		internal void RemoveFirstBindingOfType(BindingSourceType bindingSourceType)
		{
			int count = this.regularBindings.Count;
			for (int i = 0; i < count; i++)
			{
				BindingSource bindingSource = this.regularBindings[i];
				if (bindingSource.BoundTo == this && bindingSource.BindingSourceType == bindingSourceType)
				{
					bindingSource.BoundTo = null;
					this.regularBindings.RemoveAt(i);
					return;
				}
			}
		}

		internal int IndexOfFirstInvalidBinding()
		{
			int count = this.regularBindings.Count;
			for (int i = 0; i < count; i++)
			{
				if (!this.regularBindings[i].IsValid)
				{
					return i;
				}
			}
			return -1;
		}

		public void RemoveBinding(BindingSource binding)
		{
			if (binding == null)
			{
				return;
			}
			if (binding.BoundTo != this)
			{
				throw new InControlException("Cannot remove a binding source not bound to this action.");
			}
			binding.BoundTo = null;
		}

		public void RemoveBindingAt(int index)
		{
			if (index < 0 || index >= this.regularBindings.Count)
			{
				throw new InControlException("Index is out of range for bindings on this action.");
			}
			this.regularBindings[index].BoundTo = null;
		}

		public void ClearBindings()
		{
			int count = this.regularBindings.Count;
			for (int i = 0; i < count; i++)
			{
				this.regularBindings[i].BoundTo = null;
			}
			this.regularBindings.Clear();
			this.visibleBindings.Clear();
		}

		public void ResetBindings()
		{
			this.ClearBindings();
			this.regularBindings.AddRange(this.defaultBindings);
			int count = this.regularBindings.Count;
			for (int i = 0; i < count; i++)
			{
				BindingSource bindingSource = this.regularBindings[i];
				bindingSource.BoundTo = this;
				if (bindingSource.IsValid)
				{
					this.visibleBindings.Add(bindingSource);
				}
			}
		}

		public void ListenForBinding()
		{
			this.ListenForBindingReplacing(null);
		}

		public void ListenForBindingReplacing(BindingSource binding)
		{
			BindingListenOptions bindingListenOptions = this.ListenOptions ?? this.Owner.ListenOptions;
			bindingListenOptions.ReplaceBinding = binding;
			this.Owner.listenWithAction = this;
			int num = PlayerAction.bindingSourceListeners.Length;
			for (int i = 0; i < num; i++)
			{
				PlayerAction.bindingSourceListeners[i].Reset();
			}
		}

		public void StopListeningForBinding()
		{
			if (this.IsListeningForBinding)
			{
				this.Owner.listenWithAction = null;
			}
		}

		public bool IsListeningForBinding
		{
			get
			{
				return this.Owner.listenWithAction == this;
			}
		}

		public ReadOnlyCollection<BindingSource> Bindings
		{
			get
			{
				return this.bindings;
			}
		}

		public ReadOnlyCollection<BindingSource> UnfilteredBindings
		{
			get
			{
				return this.unfilteredBindings;
			}
		}

		private void RemoveOrphanedBindings()
		{
			int count = this.regularBindings.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				if (this.regularBindings[i].BoundTo != this)
				{
					this.regularBindings.RemoveAt(i);
				}
			}
		}

		internal void Update(ulong updateTick, float deltaTime, InputDevice device)
		{
			this.Device = device;
			this.UpdateBindings(updateTick, deltaTime);
			this.DetectBindings();
		}

		public bool IsBlocked
		{
			get
			{
				return this._isBlocked;
			}
		}

		public void Block()
		{
			this._isBlocked = true;
		}

		public void UnBlock()
		{
			this._isBlocked = false;
		}

		public void Activate()
		{
			this._excludedBindingsUpdate.Clear();
		}

		public void DeactivateSource(Type sourceType)
		{
			this._excludedBindingsUpdate.Add(sourceType);
			base.ClearInputState();
		}

		public void Deactivate()
		{
			this._excludedBindingsUpdate.AddRange(new List<Type>
			{
				typeof(DeviceBindingSource),
				typeof(MouseBindingSource),
				typeof(KeyBindingSource)
			});
			base.ClearInputState();
		}

		private void UpdateBindings(ulong updateTick, float deltaTime)
		{
			BindingSourceType bindingSourceType = this.LastInputType;
			ulong num = this.LastInputTypeChangedTick;
			ulong updateTick2 = base.UpdateTick;
			int count = this.regularBindings.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				BindingSource bindingSource = this.regularBindings[i];
				if (!this._excludedBindingsUpdate.Contains(bindingSource.GetType()))
				{
					if (bindingSource.BoundTo != this)
					{
						this.regularBindings.RemoveAt(i);
						this.visibleBindings.Remove(bindingSource);
					}
					else
					{
						float value = bindingSource.GetValue(this.Device);
						if (base.UpdateWithValue(value, updateTick, deltaTime))
						{
							bindingSourceType = bindingSource.BindingSourceType;
							num = updateTick;
						}
					}
				}
			}
			base.Commit();
			this.Enabled = this.Owner.Enabled;
			if (num > this.LastInputTypeChangedTick && (bindingSourceType != BindingSourceType.MouseBindingSource || Utility.Abs(base.LastValue - base.Value) >= MouseBindingSource.JitterThreshold))
			{
				bool flag = bindingSourceType != this.LastInputType;
				this.LastInputType = bindingSourceType;
				this.LastInputTypeChangedTick = num;
				if (this.OnLastInputTypeChanged != null && flag)
				{
					this.OnLastInputTypeChanged(bindingSourceType);
				}
			}
			if (base.UpdateTick > updateTick2)
			{
				this.activeDevice = ((!this.LastInputTypeIsDevice) ? null : this.Device);
			}
		}

		private void DetectBindings()
		{
			if (this.IsListeningForBinding)
			{
				BindingSource bindingSource = null;
				BindingListenOptions bindingListenOptions = this.ListenOptions ?? this.Owner.ListenOptions;
				int num = PlayerAction.bindingSourceListeners.Length;
				for (int i = 0; i < num; i++)
				{
					bindingSource = PlayerAction.bindingSourceListeners[i].Listen(bindingListenOptions, this.device);
					if (bindingSource != null)
					{
						break;
					}
				}
				if (bindingSource == null)
				{
					return;
				}
				Func<PlayerAction, BindingSource, bool> onBindingFound = bindingListenOptions.OnBindingFound;
				if (onBindingFound != null && !onBindingFound(this, bindingSource))
				{
					return;
				}
				if (this.HasBinding(bindingSource))
				{
					Action<PlayerAction, BindingSource, BindingSourceRejectionType> onBindingRejected = bindingListenOptions.OnBindingRejected;
					if (onBindingRejected != null)
					{
						onBindingRejected(this, bindingSource, BindingSourceRejectionType.DuplicateBindingOnAction);
					}
					return;
				}
				if (bindingListenOptions.UnsetDuplicateBindingsOnSet)
				{
					this.Owner.RemoveBinding(bindingSource);
				}
				if (!bindingListenOptions.AllowDuplicateBindingsPerSet && this.Owner.HasBinding(bindingSource))
				{
					Action<PlayerAction, BindingSource, BindingSourceRejectionType> onBindingRejected2 = bindingListenOptions.OnBindingRejected;
					if (onBindingRejected2 != null)
					{
						onBindingRejected2(this, bindingSource, BindingSourceRejectionType.DuplicateBindingOnActionSet);
					}
					return;
				}
				this.StopListeningForBinding();
				if (bindingListenOptions.ReplaceBinding == null)
				{
					if (bindingListenOptions.MaxAllowedBindingsPerType > 0U)
					{
						while ((long)this.CountBindingsOfType(bindingSource.BindingSourceType) >= (long)((ulong)bindingListenOptions.MaxAllowedBindingsPerType))
						{
							this.RemoveFirstBindingOfType(bindingSource.BindingSourceType);
						}
					}
					else if (bindingListenOptions.MaxAllowedBindings > 0U)
					{
						while ((long)this.regularBindings.Count >= (long)((ulong)bindingListenOptions.MaxAllowedBindings))
						{
							int num2 = Mathf.Max(0, this.IndexOfFirstInvalidBinding());
							this.regularBindings.RemoveAt(num2);
						}
					}
					this.AddBinding(bindingSource);
				}
				else
				{
					this.ReplaceBinding(bindingListenOptions.ReplaceBinding, bindingSource);
				}
				this.UpdateVisibleBindings();
				Action<PlayerAction, BindingSource> onBindingAdded = bindingListenOptions.OnBindingAdded;
				if (onBindingAdded != null)
				{
					onBindingAdded(this, bindingSource);
				}
			}
		}

		private void UpdateVisibleBindings()
		{
			this.visibleBindings.Clear();
			int count = this.regularBindings.Count;
			for (int i = 0; i < count; i++)
			{
				BindingSource bindingSource = this.regularBindings[i];
				if (bindingSource.IsValid)
				{
					this.visibleBindings.Add(bindingSource);
				}
			}
		}

		internal InputDevice Device
		{
			get
			{
				if (this.device == null)
				{
					this.device = this.Owner.Device;
					this.UpdateVisibleBindings();
				}
				return this.device;
			}
			set
			{
				if (this.device != value)
				{
					this.device = value;
					this.UpdateVisibleBindings();
				}
			}
		}

		public InputDevice ActiveDevice
		{
			get
			{
				return (this.activeDevice != null) ? this.activeDevice : InputDevice.Null;
			}
		}

		private bool LastInputTypeIsDevice
		{
			get
			{
				return this.LastInputType == BindingSourceType.DeviceBindingSource || this.LastInputType == BindingSourceType.UnknownDeviceBindingSource;
			}
		}

		[Obsolete("Please set this property on device controls directly. It does nothing here.")]
		public new float LowerDeadZone
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		[Obsolete("Please set this property on device controls directly. It does nothing here.")]
		public new float UpperDeadZone
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		internal void Load(BinaryReader reader)
		{
			this.ClearBindings();
			int num = reader.ReadInt32();
			int i = 0;
			while (i < num)
			{
				BindingSourceType bindingSourceType = (BindingSourceType)reader.ReadInt32();
				BindingSource bindingSource;
				switch (bindingSourceType)
				{
				case BindingSourceType.None:
					break;
				case BindingSourceType.DeviceBindingSource:
					bindingSource = new DeviceBindingSource();
					goto IL_81;
				case BindingSourceType.KeyBindingSource:
					bindingSource = new KeyBindingSource();
					goto IL_81;
				case BindingSourceType.MouseBindingSource:
					bindingSource = new MouseBindingSource();
					goto IL_81;
				case BindingSourceType.UnknownDeviceBindingSource:
					bindingSource = new UnknownDeviceBindingSource();
					goto IL_81;
				default:
					throw new InControlException("Don't know how to load BindingSourceType: " + bindingSourceType);
				}
				IL_90:
				i++;
				continue;
				IL_81:
				bindingSource.Load(reader);
				this.AddBinding(bindingSource);
				goto IL_90;
			}
		}

		internal void Save(BinaryWriter writer)
		{
			this.RemoveOrphanedBindings();
			writer.Write(this.Name);
			int count = this.regularBindings.Count;
			writer.Write(count);
			for (int i = 0; i < count; i++)
			{
				BindingSource bindingSource = this.regularBindings[i];
				writer.Write((int)bindingSource.BindingSourceType);
				bindingSource.Save(writer);
			}
		}

		public BindingListenOptions ListenOptions;

		public BindingSourceType LastInputType;

		public ulong LastInputTypeChangedTick;

		private List<BindingSource> defaultBindings = new List<BindingSource>();

		private List<BindingSource> regularBindings = new List<BindingSource>();

		private List<BindingSource> visibleBindings = new List<BindingSource>();

		private readonly ReadOnlyCollection<BindingSource> bindings;

		private readonly ReadOnlyCollection<BindingSource> unfilteredBindings;

		private static readonly BindingSourceListener[] bindingSourceListeners = new BindingSourceListener[]
		{
			new DeviceBindingSourceListener(),
			new UnknownDeviceBindingSourceListener(),
			new KeyBindingSourceListener(),
			new MouseBindingSourceListener()
		};

		protected bool _isBlocked;

		protected List<Type> _excludedBindingsUpdate = new List<Type>();

		private InputDevice device;

		private InputDevice activeDevice;
	}
}
