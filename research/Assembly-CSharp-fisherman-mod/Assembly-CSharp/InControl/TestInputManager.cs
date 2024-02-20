using System;
using System.Collections.Generic;
using UnityEngine;

namespace InControl
{
	public class TestInputManager : MonoBehaviour
	{
		private void OnEnable()
		{
			this.isPaused = false;
			Time.timeScale = 1f;
			Logger.OnLogMessage += delegate(LogMessage logMessage)
			{
				this.logMessages.Add(logMessage);
			};
			InputManager.OnDeviceAttached += delegate(InputDevice inputDevice)
			{
				Debug.Log("Attached: " + inputDevice.Name);
			};
			InputManager.OnDeviceDetached += delegate(InputDevice inputDevice)
			{
				Debug.Log("Detached: " + inputDevice.Name);
			};
			InputManager.OnActiveDeviceChanged += delegate(InputDevice inputDevice)
			{
				Debug.Log("Active device changed to: " + inputDevice.Name);
			};
			InputManager.OnUpdate += this.HandleInputUpdate;
		}

		private void HandleInputUpdate(ulong updateTick, float deltaTime)
		{
			this.CheckForPauseButton();
			int count = InputManager.Devices.Count;
			for (int i = 0; i < count; i++)
			{
				InputDevice inputDevice = InputManager.Devices[i];
				inputDevice.Vibrate(inputDevice.LeftTrigger, inputDevice.RightTrigger);
			}
		}

		private void Start()
		{
		}

		private void Update()
		{
			if (Input.GetKeyDown(114))
			{
				Utility.LoadScene("TestInputManager");
			}
			if (Input.GetKeyDown(101))
			{
				InputManager.Enabled = !InputManager.Enabled;
			}
		}

		private void CheckForPauseButton()
		{
			if (Input.GetKeyDown(112) || InputManager.CommandWasPressed)
			{
				Time.timeScale = ((!this.isPaused) ? 0f : 1f);
				this.isPaused = !this.isPaused;
			}
		}

		private void SetColor(Color color)
		{
			this.style.normal.textColor = color;
		}

		private void OnGUI()
		{
			int num = 300;
			int num2 = 10;
			int num3 = 10;
			int num4 = 15;
			GUI.skin.font = this.font;
			this.SetColor(Color.white);
			string text = "Devices:";
			text = text + " (Platform: " + InputManager.Platform + ")";
			text = text + " " + InputManager.ActiveDevice.Direction.Vector;
			if (this.isPaused)
			{
				this.SetColor(Color.red);
				text = "+++ PAUSED +++";
			}
			GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text, this.style);
			this.SetColor(Color.white);
			foreach (InputDevice inputDevice in InputManager.Devices)
			{
				bool flag = InputManager.ActiveDevice == inputDevice;
				Color color = ((!flag) ? Color.white : Color.yellow);
				num3 = 35;
				this.SetColor(color);
				GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), inputDevice.Name, this.style);
				num3 += num4;
				if (inputDevice.IsUnknown)
				{
					GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), inputDevice.Meta, this.style);
					num3 += num4;
				}
				GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), "Style: " + inputDevice.DeviceStyle, this.style);
				num3 += num4;
				GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), "GUID: " + inputDevice.GUID, this.style);
				num3 += num4;
				GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), "SortOrder: " + inputDevice.SortOrder, this.style);
				num3 += num4;
				GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), "LastChangeTick: " + inputDevice.LastChangeTick, this.style);
				num3 += num4;
				NativeInputDevice nativeInputDevice = inputDevice as NativeInputDevice;
				if (nativeInputDevice != null)
				{
					string text2 = string.Format("VID = 0x{0:x}, PID = 0x{1:x}, VER = 0x{2:x}", nativeInputDevice.Info.vendorID, nativeInputDevice.Info.productID, nativeInputDevice.Info.versionNumber);
					GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text2, this.style);
					num3 += num4;
				}
				num3 += num4;
				foreach (InputControl inputControl in inputDevice.Controls)
				{
					if (inputControl != null && !Utility.TargetIsAlias(inputControl.Target))
					{
						string text3;
						if (inputDevice.IsKnown)
						{
							text3 = string.Format("{0} ({1})", inputControl.Target, inputControl.Handle);
						}
						else
						{
							text3 = inputControl.Handle;
						}
						this.SetColor((!inputControl.State) ? color : Color.green);
						string text4 = string.Format("{0} {1}", text3, (!inputControl.State) ? string.Empty : ("= " + inputControl.Value));
						GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text4, this.style);
						num3 += num4;
					}
				}
				num3 += num4;
				color = ((!flag) ? Color.white : new Color(1f, 0.7f, 0.2f));
				if (inputDevice.IsKnown)
				{
					InputControl inputControl2 = inputDevice.Command;
					this.SetColor((!inputControl2.State) ? color : Color.green);
					string text5 = string.Format("{0} {1}", "Command", (!inputControl2.State) ? string.Empty : ("= " + inputControl2.Value));
					GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text5, this.style);
					num3 += num4;
					inputControl2 = inputDevice.LeftStickX;
					this.SetColor((!inputControl2.State) ? color : Color.green);
					text5 = string.Format("{0} {1}", "Left Stick X", (!inputControl2.State) ? string.Empty : ("= " + inputControl2.Value));
					GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text5, this.style);
					num3 += num4;
					inputControl2 = inputDevice.LeftStickY;
					this.SetColor((!inputControl2.State) ? color : Color.green);
					text5 = string.Format("{0} {1}", "Left Stick Y", (!inputControl2.State) ? string.Empty : ("= " + inputControl2.Value));
					GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text5, this.style);
					num3 += num4;
					this.SetColor((!inputDevice.LeftStick.State) ? color : Color.green);
					text5 = string.Format("{0} {1}", "Left Stick A", (!inputDevice.LeftStick.State) ? string.Empty : ("= " + inputDevice.LeftStick.Angle));
					GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text5, this.style);
					num3 += num4;
					inputControl2 = inputDevice.RightStickX;
					this.SetColor((!inputControl2.State) ? color : Color.green);
					text5 = string.Format("{0} {1}", "Right Stick X", (!inputControl2.State) ? string.Empty : ("= " + inputControl2.Value));
					GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text5, this.style);
					num3 += num4;
					inputControl2 = inputDevice.RightStickY;
					this.SetColor((!inputControl2.State) ? color : Color.green);
					text5 = string.Format("{0} {1}", "Right Stick Y", (!inputControl2.State) ? string.Empty : ("= " + inputControl2.Value));
					GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text5, this.style);
					num3 += num4;
					this.SetColor((!inputDevice.RightStick.State) ? color : Color.green);
					text5 = string.Format("{0} {1}", "Right Stick A", (!inputDevice.RightStick.State) ? string.Empty : ("= " + inputDevice.RightStick.Angle));
					GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text5, this.style);
					num3 += num4;
					inputControl2 = inputDevice.DPadX;
					this.SetColor((!inputControl2.State) ? color : Color.green);
					text5 = string.Format("{0} {1}", "DPad X", (!inputControl2.State) ? string.Empty : ("= " + inputControl2.Value));
					GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text5, this.style);
					num3 += num4;
					inputControl2 = inputDevice.DPadY;
					this.SetColor((!inputControl2.State) ? color : Color.green);
					text5 = string.Format("{0} {1}", "DPad Y", (!inputControl2.State) ? string.Empty : ("= " + inputControl2.Value));
					GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text5, this.style);
					num3 += num4;
				}
				this.SetColor(Color.cyan);
				InputControl anyButton = inputDevice.AnyButton;
				if (anyButton)
				{
					GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), "AnyButton = " + anyButton.Handle, this.style);
				}
				num2 += 200;
			}
			Color[] array = new Color[]
			{
				Color.gray,
				Color.yellow,
				Color.white
			};
			this.SetColor(Color.white);
			num2 = 10;
			num3 = Screen.height - (10 + num4);
			for (int i = this.logMessages.Count - 1; i >= 0; i--)
			{
				LogMessage logMessage = this.logMessages[i];
				if (logMessage.type != LogMessageType.Info)
				{
					this.SetColor(array[(int)logMessage.type]);
					foreach (string text6 in logMessage.text.Split(new char[] { '\n' }))
					{
						GUI.Label(new Rect((float)num2, (float)num3, (float)Screen.width, (float)(num3 + 10)), text6, this.style);
						num3 -= num4;
					}
				}
			}
		}

		private void DrawUnityInputDebugger()
		{
			int num = 300;
			int num2 = Screen.width / 2;
			int num3 = 10;
			int num4 = 20;
			this.SetColor(Color.white);
			string[] joystickNames = Input.GetJoystickNames();
			int num5 = joystickNames.Length;
			for (int i = 0; i < num5; i++)
			{
				string text = joystickNames[i];
				int num6 = i + 1;
				GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), string.Concat(new object[] { "Joystick ", num6, ": \"", text, "\"" }), this.style);
				num3 += num4;
				string text2 = "Buttons: ";
				for (int j = 0; j < 20; j++)
				{
					string text3 = string.Concat(new object[] { "joystick ", num6, " button ", j });
					bool key = Input.GetKey(text3);
					if (key)
					{
						string text4 = text2;
						text2 = string.Concat(new object[] { text4, "B", j, "  " });
					}
				}
				GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text2, this.style);
				num3 += num4;
				string text5 = "Analogs: ";
				for (int k = 0; k < 20; k++)
				{
					string text6 = string.Concat(new object[] { "joystick ", num6, " analog ", k });
					float axisRaw = Input.GetAxisRaw(text6);
					if (Utility.AbsoluteIsOverThreshold(axisRaw, 0.2f))
					{
						string text4 = text5;
						text5 = string.Concat(new object[]
						{
							text4,
							"A",
							k,
							": ",
							axisRaw.ToString("0.00"),
							"  "
						});
					}
				}
				GUI.Label(new Rect((float)num2, (float)num3, (float)(num2 + num), (float)(num3 + 10)), text5, this.style);
				num3 += num4;
				num3 += 25;
			}
		}

		private void OnDrawGizmos()
		{
			InputDevice activeDevice = InputManager.ActiveDevice;
			Vector2 vector = activeDevice.Direction.Vector;
			Gizmos.color = Color.blue;
			Vector2 vector2;
			vector2..ctor(-3f, -1f);
			Vector2 vector3 = vector2 + vector * 2f;
			Gizmos.DrawSphere(vector2, 0.1f);
			Gizmos.DrawLine(vector2, vector3);
			Gizmos.DrawSphere(vector3, 1f);
			Gizmos.color = Color.red;
			Vector2 vector4;
			vector4..ctor(3f, -1f);
			Vector2 vector5 = vector4 + activeDevice.RightStick.Vector * 2f;
			Gizmos.DrawSphere(vector4, 0.1f);
			Gizmos.DrawLine(vector4, vector5);
			Gizmos.DrawSphere(vector5, 1f);
		}

		public Font font;

		private GUIStyle style = new GUIStyle();

		private List<LogMessage> logMessages = new List<LogMessage>();

		private bool isPaused;
	}
}
