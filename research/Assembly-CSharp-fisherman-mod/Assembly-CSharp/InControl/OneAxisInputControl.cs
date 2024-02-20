using System;
using UnityEngine;

namespace InControl
{
	public class OneAxisInputControl : IInputControl
	{
		public ulong UpdateTick { get; protected set; }

		private void PrepareForUpdate(ulong updateTick)
		{
			if (this.IsNull)
			{
				return;
			}
			if (updateTick < this.pendingTick)
			{
				throw new InvalidOperationException("Cannot be updated with an earlier tick.");
			}
			if (this.pendingCommit && updateTick != this.pendingTick)
			{
				throw new InvalidOperationException("Cannot be updated for a new tick until pending tick is committed.");
			}
			if (updateTick > this.pendingTick)
			{
				this.lastState = this.thisState;
				this.nextState.Reset();
				this.pendingTick = updateTick;
				this.pendingCommit = true;
			}
		}

		public bool UpdateWithState(bool state, ulong updateTick, float deltaTime)
		{
			if (this.IsNull)
			{
				return false;
			}
			this.PrepareForUpdate(updateTick);
			this.nextState.Set(state || this.nextState.State);
			return state;
		}

		public bool UpdateWithValue(float value, ulong updateTick, float deltaTime)
		{
			if (this.IsNull)
			{
				return false;
			}
			this.PrepareForUpdate(updateTick);
			if (Utility.Abs(value) > Utility.Abs(this.nextState.RawValue))
			{
				this.nextState.RawValue = value;
				if (!this.Raw)
				{
					value = Utility.ApplyDeadZone(value, this.lowerDeadZone, this.upperDeadZone);
				}
				this.nextState.Set(value, this.stateThreshold);
				return true;
			}
			return false;
		}

		internal bool UpdateWithRawValue(float value, ulong updateTick, float deltaTime)
		{
			if (this.IsNull)
			{
				return false;
			}
			this.Raw = true;
			this.PrepareForUpdate(updateTick);
			if (Utility.Abs(value) > Utility.Abs(this.nextState.RawValue))
			{
				this.nextState.RawValue = value;
				this.nextState.Set(value, this.stateThreshold);
				return true;
			}
			return false;
		}

		internal void SetValue(float value, ulong updateTick)
		{
			if (this.IsNull)
			{
				return;
			}
			if (updateTick > this.pendingTick)
			{
				this.lastState = this.thisState;
				this.nextState.Reset();
				this.pendingTick = updateTick;
				this.pendingCommit = true;
			}
			this.nextState.RawValue = value;
			this.nextState.Set(value, this.StateThreshold);
		}

		public void ClearInputState()
		{
			this.lastState.Reset();
			this.thisState.Reset();
			this.nextState.Reset();
			this.wasRepeated = false;
			this.clearInputState = true;
		}

		public void Commit()
		{
			if (this.IsNull)
			{
				return;
			}
			this.pendingCommit = false;
			this.thisState = this.nextState;
			if (this.shouldFlushLongPress)
			{
				this.longPressStartTime = -1f;
				this.shouldFlushLongPress = false;
			}
			if (this.shouldFlushFastLongPress)
			{
				this.fastLongPressStartTime = -1f;
				this.shouldFlushFastLongPress = false;
			}
			if (this.clearInputState)
			{
				this.lastState = this.nextState;
				this.UpdateTick = this.pendingTick;
				this.clearInputState = false;
				this.lastCommitFrame = Time.frameCount;
				return;
			}
			bool state = this.lastState.State;
			bool state2 = this.thisState.State;
			if (this.lastCommitFrame != Time.frameCount)
			{
				this.wasRepeated = false;
			}
			if (state && !state2)
			{
				this.nextRepeatTime = 0f;
			}
			else if (state2)
			{
				if (state != state2)
				{
					this.nextRepeatTime = Time.realtimeSinceStartup + this.FirstRepeatDelay;
				}
				else if (Time.realtimeSinceStartup >= this.nextRepeatTime)
				{
					this.wasRepeated = true;
					this.nextRepeatTime = Time.realtimeSinceStartup + this.RepeatDelay;
				}
			}
			if (this.thisState != this.lastState)
			{
				this.UpdateTick = this.pendingTick;
			}
			if (this.IsPressed && this.clickStartTime < 0f)
			{
				this.clickStartTime = Time.realtimeSinceStartup;
			}
			if (this.WasPressed && this.longPressStartTime < 0f)
			{
				this.longPressStartTime = Time.realtimeSinceStartup;
			}
			if (this.WasPressed && this.fastLongPressStartTime < 0f)
			{
				this.fastLongPressStartTime = Time.realtimeSinceStartup;
			}
			if (!this.thisState && !this.lastState)
			{
				this.clickStartTime = -1f;
				this.longPressStartTime = -1f;
				this.fastLongPressStartTime = -1f;
			}
			this.lastCommitFrame = Time.frameCount;
		}

		public void CommitWithState(bool state, ulong updateTick, float deltaTime)
		{
			this.UpdateWithState(state, updateTick, deltaTime);
			this.Commit();
		}

		public void CommitWithValue(float value, ulong updateTick, float deltaTime)
		{
			this.UpdateWithValue(value, updateTick, deltaTime);
			this.Commit();
		}

		internal void CommitWithSides(InputControl negativeSide, InputControl positiveSide, ulong updateTick, float deltaTime)
		{
			this.LowerDeadZone = Mathf.Max(negativeSide.LowerDeadZone, positiveSide.LowerDeadZone);
			this.UpperDeadZone = Mathf.Min(negativeSide.UpperDeadZone, positiveSide.UpperDeadZone);
			this.Raw = negativeSide.Raw || positiveSide.Raw;
			float num = Utility.ValueFromSides(negativeSide.RawValue, positiveSide.RawValue);
			this.CommitWithValue(num, updateTick, deltaTime);
		}

		public bool State
		{
			get
			{
				return this.Enabled && this.thisState.State;
			}
		}

		public bool LastState
		{
			get
			{
				return this.Enabled && this.lastState.State;
			}
		}

		public float Value
		{
			get
			{
				return (!this.Enabled) ? 0f : this.thisState.Value;
			}
		}

		public float LastValue
		{
			get
			{
				return (!this.Enabled) ? 0f : this.lastState.Value;
			}
		}

		public float RawValue
		{
			get
			{
				return (!this.Enabled) ? 0f : this.thisState.RawValue;
			}
		}

		internal float NextRawValue
		{
			get
			{
				return (!this.Enabled) ? 0f : this.nextState.RawValue;
			}
		}

		public bool HasChanged
		{
			get
			{
				return this.Enabled && this.thisState != this.lastState;
			}
		}

		public virtual bool IsPressed
		{
			get
			{
				return this.Enabled && this.thisState.State;
			}
		}

		public virtual bool WasPressed
		{
			get
			{
				return this.Enabled && this.thisState && !this.lastState;
			}
		}

		public virtual bool WasClicked
		{
			get
			{
				return this.clickStartTime > 0f && Mathf.Abs(Time.realtimeSinceStartup - this.clickStartTime) <= this.clickTime && this.WasReleased;
			}
		}

		public virtual bool WasLongPressed
		{
			get
			{
				if (this.longPressStartTime > 0f && Mathf.Abs(Time.realtimeSinceStartup - this.longPressStartTime) >= this.longPressTime)
				{
					this.shouldFlushLongPress = true;
					return true;
				}
				return false;
			}
		}

		public virtual bool WasFastLongPressed
		{
			get
			{
				if (this.fastLongPressStartTime > 0f && Mathf.Abs(Time.realtimeSinceStartup - this.fastLongPressStartTime) >= this.fastLongPressTime)
				{
					this.shouldFlushFastLongPress = true;
					return true;
				}
				return false;
			}
		}

		public virtual bool WasReleased
		{
			get
			{
				return this.Enabled && !this.thisState && this.lastState;
			}
		}

		public virtual bool WasRepeated
		{
			get
			{
				return this.Enabled && this.wasRepeated;
			}
		}

		public float Sensitivity
		{
			get
			{
				return this.sensitivity;
			}
			set
			{
				this.sensitivity = Mathf.Clamp01(value);
			}
		}

		public float LowerDeadZone
		{
			get
			{
				return this.lowerDeadZone;
			}
			set
			{
				this.lowerDeadZone = Mathf.Clamp01(value);
			}
		}

		public float UpperDeadZone
		{
			get
			{
				return this.upperDeadZone;
			}
			set
			{
				this.upperDeadZone = Mathf.Clamp01(value);
			}
		}

		public float StateThreshold
		{
			get
			{
				return this.stateThreshold;
			}
			set
			{
				this.stateThreshold = Mathf.Clamp01(value);
			}
		}

		public bool IsNull
		{
			get
			{
				return object.ReferenceEquals(this, InputControl.Null);
			}
		}

		public static implicit operator bool(OneAxisInputControl instance)
		{
			return instance.State;
		}

		public static implicit operator float(OneAxisInputControl instance)
		{
			return instance.Value;
		}

		private float sensitivity = 1f;

		private float lowerDeadZone = 0.4f;

		private float upperDeadZone = 1f;

		private float stateThreshold;

		private float clickTime = 0.4f;

		private float longPressStartTime = -1f;

		private float longPressTime = 1f;

		private float fastLongPressStartTime = -1f;

		private float fastLongPressTime = 0.5f;

		private float clickStartTime = -1f;

		public float FirstRepeatDelay = 0.8f;

		public float RepeatDelay = 0.1f;

		public bool Raw;

		internal bool Enabled = true;

		private ulong pendingTick;

		private bool pendingCommit;

		private float nextRepeatTime;

		private float lastPressedTime;

		private bool wasRepeated;

		private bool clearInputState;

		private InputControlState lastState;

		private InputControlState nextState;

		private InputControlState thisState;

		private int lastCommitFrame;

		private bool shouldFlushLongPress;

		private bool shouldFlushFastLongPress;
	}
}
