using System;
using UnityEngine;

namespace InControl
{
	public class UnityKeyCodeAxisSource : InputControlSource
	{
		public UnityKeyCodeAxisSource()
		{
		}

		public UnityKeyCodeAxisSource(KeyCode negativeKeyCode, KeyCode positiveKeyCode)
		{
			this.NegativeKeyCode = negativeKeyCode;
			this.PositiveKeyCode = positiveKeyCode;
		}

		public float GetValue(InputDevice inputDevice)
		{
			int num = 0;
			if (Input.GetKey(this.NegativeKeyCode))
			{
				num--;
			}
			if (Input.GetKey(this.PositiveKeyCode))
			{
				num++;
			}
			return (float)num;
		}

		public bool GetState(InputDevice inputDevice)
		{
			return Utility.IsNotZero(this.GetValue(inputDevice));
		}

		public KeyCode NegativeKeyCode;

		public KeyCode PositiveKeyCode;
	}
}
