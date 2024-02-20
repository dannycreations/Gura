using System;

namespace XInputDotNetPure
{
	public struct GamePadState
	{
		internal GamePadState(bool isConnected, GamePadState.RawState rawState)
		{
			this.isConnected = isConnected;
			if (!isConnected)
			{
				rawState.dwPacketNumber = 0U;
				rawState.Gamepad.dwButtons = 0;
				rawState.Gamepad.bLeftTrigger = 0;
				rawState.Gamepad.bRightTrigger = 0;
				rawState.Gamepad.sThumbLX = 0;
				rawState.Gamepad.sThumbLY = 0;
				rawState.Gamepad.sThumbRX = 0;
				rawState.Gamepad.sThumbRY = 0;
			}
			this.packetNumber = rawState.dwPacketNumber;
			this.buttons = new GamePadButtons(((rawState.Gamepad.dwButtons & 16) == 0) ? ButtonState.Released : ButtonState.Pressed, ((rawState.Gamepad.dwButtons & 32) == 0) ? ButtonState.Released : ButtonState.Pressed, ((rawState.Gamepad.dwButtons & 64) == 0) ? ButtonState.Released : ButtonState.Pressed, ((rawState.Gamepad.dwButtons & 128) == 0) ? ButtonState.Released : ButtonState.Pressed, ((rawState.Gamepad.dwButtons & 256) == 0) ? ButtonState.Released : ButtonState.Pressed, ((rawState.Gamepad.dwButtons & 512) == 0) ? ButtonState.Released : ButtonState.Pressed, ((rawState.Gamepad.dwButtons & 4096) == 0) ? ButtonState.Released : ButtonState.Pressed, ((rawState.Gamepad.dwButtons & 8192) == 0) ? ButtonState.Released : ButtonState.Pressed, ((rawState.Gamepad.dwButtons & 16384) == 0) ? ButtonState.Released : ButtonState.Pressed, ((rawState.Gamepad.dwButtons & 32768) == 0) ? ButtonState.Released : ButtonState.Pressed);
			this.dPad = new GamePadDPad(((rawState.Gamepad.dwButtons & 1) == 0) ? ButtonState.Released : ButtonState.Pressed, ((rawState.Gamepad.dwButtons & 2) == 0) ? ButtonState.Released : ButtonState.Pressed, ((rawState.Gamepad.dwButtons & 4) == 0) ? ButtonState.Released : ButtonState.Pressed, ((rawState.Gamepad.dwButtons & 8) == 0) ? ButtonState.Released : ButtonState.Pressed);
			this.thumbSticks = new GamePadThumbSticks(new GamePadThumbSticks.StickValue((float)rawState.Gamepad.sThumbLX / 32767f, (float)rawState.Gamepad.sThumbLY / 32767f), new GamePadThumbSticks.StickValue((float)rawState.Gamepad.sThumbRX / 32767f, (float)rawState.Gamepad.sThumbRY / 32767f));
			this.triggers = new GamePadTriggers((float)rawState.Gamepad.bLeftTrigger / 255f, (float)rawState.Gamepad.bRightTrigger / 255f);
		}

		public uint PacketNumber
		{
			get
			{
				return this.packetNumber;
			}
		}

		public bool IsConnected
		{
			get
			{
				return this.isConnected;
			}
		}

		public GamePadButtons Buttons
		{
			get
			{
				return this.buttons;
			}
		}

		public GamePadDPad DPad
		{
			get
			{
				return this.dPad;
			}
		}

		public GamePadTriggers Triggers
		{
			get
			{
				return this.triggers;
			}
		}

		public GamePadThumbSticks ThumbSticks
		{
			get
			{
				return this.thumbSticks;
			}
		}

		private bool isConnected;

		private uint packetNumber;

		private GamePadButtons buttons;

		private GamePadDPad dPad;

		private GamePadThumbSticks thumbSticks;

		private GamePadTriggers triggers;

		internal struct RawState
		{
			public uint dwPacketNumber;

			public GamePadState.RawState.GamePad Gamepad;

			public struct GamePad
			{
				public ushort dwButtons;

				public byte bLeftTrigger;

				public byte bRightTrigger;

				public short sThumbLX;

				public short sThumbLY;

				public short sThumbRX;

				public short sThumbRY;
			}
		}

		private enum ButtonsConstants
		{
			DPadUp = 1,
			DPadDown,
			DPadLeft = 4,
			DPadRight = 8,
			Start = 16,
			Back = 32,
			LeftThumb = 64,
			RightThumb = 128,
			LeftShoulder = 256,
			RightShoulder = 512,
			A = 4096,
			B = 8192,
			X = 16384,
			Y = 32768
		}
	}
}
