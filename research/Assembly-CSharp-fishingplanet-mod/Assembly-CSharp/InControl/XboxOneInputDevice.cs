using System;

namespace InControl
{
	public class XboxOneInputDevice : InputDevice
	{
		public XboxOneInputDevice(uint joystickId)
			: base("Xbox One Controller")
		{
			this.JoystickId = joystickId;
			base.SortOrder = (int)joystickId;
			base.Meta = "Xbox One Device #" + joystickId;
			base.DeviceClass = InputDeviceClass.Controller;
			base.DeviceStyle = InputDeviceStyle.XboxOne;
			this.CacheAnalogAxisNames();
			base.AddControl(InputControlType.LeftStickLeft, "Left Stick Left", 0.2f, 0.9f);
			base.AddControl(InputControlType.LeftStickRight, "Left Stick Right", 0.2f, 0.9f);
			base.AddControl(InputControlType.LeftStickUp, "Left Stick Up", 0.2f, 0.9f);
			base.AddControl(InputControlType.LeftStickDown, "Left Stick Down", 0.2f, 0.9f);
			base.AddControl(InputControlType.RightStickLeft, "Right Stick Left", 0.2f, 0.9f);
			base.AddControl(InputControlType.RightStickRight, "Right Stick Right", 0.2f, 0.9f);
			base.AddControl(InputControlType.RightStickUp, "Right Stick Up", 0.2f, 0.9f);
			base.AddControl(InputControlType.RightStickDown, "Right Stick Down", 0.2f, 0.9f);
			base.AddControl(InputControlType.LeftTrigger, "Left Trigger", 0.2f, 0.9f);
			base.AddControl(InputControlType.RightTrigger, "Right Trigger", 0.2f, 0.9f);
			base.AddControl(InputControlType.DPadUp, "DPad Up", 0.2f, 0.9f);
			base.AddControl(InputControlType.DPadDown, "DPad Down", 0.2f, 0.9f);
			base.AddControl(InputControlType.DPadLeft, "DPad Left", 0.2f, 0.9f);
			base.AddControl(InputControlType.DPadRight, "DPad Right", 0.2f, 0.9f);
			base.AddControl(InputControlType.Action1, "A");
			base.AddControl(InputControlType.Action2, "B");
			base.AddControl(InputControlType.Action3, "X");
			base.AddControl(InputControlType.Action4, "Y");
			base.AddControl(InputControlType.LeftBumper, "Left Bumper");
			base.AddControl(InputControlType.RightBumper, "Right Bumper");
			base.AddControl(InputControlType.LeftStickButton, "Left Stick Button");
			base.AddControl(InputControlType.RightStickButton, "Right Stick Button");
			base.AddControl(InputControlType.View, "View");
			base.AddControl(InputControlType.Menu, "Menu");
		}

		internal uint JoystickId { get; private set; }

		public ulong ControllerId { get; private set; }

		public override void OnAttached()
		{
			base.OnAttached();
			this.UpdateUser();
		}

		public override void Update(ulong updateTick, float deltaTime)
		{
		}

		public void UpdateUser()
		{
		}

		public bool IsConnected
		{
			get
			{
				return false;
			}
		}

		public override void Vibrate(float leftMotor, float rightMotor)
		{
		}

		public void Vibrate(float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
		{
		}

		private string AnalogAxisNameForId(uint analogId)
		{
			return this.analogAxisNameForId[(int)((UIntPtr)analogId)];
		}

		private void CacheAnalogAxisNameForId(uint analogId)
		{
			this.analogAxisNameForId[(int)((UIntPtr)analogId)] = string.Concat(new object[] { "joystick ", this.JoystickId, " analog ", analogId });
		}

		private void CacheAnalogAxisNames()
		{
			this.analogAxisNameForId = new string[16];
			this.CacheAnalogAxisNameForId(0U);
			this.CacheAnalogAxisNameForId(1U);
			this.CacheAnalogAxisNameForId(3U);
			this.CacheAnalogAxisNameForId(4U);
			this.CacheAnalogAxisNameForId(8U);
			this.CacheAnalogAxisNameForId(9U);
		}

		private const uint AnalogLeftStickX = 0U;

		private const uint AnalogLeftStickY = 1U;

		private const uint AnalogRightStickX = 3U;

		private const uint AnalogRightStickY = 4U;

		private const uint AnalogLeftTrigger = 8U;

		private const uint AnalogRightTrigger = 9U;

		private const float LowerDeadZone = 0.2f;

		private const float UpperDeadZone = 0.9f;

		private bool? _change;

		private float _time;

		private string[] analogAxisNameForId;
	}
}
