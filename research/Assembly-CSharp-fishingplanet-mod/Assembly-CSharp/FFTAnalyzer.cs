using System;
using System.Diagnostics;
using System.Threading;
using ComplexMathFFT;
using UnityEngine;

public class FFTAnalyzer
{
	public FFTAnalyzer(int bufferSize, int refreshRate, int LowestFreq, int HalfFreqInterval)
	{
		this.buffer = new float[bufferSize];
		this.refreshRate = refreshRate;
		this.bufferStart = 0;
		this.bufferCounter = 0;
		this.refreshRate = refreshRate;
		this.refreshCounter = 0;
		this.amplitudes = new float[bufferSize];
		this.t_stopwatch = new Stopwatch();
		this.upd_stopwatch = new Stopwatch();
		this.t_buffer = new float[bufferSize];
		this.t_amplitudes = new float[bufferSize];
		this.t_filter = new FourierAnalysis.WindowFilter.Lanczos(bufferSize);
		this.t_fft = new Thread(new ThreadStart(this.fftThreadProc));
		this.t_fft.IsBackground = true;
		this.t_fft.Name = "FFTAnalyzer";
		this.LowestFreq = LowestFreq;
		this.HalfFreqInterval = HalfFreqInterval;
	}

	public float SNR { get; private set; }

	public float LowOscillationsScore { get; private set; }

	public float HighOscillationsScore { get; private set; }

	public float FFTThreadTimeMilliseconds { get; private set; }

	public float[] AmplitudesArray()
	{
		float[] array = new float[this.amplitudes.Length];
		object obj = this.amplitudes;
		lock (obj)
		{
			Array.Copy(this.amplitudes, array, this.amplitudes.Length);
		}
		return array;
	}

	public float[] SignalArray()
	{
		float[] array = new float[this.buffer.Length];
		object obj = this.buffer;
		lock (obj)
		{
			Array.Copy(this.buffer, this.bufferStart, array, 0, this.buffer.Length - this.bufferStart);
			if (this.bufferStart > 0)
			{
				Array.Copy(this.buffer, 0, array, this.buffer.Length - this.bufferStart, this.bufferStart);
			}
		}
		this.normalizeSignal(array);
		return array;
	}

	public float Amplitude(int index)
	{
		object obj = this.amplitudes;
		float num;
		lock (obj)
		{
			num = this.amplitudes[index];
		}
		return num;
	}

	public float AmplitudesSample(int centerIndex, FourierAnalysis.WindowFilter.Base filter)
	{
		float[] array = new float[filter.Size];
		object obj = this.amplitudes;
		lock (obj)
		{
			Array.Copy(this.amplitudes, centerIndex - filter.Size / 2, array, 0, filter.Size);
		}
		return filter.Convolve(array);
	}

	public float[] AmplitudesRaw(int firstFreq, int count)
	{
		float[] array = new float[count];
		object obj = this.amplitudes;
		lock (obj)
		{
			Array.Copy(this.amplitudes, firstFreq, array, 0, count);
		}
		return array;
	}

	public float AmplitudePeak(int firstFreq, int count, out int peakIndex)
	{
		peakIndex = -1;
		float num = 0f;
		object obj = this.amplitudes;
		lock (obj)
		{
			for (int i = firstFreq; i < firstFreq + count; i++)
			{
				if (peakIndex == -1 || num < this.amplitudes[i])
				{
					peakIndex = i;
					num = this.amplitudes[i];
				}
			}
		}
		return num;
	}

	public bool Update(float[] values)
	{
		this.upd_stopwatch.Reset();
		this.upd_stopwatch.Start();
		object obj = this.buffer;
		lock (obj)
		{
			for (int i = 0; i < values.Length; i++)
			{
				if (this.bufferCounter < this.buffer.Length)
				{
					this.bufferCounter++;
				}
				this.buffer[this.bufferStart] = values[i];
				this.bufferStart++;
				if (this.bufferStart == this.buffer.Length)
				{
					this.bufferStart = 0;
				}
			}
		}
		bool flag = false;
		this.refreshCounter++;
		if (this.bufferCounter == this.buffer.Length && this.refreshCounter >= this.refreshRate && !this.t_fft.IsAlive && !this.t_isrunning)
		{
			this.FFTThreadTimeMilliseconds = (float)this.t_elapsed;
			this.LowOscillationsScore = this.calcOscillationsScore(this.LowestFreq, this.HalfFreqInterval * 2, 0.3f);
			this.HighOscillationsScore = this.calcOscillationsScore(this.LowestFreq + this.HalfFreqInterval, this.HalfFreqInterval * 2, 0.3f);
			flag = true;
			this.refreshCounter = 0;
			this.t_fft = new Thread(new ThreadStart(this.fftThreadProc));
			this.t_fft.Start();
			this.FFTThreadTimeMilliseconds += (float)this.upd_stopwatch.ElapsedTicks;
		}
		return flag;
	}

	public void FillBuffer(float fillValue = 0f)
	{
		object obj = this.buffer;
		lock (obj)
		{
			this.bufferStart = 0;
			this.bufferCounter = this.buffer.Length;
			for (int i = 0; i < this.buffer.Length; i++)
			{
				this.buffer[i] = fillValue;
			}
		}
	}

	private void normalizeSignal(float[] buf)
	{
		float num = buf[0];
		float num2 = buf[buf.Length - 1];
		for (int i = 0; i < buf.Length; i++)
		{
			buf[i] -= Mathf.Lerp(num, num2, (float)i / (float)buf.Length);
		}
		float num3 = 0f;
		for (int j = 0; j < buf.Length; j++)
		{
			num3 += buf[j];
		}
		num3 /= (float)buf.Length;
		for (int k = 0; k < buf.Length; k++)
		{
			buf[k] -= num3;
		}
	}

	private float calcOscillationsScore(int startFreq, int countFreq, float minScore = 0.3f)
	{
		int num2;
		float num = this.AmplitudePeak(startFreq, countFreq, out num2);
		float num3 = (float)((num2 - startFreq) / countFreq);
		return num * ((1f + minScore - Mathf.Abs(2f * num3 - 1f)) / (1f + minScore));
	}

	private float calcSNR(int startFreq, int countFreq)
	{
		int num2;
		float num = this.AmplitudePeak(startFreq, countFreq, out num2);
		float[] array = this.AmplitudesRaw(startFreq, countFreq);
		float num3 = 0f;
		for (int i = 0; i < array.Length; i++)
		{
			if (startFreq + i != num2)
			{
				num3 += array[i] * array[i];
			}
		}
		if (!Mathf.Approximately(num3, 0f))
		{
			return num * num / num3;
		}
		return 0f;
	}

	private void fftThreadProc()
	{
		try
		{
			this.t_stopwatch.Reset();
			this.t_stopwatch.Start();
			this.t_isrunning = true;
			object obj = this.buffer;
			lock (obj)
			{
				Array.Copy(this.buffer, this.bufferStart, this.t_buffer, 0, this.buffer.Length - this.bufferStart);
				if (this.bufferStart > 0)
				{
					Array.Copy(this.buffer, 0, this.t_buffer, this.buffer.Length - this.bufferStart, this.bufferStart);
				}
			}
			this.normalizeSignal(this.t_buffer);
			this.t_filter.Apply(this.t_buffer);
			this.t_amplitudes = Complex.GetAmplitudes(FourierAnalysis.R2DITFFT(this.t_buffer, -1, 1, 0));
			object obj2 = this.amplitudes;
			lock (obj2)
			{
				Array.Copy(this.t_amplitudes, this.amplitudes, this.amplitudes.Length);
			}
			this.t_isrunning = false;
			this.t_elapsed = this.t_stopwatch.ElapsedTicks;
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Concat(new object[] { "fftThreadProc exception:", ex, "n/", ex.StackTrace }));
		}
		finally
		{
			this.t_isrunning = false;
		}
	}

	private float[] buffer;

	private int bufferStart;

	private int bufferCounter;

	private int refreshRate;

	private int refreshCounter;

	private float[] amplitudes;

	private Thread t_fft;

	private float[] t_buffer;

	private float[] t_amplitudes;

	private FourierAnalysis.WindowFilter.Base t_filter;

	private volatile bool t_isrunning;

	private int LowestFreq;

	private int HalfFreqInterval;

	private Stopwatch t_stopwatch;

	private Stopwatch upd_stopwatch;

	private long t_elapsed;

	private const float OscillationAmpThreshold = 10f;
}
