using System;
using System.IO;

namespace InControl
{
	public class KeyBindingSource : BindingSource
	{
		internal KeyBindingSource()
		{
		}

		public KeyBindingSource(KeyCombo keyCombo)
		{
			this.Control = keyCombo;
		}

		public KeyBindingSource(params Key[] keys)
		{
			this.Control = new KeyCombo(keys);
		}

		public KeyCombo Control { get; protected set; }

		public override float GetValue(InputDevice inputDevice)
		{
			return (!this.GetState(inputDevice)) ? 0f : 1f;
		}

		public override bool GetState(InputDevice inputDevice)
		{
			return this.Control.IsPressed;
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
				return "Keyboard";
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
			KeyBindingSource keyBindingSource = other as KeyBindingSource;
			return keyBindingSource != null && this.Control == keyBindingSource.Control;
		}

		public override bool Equals(object other)
		{
			if (other == null)
			{
				return false;
			}
			KeyBindingSource keyBindingSource = other as KeyBindingSource;
			return keyBindingSource != null && this.Control == keyBindingSource.Control;
		}

		public override int GetHashCode()
		{
			return this.Control.GetHashCode();
		}

		public override BindingSourceType BindingSourceType
		{
			get
			{
				return BindingSourceType.KeyBindingSource;
			}
		}

		internal override void Load(BinaryReader reader)
		{
			KeyCombo keyCombo = default(KeyCombo);
			keyCombo.Load(reader);
			this.Control = keyCombo;
			this.Number = reader.ReadInt32();
		}

		internal override void Save(BinaryWriter writer)
		{
			this.Control.Save(writer);
			writer.Write(this.Number);
		}

		private int _number;
	}
}
