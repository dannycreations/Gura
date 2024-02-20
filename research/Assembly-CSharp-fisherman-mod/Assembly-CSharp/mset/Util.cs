using System;
using UnityEngine;

namespace mset
{
	public class Util
	{
		public static void cubeLookup(ref float s, ref float t, ref ulong face, Vector3 dir)
		{
			float num = Mathf.Abs(dir.x);
			float num2 = Mathf.Abs(dir.y);
			float num3 = Mathf.Abs(dir.z);
			if (num >= num2 && num >= num3)
			{
				if (dir.x >= 0f)
				{
					face = 0UL;
				}
				else
				{
					face = 1UL;
				}
			}
			else if (num2 >= num && num2 >= num3)
			{
				if (dir.y >= 0f)
				{
					face = 2UL;
				}
				else
				{
					face = 3UL;
				}
			}
			else if (dir.z >= 0f)
			{
				face = 4UL;
			}
			else
			{
				face = 5UL;
			}
			if (face >= 0UL && face <= 5UL)
			{
				switch ((int)face)
				{
				case 0:
					s = 0.5f * (-dir.z / num + 1f);
					t = 0.5f * (-dir.y / num + 1f);
					break;
				case 1:
					s = 0.5f * (dir.z / num + 1f);
					t = 0.5f * (-dir.y / num + 1f);
					break;
				case 2:
					s = 0.5f * (dir.x / num2 + 1f);
					t = 0.5f * (dir.z / num2 + 1f);
					break;
				case 3:
					s = 0.5f * (dir.x / num2 + 1f);
					t = 0.5f * (-dir.z / num2 + 1f);
					break;
				case 4:
					s = 0.5f * (dir.x / num3 + 1f);
					t = 0.5f * (-dir.y / num3 + 1f);
					break;
				case 5:
					s = 0.5f * (-dir.x / num3 + 1f);
					t = 0.5f * (-dir.y / num3 + 1f);
					break;
				}
			}
		}

		public static void invCubeLookup(ref Vector3 dst, ref float weight, ulong face, ulong col, ulong row, ulong faceSize)
		{
			float num = 2f / faceSize;
			float num2 = (col + 0.5f) * num - 1f;
			float num3 = (row + 0.5f) * num - 1f;
			if (face >= 0UL && face <= 5UL)
			{
				switch ((int)face)
				{
				case 0:
					dst[0] = 1f;
					dst[1] = -num3;
					dst[2] = -num2;
					break;
				case 1:
					dst[0] = -1f;
					dst[1] = -num3;
					dst[2] = num2;
					break;
				case 2:
					dst[0] = num2;
					dst[1] = 1f;
					dst[2] = num3;
					break;
				case 3:
					dst[0] = num2;
					dst[1] = -1f;
					dst[2] = -num3;
					break;
				case 4:
					dst[0] = num2;
					dst[1] = -num3;
					dst[2] = 1f;
					break;
				case 5:
					dst[0] = -num2;
					dst[1] = -num3;
					dst[2] = -1f;
					break;
				}
			}
			float magnitude = dst.magnitude;
			weight = 4f / (magnitude * magnitude * magnitude);
			dst /= magnitude;
		}

		public static void invLatLongLookup(ref Vector3 dst, ref float cosPhi, ulong col, ulong row, ulong width, ulong height)
		{
			float num = 0.5f;
			float num2 = (col + num) / width;
			float num3 = (row + num) / height;
			float num4 = -6.2831855f * num2 - 1.5707964f;
			float num5 = 1.5707964f * (2f * num3 - 1f);
			cosPhi = Mathf.Cos(num5);
			dst.x = Mathf.Cos(num4) * cosPhi;
			dst.y = Mathf.Sin(num5);
			dst.z = Mathf.Sin(num4) * cosPhi;
		}

		public static void cubeToLatLongLookup(ref float pano_u, ref float pano_v, ulong face, ulong col, ulong row, ulong faceSize)
		{
			Vector3 vector = default(Vector3);
			float num = -1f;
			Util.invCubeLookup(ref vector, ref num, face, col, row, faceSize);
			pano_v = Mathf.Asin(vector.y) / 3.1415927f + 0.5f;
			pano_u = 0.5f * Mathf.Atan2(-vector.x, -vector.z) / 3.1415927f;
			pano_u = Mathf.Repeat(pano_u, 1f);
		}

		public static void latLongToCubeLookup(ref float cube_u, ref float cube_v, ref ulong face, ulong col, ulong row, ulong width, ulong height)
		{
			Vector3 vector = default(Vector3);
			float num = -1f;
			Util.invLatLongLookup(ref vector, ref num, col, row, width, height);
			Util.cubeLookup(ref cube_u, ref cube_v, ref face, vector);
		}

		public static void rotationToInvLatLong(out float u, out float v, Quaternion rot)
		{
			u = rot.eulerAngles.y;
			v = rot.eulerAngles.x;
			u = Mathf.Repeat(u, 360f) / 360f;
			v = 1f - Mathf.Repeat(v + 90f, 360f) / 180f;
		}

		public static void dirToLatLong(out float u, out float v, Vector3 dir)
		{
			dir = dir.normalized;
			u = 0.5f * Mathf.Atan2(-dir.x, -dir.z) / 3.1415927f;
			u = Mathf.Repeat(u, 1f);
			v = Mathf.Asin(dir.y) / 3.1415927f + 0.5f;
			v = 1f - Mathf.Repeat(v, 1f);
		}

		public static void applyGamma(ref Color c, float gamma)
		{
			c.r = Mathf.Pow(c.r, gamma);
			c.g = Mathf.Pow(c.g, gamma);
			c.b = Mathf.Pow(c.b, gamma);
		}

		public static void applyGamma(ref Color[] c, float gamma)
		{
			for (int i = 0; i < c.Length; i++)
			{
				c[i].r = Mathf.Pow(c[i].r, gamma);
				c[i].g = Mathf.Pow(c[i].g, gamma);
				c[i].b = Mathf.Pow(c[i].b, gamma);
			}
		}

		public static void applyGamma(ref Color[] dst, Color[] src, float gamma)
		{
			for (int i = 0; i < src.Length; i++)
			{
				dst[i].r = Mathf.Pow(src[i].r, gamma);
				dst[i].g = Mathf.Pow(src[i].g, gamma);
				dst[i].b = Mathf.Pow(src[i].b, gamma);
				dst[i].a = src[i].a;
			}
		}

		public static void applyGamma(ref Color[] dst, int dst_offset, Color[] src, int src_offset, int count, float gamma)
		{
			int num = 0;
			while (num < count && num < src.Length)
			{
				dst[num + dst_offset].r = Mathf.Pow(src[num + src_offset].r, gamma);
				dst[num + dst_offset].g = Mathf.Pow(src[num + src_offset].g, gamma);
				dst[num + dst_offset].b = Mathf.Pow(src[num + src_offset].b, gamma);
				dst[num + dst_offset].a = src[num + src_offset].a;
				num++;
			}
		}

		public static void applyGamma2D(ref Texture2D tex, float gamma)
		{
			for (int i = 0; i < tex.mipmapCount; i++)
			{
				Color[] pixels = tex.GetPixels(i);
				Util.applyGamma(ref pixels, gamma);
				tex.SetPixels(pixels);
			}
			tex.Apply(false);
		}

		public static void clearTo(ref Color[] c, Color color)
		{
			for (int i = 0; i < c.Length; i++)
			{
				c[i] = color;
			}
		}

		public static void clearTo2D(ref Texture2D tex, Color color)
		{
			for (int i = 0; i < tex.mipmapCount; i++)
			{
				Color[] pixels = tex.GetPixels(i);
				Util.clearTo(ref pixels, color);
				tex.SetPixels(pixels, i);
			}
			tex.Apply(false);
		}

		public static void clearChecker2D(ref Texture2D tex)
		{
			Color color;
			color..ctor(0.25f, 0.25f, 0.25f, 0.25f);
			Color color2;
			color2..ctor(0.5f, 0.5f, 0.5f, 0.25f);
			Color[] pixels = tex.GetPixels();
			int width = tex.width;
			int height = tex.height;
			int num = height / 4;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					if (i / num % 2 == j / num % 2)
					{
						pixels[j * width + i] = color;
					}
					else
					{
						pixels[j * width + i] = color2;
					}
				}
			}
			tex.SetPixels(pixels);
			tex.Apply(false);
		}

		public static void clearCheckerCube(ref Cubemap cube)
		{
			Color color;
			color..ctor(0.25f, 0.25f, 0.25f, 0.25f);
			Color color2;
			color2..ctor(0.5f, 0.5f, 0.5f, 0.25f);
			Color[] pixels = cube.GetPixels(1);
			int width = cube.width;
			int num = Mathf.Max(1, width / 4);
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < width; j++)
				{
					for (int k = 0; k < width; k++)
					{
						if (j / num % 2 == k / num % 2)
						{
							pixels[k * width + j] = color;
						}
						else
						{
							pixels[k * width + j] = color2;
						}
					}
				}
				cube.SetPixels(pixels, i);
			}
			cube.Apply(true);
		}
	}
}
