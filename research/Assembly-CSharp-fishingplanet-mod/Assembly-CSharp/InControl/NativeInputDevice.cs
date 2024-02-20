using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace InControl
{
	public class NativeInputDevice : InputDevice
	{
		internal NativeInputDevice()
		{
		}

		internal uint Handle { get; private set; }

		internal NativeDeviceInfo Info { get; private set; }

		internal void Initialize(uint deviceHandle, NativeDeviceInfo deviceInfo, NativeInputDeviceProfile deviceProfile)
		{
			this.Handle = deviceHandle;
			this.Info = deviceInfo;
			this.profile = deviceProfile;
			base.SortOrder = (int)(1000U + this.Handle);
			this.buttons = new short[this.Info.numButtons];
			this.analogs = new short[this.Info.numAnalogs];
			base.AnalogSnapshot = null;
			base.ClearInputState();
			base.ClearControls();
			if (this.IsKnown)
			{
				base.Name = this.profile.Name ?? this.Info.name;
				base.Meta = this.profile.Meta ?? this.Info.name;
				base.DeviceClass = this.profile.DeviceClass;
				base.DeviceStyle = this.profile.DeviceStyle;
				int analogCount = this.profile.AnalogCount;
				for (int i = 0; i < analogCount; i++)
				{
					InputControlMapping inputControlMapping = this.profile.AnalogMappings[i];
					InputControl inputControl = base.AddControl(inputControlMapping.Target, inputControlMapping.Handle);
					inputControl.Sensitivity = Mathf.Min(this.profile.Sensitivity, inputControlMapping.Sensitivity);
					inputControl.LowerDeadZone = Mathf.Max(this.profile.LowerDeadZone, inputControlMapping.LowerDeadZone);
					inputControl.UpperDeadZone = Mathf.Min(this.profile.UpperDeadZone, inputControlMapping.UpperDeadZone);
					inputControl.Raw = inputControlMapping.Raw;
					inputControl.Passive = inputControlMapping.Passive;
				}
				int buttonCount = this.profile.ButtonCount;
				for (int j = 0; j < buttonCount; j++)
				{
					InputControlMapping inputControlMapping2 = this.profile.ButtonMappings[j];
					InputControl inputControl2 = base.AddControl(inputControlMapping2.Target, inputControlMapping2.Handle);
					inputControl2.Passive = inputControlMapping2.Passive;
				}
			}
			else
			{
				base.Name = "Unknown Device";
				base.Meta = this.Info.name;
				for (int k = 0; k < this.NumUnknownButtons; k++)
				{
					base.AddControl(InputControlType.Button0 + k, "Button " + k);
				}
				for (int l = 0; l < this.NumUnknownAnalogs; l++)
				{
					base.AddControl(InputControlType.Analog0 + l, "Analog " + l, 0.2f, 0.9f);
				}
			}
			this.skipUpdateFrames = 1;
		}

		internal void Initialize(uint deviceHandle, NativeDeviceInfo deviceInfo)
		{
			this.Initialize(deviceHandle, deviceInfo, this.profile);
		}

		public override void Update(ulong updateTick, float deltaTime)
		{
			if (this.skipUpdateFrames > 0)
			{
				this.skipUpdateFrames--;
				return;
			}
			IntPtr intPtr;
			if (Native.GetDeviceState(this.Handle, out intPtr))
			{
				Marshal.Copy(intPtr, this.buttons, 0, this.buttons.Length);
				intPtr = new IntPtr(intPtr.ToInt64() + (long)(this.buttons.Length * 2));
				Marshal.Copy(intPtr, this.analogs, 0, this.analogs.Length);
			}
			if (this.IsKnown)
			{
				int analogCount = this.profile.AnalogCount;
				for (int i = 0; i < analogCount; i++)
				{
					InputControlMapping inputControlMapping = this.profile.AnalogMappings[i];
					float value = inputControlMapping.Source.GetValue(this);
					InputControl control = base.GetControl(inputControlMapping.Target);
					if (!inputControlMapping.IgnoreInitialZeroValue || !control.IsOnZeroTick || !Utility.IsZero(value))
					{
						float num = inputControlMapping.MapValue(value);
						control.UpdateWithValue(num, updateTick, deltaTime);
					}
				}
				int buttonCount = this.profile.ButtonCount;
				for (int j = 0; j < buttonCount; j++)
				{
					InputControlMapping inputControlMapping2 = this.profile.ButtonMappings[j];
					bool state = inputControlMapping2.Source.GetState(this);
					base.UpdateWithState(inputControlMapping2.Target, state, updateTick, deltaTime);
				}
			}
			else
			{
				int num2 = 0;
				while ((long)num2 < (long)((ulong)this.Info.numButtons))
				{
					base.UpdateWithState(InputControlType.Button0 + num2, this.ReadRawButtonState(num2), updateTick, deltaTime);
					num2++;
				}
				int num3 = 0;
				while ((long)num3 < (long)((ulong)this.Info.numAnalogs))
				{
					base.UpdateWithValue(InputControlType.Analog0 + num3, this.ReadRawAnalogValue(num3), updateTick, deltaTime);
					num3++;
				}
			}
		}

		internal override bool ReadRawButtonState(int index)
		{
			return index < this.buttons.Length && this.buttons[index] > -32767;
		}

		internal override float ReadRawAnalogValue(int index)
		{
			if (index < this.analogs.Length)
			{
				return (float)this.analogs[index] / 32767f;
			}
			return 0f;
		}

		private byte FloatToByte(float value)
		{
			return (byte)(Mathf.Clamp01(value) * 255f);
		}

		public override void Vibrate(float leftMotor, float rightMotor)
		{
			Native.SetHapticState(this.Handle, this.FloatToByte(leftMotor), this.FloatToByte(rightMotor));
		}

		public override void SetLightColor(float red, float green, float blue)
		{
			Native.SetLightColor(this.Handle, this.FloatToByte(red), this.FloatToByte(green), this.FloatToByte(blue));
		}

		public override void SetLightFlash(float flashOnDuration, float flashOffDuration)
		{
			Native.SetLightFlash(this.Handle, this.FloatToByte(flashOnDuration), this.FloatToByte(flashOffDuration));
		}

		public bool HasSameVendorID(NativeDeviceInfo deviceInfo)
		{
			return this.Info.HasSameVendorID(deviceInfo);
		}

		public bool HasSameProductID(NativeDeviceInfo deviceInfo)
		{
			return this.Info.HasSameProductID(deviceInfo);
		}

		public bool HasSameVersionNumber(NativeDeviceInfo deviceInfo)
		{
			return this.Info.HasSameVersionNumber(deviceInfo);
		}

		public bool HasSameLocation(NativeDeviceInfo deviceInfo)
		{
			return this.Info.HasSameLocation(deviceInfo);
		}

		public bool HasSameSerialNumber(NativeDeviceInfo deviceInfo)
		{
			return this.Info.HasSameSerialNumber(deviceInfo);
		}

		public override bool IsSupportedOnThisPlatform
		{
			get
			{
				return this.profile == null || this.profile.IsSupportedOnThisPlatform;
			}
		}

		public override bool IsKnown
		{
			get
			{
				return this.profile != null;
			}
		}

		internal override int NumUnknownButtons
		{
			get
			{
				return (int)this.Info.numButtons;
			}
		}

		internal override int NumUnknownAnalogs
		{
			get
			{
				return (int)this.Info.numAnalogs;
			}
		}

		private short[] buttons;

		private short[] analogs;

		private NativeInputDeviceProfile profile;

		private int skipUpdateFrames;
	}
}
