using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Flowmap
{
	public class TextureUtilities
	{
		public static string[] GetSupportedFormatsWithExtension()
		{
			string[] array = new string[TextureUtilities.SupportedFormats.Length];
			for (int i = 0; i < TextureUtilities.SupportedFormats.Length; i++)
			{
				array[i] = TextureUtilities.SupportedFormats[i].name + " (*." + TextureUtilities.SupportedFormats[i].extension + ")";
			}
			return array;
		}

		public static void WriteRenderTextureToFile(RenderTexture textureToWrite, string filename, TextureUtilities.FileTextureFormat format)
		{
			TextureUtilities.WriteRenderTextureToFile(textureToWrite, filename, false, format);
		}

		public static void WriteRenderTextureToFile(RenderTexture textureToWrite, string filename, bool linear, TextureUtilities.FileTextureFormat format)
		{
			Texture2D texture2D = new Texture2D(textureToWrite.width, textureToWrite.height, 5, false, linear);
			RenderTexture temporary = RenderTexture.GetTemporary(textureToWrite.width, textureToWrite.height, 0, 0, 1);
			Graphics.Blit(textureToWrite, temporary);
			RenderTexture.active = temporary;
			texture2D.ReadPixels(new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), 0, 0);
			texture2D.Apply(false);
			TextureUtilities.WriteTexture2DToFile(texture2D, filename, format);
			if (Application.isPlaying)
			{
				Object.Destroy(texture2D);
			}
			else
			{
				Object.DestroyImmediate(texture2D);
			}
			RenderTexture.ReleaseTemporary(temporary);
		}

		public static void WriteRenderTextureToFile(RenderTexture textureToWrite, string filename, bool linear, TextureUtilities.FileTextureFormat format, string customShader)
		{
			Texture2D texture2D = new Texture2D(textureToWrite.width, textureToWrite.height, 5, false, linear);
			RenderTexture temporary = RenderTexture.GetTemporary(textureToWrite.width, textureToWrite.height, 0, 0, 1);
			Material material = new Material(Shader.Find(customShader));
			material.SetTexture("_RenderTex", textureToWrite);
			Graphics.Blit(null, temporary, material);
			if (Application.isPlaying)
			{
				Object.Destroy(material);
			}
			else
			{
				Object.DestroyImmediate(material);
			}
			RenderTexture.active = temporary;
			texture2D.ReadPixels(new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), 0, 0);
			texture2D.Apply(false);
			TextureUtilities.WriteTexture2DToFile(texture2D, filename, format);
			if (Application.isPlaying)
			{
				Object.Destroy(texture2D);
			}
			else
			{
				Object.DestroyImmediate(texture2D);
			}
			RenderTexture.ReleaseTemporary(temporary);
		}

		public static void WriteTexture2DToFile(Texture2D textureToWrite, string filename, TextureUtilities.FileTextureFormat format)
		{
			byte[] array = null;
			string name = format.name;
			if (name != null)
			{
				if (!(name == "Png"))
				{
					if (name == "Tga")
					{
						array = TextureUtilities.EncodeToTGA(textureToWrite);
					}
				}
				else
				{
					array = ImageConversion.EncodeToPNG(textureToWrite);
				}
			}
			if (!filename.EndsWith("." + format.extension))
			{
				filename = filename + "." + format.extension;
			}
			File.WriteAllBytes(filename, array);
		}

		public static Color SampleColorBilinear(Color[] data, int resolutionX, int resolutionY, float u, float v)
		{
			u = Mathf.Clamp(u * (float)(resolutionX - 1), 0f, (float)(resolutionX - 1));
			v = Mathf.Clamp(v * (float)(resolutionY - 1), 0f, (float)(resolutionY - 1));
			if (Mathf.FloorToInt(u) + resolutionX * Mathf.FloorToInt(v) >= data.Length || Mathf.FloorToInt(u) + resolutionX * Mathf.FloorToInt(v) < 0)
			{
				Debug.Log(string.Concat(new object[] { "out of range ", u, " ", v, " ", resolutionX, " ", resolutionY }));
				return Color.black;
			}
			Color color = data[Mathf.FloorToInt(u) + resolutionX * Mathf.FloorToInt(v)];
			Color color2 = data[Mathf.CeilToInt(u) + resolutionX * Mathf.FloorToInt(v)];
			Color color3 = data[Mathf.FloorToInt(u) + resolutionX * Mathf.CeilToInt(v)];
			Color color4 = data[Mathf.CeilToInt(u) + resolutionX * Mathf.CeilToInt(v)];
			float num = Mathf.Floor(u);
			float num2 = Mathf.Floor(u + 1f);
			float num3 = Mathf.Floor(v);
			float num4 = Mathf.Floor(v + 1f);
			Color color5 = (num2 - u) / (num2 - num) * color + (u - num) / (num2 - num) * color2;
			Color color6 = (num2 - u) / (num2 - num) * color3 + (u - num) / (num2 - num) * color4;
			return (num4 - v) / (num4 - num3) * color5 + (v - num3) / (num4 - num3) * color6;
		}

		public static float[,] ReadRawImage(string path, int resX, int resY, bool pcByteOrder)
		{
			float[,] array = new float[resX, resY];
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			BinaryReader binaryReader = new BinaryReader(fileStream);
			for (int i = resY - 1; i > -1; i--)
			{
				for (int j = 0; j < resX; j++)
				{
					byte[] array2 = binaryReader.ReadBytes(2);
					if (!pcByteOrder)
					{
						byte b = array2[0];
						array2[0] = array2[1];
						array2[1] = b;
					}
					ushort num = BitConverter.ToUInt16(array2, 0);
					array[j, i] = (float)num / 65536f;
				}
			}
			binaryReader.Close();
			return array;
		}

		public static Texture2D ReadRawImageToTexture(string path, int resX, int resY, bool pcByteOrder)
		{
			float[,] array = TextureUtilities.ReadRawImage(path, resX, resY, pcByteOrder);
			Texture2D texture2D = new Texture2D(resX, resY, 5, false, true);
			texture2D.wrapMode = 1;
			texture2D.anisoLevel = 9;
			texture2D.filterMode = 2;
			int processorCount = SystemInfo.processorCount;
			int num = Mathf.CeilToInt((float)(resY / processorCount));
			Color[] array2 = new Color[resX * resY];
			ManualResetEvent[] array3 = new ManualResetEvent[processorCount];
			for (int i = 0; i < processorCount; i++)
			{
				int num2 = i * num;
				int num3 = ((i != processorCount - 1) ? num : (resX - 1 - i * num));
				array3[i] = new ManualResetEvent(false);
				ThreadPool.QueueUserWorkItem(new WaitCallback(TextureUtilities.ThreadedEncodeFloat), new TextureUtilities.ColorArrayThreadedInfo(num2, num3, ref array2, resX, resY, array, array3[i]));
			}
			WaitHandle.WaitAll(array3);
			texture2D.SetPixels(array2);
			texture2D.Apply(false);
			return texture2D;
		}

		private static void ThreadedEncodeFloat(object info)
		{
			TextureUtilities.ColorArrayThreadedInfo colorArrayThreadedInfo = info as TextureUtilities.ColorArrayThreadedInfo;
			try
			{
				for (int i = colorArrayThreadedInfo.start; i < colorArrayThreadedInfo.start + colorArrayThreadedInfo.length; i++)
				{
					for (int j = 0; j < colorArrayThreadedInfo.resY; j++)
					{
						colorArrayThreadedInfo.colorArray[i + j * colorArrayThreadedInfo.resX] = TextureUtilities.EncodeFloatRGBA(colorArrayThreadedInfo.heightArray[i, j]);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.Log(ex.ToString());
			}
			colorArrayThreadedInfo.resetEvent.Set();
		}

		public static Texture2D GetRawPreviewTexture(Texture2D rawTexture)
		{
			Texture2D texture2D = new Texture2D(rawTexture.width, rawTexture.height, 5, true, true);
			Color[] array = new Color[texture2D.width * texture2D.height];
			for (int i = 0; i < texture2D.height; i++)
			{
				for (int j = 0; j < texture2D.width; j++)
				{
					float num = TextureUtilities.DecodeFloatRGBA(rawTexture.GetPixel(j, i));
					array[j + i * texture2D.width] = new Color(num, num, num, 1f);
				}
			}
			texture2D.SetPixels(array);
			texture2D.Apply();
			return texture2D;
		}

		public static Color EncodeFloatRGBA(float v)
		{
			v = Mathf.Min(v, 0.999f);
			Color color;
			color..ctor(1f, 255f, 65025f, 160581380f);
			float num = 0.003921569f;
			Color color2 = color * v;
			color2.r -= Mathf.Floor(color2.r);
			color2.g -= Mathf.Floor(color2.g);
			color2.b -= Mathf.Floor(color2.b);
			color2.a -= Mathf.Floor(color2.a);
			color2.r -= color2.g * num;
			color2.g -= color2.b * num;
			color2.b -= color2.a * num;
			color2.a -= color2.a * num;
			return color2;
		}

		public static float DecodeFloatRGBA(Color enc)
		{
			Color color;
			color..ctor(1f, 0.003921569f, 1.53787E-05f, 6.2273724E-09f);
			return Vector4.Dot(enc, color);
		}

		public static Texture2D ImportTGA(string path)
		{
			Texture2D texture2D2;
			try
			{
				FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
				BinaryReader binaryReader = new BinaryReader(fileStream);
				binaryReader.ReadByte();
				binaryReader.ReadByte();
				binaryReader.ReadByte();
				binaryReader.ReadInt16();
				binaryReader.ReadInt16();
				binaryReader.ReadByte();
				binaryReader.ReadInt16();
				binaryReader.ReadInt16();
				short num = binaryReader.ReadInt16();
				short num2 = binaryReader.ReadInt16();
				byte b = binaryReader.ReadByte();
				binaryReader.ReadByte();
				Texture2D texture2D = new Texture2D((int)num, (int)num2, (b != 32) ? 3 : 5, true);
				Color32[] array = new Color32[(int)(num * num2)];
				for (int i = 0; i < (int)num2; i++)
				{
					for (int j = 0; j < (int)num; j++)
					{
						if (b == 32)
						{
							byte b2 = binaryReader.ReadByte();
							byte b3 = binaryReader.ReadByte();
							byte b4 = binaryReader.ReadByte();
							byte b5 = binaryReader.ReadByte();
							array[j + i * (int)num] = new Color32(b4, b3, b2, b5);
						}
						else
						{
							byte b6 = binaryReader.ReadByte();
							byte b7 = binaryReader.ReadByte();
							byte b8 = binaryReader.ReadByte();
							array[j + i * (int)num] = new Color32(b8, b7, b6, 1);
						}
					}
				}
				texture2D.SetPixels32(array);
				texture2D.Apply();
				texture2D2 = texture2D;
			}
			catch
			{
				texture2D2 = null;
			}
			return texture2D2;
		}

		public static byte[] EncodeToTGA(Texture2D texture)
		{
			List<byte> list = new List<byte>();
			list.Add(0);
			list.Add(0);
			list.Add(2);
			list.AddRange(BitConverter.GetBytes(0));
			list.AddRange(BitConverter.GetBytes(0));
			list.Add(0);
			list.AddRange(BitConverter.GetBytes(0));
			list.AddRange(BitConverter.GetBytes(0));
			list.AddRange(BitConverter.GetBytes((short)texture.width));
			list.AddRange(BitConverter.GetBytes((short)texture.height));
			short num = 0;
			TextureFormat format = texture.format;
			if (format != 5)
			{
				if (format == 3)
				{
					num = 24;
				}
			}
			else
			{
				num = 32;
			}
			list.AddRange(BitConverter.GetBytes(num));
			if (num != 24)
			{
				if (num == 32)
				{
					list.Add(8);
				}
			}
			else
			{
				list.Add(0);
			}
			Color32[] pixels = texture.GetPixels32();
			for (int i = 0; i < texture.height; i++)
			{
				for (int j = 0; j < texture.width; j++)
				{
					list.Add(pixels[j + i * texture.width].g);
					list.Add(pixels[j + i * texture.width].r);
					if (num == 32)
					{
						list.Add(pixels[j + i * texture.width].a);
					}
					list.Add(pixels[j + i * texture.width].b);
				}
			}
			return list.ToArray();
		}

		public static TextureUtilities.FileTextureFormat[] SupportedFormats = new TextureUtilities.FileTextureFormat[]
		{
			new TextureUtilities.FileTextureFormat("Tga", "tga"),
			new TextureUtilities.FileTextureFormat("Png", "png")
		};

		public static TextureUtilities.FileTextureFormat[] SupportedRawFormats = new TextureUtilities.FileTextureFormat[]
		{
			new TextureUtilities.FileTextureFormat("Raw", "raw")
		};

		public struct FileTextureFormat
		{
			public FileTextureFormat(string name, string extension)
			{
				this.name = name;
				this.extension = extension;
			}

			public string name;

			public string extension;
		}

		private class ColorArrayThreadedInfo
		{
			public ColorArrayThreadedInfo(int start, int length, ref Color[] colors, int resX, int resY, float[,] heights, ManualResetEvent resetEvent)
			{
				this.start = start;
				this.length = length;
				this.resetEvent = resetEvent;
				this.colorArray = colors;
				this.resX = resX;
				this.resY = resY;
				this.heightArray = heights;
			}

			public int start;

			public int length;

			public ManualResetEvent resetEvent;

			public Color[] colorArray;

			public float[,] heightArray;

			public int resX;

			public int resY;
		}
	}
}
