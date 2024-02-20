using System;

namespace FSMAnimator
{
	public class AnimProperties<TFloat, TInt, TBool> where TFloat : struct, IConvertible where TInt : struct, IConvertible where TBool : struct, IConvertible
	{
		public AnimProperties(byte floatsCount, byte intsCount, byte boolsCount)
		{
			this._floats = new AnimProperties<TFloat, TInt, TBool>.Container<TFloat, float>((int)floatsCount);
			this._ints = new AnimProperties<TFloat, TInt, TBool>.Container<TInt, int>((int)intsCount);
			this._bools = new AnimProperties<TFloat, TInt, TBool>.Container<TBool, bool>((int)boolsCount);
		}

		public bool ValidateProperty(TFloat name)
		{
			return this._floats.ValidateKey(name);
		}

		public float this[TFloat name]
		{
			get
			{
				return this._floats[name];
			}
			set
			{
				this._floats[name] = value;
			}
		}

		public int this[TInt name]
		{
			get
			{
				return this._ints[name];
			}
			set
			{
				this._ints[name] = value;
			}
		}

		public bool this[TBool name]
		{
			get
			{
				return this._bools[name];
			}
			set
			{
				this._bools[name] = value;
			}
		}

		private AnimProperties<TFloat, TInt, TBool>.Container<TFloat, float> _floats;

		private AnimProperties<TFloat, TInt, TBool>.Container<TInt, int> _ints;

		private AnimProperties<TFloat, TInt, TBool>.Container<TBool, bool> _bools;

		private class Container<TKey, TValue> where TKey : struct, IConvertible where TValue : struct
		{
			public Container(int count)
			{
				this._values = new TValue[count];
			}

			public TValue this[TKey key]
			{
				get
				{
					byte b = key.ToByte(null);
					if (b >= 0 && (int)b < this._values.Length)
					{
						return this._values[(int)b];
					}
					LogHelper.Error("GetFProperty({0}) for invalid property", new object[] { key });
					return default(TValue);
				}
				set
				{
					byte b = key.ToByte(null);
					if (b >= 0 && (int)b < this._values.Length)
					{
						this._values[(int)b] = value;
					}
					else
					{
						LogHelper.Error("SetProperty({0}, {1}) for invalid property", new object[] { key, value });
					}
				}
			}

			public bool ValidateKey(TKey key)
			{
				byte b = key.ToByte(null);
				return (int)b < this._values.Length;
			}

			private TValue[] _values;
		}
	}
}
