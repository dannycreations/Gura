using System;
using InControl;
using UnityEngine;

namespace VirtualDeviceExample
{
	public class VirtualDevice : InputDevice
	{
		public VirtualDevice()
			: base("Virtual Controller")
		{
			base.AddControl(InputControlType.LeftStickLeft, "Left Stick Left");
			base.AddControl(InputControlType.LeftStickRight, "Left Stick Right");
			base.AddControl(InputControlType.LeftStickUp, "Left Stick Up");
			base.AddControl(InputControlType.LeftStickDown, "Left Stick Down");
			base.AddControl(InputControlType.RightStickLeft, "Right Stick Left");
			base.AddControl(InputControlType.RightStickRight, "Right Stick Right");
			base.AddControl(InputControlType.RightStickUp, "Right Stick Up");
			base.AddControl(InputControlType.RightStickDown, "Right Stick Down");
			base.AddControl(InputControlType.Action1, "A");
			base.AddControl(InputControlType.Action2, "B");
			base.AddControl(InputControlType.Action3, "X");
			base.AddControl(InputControlType.Action4, "Y");
		}

		public override void Update(ulong updateTick, float deltaTime)
		{
			Vector2 vectorFromKeyboard = this.GetVectorFromKeyboard(deltaTime, true);
			base.UpdateLeftStickWithValue(vectorFromKeyboard, updateTick, deltaTime);
			Vector2 vectorFromMouse = this.GetVectorFromMouse(deltaTime, true);
			base.UpdateRightStickWithRawValue(vectorFromMouse, updateTick, deltaTime);
			base.UpdateWithState(InputControlType.Action1, Input.GetKey(32), updateTick, deltaTime);
			base.UpdateWithState(InputControlType.Action2, Input.GetKey(115), updateTick, deltaTime);
			base.UpdateWithState(InputControlType.Action3, Input.GetKey(100), updateTick, deltaTime);
			base.UpdateWithState(InputControlType.Action4, Input.GetKey(102), updateTick, deltaTime);
			base.Commit(updateTick, deltaTime);
		}

		private Vector2 GetVectorFromKeyboard(float deltaTime, bool smoothed)
		{
			if (smoothed)
			{
				this.kx = this.ApplySmoothing(this.kx, this.GetXFromKeyboard(), deltaTime, 0.1f);
				this.ky = this.ApplySmoothing(this.ky, this.GetYFromKeyboard(), deltaTime, 0.1f);
			}
			else
			{
				this.kx = this.GetXFromKeyboard();
				this.ky = this.GetYFromKeyboard();
			}
			return new Vector2(this.kx, this.ky);
		}

		private float GetXFromKeyboard()
		{
			float num = ((!Input.GetKey(276)) ? 0f : (-1f));
			float num2 = ((!Input.GetKey(275)) ? 0f : 1f);
			return num + num2;
		}

		private float GetYFromKeyboard()
		{
			float num = ((!Input.GetKey(273)) ? 0f : 1f);
			float num2 = ((!Input.GetKey(274)) ? 0f : (-1f));
			return num + num2;
		}

		private Vector2 GetVectorFromMouse(float deltaTime, bool smoothed)
		{
			if (smoothed)
			{
				this.mx = this.ApplySmoothing(this.mx, Input.GetAxisRaw("mouse x") * 0.05f, deltaTime, 0.1f);
				this.my = this.ApplySmoothing(this.my, Input.GetAxisRaw("mouse y") * 0.05f, deltaTime, 0.1f);
			}
			else
			{
				this.mx = Input.GetAxisRaw("mouse x") * 0.05f;
				this.my = Input.GetAxisRaw("mouse y") * 0.05f;
			}
			return new Vector2(this.mx, this.my);
		}

		private float ApplySmoothing(float lastValue, float thisValue, float deltaTime, float sensitivity)
		{
			sensitivity = Mathf.Clamp(sensitivity, 0.001f, 1f);
			if (Mathf.Approximately(sensitivity, 1f))
			{
				return thisValue;
			}
			return Mathf.Lerp(lastValue, thisValue, deltaTime * sensitivity * 100f);
		}

		private const float sensitivity = 0.1f;

		private const float mouseScale = 0.05f;

		private float kx;

		private float ky;

		private float mx;

		private float my;
	}
}
