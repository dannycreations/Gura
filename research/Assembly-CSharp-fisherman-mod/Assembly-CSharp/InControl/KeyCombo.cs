using System;
using System.Collections.Generic;
using System.IO;

namespace InControl
{
	public struct KeyCombo
	{
		public KeyCombo(params Key[] keys)
		{
			this.data = 0UL;
			this.size = 0;
			for (int i = 0; i < keys.Length; i++)
			{
				this.Add(keys[i]);
			}
		}

		private void AddInt(int key)
		{
			if (this.size == 8)
			{
				return;
			}
			this.data |= (ulong)((ulong)((long)key & 255L) << this.size * 8);
			this.size++;
		}

		private int GetInt(int index)
		{
			return (int)((this.data >> index * 8) & 255UL);
		}

		public void Add(Key key)
		{
			this.AddInt((int)key);
		}

		public Key Get(int index)
		{
			if (index < 0 || index >= this.size)
			{
				throw new IndexOutOfRangeException(string.Concat(new object[] { "Index ", index, " is out of the range 0..", this.size }));
			}
			return (Key)this.GetInt(index);
		}

		public void Clear()
		{
			this.data = 0UL;
			this.size = 0;
		}

		public int Count
		{
			get
			{
				return this.size;
			}
		}

		public bool IsPressed
		{
			get
			{
				if (this.size == 0)
				{
					return false;
				}
				bool flag = true;
				for (int i = 0; i < this.size; i++)
				{
					int @int = this.GetInt(i);
					flag = flag && KeyInfo.KeyList[@int].IsPressed;
				}
				return flag;
			}
		}

		public static KeyCombo Detect(bool modifiersAsKeys)
		{
			KeyCombo keyCombo = default(KeyCombo);
			if (modifiersAsKeys)
			{
				for (int i = 5; i < 13; i++)
				{
					if (KeyInfo.KeyList[i].IsPressed)
					{
						keyCombo.AddInt(i);
						return keyCombo;
					}
				}
			}
			else
			{
				for (int j = 1; j < 5; j++)
				{
					if (KeyInfo.KeyList[j].IsPressed)
					{
						keyCombo.AddInt(j);
					}
				}
			}
			for (int k = 13; k < KeyInfo.KeyList.Length; k++)
			{
				if (KeyInfo.KeyList[k].IsPressed)
				{
					keyCombo.AddInt(k);
					return keyCombo;
				}
			}
			keyCombo.Clear();
			return keyCombo;
		}

		public override string ToString()
		{
			string text;
			if (!KeyCombo.cachedStrings.TryGetValue(this.data, out text))
			{
				text = string.Empty;
				for (int i = 0; i < this.size; i++)
				{
					if (i != 0)
					{
						text += " ";
					}
					int @int = this.GetInt(i);
					text += KeyInfo.KeyList[@int].Name;
				}
			}
			return text;
		}

		public static bool operator ==(KeyCombo a, KeyCombo b)
		{
			return a.data == b.data;
		}

		public static bool operator !=(KeyCombo a, KeyCombo b)
		{
			return a.data != b.data;
		}

		public override bool Equals(object other)
		{
			if (other is KeyCombo)
			{
				KeyCombo keyCombo = (KeyCombo)other;
				return this.data == keyCombo.data;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return this.data.GetHashCode();
		}

		internal void Load(BinaryReader reader)
		{
			this.size = reader.ReadInt32();
			this.data = reader.ReadUInt64();
		}

		internal void Save(BinaryWriter writer)
		{
			writer.Write(this.size);
			writer.Write(this.data);
		}

		private int size;

		private ulong data;

		private static Dictionary<ulong, string> cachedStrings = new Dictionary<ulong, string>();
	}
}
