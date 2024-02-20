using System;
using UnityEngine;

namespace InControl
{
	public class TwoAxisInputControl : IInputControl
	{
		public TwoAxisInputControl()
		{
			this.Left = new OneAxisInputControl();
			this.Right = new OneAxisInputControl();
			this.Up = new OneAxisInputControl();
			this.Down = new OneAxisInputControl();
		}

		public virtual float X { get; protected set; }

		public virtual float Y { get; protected set; }

		public OneAxisInputControl Left { get; protected set; }

		public OneAxisInputControl Right { get; protected set; }

		public OneAxisInputControl Up { get; protected set; }

		public OneAxisInputControl Down { get; protected set; }

		public ulong UpdateTick { get; protected set; }

		public void ClearInputState()
		{
			if (this.Left != null)
			{
				this.Left.ClearInputState();
			}
			if (this.Right != null)
			{
				this.Right.ClearInputState();
			}
			if (this.Up != null)
			{
				this.Up.ClearInputState();
			}
			if (this.Down != null)
			{
				this.Down.ClearInputState();
			}
			this.lastState = false;
			this.lastValue = Vector2.zero;
			this.thisState = false;
			this.thisValue = Vector2.zero;
			this.X = 0f;
			this.Y = 0f;
			this.clearInputState = true;
		}

		public void Filter(TwoAxisInputControl twoAxisInputControl, float deltaTime)
		{
			this.UpdateWithAxes(twoAxisInputControl.X, twoAxisInputControl.Y, InputManager.CurrentTick, deltaTime);
		}

		internal void UpdateWithAxes(float x, float y, ulong updateTick, float deltaTime)
		{
			this.lastState = this.thisState;
			this.lastValue = this.thisValue;
			this.thisValue = ((!this.Raw) ? Utility.ApplyCircularDeadZone(x, y, this.LowerDeadZone, this.UpperDeadZone) : new Vector2(x, y));
			this.X = this.thisValue.x;
			this.Y = this.thisValue.y;
			this.Left.CommitWithValue(Mathf.Max(0f, -this.X), updateTick, deltaTime);
			this.Right.CommitWithValue(Mathf.Max(0f, this.X), updateTick, deltaTime);
			if (InputManager.InvertYAxis)
			{
				this.Up.CommitWithValue(Mathf.Max(0f, -this.Y), updateTick, deltaTime);
				this.Down.CommitWithValue(Mathf.Max(0f, this.Y), updateTick, deltaTime);
			}
			else
			{
				this.Up.CommitWithValue(Mathf.Max(0f, this.Y), updateTick, deltaTime);
				this.Down.CommitWithValue(Mathf.Max(0f, -this.Y), updateTick, deltaTime);
			}
			this.thisState = this.Up.State || this.Down.State || this.Left.State || this.Right.State;
			if (this.clearInputState)
			{
				this.lastState = this.thisState;
				this.lastValue = this.thisValue;
				this.clearInputState = false;
				this.HasChanged = false;
				return;
			}
			if (this.thisValue != this.lastValue)
			{
				this.UpdateTick = updateTick;
				this.HasChanged = true;
			}
			else
			{
				this.HasChanged = false;
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
				this.Left.Sensitivity = this.sensitivity;
				this.Right.Sensitivity = this.sensitivity;
				this.Up.Sensitivity = this.sensitivity;
				this.Down.Sensitivity = this.sensitivity;
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
				this.Left.StateThreshold = this.stateThreshold;
				this.Right.StateThreshold = this.stateThreshold;
				this.Up.StateThreshold = this.stateThreshold;
				this.Down.StateThreshold = this.stateThreshold;
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
				this.Left.LowerDeadZone = this.lowerDeadZone;
				this.Right.LowerDeadZone = this.lowerDeadZone;
				this.Up.LowerDeadZone = this.lowerDeadZone;
				this.Down.LowerDeadZone = this.lowerDeadZone;
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
				this.Left.UpperDeadZone = this.upperDeadZone;
				this.Right.UpperDeadZone = this.upperDeadZone;
				this.Up.UpperDeadZone = this.upperDeadZone;
				this.Down.UpperDeadZone = this.upperDeadZone;
			}
		}

		public bool State
		{
			get
			{
				return this.thisState;
			}
		}

		public bool LastState
		{
			get
			{
				return this.lastState;
			}
		}

		public Vector2 Value
		{
			get
			{
				return this.thisValue;
			}
		}

		public Vector2 LastValue
		{
			get
			{
				return this.lastValue;
			}
		}

		public Vector2 Vector
		{
			get
			{
				return this.thisValue;
			}
		}

		public bool HasChanged { get; protected set; }

		public bool IsPressed
		{
			get
			{
				return this.thisState;
			}
		}

		public bool WasPressed
		{
			get
			{
				return this.thisState && !this.lastState;
			}
		}

		public bool WasReleased
		{
			get
			{
				return !this.thisState && this.lastState;
			}
		}

		public float Angle
		{
			get
			{
				return Utility.VectorToAngle(this.thisValue);
			}
		}

		public static implicit operator bool(TwoAxisInputControl instance)
		{
			return instance.thisState;
		}

		public static implicit operator Vector2(TwoAxisInputControl instance)
		{
			return instance.thisValue;
		}

		public static implicit operator Vector3(TwoAxisInputControl instance)
		{
			return instance.thisValue;
		}

		public static readonly TwoAxisInputControl Null = new TwoAxisInputControl();

		private float sensitivity = 1f;

		private float lowerDeadZone;

		private float upperDeadZone = 1f;

		private float stateThreshold;

		public bool Raw;

		private bool thisState;

		private bool lastState;

		private Vector2 thisValue;

		private Vector2 lastValue;

		private bool clearInputState;
	}
}
