using System;
using UnityEngine;

namespace mset
{
	public class CubeBuffer
	{
		public CubeBuffer()
		{
			this.filterMode = CubeBuffer.FilterMode.BILINEAR;
			this.clear();
		}

		public CubeBuffer.FilterMode filterMode
		{
			get
			{
				return this._filterMode;
			}
			set
			{
				this._filterMode = value;
				CubeBuffer.FilterMode filterMode = this._filterMode;
				if (filterMode != CubeBuffer.FilterMode.NEAREST)
				{
					if (filterMode != CubeBuffer.FilterMode.BILINEAR)
					{
						if (filterMode == CubeBuffer.FilterMode.BICUBIC)
						{
							this.sample = new CubeBuffer.SampleFunc(this.sampleBicubic);
						}
					}
					else
					{
						this.sample = new CubeBuffer.SampleFunc(this.sampleBilinear);
					}
				}
				else
				{
					this.sample = new CubeBuffer.SampleFunc(this.sampleNearest);
				}
			}
		}

		public int width
		{
			get
			{
				return this.faceSize;
			}
		}

		public int height
		{
			get
			{
				return this.faceSize * 6;
			}
		}

		~CubeBuffer()
		{
		}

		public void clear()
		{
			this.pixels = null;
			this.faceSize = 0;
		}

		public bool empty()
		{
			return this.pixels == null || this.pixels.Length == 0;
		}

		public static void pixelCopy(ref Color[] dst, int dst_offset, Color[] src, int src_offset, int count)
		{
			for (int i = 0; i < count; i++)
			{
				dst[dst_offset + i] = src[src_offset + i];
			}
		}

		public static void pixelCopy(ref Color[] dst, int dst_offset, Color32[] src, int src_offset, int count)
		{
			float num = 0.003921569f;
			for (int i = 0; i < count; i++)
			{
				dst[dst_offset + i].r = (float)src[src_offset + i].r * num;
				dst[dst_offset + i].g = (float)src[src_offset + i].g * num;
				dst[dst_offset + i].b = (float)src[src_offset + i].b * num;
				dst[dst_offset + i].a = (float)src[src_offset + i].a * num;
			}
		}

		public static void pixelCopy(ref Color32[] dst, int dst_offset, Color[] src, int src_offset, int count)
		{
			for (int i = 0; i < count; i++)
			{
				dst[dst_offset + i].r = (byte)Mathf.Clamp(src[src_offset + i].r * 255f, 0f, 255f);
				dst[dst_offset + i].g = (byte)Mathf.Clamp(src[src_offset + i].g * 255f, 0f, 255f);
				dst[dst_offset + i].b = (byte)Mathf.Clamp(src[src_offset + i].b * 255f, 0f, 255f);
				dst[dst_offset + i].a = (byte)Mathf.Clamp(src[src_offset + i].a * 255f, 0f, 255f);
			}
		}

		public static void pixelCopyBlock<T>(ref T[] dst, int dst_x, int dst_y, int dst_w, T[] src, int src_x, int src_y, int src_w, int block_w, int block_h, bool flip)
		{
			if (flip)
			{
				for (int i = 0; i < block_w; i++)
				{
					for (int j = 0; j < block_h; j++)
					{
						int num = (dst_y + j) * dst_w + dst_x + i;
						int num2 = (src_y + (block_h - j - 1)) * src_w + src_x + i;
						dst[num] = src[num2];
					}
				}
			}
			else
			{
				for (int k = 0; k < block_w; k++)
				{
					for (int l = 0; l < block_h; l++)
					{
						int num3 = (dst_y + l) * dst_w + dst_x + k;
						int num4 = (src_y + l) * src_w + src_x + k;
						dst[num3] = src[num4];
					}
				}
			}
		}

		public static void encode(ref Color[] dst, Color[] src, ColorMode outMode, bool useGamma)
		{
			if (outMode == ColorMode.RGBM8)
			{
				for (int i = 0; i < src.Length; i++)
				{
					RGB.toRGBM(ref dst[i], src[i], useGamma);
				}
			}
			else if (useGamma)
			{
				Util.applyGamma(ref dst, src, Gamma.toSRGB);
			}
			else
			{
				CubeBuffer.pixelCopy(ref dst, 0, src, 0, src.Length);
			}
		}

		public static void encode(ref Color32[] dst, Color[] src, ColorMode outMode, bool useGamma)
		{
			if (outMode == ColorMode.RGBM8)
			{
				for (int i = 0; i < src.Length; i++)
				{
					RGB.toRGBM(ref dst[i], src[i], useGamma);
				}
			}
			else
			{
				if (useGamma)
				{
					Util.applyGamma(ref src, src, Gamma.toSRGB);
				}
				CubeBuffer.pixelCopy(ref dst, 0, src, 0, src.Length);
			}
		}

		public static void decode(ref Color[] dst, Color[] src, ColorMode inMode, bool useGamma)
		{
			if (inMode == ColorMode.RGBM8)
			{
				for (int i = 0; i < src.Length; i++)
				{
					RGB.fromRGBM(ref dst[i], src[i], useGamma);
				}
			}
			else
			{
				if (useGamma)
				{
					Util.applyGamma(ref dst, src, Gamma.toLinear);
				}
				else
				{
					CubeBuffer.pixelCopy(ref dst, 0, src, 0, src.Length);
				}
				CubeBuffer.clearAlpha(ref dst);
			}
		}

		public static void decode(ref Color[] dst, Color32[] src, ColorMode inMode, bool useGamma)
		{
			if (inMode == ColorMode.RGBM8)
			{
				for (int i = 0; i < src.Length; i++)
				{
					RGB.fromRGBM(ref dst[i], src[i], useGamma);
				}
			}
			else
			{
				CubeBuffer.pixelCopy(ref dst, 0, src, 0, src.Length);
				if (useGamma)
				{
					Util.applyGamma(ref dst, Gamma.toLinear);
				}
				CubeBuffer.clearAlpha(ref dst);
			}
		}

		public static void decode(ref Color[] dst, int dst_offset, Color[] src, int src_offset, int count, ColorMode inMode, bool useGamma)
		{
			if (inMode == ColorMode.RGBM8)
			{
				for (int i = 0; i < count; i++)
				{
					RGB.fromRGBM(ref dst[i + dst_offset], src[i + src_offset], useGamma);
				}
			}
			else
			{
				if (useGamma)
				{
					Util.applyGamma(ref dst, dst_offset, src, src_offset, count, Gamma.toLinear);
				}
				else
				{
					CubeBuffer.pixelCopy(ref dst, dst_offset, src, src_offset, count);
				}
				CubeBuffer.clearAlpha(ref dst, dst_offset, count);
			}
		}

		public static void decode(ref Color[] dst, int dst_offset, Color32[] src, int src_offset, int count, ColorMode inMode, bool useGamma)
		{
			if (inMode == ColorMode.RGBM8)
			{
				for (int i = 0; i < count; i++)
				{
					RGB.fromRGBM(ref dst[i + dst_offset], src[i + src_offset], useGamma);
				}
			}
			else
			{
				CubeBuffer.pixelCopy(ref dst, dst_offset, src, src_offset, count);
				if (useGamma)
				{
					Util.applyGamma(ref dst, dst_offset, dst, dst_offset, count, Gamma.toLinear);
				}
				CubeBuffer.clearAlpha(ref dst, dst_offset, count);
			}
		}

		public static void clearAlpha(ref Color[] dst)
		{
			CubeBuffer.clearAlpha(ref dst, 0, dst.Length);
		}

		public static void clearAlpha(ref Color[] dst, int offset, int count)
		{
			for (int i = offset; i < offset + count; i++)
			{
				dst[i].a = 1f;
			}
		}

		public static void clearAlpha(ref Color32[] dst)
		{
			CubeBuffer.clearAlpha(ref dst, 0, dst.Length);
		}

		public static void clearAlpha(ref Color32[] dst, int offset, int count)
		{
			for (int i = offset; i < offset + count; i++)
			{
				dst[i].a = byte.MaxValue;
			}
		}

		public static void applyExposure(ref Color[] pixels, float mult)
		{
			for (int i = 0; i < pixels.Length; i++)
			{
				Color[] array = pixels;
				int num = i;
				array[num].r = array[num].r * mult;
				Color[] array2 = pixels;
				int num2 = i;
				array2[num2].g = array2[num2].g * mult;
				Color[] array3 = pixels;
				int num3 = i;
				array3[num3].b = array3[num3].b * mult;
			}
		}

		public void applyExposure(float mult)
		{
			for (int i = 0; i < this.pixels.Length; i++)
			{
				Color[] array = this.pixels;
				int num = i;
				array[num].r = array[num].r * mult;
				Color[] array2 = this.pixels;
				int num2 = i;
				array2[num2].g = array2[num2].g * mult;
				Color[] array3 = this.pixels;
				int num3 = i;
				array3[num3].b = array3[num3].b * mult;
			}
		}

		public int toIndex(int face, int x, int y)
		{
			x = Mathf.Clamp(x, 0, this.faceSize - 1);
			y = Mathf.Clamp(y, 0, this.faceSize - 1);
			return this.faceSize * this.faceSize * face + this.faceSize * y + x;
		}

		public int toIndex(CubemapFace face, int x, int y)
		{
			x = Mathf.Clamp(x, 0, this.faceSize - 1);
			y = Mathf.Clamp(y, 0, this.faceSize - 1);
			return this.faceSize * this.faceSize * face + this.faceSize * y + x;
		}

		private static void linkEdges()
		{
			if (CubeBuffer._leftEdges == null)
			{
				CubeBuffer._leftEdges = new CubeBuffer.CubeEdge[6];
				CubeBuffer._leftEdges[1] = new CubeBuffer.CubeEdge(5, false, false);
				CubeBuffer._leftEdges[0] = new CubeBuffer.CubeEdge(4, false, false);
				CubeBuffer._leftEdges[3] = new CubeBuffer.CubeEdge(1, true, true);
				CubeBuffer._leftEdges[2] = new CubeBuffer.CubeEdge(1, false, true, true);
				CubeBuffer._leftEdges[5] = new CubeBuffer.CubeEdge(0, false, false);
				CubeBuffer._leftEdges[4] = new CubeBuffer.CubeEdge(1, false, false);
				CubeBuffer._rightEdges = new CubeBuffer.CubeEdge[6];
				CubeBuffer._rightEdges[1] = new CubeBuffer.CubeEdge(4, false, false);
				CubeBuffer._rightEdges[0] = new CubeBuffer.CubeEdge(5, false, false);
				CubeBuffer._rightEdges[3] = new CubeBuffer.CubeEdge(0, false, true, true);
				CubeBuffer._rightEdges[2] = new CubeBuffer.CubeEdge(0, true, true);
				CubeBuffer._rightEdges[5] = new CubeBuffer.CubeEdge(1, false, false);
				CubeBuffer._rightEdges[4] = new CubeBuffer.CubeEdge(0, false, false);
				CubeBuffer._upEdges = new CubeBuffer.CubeEdge[6];
				CubeBuffer._upEdges[1] = new CubeBuffer.CubeEdge(2, false, true, true);
				CubeBuffer._upEdges[0] = new CubeBuffer.CubeEdge(2, true, true);
				CubeBuffer._upEdges[3] = new CubeBuffer.CubeEdge(4, false, false);
				CubeBuffer._upEdges[2] = new CubeBuffer.CubeEdge(5, true, false, true);
				CubeBuffer._upEdges[5] = new CubeBuffer.CubeEdge(2, true, false, true);
				CubeBuffer._upEdges[4] = new CubeBuffer.CubeEdge(2, false, false);
				CubeBuffer._downEdges = new CubeBuffer.CubeEdge[6];
				CubeBuffer._downEdges[1] = new CubeBuffer.CubeEdge(3, true, true);
				CubeBuffer._downEdges[0] = new CubeBuffer.CubeEdge(3, false, true, true);
				CubeBuffer._downEdges[3] = new CubeBuffer.CubeEdge(5, true, false, true);
				CubeBuffer._downEdges[2] = new CubeBuffer.CubeEdge(4, false, false);
				CubeBuffer._downEdges[5] = new CubeBuffer.CubeEdge(3, true, false, true);
				CubeBuffer._downEdges[4] = new CubeBuffer.CubeEdge(3, false, false);
				for (int i = 0; i < 6; i++)
				{
					CubeBuffer._leftEdges[i].minEdge = (CubeBuffer._upEdges[i].minEdge = true);
					CubeBuffer._rightEdges[i].minEdge = (CubeBuffer._downEdges[i].minEdge = false);
				}
			}
		}

		public int toIndexLinked(int face, int u, int v)
		{
			CubeBuffer.linkEdges();
			int num = face;
			CubeBuffer._leftEdges[num].transmogrify(ref u, ref v, ref num, this.faceSize);
			CubeBuffer._upEdges[num].transmogrify(ref v, ref u, ref num, this.faceSize);
			CubeBuffer._rightEdges[num].transmogrify(ref u, ref v, ref num, this.faceSize);
			CubeBuffer._downEdges[num].transmogrify(ref v, ref u, ref num, this.faceSize);
			u = Mathf.Clamp(u, 0, this.faceSize - 1);
			v = Mathf.Clamp(v, 0, this.faceSize - 1);
			return this.toIndex(num, u, v);
		}

		public void sampleNearest(ref Color dst, float u, float v, int face)
		{
			int num = Mathf.FloorToInt((float)this.faceSize * u);
			int num2 = Mathf.FloorToInt((float)this.faceSize * v);
			dst = this.pixels[this.faceSize * this.faceSize * face + this.faceSize * num2 + num];
		}

		public void sampleBilinear(ref Color dst, float u, float v, int face)
		{
			u = (float)this.faceSize * u + 0.5f;
			int num = Mathf.FloorToInt(u) - 1;
			u = Mathf.Repeat(u, 1f);
			v = (float)this.faceSize * v + 0.5f;
			int num2 = Mathf.FloorToInt(v) - 1;
			v = Mathf.Repeat(v, 1f);
			int num3 = this.toIndexLinked(face, num, num2);
			int num4 = this.toIndexLinked(face, num + 1, num2);
			int num5 = this.toIndexLinked(face, num + 1, num2 + 1);
			int num6 = this.toIndexLinked(face, num, num2 + 1);
			Color color = Color.Lerp(this.pixels[num3], this.pixels[num4], u);
			Color color2 = Color.Lerp(this.pixels[num6], this.pixels[num5], u);
			dst = Color.Lerp(color, color2, v);
		}

		public void sampleBicubic(ref Color dst, float u, float v, int face)
		{
			u = (float)this.faceSize * u + 0.5f;
			int num = Mathf.FloorToInt(u) - 1;
			u = Mathf.Repeat(u, 1f);
			v = (float)this.faceSize * v + 0.5f;
			int num2 = Mathf.FloorToInt(v) - 1;
			v = Mathf.Repeat(v, 1f);
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					int num3 = this.toIndexLinked(face, num - 1 + i, num2 - 1 + j);
					CubeBuffer.cubicKernel[i, j] = this.pixels[num3];
				}
			}
			float num4 = 0.85f;
			float num5 = 0.333f;
			Color color;
			Color color2;
			Color color3;
			Color color4;
			Color color5;
			Color color6;
			Color color7;
			for (int k = 0; k < 4; k++)
			{
				color = CubeBuffer.cubicKernel[0, k];
				color2 = CubeBuffer.cubicKernel[1, k];
				color3 = CubeBuffer.cubicKernel[2, k];
				color4 = CubeBuffer.cubicKernel[3, k];
				color = Color.Lerp(color2, color, num4);
				color4 = Color.Lerp(color3, color4, num4);
				color = color2 + num5 * (color2 - color);
				color4 = color3 + num5 * (color3 - color4);
				color5 = Color.Lerp(color2, color, u);
				color6 = Color.Lerp(color, color4, u);
				color7 = Color.Lerp(color4, color3, u);
				color5 = Color.Lerp(color5, color6, u);
				color6 = Color.Lerp(color6, color7, u);
				CubeBuffer.cubicKernel[0, k] = Color.Lerp(color5, color6, u);
			}
			color = CubeBuffer.cubicKernel[0, 0];
			color2 = CubeBuffer.cubicKernel[0, 1];
			color3 = CubeBuffer.cubicKernel[0, 2];
			color4 = CubeBuffer.cubicKernel[0, 3];
			color = Color.Lerp(color2, color, num4);
			color4 = Color.Lerp(color3, color4, num4);
			color = color2 + num5 * (color2 - color);
			color4 = color3 + num5 * (color3 - color4);
			color5 = Color.Lerp(color2, color, v);
			color6 = Color.Lerp(color, color4, v);
			color7 = Color.Lerp(color4, color3, v);
			color5 = Color.Lerp(color5, color6, v);
			color6 = Color.Lerp(color6, color7, v);
			dst = Color.Lerp(color5, color6, v);
		}

		public void resize(int newFaceSize)
		{
			if (newFaceSize == this.faceSize)
			{
				return;
			}
			this.faceSize = newFaceSize;
			this.pixels = null;
			this.pixels = new Color[this.faceSize * this.faceSize * 6];
			Util.clearTo(ref this.pixels, Color.black);
		}

		public void resize(int newFaceSize, Color clearColor)
		{
			this.resize(newFaceSize);
			Util.clearTo(ref this.pixels, clearColor);
		}

		public void resample(int newSize)
		{
			if (newSize == this.faceSize)
			{
				return;
			}
			Color[] array = new Color[newSize * newSize * 6];
			this.resample(ref array, newSize);
			this.pixels = array;
			this.faceSize = newSize;
		}

		public void resample(ref Color[] dst, int newSize)
		{
			int num = newSize * newSize;
			float num2 = 1f / (float)newSize;
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < newSize; j++)
				{
					float num3 = ((float)j + 0.5f) * num2;
					for (int k = 0; k < newSize; k++)
					{
						float num4 = ((float)k + 0.5f) * num2;
						int num5 = num * i + j * newSize + k;
						this.sample(ref dst[num5], num4, num3, i);
					}
				}
			}
		}

		public void resampleFace(ref Color[] dst, int face, int newSize, bool flipY)
		{
			this.resampleFace(ref dst, 0, face, newSize, flipY);
		}

		public void resampleFace(ref Color[] dst, int dstOffset, int face, int newSize, bool flipY)
		{
			if (newSize == this.faceSize)
			{
				CubeBuffer.pixelCopy(ref dst, dstOffset, this.pixels, face * this.faceSize * this.faceSize, this.faceSize * this.faceSize);
				return;
			}
			float num = 1f / (float)newSize;
			if (flipY)
			{
				for (int i = 0; i < newSize; i++)
				{
					float num2 = 1f - ((float)i + 0.5f) * num;
					for (int j = 0; j < newSize; j++)
					{
						float num3 = ((float)j + 0.5f) * num;
						int num4 = i * newSize + j + dstOffset;
						this.sample(ref dst[num4], num3, num2, face);
					}
				}
			}
			else
			{
				for (int k = 0; k < newSize; k++)
				{
					float num5 = ((float)k + 0.5f) * num;
					for (int l = 0; l < newSize; l++)
					{
						float num6 = ((float)l + 0.5f) * num;
						int num7 = k * newSize + l + dstOffset;
						this.sample(ref dst[num7], num6, num5, face);
					}
				}
			}
		}

		public void fromCube(Cubemap cube, int mip, ColorMode cubeColorMode, bool useGamma)
		{
			int num = cube.width >> mip;
			if (this.pixels == null || this.faceSize != num)
			{
				this.resize(num);
			}
			for (int i = 0; i < 6; i++)
			{
				Color[] array = cube.GetPixels(i, mip);
				CubeBuffer.pixelCopy(ref this.pixels, i * this.faceSize * this.faceSize, array, 0, array.Length);
			}
			CubeBuffer.decode(ref this.pixels, this.pixels, cubeColorMode, useGamma);
		}

		public void toCube(ref Cubemap cube, int mip, ColorMode cubeColorMode, bool useGamma)
		{
			int num = this.faceSize * this.faceSize;
			Color[] array = new Color[num];
			for (int i = 0; i < 6; i++)
			{
				CubeBuffer.pixelCopy(ref array, 0, this.pixels, i * num, num);
				CubeBuffer.encode(ref array, array, cubeColorMode, useGamma);
				cube.SetPixels(array, i, mip);
			}
			cube.Apply(false);
		}

		public void resampleToCube(ref Cubemap cube, int mip, ColorMode cubeColorMode, bool useGamma, float exposureMult)
		{
			int num = cube.width >> mip;
			int num2 = num * num * 6;
			Color[] array = new Color[num2];
			for (int i = 0; i < 6; i++)
			{
				this.resampleFace(ref array, i, num, false);
				if (exposureMult != 1f)
				{
					CubeBuffer.applyExposure(ref array, exposureMult);
				}
				CubeBuffer.encode(ref array, array, cubeColorMode, useGamma);
				cube.SetPixels(array, i, mip);
			}
			cube.Apply(false);
		}

		public void resampleToBuffer(ref CubeBuffer dst, float exposureMult)
		{
			int num = dst.faceSize * dst.faceSize;
			for (int i = 0; i < 6; i++)
			{
				this.resampleFace(ref dst.pixels, i * num, i, dst.faceSize, false);
				dst.applyExposure(exposureMult);
			}
		}

		public void fromBuffer(CubeBuffer src)
		{
			this.clear();
			this.faceSize = src.faceSize;
			this.pixels = new Color[src.pixels.Length];
			CubeBuffer.pixelCopy(ref this.pixels, 0, src.pixels, 0, this.pixels.Length);
		}

		public void fromPanoTexture(Texture2D tex, int _faceSize, ColorMode texColorMode, bool useGamma)
		{
			this.resize(_faceSize);
			ulong num = (ulong)((long)this.faceSize);
			for (ulong num2 = 0UL; num2 < 6UL; num2 += 1UL)
			{
				for (ulong num3 = 0UL; num3 < num; num3 += 1UL)
				{
					for (ulong num4 = 0UL; num4 < num; num4 += 1UL)
					{
						float num5 = 0f;
						float num6 = 0f;
						Util.cubeToLatLongLookup(ref num5, ref num6, num2, num4, num3, num);
						float num7 = 1f / (float)this.faceSize;
						num6 = Mathf.Clamp(num6, num7, 1f - num7);
						checked
						{
							this.pixels[(int)((IntPtr)(unchecked(num2 * num * num + num3 * num + num4)))] = tex.GetPixelBilinear(num5, num6);
						}
					}
				}
			}
			CubeBuffer.decode(ref this.pixels, this.pixels, texColorMode, useGamma);
		}

		public void fromColTexture(Texture2D tex, ColorMode texColorMode, bool useGamma)
		{
			this.fromColTexture(tex, 0, texColorMode, useGamma);
		}

		public void fromColTexture(Texture2D tex, int mip, ColorMode texColorMode, bool useGamma)
		{
			if (tex.width * 6 != tex.height)
			{
				Debug.LogError("CubeBuffer.fromColTexture takes textures of a 1x6 aspect ratio");
				return;
			}
			int num = tex.width >> mip;
			if (this.pixels == null || this.faceSize != num)
			{
				this.resize(num);
			}
			Color32[] pixels = tex.GetPixels32(mip);
			if ((float)pixels[0].a != 1f)
			{
				CubeBuffer.clearAlpha(ref pixels);
			}
			CubeBuffer.decode(ref this.pixels, pixels, texColorMode, useGamma);
		}

		public void fromHorizCrossTexture(Texture2D tex, ColorMode texColorMode, bool useGamma)
		{
			this.fromHorizCrossTexture(tex, 0, texColorMode, useGamma);
		}

		public void fromHorizCrossTexture(Texture2D tex, int mip, ColorMode texColorMode, bool useGamma)
		{
			if (tex.width * 3 != tex.height * 4)
			{
				Debug.LogError("CubeBuffer.fromHorizCrossTexture takes textures of a 4x3 aspect ratio");
				return;
			}
			int num = tex.width / 4 >> mip;
			if (this.pixels == null || this.faceSize != num)
			{
				this.resize(num);
			}
			Color32[] pixels = tex.GetPixels32(mip);
			if ((float)pixels[0].a != 1f)
			{
				CubeBuffer.clearAlpha(ref pixels);
			}
			Color32[] array = new Color32[this.faceSize * this.faceSize];
			for (int i = 0; i < 6; i++)
			{
				CubemapFace cubemapFace = i;
				int num2 = 0;
				int num3 = 0;
				int num4 = i * this.faceSize * this.faceSize;
				switch (cubemapFace)
				{
				case 0:
					num2 = this.faceSize * 2;
					num3 = this.faceSize;
					break;
				case 1:
					num2 = 0;
					num3 = this.faceSize;
					break;
				case 2:
					num2 = this.faceSize;
					num3 = this.faceSize * 2;
					break;
				case 3:
					num2 = this.faceSize;
					num3 = 0;
					break;
				case 4:
					num2 = this.faceSize;
					num3 = this.faceSize;
					break;
				case 5:
					num2 = this.faceSize * 3;
					num3 = this.faceSize;
					break;
				}
				CubeBuffer.pixelCopyBlock<Color32>(ref array, 0, 0, this.faceSize, pixels, num2, num3, this.faceSize * 4, this.faceSize, this.faceSize, true);
				CubeBuffer.decode(ref this.pixels, num4, array, 0, this.faceSize * this.faceSize, texColorMode, useGamma);
			}
		}

		public void toColTexture(ref Texture2D tex, ColorMode texColorMode, bool useGamma)
		{
			if (tex.width != this.faceSize || tex.height != this.faceSize * 6)
			{
				tex.Resize(this.faceSize, 6 * this.faceSize);
			}
			Color32[] pixels = tex.GetPixels32();
			CubeBuffer.encode(ref pixels, this.pixels, texColorMode, useGamma);
			tex.SetPixels32(pixels);
			tex.Apply(false);
		}

		public void toPanoTexture(ref Texture2D tex, ColorMode texColorMode, bool useGamma)
		{
			ulong num = (ulong)((long)tex.width);
			ulong num2 = (ulong)((long)tex.height);
			Color[] array = tex.GetPixels();
			for (ulong num3 = 0UL; num3 < num; num3 += 1UL)
			{
				for (ulong num4 = 0UL; num4 < num2; num4 += 1UL)
				{
					float num5 = 0f;
					float num6 = 0f;
					ulong num7 = 0UL;
					Util.latLongToCubeLookup(ref num5, ref num6, ref num7, num3, num4, num, num2);
					this.sample(ref array[(int)(checked((IntPtr)(unchecked(num4 * num + num3))))], num5, num6, (int)num7);
				}
			}
			CubeBuffer.encode(ref array, array, texColorMode, useGamma);
			tex.SetPixels(array);
			tex.Apply(tex.mipmapCount > 1);
		}

		public void toPanoBuffer(ref Color[] buffer, int width, int height)
		{
			ulong num = (ulong)((long)width);
			ulong num2 = (ulong)((long)height);
			for (ulong num3 = 0UL; num3 < num; num3 += 1UL)
			{
				for (ulong num4 = 0UL; num4 < num2; num4 += 1UL)
				{
					float num5 = 0f;
					float num6 = 0f;
					ulong num7 = 0UL;
					Util.latLongToCubeLookup(ref num5, ref num6, ref num7, num3, num4, num, num2);
					this.sample(ref buffer[(int)(checked((IntPtr)(unchecked(num4 * num + num3))))], num5, num6, (int)num7);
				}
			}
		}

		public CubeBuffer.SampleFunc sample;

		private CubeBuffer.FilterMode _filterMode;

		public int faceSize;

		public Color[] pixels;

		private static CubeBuffer.CubeEdge[] _leftEdges = null;

		private static CubeBuffer.CubeEdge[] _rightEdges = null;

		private static CubeBuffer.CubeEdge[] _upEdges = null;

		private static CubeBuffer.CubeEdge[] _downEdges = null;

		private static Color[,] cubicKernel = new Color[4, 4];

		public enum FilterMode
		{
			NEAREST,
			BILINEAR,
			BICUBIC
		}

		public delegate void SampleFunc(ref Color dst, float u, float v, int face);

		private class CubeEdge
		{
			public CubeEdge(int Other, bool flip, bool swizzle)
			{
				this.other = Other;
				this.flipped = flip;
				this.swizzled = swizzle;
				this.mirrored = false;
				this.minEdge = false;
			}

			public CubeEdge(int Other, bool flip, bool swizzle, bool mirror)
			{
				this.other = Other;
				this.flipped = flip;
				this.swizzled = swizzle;
				this.mirrored = mirror;
				this.minEdge = false;
			}

			public void transmogrify(ref int primary, ref int secondary, ref int face, int faceSize)
			{
				bool flag = false;
				if (this.minEdge && primary < 0)
				{
					primary = faceSize + primary;
					flag = true;
				}
				else if (!this.minEdge && primary >= faceSize)
				{
					primary %= faceSize;
					flag = true;
				}
				if (flag)
				{
					if (this.mirrored)
					{
						primary = faceSize - primary - 1;
					}
					if (this.flipped)
					{
						secondary = faceSize - secondary - 1;
					}
					if (this.swizzled)
					{
						int num = secondary;
						secondary = primary;
						primary = num;
					}
					face = this.other;
				}
			}

			public void transmogrify(ref int primary_i, ref int primary_j, ref int secondary_i, ref int secondary_j, ref int face_i, ref int face_j, int faceSize)
			{
				if (primary_i < 0)
				{
					primary_i = (primary_j = faceSize - 1);
				}
				else
				{
					primary_i = (primary_j = 0);
				}
				if (this.mirrored)
				{
					primary_i = faceSize - primary_i - 1;
					primary_j = faceSize - primary_j - 1;
				}
				if (this.flipped)
				{
					secondary_i = faceSize - secondary_i - 1;
					secondary_j = faceSize - secondary_j - 1;
				}
				if (this.swizzled)
				{
					int num = secondary_i;
					secondary_i = primary_i;
					primary_i = num;
					num = secondary_j;
					secondary_j = primary_j;
					primary_j = num;
				}
				face_i = (face_j = this.other);
			}

			public int other;

			public bool flipped;

			public bool swizzled;

			public bool mirrored;

			public bool minEdge;
		}
	}
}
