using System;
using System.Collections.Generic;
using UnityEngine;

namespace InControl
{
	public class XboxOneInputDeviceManager : InputDeviceManager
	{
		public XboxOneInputDeviceManager()
		{
			for (uint num = 1U; num <= 8U; num += 1U)
			{
				this.devices.Add(new XboxOneInputDevice(num));
			}
			this.Update(0UL, 0f);
		}

		public override void Update(ulong updateTick, float deltaTime)
		{
			for (int i = 0; i < 8; i++)
			{
				XboxOneInputDevice xboxOneInputDevice = this.devices[i] as XboxOneInputDevice;
				if (xboxOneInputDevice.IsConnected != this.deviceConnected[i])
				{
					if (xboxOneInputDevice.IsConnected)
					{
						InputManager.AttachDevice(xboxOneInputDevice);
					}
					else
					{
						InputManager.DetachDevice(xboxOneInputDevice);
					}
					this.deviceConnected[i] = xboxOneInputDevice.IsConnected;
				}
			}
		}

		public override void Destroy()
		{
		}

		public static bool CheckPlatformSupport(ICollection<string> errors)
		{
			return Application.platform == 27;
		}

		internal static bool Enable()
		{
			List<string> list = new List<string>();
			if (XboxOneInputDeviceManager.CheckPlatformSupport(list))
			{
				InputManager.AddDeviceManager<XboxOneInputDeviceManager>();
				return true;
			}
			foreach (string text in list)
			{
				Logger.LogError(text);
			}
			return false;
		}

		private const int maxDevices = 8;

		private bool[] deviceConnected = new bool[8];
	}
}
