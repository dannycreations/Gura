using System;
using UnityEngine;

namespace InputHelpers
{
	public class AxisToButtons
	{
		public AxisToButtons(string axisName, float longPressDelay = 0.35f, float pressEnoughValue = 0.5f)
		{
			this._axisName = axisName;
			this._longPressDelay = longPressDelay;
			this._pressEnoughValue = pressEnoughValue;
		}

		public void Update()
		{
			this._wasLongPressed = false;
			this._wasReleased = false;
			this._isKeyDown = false;
			float axisRaw = Input.GetAxisRaw(this._axisName);
			AxisToButtons.State state = AxisToButtons.State.Zero;
			if (Mathf.Abs(axisRaw) > this._pressEnoughValue)
			{
				state = ((axisRaw <= 0f) ? AxisToButtons.State.Min : AxisToButtons.State.Max);
			}
			this._prevState = this._curState;
			if (state != this._curState)
			{
				if (state != AxisToButtons.State.Zero)
				{
					this._pressedAt = Time.realtimeSinceStartup;
					this._isKeyDown = true;
					this._downCounter = 0;
				}
				else
				{
					this._wasLongPressed = Time.realtimeSinceStartup - this._pressedAt > this._longPressDelay;
					this._pressedAt = -1f;
					this._wasReleased = true;
				}
				this._curState = state;
			}
			else if (this._pressedAt > 0f)
			{
				int num = (int)((Time.realtimeSinceStartup - this._pressedAt) / this._longPressDelay);
				if (num != this._downCounter)
				{
					this._downCounter = num;
					this._isKeyDown = true;
				}
			}
		}

		public bool IsMinDown
		{
			get
			{
				return this.IsButtonDown(false);
			}
		}

		public bool IsMaxDown
		{
			get
			{
				return this.IsButtonDown(true);
			}
		}

		public bool IsMinPressed
		{
			get
			{
				return this.IsButtonPressed(false);
			}
		}

		public bool IsMaxPressed
		{
			get
			{
				return this.IsButtonPressed(true);
			}
		}

		public bool WasMinReleased
		{
			get
			{
				return this.WasButtonReleased(false);
			}
		}

		public bool WasMaxReleased
		{
			get
			{
				return this.WasButtonReleased(true);
			}
		}

		public bool WasMinShortReleased
		{
			get
			{
				return this.WasButtonShortReleased(false);
			}
		}

		public bool WasMaxShortReleased
		{
			get
			{
				return this.WasButtonShortReleased(true);
			}
		}

		public bool WasMinLongPressed
		{
			get
			{
				return this.WasButtonLongPressed(false);
			}
		}

		public bool WasMaxLongPressed
		{
			get
			{
				return this.WasButtonLongPressed(true);
			}
		}

		private bool IsButtonDown(bool max)
		{
			return this._isKeyDown && ((!max) ? (this._curState == AxisToButtons.State.Min) : (this._curState == AxisToButtons.State.Max));
		}

		private bool IsButtonPressed(bool max)
		{
			return this._curState != AxisToButtons.State.Zero && ((!max) ? (this._curState == AxisToButtons.State.Min) : (this._curState == AxisToButtons.State.Max));
		}

		private bool WasButtonReleased(bool max)
		{
			return this._curState == AxisToButtons.State.Zero && this._curState != this._prevState && ((!max) ? (this._prevState == AxisToButtons.State.Min) : (this._prevState == AxisToButtons.State.Max));
		}

		private bool WasButtonShortReleased(bool max)
		{
			return !this._wasLongPressed && this._curState == AxisToButtons.State.Zero && this._curState != this._prevState && ((!max) ? (this._prevState == AxisToButtons.State.Min) : (this._prevState == AxisToButtons.State.Max));
		}

		private bool WasButtonLongPressed(bool max)
		{
			return this._wasLongPressed && ((!max) ? (this._prevState == AxisToButtons.State.Min) : (this._prevState == AxisToButtons.State.Max));
		}

		private const float LONG_PRESS_DELAY = 0.35f;

		private const float PRESS_ENOUGH_VALUE = 0.5f;

		private readonly float _longPressDelay;

		private readonly float _pressEnoughValue;

		private float _pressedAt = -1f;

		private bool _wasReleased;

		private bool _wasLongPressed;

		private bool _isKeyDown;

		private int _downCounter;

		private string _axisName;

		private AxisToButtons.State _curState;

		private AxisToButtons.State _prevState;

		private enum State
		{
			Zero,
			Max,
			Min
		}
	}
}
