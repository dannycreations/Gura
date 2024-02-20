using System;
using UnityEngine;

namespace mset
{
	public class SHUtil
	{
		private static float project_l0_m0(Vector3 u)
		{
			return SHEncoding.sEquationConstants[0];
		}

		private static float project_l1_mneg1(Vector3 u)
		{
			return SHEncoding.sEquationConstants[1] * u.y;
		}

		private static float project_l1_m0(Vector3 u)
		{
			return SHEncoding.sEquationConstants[2] * u.z;
		}

		private static float project_l1_m1(Vector3 u)
		{
			return SHEncoding.sEquationConstants[3] * u.x;
		}

		private static float project_l2_mneg2(Vector3 u)
		{
			return SHEncoding.sEquationConstants[4] * u.y * u.x;
		}

		private static float project_l2_mneg1(Vector3 u)
		{
			return SHEncoding.sEquationConstants[5] * u.y * u.z;
		}

		private static float project_l2_m0(Vector3 u)
		{
			return SHEncoding.sEquationConstants[6] * (3f * u.z * u.z - 1f);
		}

		private static float project_l2_m1(Vector3 u)
		{
			return SHEncoding.sEquationConstants[7] * u.z * u.x;
		}

		private static float project_l2_m2(Vector3 u)
		{
			return SHEncoding.sEquationConstants[8] * (u.x * u.x - u.y * u.y);
		}

		private static void scale(ref SHEncoding sh, float s)
		{
			for (int i = 0; i < 27; i++)
			{
				sh.c[i] *= s;
			}
		}

		public static void projectCubeBuffer(ref SHEncoding sh, CubeBuffer cube)
		{
			sh.clearToBlack();
			float num = 0f;
			ulong num2 = (ulong)((long)cube.faceSize);
			float[] array = new float[9];
			Vector3 zero = Vector3.zero;
			for (ulong num3 = 0UL; num3 < 6UL; num3 += 1UL)
			{
				for (ulong num4 = 0UL; num4 < num2; num4 += 1UL)
				{
					for (ulong num5 = 0UL; num5 < num2; num5 += 1UL)
					{
						float num6 = 1f;
						Util.invCubeLookup(ref zero, ref num6, num3, num5, num4, num2);
						float num7 = 1.3333334f;
						ulong num8 = num3 * num2 * num2 + num4 * num2 + num5;
						Color color = cube.pixels[(int)(checked((IntPtr)num8))];
						array[0] = SHUtil.project_l0_m0(zero);
						array[1] = SHUtil.project_l1_mneg1(zero);
						array[2] = SHUtil.project_l1_m0(zero);
						array[3] = SHUtil.project_l1_m1(zero);
						array[4] = SHUtil.project_l2_mneg2(zero);
						array[5] = SHUtil.project_l2_mneg1(zero);
						array[6] = SHUtil.project_l2_m0(zero);
						array[7] = SHUtil.project_l2_m1(zero);
						array[8] = SHUtil.project_l2_m2(zero);
						for (int i = 0; i < 9; i++)
						{
							sh.c[3 * i] += num7 * num6 * color[0] * array[i];
							sh.c[3 * i + 1] += num7 * num6 * color[1] * array[i];
							sh.c[3 * i + 2] += num7 * num6 * color[2] * array[i];
						}
						num += num6;
					}
				}
			}
			SHUtil.scale(ref sh, 16f / num);
		}

		public static void projectCube(ref SHEncoding sh, Cubemap cube, int mip, bool hdr)
		{
			sh.clearToBlack();
			float num = 0f;
			ulong num2 = (ulong)((long)cube.width);
			mip = Mathf.Min(QPow.Log2i(num2) + 1, mip);
			num2 >>= mip;
			float[] array = new float[9];
			Vector3 zero = Vector3.zero;
			for (ulong num3 = 0UL; num3 < 6UL; num3 += 1UL)
			{
				Color color = Color.black;
				Color[] pixels = cube.GetPixels((int)num3, mip);
				for (ulong num4 = 0UL; num4 < num2; num4 += 1UL)
				{
					for (ulong num5 = 0UL; num5 < num2; num5 += 1UL)
					{
						float num6 = 1f;
						Util.invCubeLookup(ref zero, ref num6, num3, num5, num4, num2);
						float num7 = 1.3333334f;
						ulong num8 = num4 * num2 + num5;
						checked
						{
							if (hdr)
							{
								RGB.fromRGBM(ref color, pixels[(int)((IntPtr)num8)], true);
							}
							else
							{
								color = pixels[(int)((IntPtr)num8)];
							}
							array[0] = SHUtil.project_l0_m0(zero);
							array[1] = SHUtil.project_l1_mneg1(zero);
							array[2] = SHUtil.project_l1_m0(zero);
							array[3] = SHUtil.project_l1_m1(zero);
							array[4] = SHUtil.project_l2_mneg2(zero);
							array[5] = SHUtil.project_l2_mneg1(zero);
							array[6] = SHUtil.project_l2_m0(zero);
							array[7] = SHUtil.project_l2_m1(zero);
							array[8] = SHUtil.project_l2_m2(zero);
						}
						for (int i = 0; i < 9; i++)
						{
							sh.c[3 * i] += num7 * num6 * color[0] * array[i];
							sh.c[3 * i + 1] += num7 * num6 * color[1] * array[i];
							sh.c[3 * i + 2] += num7 * num6 * color[2] * array[i];
						}
						num += num6;
					}
				}
			}
			SHUtil.scale(ref sh, 16f / num);
		}

		public static void convolve(ref SHEncoding sh)
		{
			SHUtil.convolve(ref sh, 1f, 0.6666667f, 0.25f);
		}

		public static void convolve(ref SHEncoding sh, float conv0, float conv1, float conv2)
		{
			for (int i = 0; i < 27; i++)
			{
				if (i < 3)
				{
					sh.c[i] *= conv0;
				}
				else if (i < 12)
				{
					sh.c[i] *= conv1;
				}
				else
				{
					sh.c[i] *= conv2;
				}
			}
		}
	}
}
