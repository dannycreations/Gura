using System;
using UnityEngine;

namespace ComplexMathFFT
{
	public struct ComplexPolar
	{
		public ComplexPolar(float r, float phi = 0f)
		{
			this.r = r;
			this.phi = phi;
		}

		public static ComplexPolar Zero
		{
			get
			{
				return new ComplexPolar(0f, 0f);
			}
		}

		public static ComplexPolar One
		{
			get
			{
				return new ComplexPolar(1f, 0f);
			}
		}

		public static ComplexPolar ImUnit
		{
			get
			{
				return new ComplexPolar(0f, 1.5707964f);
			}
		}

		public Complex AsCartesian()
		{
			return new Complex(this.r * Mathf.Cos(this.phi), this.r * Mathf.Sin(this.phi));
		}

		public static bool operator ==(ComplexPolar a, ComplexPolar b)
		{
			return a.r == b.r && a.phi == b.phi;
		}

		public static bool operator !=(ComplexPolar a, ComplexPolar b)
		{
			return a.r != b.r || a.phi != b.phi;
		}

		public static ComplexPolar operator *(ComplexPolar a, ComplexPolar b)
		{
			return new ComplexPolar(a.r * b.r, a.phi + b.phi);
		}

		public static ComplexPolar operator /(ComplexPolar a, ComplexPolar b)
		{
			return new ComplexPolar(a.r / b.r, a.phi - b.phi);
		}

		public float r;

		public float phi;
	}
}
