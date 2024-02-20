using System;
using System.Linq;
using UnityEngine;

namespace FSMAnimator
{
	public class BlendTree1D<T> : BlendTree<T> where T : struct, IConvertible
	{
		public BlendTree1D(string name, float blendTime, BlendTree1D<T>.Data[] data)
			: base(name, blendTime)
		{
			if (data.Length > 0)
			{
				this._data = (from r in data.Distinct<BlendTree1D<T>.Data>()
					orderby r.Value
					select r).ToArray<BlendTree1D<T>.Data>();
				if (this._data.Length != data.Length)
				{
					LogHelper.Error("Blend tree {0} has duplicated clips!", new object[] { name });
				}
				this._minValue = this._data[0].Value;
				this._maxValue = this._data[this._data.Length - 1].Value;
				this._lastResult = new BlendTree<T>.UpdateResult[this._data.Length];
				for (int i = 0; i < this._data.Length; i++)
				{
					this._lastResult[i] = new BlendTree<T>.UpdateResult(this._data[i].Clip, this._data[i].Speed);
				}
				return;
			}
			throw new Exception("Empty BlendTree1D");
		}

		public override string[] Clips
		{
			get
			{
				return this._data.Select((BlendTree1D<T>.Data r) => r.Clip).ToArray<string>();
			}
		}

		public void SetProperties(T name)
		{
			base.Properties = new T[] { name };
		}

		public override void Update(float[] properties)
		{
			if (this._data.Length == 1)
			{
				this._lastResult[0].Weight = 1f;
				return;
			}
			this._lastResult[0].Weight = 0f;
			float num = Mathf.Clamp(properties[0], this._minValue, this._maxValue);
			for (int i = 1; i < this._data.Length; i++)
			{
				if (this._data[i].Value > num)
				{
					float value = this._data[i - 1].Value;
					float value2 = this._data[i].Value;
					float num2 = Mathf.Clamp01((num - value) / (value2 - value));
					this._lastResult[i - 1].Weight = 1f - num2;
					this._lastResult[i].Weight = num2;
					for (int j = i + 1; j < this._data.Length; j++)
					{
						this._lastResult[j].Weight = 0f;
					}
					return;
				}
				this._lastResult[i].Weight = 0f;
			}
			this._lastResult[this._data.Length - 1].Weight = 1f;
		}

		private BlendTree1D<T>.Data[] _data;

		private float _minValue;

		private float _maxValue;

		private string _leftClip;

		public struct Data
		{
			public Data(string clip, float value, float speed = 1f)
			{
				this.Clip = clip;
				this.Value = value;
				this.Speed = speed;
			}

			public readonly string Clip;

			public readonly float Value;

			public readonly float Speed;
		}
	}
}
