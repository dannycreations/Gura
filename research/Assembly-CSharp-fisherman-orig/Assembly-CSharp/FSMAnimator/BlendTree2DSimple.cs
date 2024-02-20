using System;
using System.Linq;
using UnityEngine;

namespace FSMAnimator
{
	public class BlendTree2DSimple<T> : BlendTree<T> where T : struct, IConvertible
	{
		public BlendTree2DSimple(string name, float blendTime, BlendTree2DSimple<T>.Data center, BlendTree2DSimple<T>.Data left, BlendTree2DSimple<T>.Data right, BlendTree2DSimple<T>.Data up, BlendTree2DSimple<T>.Data down)
			: base(name, blendTime)
		{
			this._lastResult = new BlendTree<T>.UpdateResult[]
			{
				new BlendTree<T>.UpdateResult(center.Clip, center.Speed),
				new BlendTree<T>.UpdateResult(left.Clip, left.Speed),
				new BlendTree<T>.UpdateResult(right.Clip, right.Speed),
				new BlendTree<T>.UpdateResult(up.Clip, up.Speed),
				new BlendTree<T>.UpdateResult(down.Clip, down.Speed)
			};
		}

		public override string[] Clips
		{
			get
			{
				return this._lastResult.Select((BlendTree<T>.UpdateResult r) => r.Clip).ToArray<string>();
			}
		}

		public void SetProperties(T property1, T property2)
		{
			base.Properties = new T[] { property1, property2 };
		}

		public override void Update(float[] properties)
		{
			float num = Mathf.Clamp(properties[0], -1f, 1f);
			float num2 = Mathf.Clamp(properties[1], -1f, 1f);
			bool flag = Mathf.Approximately(num, 0f);
			bool flag2 = Mathf.Approximately(num2, 0f);
			if (flag && flag2)
			{
				this._lastResult[0].Weight = 1f;
				this._lastResult[1].Weight = 0f;
				this._lastResult[2].Weight = 0f;
				this._lastResult[3].Weight = 0f;
				this._lastResult[4].Weight = 0f;
			}
			else
			{
				byte b;
				if (num > 0f)
				{
					b = 2;
					this._lastResult[1].Weight = 0f;
				}
				else
				{
					b = 1;
					this._lastResult[2].Weight = 0f;
				}
				byte b2;
				if (num2 > 0f)
				{
					b2 = 3;
					this._lastResult[4].Weight = 0f;
				}
				else
				{
					b2 = 4;
					this._lastResult[3].Weight = 0f;
				}
				float num3 = Mathf.Abs(num);
				float num4 = Mathf.Abs(num2);
				if (flag)
				{
					this._lastResult[(int)b].Weight = 0f;
					this._lastResult[0].Weight = 1f - num4;
					this._lastResult[(int)b2].Weight = num4;
				}
				else if (flag2)
				{
					this._lastResult[(int)b2].Weight = 0f;
					this._lastResult[0].Weight = 1f - num3;
					this._lastResult[(int)b].Weight = num3;
				}
				else
				{
					float num5 = 1f - num3 - num4;
					this._lastResult[0].Weight = Mathf.Max(0f, num5);
					this._lastResult[(int)b].Weight = num3;
					this._lastResult[(int)b2].Weight = num4;
					if (num5 <= 0f)
					{
						float num6 = this._lastResult[(int)b].Weight + this._lastResult[(int)b2].Weight;
						this._lastResult[(int)b].Weight /= num6;
						this._lastResult[(int)b2].Weight /= num6;
					}
				}
			}
		}

		public struct Data
		{
			public Data(string clip, float speed = 1f)
			{
				this.Clip = clip;
				this.Speed = speed;
			}

			public readonly string Clip;

			public readonly float Speed;
		}
	}
}
