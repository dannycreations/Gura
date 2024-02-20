using System;
using UnityEngine;

namespace Phy
{
	public class BezierCurve
	{
		public BezierCurve(int order = 2)
		{
			order = ((order >= 2) ? order : 2);
			this.order = order;
			this.AnchorPoints = new Vector3[order + 1];
			this.tk = new float[order + 1];
			this.sk = new float[order + 1];
			this.Binomial = new float[order + 1];
			this.DerivativeBinomial = new float[order];
			int num = order - 1;
			this.DerivativeBinomial[0] = 1f;
			this.DerivativeBinomial[num] = 1f;
			this.Binomial[0] = 1f;
			this.Binomial[order] = 1f;
			if (order == 2)
			{
				this.Binomial[1] = 2f;
			}
			else
			{
				float num2 = (float)num;
				int i = 1;
				int num3 = num - i;
				while (i < num3 - 1)
				{
					this.DerivativeBinomial[i++] = num2;
					this.DerivativeBinomial[num3] = num2;
					num2 *= (float)num3--;
					num2 /= (float)i;
				}
				this.DerivativeBinomial[i] = num2;
				if (num3 > i)
				{
					this.DerivativeBinomial[num3] = num2;
				}
				num2 = (float)order;
				i = 1;
				num3 = order - i;
				while (i < num3 - 1)
				{
					this.Binomial[i++] = num2;
					this.Binomial[num3--] = num2;
					num2 = this.DerivativeBinomial[i] + this.DerivativeBinomial[i - 1];
				}
				this.Binomial[i] = num2;
				if (num3 > i)
				{
					this.Binomial[num3] = num2;
				}
			}
		}

		public int Order
		{
			get
			{
				return this.order;
			}
		}

		public int NumberOfPoints
		{
			get
			{
				return this.order + 1;
			}
		}

		public void SetT(float t)
		{
			float num = 1f - t;
			float num2 = 1f;
			float num3 = 1f;
			this.tk[0] = num2;
			this.sk[0] = num3;
			for (int i = 1; i <= this.order; i++)
			{
				num2 *= t;
				num3 *= num;
				this.tk[i] = num2;
				this.sk[i] = num3;
			}
		}

		public Vector3 Point()
		{
			Vector3 vector = Vector3.zero;
			for (int i = 0; i <= this.order; i++)
			{
				vector += this.Binomial[i] * this.tk[i] * this.sk[this.order - i] * this.AnchorPoints[i];
			}
			return vector;
		}

		public Vector3 Derivative()
		{
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < this.order; i++)
			{
				vector += this.DerivativeBinomial[i] * this.tk[i] * this.sk[this.order - 1 - i] * (this.AnchorPoints[i + 1] - this.AnchorPoints[i]);
			}
			return vector * (float)this.order;
		}

		public Vector3 Point(float t)
		{
			this.SetT(t);
			return this.Point();
		}

		public Vector3 Derivative(float t)
		{
			this.SetT(t);
			return this.Derivative();
		}

		public Quaternion Rotation(float t)
		{
			this.SetT(t);
			return this.Rotation();
		}

		public Quaternion Rotation()
		{
			Vector3 normalized = this.Derivative().normalized;
			Vector3 vector;
			vector..ctor(1f, 0f, 0f);
			Vector3 normalized2 = Vector3.Cross(normalized, vector).normalized;
			float magnitude = normalized2.magnitude;
			if (!Mathf.Approximately(magnitude, 0f))
			{
				return Quaternion.LookRotation(normalized, normalized2);
			}
			return Quaternion.identity;
		}

		public virtual Vector3 CurvedCylinderTransform(Vector3 point)
		{
			Vector3 normalized = this.Derivative().normalized;
			Vector3 normalized2 = Vector3.Cross(Vector3.forward, normalized).normalized;
			float num = Mathf.Acos(Vector3.Dot(Vector3.forward, normalized));
			Quaternion quaternion = Quaternion.AngleAxis(num * 57.29578f, normalized2);
			Vector3 vector;
			vector..ctor(point.x, point.y, 0f);
			return this.Point() + quaternion * vector;
		}

		public float CalculateLength(int iterations, out float[] path)
		{
			float num = 0f;
			path = new float[iterations + 1];
			path[0] = 0f;
			float num2 = 1f / (float)iterations;
			for (int i = 0; i < iterations; i++)
			{
				float num3 = num2 * (float)i;
				this.SetT(num3);
				num += this.Derivative().magnitude * num2;
				path[i + 1] = num;
			}
			return num;
		}

		public void BuildReparametrizationCurveMap(int size, int iterations)
		{
			float[] array;
			float num = this.CalculateLength(iterations, out array);
			if (this.reparamMap == null || this.reparamMap.Length != size + 1)
			{
				this.reparamMap = new float[size + 1];
			}
			int num2 = 1;
			for (int i = 0; i < size; i++)
			{
				float num3 = (float)i / (float)size;
				while (num3 > array[num2] / num)
				{
					num2++;
					if (num2 >= array.Length)
					{
						Debug.LogError("BuildReparametrizationCurveMap: cannot reach path for x = " + num3);
						return;
					}
				}
				float num4 = (float)(num2 - 1) / (float)iterations;
				float num5 = (float)num2 / (float)iterations;
				float num6 = array[num2 - 1] / num;
				float num7 = array[num2] / num;
				this.reparamMap[i] = Mathf.Lerp(num4, num5, (num3 - num6) / (num7 - num6));
			}
			this.reparamMap[size] = 1f;
		}

		public float SampleReparametrizationCurveMap(float pos)
		{
			return MathHelper.FloatArrayLerp(this.reparamMap, pos);
		}

		protected int order;

		protected float[] tk;

		protected float[] sk;

		public float[] Binomial;

		public float[] DerivativeBinomial;

		public Vector3[] AnchorPoints;

		protected float[] reparamMap;
	}
}
