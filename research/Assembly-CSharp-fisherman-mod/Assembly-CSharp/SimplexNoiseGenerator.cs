using System;
using UnityEngine;

public class SimplexNoiseGenerator
{
	public SimplexNoiseGenerator()
	{
		if (this.T == null)
		{
			Random random = new Random();
			this.T = new int[8];
			for (int i = 0; i < 8; i++)
			{
				this.T[i] = random.Next();
			}
		}
	}

	public SimplexNoiseGenerator(string seed)
	{
		this.T = new int[8];
		string[] array = seed.Split(new char[] { ' ' });
		for (int i = 0; i < 8; i++)
		{
			int num;
			try
			{
				num = int.Parse(array[i]);
			}
			catch
			{
				num = 0;
			}
			this.T[i] = num;
		}
	}

	public SimplexNoiseGenerator(int[] seed)
	{
		this.T = seed;
	}

	public string GetSeed()
	{
		string text = string.Empty;
		for (int i = 0; i < 8; i++)
		{
			text += this.T[i].ToString();
			if (i < 7)
			{
				text += " ";
			}
		}
		return text;
	}

	public float coherentNoise(float x, float y, float z, int octaves = 1, int multiplier = 25, float amplitude = 0.5f, float lacunarity = 2f, float persistence = 0.9f)
	{
		Vector3 vector = new Vector3(x, y, z) / (float)multiplier;
		float num = 0f;
		for (int i = 0; i < octaves; i++)
		{
			num += this.noise(vector.x, vector.y, vector.z) * amplitude;
			vector *= lacunarity;
			amplitude *= persistence;
		}
		return num;
	}

	public int getDensity(Vector3 loc)
	{
		float num = this.coherentNoise(loc.x, loc.y, loc.z, 1, 25, 0.5f, 2f, 0.9f);
		return (int)Mathf.Lerp(0f, 255f, num);
	}

	public float noise(float x, float y, float z)
	{
		this.s = (x + y + z) * this.onethird;
		this.i = this.fastfloor(x + this.s);
		this.j = this.fastfloor(y + this.s);
		this.k = this.fastfloor(z + this.s);
		this.s = (float)(this.i + this.j + this.k) * this.onesixth;
		this.u = x - (float)this.i + this.s;
		this.v = y - (float)this.j + this.s;
		this.w = z - (float)this.k + this.s;
		this.A[0] = 0;
		this.A[1] = 0;
		this.A[2] = 0;
		int num = ((this.u < this.w) ? ((this.v < this.w) ? 2 : 1) : ((this.u < this.v) ? 1 : 0));
		int num2 = ((this.u >= this.w) ? ((this.v >= this.w) ? 2 : 1) : ((this.u >= this.v) ? 1 : 0));
		return this.kay(num) + this.kay(3 - num - num2) + this.kay(num2) + this.kay(0);
	}

	private float kay(int a)
	{
		this.s = (float)(this.A[0] + this.A[1] + this.A[2]) * this.onesixth;
		float num = this.u - (float)this.A[0] + this.s;
		float num2 = this.v - (float)this.A[1] + this.s;
		float num3 = this.w - (float)this.A[2] + this.s;
		float num4 = 0.6f - num * num - num2 * num2 - num3 * num3;
		int num5 = this.shuffle(this.i + this.A[0], this.j + this.A[1], this.k + this.A[2]);
		this.A[a]++;
		if (num4 < 0f)
		{
			return 0f;
		}
		int num6 = (num5 >> 5) & 1;
		int num7 = (num5 >> 4) & 1;
		int num8 = (num5 >> 3) & 1;
		int num9 = (num5 >> 2) & 1;
		int num10 = num5 & 3;
		float num11 = ((num10 != 1) ? ((num10 != 2) ? num3 : num2) : num);
		float num12 = ((num10 != 1) ? ((num10 != 2) ? num : num3) : num2);
		float num13 = ((num10 != 1) ? ((num10 != 2) ? num2 : num) : num3);
		num11 = ((num6 != num8) ? num11 : (-num11));
		num12 = ((num6 != num7) ? num12 : (-num12));
		num13 = ((num6 == (num7 ^ num8)) ? num13 : (-num13));
		num4 *= num4;
		return 8f * num4 * num4 * (num11 + ((num10 != 0) ? ((num9 != 0) ? num13 : num12) : (num12 + num13)));
	}

	private int shuffle(int i, int j, int k)
	{
		return this.b(i, j, k, 0) + this.b(j, k, i, 1) + this.b(k, i, j, 2) + this.b(i, j, k, 3) + this.b(j, k, i, 4) + this.b(k, i, j, 5) + this.b(i, j, k, 6) + this.b(j, k, i, 7);
	}

	private int b(int i, int j, int k, int B)
	{
		return this.T[(this.b(i, B) << 2) | (this.b(j, B) << 1) | this.b(k, B)];
	}

	private int b(int N, int B)
	{
		return (N >> B) & 1;
	}

	private int fastfloor(float n)
	{
		return (n <= 0f) ? ((int)n - 1) : ((int)n);
	}

	private int[] A = new int[3];

	private float s;

	private float u;

	private float v;

	private float w;

	private int i;

	private int j;

	private int k;

	private float onethird = 0.33333334f;

	private float onesixth = 0.16666667f;

	private int[] T;
}
