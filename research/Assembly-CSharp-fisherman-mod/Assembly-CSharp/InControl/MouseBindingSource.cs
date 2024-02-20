using System;
using System.IO;
using UnityEngine;

namespace InControl
{
	public class MouseBindingSource : BindingSource
	{
		internal MouseBindingSource()
		{
		}

		public MouseBindingSource(Mouse mouseControl)
		{
			this.Control = mouseControl;
		}

		public Mouse Control { get; protected set; }

		internal static bool SafeGetMouseButton(int button)
		{
			try
			{
				return Input.GetMouseButton(button);
			}
			catch (ArgumentException)
			{
			}
			return false;
		}

		internal static bool ButtonIsPressed(Mouse control)
		{
			int num = MouseBindingSource.buttonTable[(int)control];
			return num >= 0 && MouseBindingSource.SafeGetMouseButton(num);
		}

		public override float GetValue(InputDevice inputDevice)
		{
			int num = MouseBindingSource.buttonTable[(int)this.Control];
			if (num >= 0)
			{
				return (!MouseBindingSource.SafeGetMouseButton(num)) ? 0f : 1f;
			}
			switch (this.Control)
			{
			case Mouse.NegativeX:
				return -Mathf.Min(Input.GetAxisRaw("mouse x") * MouseBindingSource.ScaleX, 0f);
			case Mouse.PositiveX:
				return Mathf.Max(0f, Input.GetAxisRaw("mouse x") * MouseBindingSource.ScaleX);
			case Mouse.NegativeY:
				return -Mathf.Min(Input.GetAxisRaw("mouse y") * MouseBindingSource.ScaleY, 0f);
			case Mouse.PositiveY:
				return Mathf.Max(0f, Input.GetAxisRaw("mouse y") * MouseBindingSource.ScaleY);
			case Mouse.PositiveScrollWheel:
				return Mathf.Max(0f, Input.GetAxisRaw("mouse z") * MouseBindingSource.ScaleZ);
			case Mouse.NegativeScrollWheel:
				return -Mathf.Min(Input.GetAxisRaw("mouse z") * MouseBindingSource.ScaleZ, 0f);
			default:
				return 0f;
			}
		}

		public override bool GetState(InputDevice inputDevice)
		{
			return Utility.IsNotZero(this.GetValue(inputDevice));
		}

		public override string Name
		{
			get
			{
				return this.Control.ToString();
			}
		}

		public override string DeviceName
		{
			get
			{
				return "Mouse";
			}
		}

		public override int Number
		{
			get
			{
				return this._number;
			}
			set
			{
				this._number = value;
			}
		}

		public override bool Equals(BindingSource other)
		{
			if (other == null)
			{
				return false;
			}
			MouseBindingSource mouseBindingSource = other as MouseBindingSource;
			return mouseBindingSource != null && this.Control == mouseBindingSource.Control;
		}

		public override bool Equals(object other)
		{
			if (other == null)
			{
				return false;
			}
			MouseBindingSource mouseBindingSource = other as MouseBindingSource;
			return mouseBindingSource != null && this.Control == mouseBindingSource.Control;
		}

		public override int GetHashCode()
		{
			return this.Control.GetHashCode();
		}

		public override BindingSourceType BindingSourceType
		{
			get
			{
				return BindingSourceType.MouseBindingSource;
			}
		}

		internal override void Save(BinaryWriter writer)
		{
			writer.Write((int)this.Control);
		}

		internal override void Load(BinaryReader reader)
		{
			this.Control = (Mouse)reader.ReadInt32();
		}

		public static float ScaleX = 0.1f;

		public static float ScaleY = 0.1f;

		public static float ScaleZ = 0.1f;

		public static float JitterThreshold = 0.05f;

		private int _number;

		private static readonly int[] buttonTable = new int[]
		{
			-1, 0, 1, 2, -1, -1, -1, -1, -1, -1,
			3, 4, 5, 6, 7, 8
		};
	}
}
