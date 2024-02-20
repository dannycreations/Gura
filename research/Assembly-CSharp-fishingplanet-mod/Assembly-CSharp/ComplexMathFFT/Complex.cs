using System;
using UnityEngine;

namespace ComplexMathFFT
{
	public struct Complex
	{
		public Complex(float re, float im = 0f)
		{
			this.re = re;
			this.im = im;
		}

		public Complex(Vector2 v)
		{
			this.re = v.x;
			this.im = v.y;
		}

		public static Complex Zero
		{
			get
			{
				return new Complex(0f, 0f);
			}
		}

		public static Complex One
		{
			get
			{
				return new Complex(1f, 0f);
			}
		}

		public static Complex ImUnit
		{
			get
			{
				return new Complex(0f, 1f);
			}
		}

		public static explicit operator Vector2(Complex c)
		{
			return c.AsVector2();
		}

		public Vector2 AsVector2()
		{
			return new Vector2(this.re, this.im);
		}

		public static explicit operator Complex(Vector2 v)
		{
			return Complex.FromVector2(v);
		}

		public static Complex FromVector2(Vector2 v)
		{
			return new Complex(v.x, v.y);
		}

		public ComplexPolar AsPolar()
		{
			return new ComplexPolar(this.Magnitude(), Mathf.Atan2(this.im, this.re));
		}

		public static Complex operator +(Complex a, Complex b)
		{
			return new Complex(a.re + b.re, a.im + b.im);
		}

		public static Complex operator -(Complex a, Complex b)
		{
			return new Complex(a.re - b.re, a.im - b.im);
		}

		public static Complex operator -(Complex a)
		{
			return new Complex(-a.re, -a.im);
		}

		public static bool operator ==(Complex a, Complex b)
		{
			return a.re == b.re && a.im == b.im;
		}

		public static bool operator !=(Complex a, Complex b)
		{
			return a.re != b.re || a.im != b.im;
		}

		public static Complex Conjugate(Complex a)
		{
			return new Complex(a.re, -a.im);
		}

		public void Conjugate()
		{
			this.im = -this.im;
		}

		public static Complex operator *(Complex a, Complex b)
		{
			return new Complex(a.re * b.re - a.im * b.im, a.im * b.re + a.re * b.im);
		}

		public static Complex operator /(Complex a, Complex b)
		{
			float num = b.re * b.re + b.im * b.im;
			return new Complex((a.re * b.re + a.im * b.im) / num, (a.im * b.re - a.re * b.im) / num);
		}

		public float Magnitude()
		{
			return Mathf.Sqrt(this.re * this.re + this.im * this.im);
		}

		public override string ToString()
		{
			return string.Concat(new object[] { this.re, " + ", this.im, "i" });
		}

		public static float[] GetAmplitudes(Complex[] array)
		{
			float[] array2 = new float[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = array[i].Magnitude();
			}
			return array2;
		}

		public static Complex[] RealsToComplex(float[] array)
		{
			Complex[] array2 = new Complex[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = new Complex(array[i], 0f);
			}
			return array2;
		}

		public float re;

		public float im;
	}
}
