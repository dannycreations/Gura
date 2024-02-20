using System;
using UnityEngine;

namespace FuzzyLogic
{
	public struct FuzzyValue
	{
		public FuzzyValue(float v)
		{
			this.v = Mathf.Clamp01(v);
		}

		public static implicit operator FuzzyValue(float v)
		{
			return new FuzzyValue(v);
		}

		public static implicit operator float(FuzzyValue f)
		{
			return f.v;
		}

		public float Value
		{
			get
			{
				return this.v;
			}
			set
			{
				this.v = Mathf.Clamp01(value);
			}
		}

		public static float tanh(float x)
		{
			float num = Mathf.Exp(-2f * x);
			return (1f - num) / (1f + num);
		}

		public static FuzzyValue NumberIsBig(float number, float threshold)
		{
			return new FuzzyValue(FuzzyValue.tanh(0.5493f * number / threshold));
		}

		public static bool operator ==(FuzzyValue A, FuzzyValue B)
		{
			return Mathf.Approximately(A.v, B.v);
		}

		public static bool operator !=(FuzzyValue A, FuzzyValue B)
		{
			return !Mathf.Approximately(A.v, B.v);
		}

		public static bool operator >=(FuzzyValue A, FuzzyValue B)
		{
			return A.v >= B.v;
		}

		public static bool operator <=(FuzzyValue A, FuzzyValue B)
		{
			return A.v <= B.v;
		}

		public static bool operator >(FuzzyValue A, FuzzyValue B)
		{
			return A.v > B.v;
		}

		public static bool operator <(FuzzyValue A, FuzzyValue B)
		{
			return A.v < B.v;
		}

		public static FuzzyValue operator ~(FuzzyValue A)
		{
			return new FuzzyValue(1f - A.v);
		}

		public static FuzzyValue operator |(FuzzyValue A, FuzzyValue B)
		{
			return new FuzzyValue(1f - (1f - A.v) * (1f - B.v));
		}

		public static FuzzyValue operator &(FuzzyValue A, FuzzyValue B)
		{
			return new FuzzyValue(A.v * B.v);
		}

		public static FuzzyValue operator *(float scalar, FuzzyValue A)
		{
			return new FuzzyValue(A.v * scalar);
		}

		public override string ToString()
		{
			return this.v.ToString();
		}

		public static int GetMax(FuzzyValue[] array)
		{
			FuzzyValue fuzzyValue = array[0];
			int num = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] > fuzzyValue)
				{
					num = i;
					fuzzyValue = array[i];
				}
			}
			return num;
		}

		private float v;
	}
}
