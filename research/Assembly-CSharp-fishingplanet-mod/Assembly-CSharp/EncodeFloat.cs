using System;
using System.IO;
using UnityEngine;

public static class EncodeFloat
{
	public static void WriteIntoRenderTexture(RenderTexture tex, int channels, float[] data)
	{
		if (tex == null)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture - RenderTexture is null");
			return;
		}
		if (data == null)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture - Data is null");
			return;
		}
		if (channels < 1 || channels > 4)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture - Channels must be 1, 2, 3, or 4");
			return;
		}
		int width = tex.width;
		int height = tex.height;
		int num = width * height * channels;
		Color[] array = new Color[num];
		float num2 = 1f;
		float num3 = 0f;
		EncodeFloat.LoadData(data, array, num, ref num3, ref num2);
		EncodeFloat.DecodeFloat(width, height, channels, num3, num2, tex, array);
	}

	public static void WriteIntoRenderTexture(RenderTexture tex, int channels, string path, float[] fdata = null)
	{
		if (tex == null)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture- RenderTexture is null");
			return;
		}
		if (channels < 1 || channels > 4)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture - Channels must be 1, 2, 3, or 4");
			return;
		}
		int width = tex.width;
		int height = tex.height;
		int num = width * height * channels;
		Color[] array = new Color[num];
		if (fdata == null)
		{
			fdata = new float[num];
		}
		float num2 = 1f;
		float num3 = 0f;
		if (!EncodeFloat.LoadRawFile(path, array, num, ref num3, ref num2, fdata))
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture - Error loading raw file " + path);
			return;
		}
		EncodeFloat.DecodeFloat(width, height, channels, num3, num2, tex, array);
	}

	public static void WriteIntoRenderTexture16bit(RenderTexture tex, int channels, string path, bool bigEndian, float[] fdata = null)
	{
		if (tex == null)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture16bit- RenderTexture is null");
			return;
		}
		if (channels < 1 || channels > 4)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture16bit - Channels must be 1, 2, 3, or 4");
			return;
		}
		int width = tex.width;
		int height = tex.height;
		int num = width * height * channels;
		Color[] array = new Color[num];
		if (fdata == null)
		{
			fdata = new float[num];
		}
		float num2 = 1f;
		float num3 = 0f;
		if (!EncodeFloat.LoadRawFile16(path, array, num, ref num3, ref num2, fdata, bigEndian))
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture16bit - Error loading raw file " + path);
			return;
		}
		EncodeFloat.DecodeFloat(width, height, channels, num3, num2, tex, array);
	}

	public static void ReadFromRenderTexture(RenderTexture tex, int channels, float[] data)
	{
		if (tex == null)
		{
			Debug.Log("EncodeFloat::ReadFromRenderTexture - RenderTexture is null");
			return;
		}
		if (data == null)
		{
			Debug.Log("EncodeFloat::ReadFromRenderTexture - Data is null");
			return;
		}
		if (channels < 1 || channels > 4)
		{
			Debug.Log("EncodeFloat::ReadFromRenderTexture - Channels must be 1, 2, 3, or 4");
			return;
		}
		if (EncodeFloat.m_encodeToFloat == null)
		{
			Shader shader = Shader.Find("EncodeFloat/EncodeToFloat");
			if (shader == null)
			{
				Debug.Log("EncodeFloat::ReadFromRenderTexture - could not find shader EncodeFloat/EncodeToFloat. Did you change the shaders name?");
				return;
			}
			EncodeFloat.m_encodeToFloat = new Material(shader);
		}
		int width = tex.width;
		int height = tex.height;
		if (!EncodeFloat.encodeTex)
		{
			EncodeFloat.encodeTex = new RenderTexture(width, height, 0, 0, 1);
		}
		RTUtility.ClearColor(EncodeFloat.encodeTex);
		EncodeFloat.encodeTex.filterMode = 0;
		if (!EncodeFloat.readTex)
		{
			EncodeFloat.readTex = new Texture2D(width, height, 5, false, true);
		}
		Vector4 vector;
		vector..ctor(1f, 0.003921569f, 1.53787E-05f, 6.2273724E-09f);
		for (int i = 0; i < channels; i++)
		{
			Graphics.Blit(tex, EncodeFloat.encodeTex, EncodeFloat.m_encodeToFloat, i);
			RenderTexture.active = EncodeFloat.encodeTex;
			EncodeFloat.readTex.ReadPixels(new Rect(0f, 0f, (float)width, (float)height), 0, 0);
			EncodeFloat.readTex.Apply();
			RenderTexture.active = null;
			for (int j = 0; j < width; j++)
			{
				for (int k = 0; k < height; k++)
				{
					data[k + j * height + i * width * height] = Vector4.Dot(EncodeFloat.readTex.GetPixel(j, k), vector);
				}
			}
		}
	}

	private static void DecodeFloat(int w, int h, int c, float min, float max, RenderTexture tex, Color[] map)
	{
		Color[] array = new Color[w * h];
		Color[] array2 = new Color[w * h];
		Color[] array3 = new Color[w * h];
		Color[] array4 = new Color[w * h];
		for (int i = 0; i < w; i++)
		{
			for (int j = 0; j < h; j++)
			{
				array[i + j * w] = new Color(0f, 0f, 0f, 0f);
				array2[i + j * w] = new Color(0f, 0f, 0f, 0f);
				array3[i + j * w] = new Color(0f, 0f, 0f, 0f);
				array4[i + j * w] = new Color(0f, 0f, 0f, 0f);
				if (c > 0)
				{
					array[i + j * w] = map[(i + j * w) * c];
				}
				if (c > 1)
				{
					array2[i + j * w] = map[(i + j * w) * c + 1];
				}
				if (c > 2)
				{
					array3[i + j * w] = map[(i + j * w) * c + 2];
				}
				if (c > 3)
				{
					array4[i + j * w] = map[(i + j * w) * c + 3];
				}
			}
		}
		Texture2D texture2D = new Texture2D(w, h, 5, false, true);
		texture2D.filterMode = 0;
		texture2D.wrapMode = 1;
		texture2D.SetPixels(array);
		texture2D.Apply();
		Texture2D texture2D2 = new Texture2D(w, h, 5, false, true);
		texture2D2.filterMode = 0;
		texture2D2.wrapMode = 1;
		texture2D2.SetPixels(array2);
		texture2D2.Apply();
		Texture2D texture2D3 = new Texture2D(w, h, 5, false, true);
		texture2D3.filterMode = 0;
		texture2D3.wrapMode = 1;
		texture2D3.SetPixels(array3);
		texture2D3.Apply();
		Texture2D texture2D4 = new Texture2D(w, h, 5, false, true);
		texture2D4.filterMode = 0;
		texture2D4.wrapMode = 1;
		texture2D4.SetPixels(array4);
		texture2D4.Apply();
		if (EncodeFloat.m_decodeToFloat == null)
		{
			Shader shader = Shader.Find("EncodeFloat/DecodeToFloat");
			if (shader == null)
			{
				Debug.Log("EncodeFloat::WriteIntoRenderTexture2D - could not find shader EncodeFloat/DecodeToFloat. Did you change the shaders name?");
				return;
			}
			EncodeFloat.m_decodeToFloat = new Material(shader);
		}
		EncodeFloat.m_decodeToFloat.SetFloat("_Max", max);
		EncodeFloat.m_decodeToFloat.SetFloat("_Min", min);
		EncodeFloat.m_decodeToFloat.SetTexture("_TexR", texture2D);
		EncodeFloat.m_decodeToFloat.SetTexture("_TexG", texture2D2);
		EncodeFloat.m_decodeToFloat.SetTexture("_TexB", texture2D3);
		EncodeFloat.m_decodeToFloat.SetTexture("_TexA", texture2D4);
		Graphics.Blit(null, tex, EncodeFloat.m_decodeToFloat);
	}

	private static float[] EncodeFloatRGBA(float val)
	{
		float[] array = new float[] { 1f, 255f, 65025f, 160581380f };
		float num = 0.003921569f;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] *= val;
			array[i] = (float)((double)array[i] - Math.Truncate((double)array[i]));
		}
		float[] array2 = new float[]
		{
			array[1],
			array[2],
			array[3],
			array[3]
		};
		for (int j = 0; j < array.Length; j++)
		{
			array[j] -= array2[j] * num;
		}
		return array;
	}

	private static void LoadData(float[] data, Color[] map, int size, ref float min, ref float max)
	{
		for (int i = 0; i < size; i++)
		{
			if (data[i] > max)
			{
				max = data[i];
			}
			if (data[i] < min)
			{
				min = data[i];
			}
		}
		min = Mathf.Abs(min);
		max += min;
		for (int j = 0; j < size; j++)
		{
			float num = (data[j] + min) / max;
			if (num >= 1f)
			{
				num = 0.999999f;
			}
			float[] array = EncodeFloat.EncodeFloatRGBA(num);
			map[j] = new Color(array[0], array[1], array[2], array[3]);
		}
	}

	private static bool LoadRawFile(string path, Color[] map, int size, ref float min, ref float max, float[] fdata)
	{
		FileInfo fileInfo = new FileInfo(path);
		if (fileInfo == null)
		{
			Debug.Log("EncodeFloat::LoadRawFile - Raw file not found");
			return false;
		}
		FileStream fileStream = fileInfo.OpenRead();
		byte[] array = new byte[fileInfo.Length];
		fileStream.Read(array, 0, (int)fileInfo.Length);
		fileStream.Close();
		if ((long)size > fileInfo.Length / 4L)
		{
			Debug.Log("EncodeFloat::LoadRawFile - Raw file is not the required size");
			return false;
		}
		int num = 0;
		for (int i = 0; i < size; i++)
		{
			fdata[i] = BitConverter.ToSingle(array, num);
			if (fdata[i] > max)
			{
				max = fdata[i];
			}
			if (fdata[i] < min)
			{
				min = fdata[i];
			}
			num += 4;
		}
		min = Mathf.Abs(min);
		max += min;
		for (int j = 0; j < size; j++)
		{
			float num2 = (fdata[j] + min) / max;
			if (num2 >= 1f)
			{
				num2 = 0.999999f;
			}
			float[] array2 = EncodeFloat.EncodeFloatRGBA(num2);
			map[j] = new Color(array2[0], array2[1], array2[2], array2[3]);
		}
		return true;
	}

	private static bool LoadRawFile16(string path, Color[] map, int size, ref float min, ref float max, float[] fdata, bool bigendian)
	{
		FileInfo fileInfo = new FileInfo(path);
		if (fileInfo == null)
		{
			Debug.Log("EncodeFloat::LoadRawFile16 - Raw file not found");
			return false;
		}
		FileStream fileStream = fileInfo.OpenRead();
		byte[] array = new byte[fileInfo.Length];
		fileStream.Read(array, 0, (int)fileInfo.Length);
		fileStream.Close();
		if ((long)size > fileInfo.Length / 2L)
		{
			Debug.Log("EncodeFloat::LoadRawFile16 - Raw file is not the required size");
			return false;
		}
		int num = 0;
		for (int i = 0; i < size; i++)
		{
			fdata[i] = ((!bigendian) ? ((float)array[num++] + (float)array[num++] * 256f) : ((float)array[num++] * 256f + (float)array[num++]));
			fdata[i] /= 65535f;
			if (fdata[i] > max)
			{
				max = fdata[i];
			}
			if (fdata[i] < min)
			{
				min = fdata[i];
			}
		}
		min = Mathf.Abs(min);
		max += min;
		for (int j = 0; j < size; j++)
		{
			float num2 = (fdata[j] + min) / max;
			float[] array2 = EncodeFloat.EncodeFloatRGBA(num2);
			map[j] = new Color(array2[0], array2[1], array2[2], array2[3]);
		}
		return true;
	}

	private static Material m_decodeToFloat;

	private static Material m_encodeToFloat;

	private static RenderTexture encodeTex;

	private static Texture2D readTex;
}
