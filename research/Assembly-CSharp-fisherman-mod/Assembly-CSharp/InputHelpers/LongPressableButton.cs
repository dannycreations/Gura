using System;
using UnityEngine;

namespace InputHelpers
{
	public class LongPressableButton
	{
		public LongPressableButton(KeyCode key)
		{
			this._key = key;
		}

		public void Update()
		{
			if (Input.GetKeyDown(this._key))
			{
				this._pressedAt = Time.realtimeSinceStartup;
				this._isLongHold = false;
				this._wasLongPressed = false;
			}
			else if (Input.GetKey(this._key))
			{
				bool flag = Time.realtimeSinceStartup - this._pressedAt > 0.35f;
				this._isLongHold = !this._wasLongPressed && flag;
				if (flag)
				{
					this._wasLongPressed = true;
				}
			}
			else if (Input.GetKeyUp(this._key))
			{
				this._pressedAt = -1f;
				this._isLongHold = false;
			}
		}

		public bool WasReleased
		{
			get
			{
				return Input.GetKeyUp(this._key) && !this._wasLongPressed;
			}
		}

		public bool IsLongHold
		{
			get
			{
				return this._isLongHold;
			}
		}

		private const float LONG_PRESS_DELAY = 0.35f;

		private KeyCode _key;

		private float _pressedAt = -1f;

		private bool _wasLongPressed;

		private bool _isLongHold;
	}
}
