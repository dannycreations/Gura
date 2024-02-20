using System;
using Mono.Simd;
using UnityEngine;

namespace ComplexMathFFT
{
	public static class FourierAnalysis
	{
		public static Complex[] R2DITFFT(float[] signal, int count = -1, int step = 1, int offset = 0)
		{
			if (count < 0)
			{
				count = signal.Length;
				if (!Mathf.IsPowerOfTwo(count))
				{
					throw new ArgumentException("Signal length must be a power of 2", "signal");
				}
			}
			if (count == 1)
			{
				return new Complex[]
				{
					new Complex(signal[offset], 0f)
				};
			}
			Complex[] array = new Complex[count];
			int num = count / 2;
			Array.Copy(FourierAnalysis.R2DITFFT(signal, num, step * 2, offset), 0, array, 0, num);
			Array.Copy(FourierAnalysis.R2DITFFT(signal, num, step * 2, offset + step), 0, array, num, num);
			for (int i = 0; i < num; i++)
			{
				Complex complex = array[i];
				ComplexPolar complexPolar = new ComplexPolar(1f, -6.2831855f * (float)i / (float)count);
				Complex complex2 = complexPolar.AsCartesian() * array[i + num];
				array[i] = complex + complex2;
				array[i + num] = complex - complex2;
			}
			return array;
		}

		public static class WindowFilter
		{
			public abstract class Base
			{
				protected Base(int Size)
				{
					if (Size < 4)
					{
						throw new ArgumentException("Filter window size cannot be less than 4", "Size");
					}
					this.Size = Size;
					this.precomputeWindow();
				}

				public int Size { get; private set; }

				protected void precomputeWindow()
				{
					this.window = new Vector4f[this.Size / 4];
					float num = (float)(this.Size - 1);
					float num2 = 1f / num;
					for (int i = 0; i < this.Size; i++)
					{
						float num3 = (float)i / num;
						int num4 = i / 4;
						this.window[num4].set_Component(i - num4 * 4, this.WindowFunction(num3));
					}
				}

				public abstract float WindowFunction(float x);

				public void Apply(float[] signal)
				{
					for (int i = 0; i < signal.Length / 4; i++)
					{
						int num = i * 4;
						Vector4f vector4f;
						vector4f..ctor(signal[num], signal[num + 1], signal[num + 2], signal[num + 3]);
						vector4f *= this.window[i];
						signal[num] = vector4f.get_Component(0);
						signal[num + 1] = vector4f.get_Component(1);
						signal[num + 2] = vector4f.get_Component(2);
						signal[num + 3] = vector4f.get_Component(3);
					}
				}

				public float Convolve(float[] signal)
				{
					Vector4f vector4f;
					vector4f..ctor(0f);
					for (int i = 0; i < signal.Length / 4; i++)
					{
						int num = i * 4;
						Vector4f vector4f2;
						vector4f2..ctor(signal[num], signal[num + 1], signal[num + 2], signal[num + 3]);
						vector4f2 *= this.window[i];
						vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f2);
					}
					vector4f = VectorOperations.HorizontalAdd(vector4f, vector4f);
					return VectorOperations.HorizontalAdd(vector4f, vector4f).X;
				}

				protected Vector4f[] window;
			}

			public class Inverter : FourierAnalysis.WindowFilter.Base
			{
				public Inverter(FourierAnalysis.WindowFilter.Base source)
					: base(source.Size)
				{
					this.source = source;
					base.precomputeWindow();
				}

				public override float WindowFunction(float x)
				{
					if (this.source != null)
					{
						return 1f - this.source.WindowFunction(x);
					}
					return 0f;
				}

				private FourierAnalysis.WindowFilter.Base source;
			}

			public class Rectangular : FourierAnalysis.WindowFilter.Base
			{
				public Rectangular(int Size)
					: base(Size)
				{
				}

				public override float WindowFunction(float x)
				{
					return 1f;
				}
			}

			public class Triangular : FourierAnalysis.WindowFilter.Base
			{
				public Triangular(int Size)
					: base(Size)
				{
				}

				public override float WindowFunction(float x)
				{
					return 1f - Mathf.Abs(2f * x - 1f);
				}
			}

			public class GeneralizedHamming : FourierAnalysis.WindowFilter.Base
			{
				public GeneralizedHamming(int Size, float alpha, float beta)
					: base(Size)
				{
					this.alpha = alpha;
					this.beta = beta;
					base.precomputeWindow();
				}

				public override float WindowFunction(float x)
				{
					return this.alpha - this.beta * Mathf.Cos(6.2831855f * x);
				}

				protected float alpha;

				protected float beta;
			}

			public class Hann : FourierAnalysis.WindowFilter.GeneralizedHamming
			{
				public Hann(int Size)
					: base(Size, 0.5f, 0.5f)
				{
				}
			}

			public class Hamming : FourierAnalysis.WindowFilter.GeneralizedHamming
			{
				public Hamming(int Size)
					: base(Size, 0.53836f, 0.46164f)
				{
				}
			}

			public class GeneralizedBlackman : FourierAnalysis.WindowFilter.Base
			{
				public GeneralizedBlackman(int Size, float alpha0, float alpha1, float alpha2, float alpha3)
					: base(Size)
				{
					this.alpha0 = alpha0;
					this.alpha1 = alpha1;
					this.alpha2 = alpha2;
					this.alpha3 = alpha3;
					base.precomputeWindow();
				}

				public override float WindowFunction(float x)
				{
					return this.alpha0 - this.alpha1 * Mathf.Cos(6.2831855f * x) + this.alpha2 * Mathf.Cos(12.566371f * x) - this.alpha3 * Mathf.Cos(18.849556f * x);
				}

				protected float alpha0;

				protected float alpha1;

				protected float alpha2;

				protected float alpha3;
			}

			public class Blackman : FourierAnalysis.WindowFilter.GeneralizedBlackman
			{
				public Blackman(int Size)
					: base(Size, 0.42659f, 0.49656f, 0.076849f, 0f)
				{
				}
			}

			public class Nuttall : FourierAnalysis.WindowFilter.GeneralizedBlackman
			{
				public Nuttall(int Size)
					: base(Size, 0.355768f, 0.487396f, 0.144232f, 0.012604f)
				{
				}
			}

			public class BlackmanNuttall : FourierAnalysis.WindowFilter.GeneralizedBlackman
			{
				public BlackmanNuttall(int Size)
					: base(Size, 0.3635819f, 0.4891775f, 0.1365995f, 0.0106411f)
				{
				}
			}

			public class BlackmanHarris : FourierAnalysis.WindowFilter.GeneralizedBlackman
			{
				public BlackmanHarris(int Size)
					: base(Size, 0.35875f, 0.48829f, 0.14128f, 0.01168f)
				{
				}
			}

			public class SRSFlatTop : FourierAnalysis.WindowFilter.Base
			{
				public SRSFlatTop(int Size)
					: base(Size)
				{
				}

				public override float WindowFunction(float x)
				{
					return 1f - 1.93f * Mathf.Cos(6.2831855f * x) + 1.29f * Mathf.Cos(12.566371f * x) - 0.388f * Mathf.Cos(18.849556f * x) + 0.28f * Mathf.Cos(25.132742f * x);
				}

				private const float alpha0 = 1f;

				private const float alpha1 = 1.93f;

				private const float alpha2 = 1.29f;

				private const float alpha3 = 0.388f;

				private const float alpha4 = 0.28f;
			}

			public class Lanczos : FourierAnalysis.WindowFilter.Base
			{
				public Lanczos(int Size)
					: base(Size)
				{
				}

				public override float WindowFunction(float x)
				{
					float num = 2f * x - 1f;
					if (!Mathf.Approximately(num, 0f))
					{
						return Mathf.Sin(num) / num;
					}
					return 1f;
				}
			}
		}
	}
}
