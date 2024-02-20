using System;
using UnityEngine;

namespace OrbCreationExtensions
{
	public static class Texture2DExtensions
	{
		public static Texture2D GetCopy(this Texture2D tex)
		{
			return tex.GetCopy(0, 0, tex.width, tex.height, tex.mipmapCount > 1);
		}

		public static Texture2D GetCopy(this Texture2D tex, int x, int y, int w, int h)
		{
			return tex.GetCopy(x, y, w, h, tex.mipmapCount > 1);
		}

		public static Texture2D GetSection(this Texture2D tex, int x, int y, int w, int h)
		{
			return tex.GetCopy(x, y, w, h, tex.mipmapCount > 1);
		}

		public static Texture2D GetCopy(this Texture2D tex, int x, int y, int w, int h, bool mipMaps)
		{
			Texture2D texture2D = new Texture2D(w, h, Texture2DExtensions.GetWritableFormat(tex.format), mipMaps);
			texture2D.SetPixels(tex.GetPixels(x, y, w, h, 0), 0);
			texture2D.Apply(mipMaps, false);
			return texture2D;
		}

		public static Texture2D ScaledCopy(this Texture2D tex, int width, int height, bool mipMaps)
		{
			if (width <= 0 || height <= 0)
			{
				return null;
			}
			if (width == tex.width && height == tex.height)
			{
				return tex.GetCopy(0, 0, tex.width, tex.height, mipMaps);
			}
			Color[] array = Texture2DExtensions.ScaledPixels(tex.GetPixels(0), tex.width, tex.height, width, height);
			Texture2D texture2D = new Texture2D(width, height, Texture2DExtensions.GetWritableFormat(tex.format), mipMaps);
			texture2D.SetPixels(array, 0);
			texture2D.Apply(mipMaps, false);
			return texture2D;
		}

		public static void CopyFrom(this Texture2D tex, Texture2D fromTex, int toX, int toY, int fromX, int fromY, int width, int height)
		{
			tex.MakeFormatWritable();
			int width2 = tex.width;
			Color[] pixels = tex.GetPixels(0);
			Color[] pixels2 = fromTex.GetPixels(fromX, fromY, width, height, 0);
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					pixels[(i + toY) * width2 + j + toX] = pixels2[i * width + j];
				}
			}
			tex.SetPixels(pixels, 0);
			tex.Apply(tex.mipmapCount > 1, false);
		}

		public static void Scale(this Texture2D tex, int width, int height)
		{
			if (width <= 0 || height <= 0 || (width == tex.width && height == tex.height))
			{
				return;
			}
			tex.MakeFormatWritable();
			Color[] array = Texture2DExtensions.ScaledPixels(tex.GetPixels(0), tex.width, tex.height, width, height);
			if (tex.Resize(width, height, tex.format, tex.mipmapCount > 1))
			{
				tex.SetPixels(array, 0);
				tex.Apply(tex.mipmapCount > 1, false);
			}
		}

		public static void MakeFormatWritable(this Texture2D tex)
		{
			TextureFormat format = tex.format;
			TextureFormat writableFormat = Texture2DExtensions.GetWritableFormat(tex.format);
			if (writableFormat != format)
			{
				Color[] pixels = tex.GetPixels(0);
				tex.Resize(tex.width, tex.height, writableFormat, tex.mipmapCount > 1);
				tex.SetPixels(pixels, 0);
				tex.Apply(tex.mipmapCount > 1, false);
			}
		}

		public static TextureFormat GetWritableFormat(TextureFormat format)
		{
			if (format != 1 && format != 3 && format != 5 && format != 4)
			{
				if (format == 3 || format == 10 || format == 30 || format == 32 || format == 34 || format == 34 || format == 47 || format == 45 || format == 48 || format == 49 || format == 49 || format == 49 || format == 50 || format == 52 || format == 53)
				{
					format = 3;
				}
				else
				{
					format = 4;
				}
			}
			return format;
		}

		public static Color GetAverageColor(this Texture2D tex)
		{
			Vector4 vector = Vector4.zero;
			float num = 0f;
			Color[] pixels = tex.GetPixels(0);
			for (int i = 0; i < pixels.Length; i++)
			{
				vector += pixels[i] * pixels[i].a;
				num += pixels[i].a;
			}
			if (num < 1f)
			{
				num = 1f;
			}
			vector.w = num;
			return vector / num;
		}

		public static Color GetAverageColor(this Texture2D tex, Color useThisColorForAlpha)
		{
			Vector4 vector = Vector4.zero;
			float num = 0f;
			Color[] pixels = tex.GetPixels(0);
			for (int i = 0; i < pixels.Length; i++)
			{
				vector += pixels[i] * pixels[i].a;
				vector += useThisColorForAlpha * (1f - pixels[i].a);
				num += 1f;
			}
			if (num < 1f)
			{
				num = 1f;
			}
			vector.w = num;
			return vector / num;
		}

		public static bool IsReadable(this Texture2D tex)
		{
			try
			{
				tex.GetPixels(0, 0, 1, 1, 0);
			}
			catch (Exception ex)
			{
				return ex == null;
			}
			return true;
		}

		public static bool HasTransparency(this Texture2D tex)
		{
			Color[] pixels;
			try
			{
				pixels = tex.GetPixels(0);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex);
				return false;
			}
			for (int i = 0; i < pixels.Length; i++)
			{
				if (pixels[i].a < 1f)
				{
					return true;
				}
			}
			return false;
		}

		private static Color[] ScaledPixels(Color[] originalPixels, int oldWidth, int oldHeight, int width, int height)
		{
			if (width <= 0 || height <= 0 || (width == oldWidth && height == oldHeight))
			{
				return originalPixels;
			}
			float num = (float)width / (float)oldWidth;
			float num2 = (float)height / (float)oldHeight;
			Color[] array = new Color[width * height];
			for (int i = 0; i < height; i++)
			{
				float num3 = (float)i / num2;
				int num4 = Mathf.FloorToInt(num3);
				int num5 = Mathf.CeilToInt(num3);
				for (int j = 0; j < width; j++)
				{
					float num6 = (float)j / num;
					int num7 = Mathf.FloorToInt(num6);
					int num8 = Mathf.CeilToInt(num6);
					Color color = originalPixels[num4 * oldWidth + num7] * (1f - (num3 - (float)num4)) * (1f - (num6 - (float)num7));
					if (num7 < num8 && num8 < oldWidth)
					{
						color += originalPixels[num4 * oldWidth + num8] * (1f - (num3 - (float)num4)) * (1f - ((float)num8 - num6));
					}
					if (num4 < num5 && num5 < oldHeight)
					{
						color += originalPixels[num5 * oldWidth + num7] * (1f - ((float)num5 - num3)) * (1f - (num6 - (float)num7));
						if (num7 < num8 && num8 < oldWidth)
						{
							color += originalPixels[num5 * oldWidth + num8] * (1f - ((float)num5 - num3)) * (1f - ((float)num8 - num6));
						}
					}
					array[i * width + j] = color;
				}
			}
			return array;
		}

		public static Texture2D GetUnityNormalMap(this Texture2D tex)
		{
			Texture2D texture2D = new Texture2D(tex.width, tex.height, 5, tex.mipmapCount > 1);
			Color[] pixels = tex.GetPixels(0);
			Color[] array = new Color[pixels.Length];
			for (int i = 0; i < tex.height; i++)
			{
				for (int j = 0; j < tex.width; j++)
				{
					Color color = pixels[i * tex.width + j];
					Color color2;
					color2..ctor(0f, 0f, 0f, 0f);
					color2.r = color.g;
					color2.g = color.g;
					color2.b = color.g;
					color2.a = color.r;
					array[i * tex.width + j] = color2;
				}
			}
			texture2D.SetPixels(array, 0);
			texture2D.Apply(tex.mipmapCount > 1, false);
			return texture2D;
		}

		public static Texture2D FromUnityNormalMap(this Texture2D tex)
		{
			Texture2D texture2D = new Texture2D(tex.width, tex.height, 3, tex.mipmapCount > 1);
			Color[] pixels = tex.GetPixels(0);
			Color[] array = new Color[pixels.Length];
			for (int i = 0; i < tex.height; i++)
			{
				for (int j = 0; j < tex.width; j++)
				{
					Color color = pixels[i * tex.width + j];
					Color color2;
					color2..ctor(0f, 0f, 0f, 0f);
					color2.g = color.r;
					color2.r = color.a;
					color2.b = 1f;
					array[i * tex.width + j] = color2;
				}
			}
			texture2D.SetPixels(array, 0);
			texture2D.Apply(tex.mipmapCount > 1, false);
			return texture2D;
		}

		public static void Fill(this Texture2D tex, Color aColor)
		{
			tex.MakeFormatWritable();
			Color[] pixels = tex.GetPixels(0);
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = aColor;
			}
			tex.SetPixels(pixels, 0);
			tex.Apply(tex.mipmapCount > 1, false);
		}
	}
}
