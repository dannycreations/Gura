using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phy
{
	public static class SplineBuilder
	{
		public static void BuildHermite(IList<Vector3> masses, Vector3 lineEndingPosition, LineRenderer renderer)
		{
			int num = 0;
			Vector3? vector = null;
			int i = 0;
			float num2 = 0f;
			for (int j = 0; j < masses.Count; j++)
			{
				Vector3 vector2 = masses[j];
				if (vector != null)
				{
					float magnitude = (vector2 - vector.Value).magnitude;
					num2 += magnitude;
					if (num2 < 100f)
					{
						SplineBuilder.BuildHermite_countsBuffer[i] = (ushort)((int)(magnitude / 0.05f) + 1);
					}
					else
					{
						SplineBuilder.BuildHermite_countsBuffer[i] = 2;
					}
					num += (int)SplineBuilder.BuildHermite_countsBuffer[i];
					i++;
				}
				vector = new Vector3?(vector2);
			}
			renderer.positionCount = num + 1;
			int num3 = 0;
			for (i = 0; i < masses.Count - 1; i++)
			{
				Vector3 vector3 = masses[i];
				Vector3 vector4 = masses[i + 1];
				Vector3 vector5;
				if (i > 0)
				{
					vector5 = 0.5f * (vector4 - masses[i - 1]);
				}
				else
				{
					vector5 = vector4 - vector3;
				}
				Vector3 vector6;
				if (i < masses.Count - 2)
				{
					vector6 = 0.5f * (masses[i + 2] - vector3);
				}
				else
				{
					vector6 = vector4 - vector3;
				}
				int num4 = (int)SplineBuilder.BuildHermite_countsBuffer[i];
				float num5 = 1f / (float)num4;
				if (i == masses.Count - 2 && num4 > 1)
				{
					num5 = 1f / (float)(num4 - 1);
				}
				for (int k = 0; k < num4; k++)
				{
					float num6 = (float)k * num5;
					Vector3 vector7 = (2f * num6 * num6 * num6 - 3f * num6 * num6 + 1f) * vector3 + (num6 * num6 * num6 - 2f * num6 * num6 + num6) * vector5 + (-2f * num6 * num6 * num6 + 3f * num6 * num6) * vector4 + (num6 * num6 * num6 - num6 * num6) * vector6;
					renderer.SetPosition(num3, vector7);
					num3++;
				}
			}
			renderer.SetPosition(num3, lineEndingPosition);
		}

		public static void BuildBezier(IList<Mass> masses, LineRenderer renderer)
		{
			int num = 0;
			Mass mass = null;
			int i = 0;
			int[] array = new int[masses.Count - 1];
			float num2 = 0f;
			for (int j = 0; j < masses.Count; j++)
			{
				Mass mass2 = masses[j];
				if (mass != null)
				{
					float magnitude = (mass2.Position - mass.Position).magnitude;
					num2 += magnitude;
					if (num2 < 100f)
					{
						array[i] = (int)(magnitude / 0.05f) + 1;
					}
					else
					{
						array[i] = 2;
					}
					num += array[i];
					i++;
				}
				mass = mass2;
			}
			renderer.SetVertexCount(num - array[array.Length - 1] + 1);
			int num3 = 0;
			renderer.SetPosition(num3, masses[0].Position);
			num3++;
			for (i = 0; i < masses.Count - 2; i++)
			{
				Vector3 vector = 0.5f * (masses[i].Position + masses[i + 1].Position);
				Vector3 position = masses[i + 1].Position;
				Vector3 vector2 = 0.5f * (masses[i + 1].Position + masses[i + 2].Position);
				int num4 = array[i];
				if (i == masses.Count - 3)
				{
					num4--;
				}
				float num5 = 1f / (float)num4;
				for (int k = 0; k < num4; k++)
				{
					float num6 = (float)k * num5;
					Vector3 vector3 = (1f - num6) * (1f - num6) * vector + 2f * (1f - num6) * num6 * position + num6 * num6 * vector2;
					renderer.SetPosition(num3, vector3);
					num3++;
				}
			}
			renderer.SetPosition(num3, masses[masses.Count - 1].Position);
		}

		public static BezierConfig InitBezier(IList<Mass> masses)
		{
			BezierConfig bezierConfig = new BezierConfig
			{
				OriginalMasses = masses,
				Masses = new List<Mass>()
			};
			for (int i = 0; i < masses.Count; i++)
			{
				bezierConfig.Masses.Add(new Mass(GameFactory.Player.RodSlot.Sim, 0f, masses[i].Position, Mass.MassType.Unknown));
			}
			bezierConfig.FirstMass = masses[0];
			bezierConfig.SecondMass = masses[1];
			Vector3 position = bezierConfig.FirstMass.Position;
			bezierConfig.FirstSegLength = (position - bezierConfig.SecondMass.Position).magnitude;
			int num = masses.Count - 2;
			bezierConfig.PreLastMass = masses[num];
			bezierConfig.LastMass = masses[num + 1];
			Vector3 position2 = bezierConfig.PreLastMass.Position;
			Vector3 position3 = bezierConfig.LastMass.Position;
			bezierConfig.LastSegLength = (position2 - position3).magnitude;
			bezierConfig.FullLength = 0f;
			for (int j = 1; j < masses.Count; j++)
			{
				bezierConfig.FullLength += (masses[j].Position - masses[j - 1].Position).magnitude;
			}
			bezierConfig.LengthCorrectionMultiplier = (bezierConfig.FullLength + (bezierConfig.FirstSegLength + bezierConfig.LastSegLength) / 4f) / bezierConfig.FullLength;
			return bezierConfig;
		}

		public static void UpdatePlane(BezierConfig config)
		{
			Vector3 vector;
			Vector3 vector2;
			Math3d.PlaneFrom3Points(out vector, out vector2, config.OriginalMasses[0].Position, config.OriginalMasses[2].Position, config.OriginalMasses[5].Position);
			config.Masses[0].Position = config.OriginalMasses[0].Position;
			config.Masses[2].Position = config.OriginalMasses[2].Position;
			config.Masses[5].Position = config.OriginalMasses[5].Position;
			config.Masses[1].Position = Math3d.ProjectPointOnPlane(vector, vector2, config.OriginalMasses[1].Position);
			config.Masses[3].Position = Math3d.ProjectPointOnPlane(vector, vector2, config.OriginalMasses[3].Position);
			config.Masses[4].Position = Math3d.ProjectPointOnPlane(vector, vector2, config.OriginalMasses[4].Position);
		}

		public static TransformPoint GetBezierPoint(BezierConfig config, float length)
		{
			Vector3 position = config.FirstMass.Position;
			Vector3 position2 = config.PreLastMass.Position;
			Vector3 position3 = config.LastMass.Position;
			Vector3 vector = 0.5f * (position2 + position3);
			TransformPoint bezierPoint = SplineBuilder.GetBezierPoint(config.Masses, length);
			if (bezierPoint == null)
			{
				bezierPoint = SplineBuilder.priorOriginalBezierPoint;
			}
			else
			{
				SplineBuilder.priorOriginalBezierPoint = bezierPoint;
			}
			length *= config.LengthCorrectionMultiplier;
			float num;
			if (length < config.FirstSegLength / 2f)
			{
				Vector3 position4 = SplineBuilder.GetBezierPoint(config.Masses, 0f).Position;
				num = 2f * length / config.FirstSegLength;
				return new TransformPoint
				{
					Position = Vector3.Lerp(position, position4, num),
					Rotation = bezierPoint.Rotation
				};
			}
			TransformPoint bezierPoint2 = SplineBuilder.GetBezierPoint(config.Masses, length - config.FirstSegLength / 2f);
			if (bezierPoint2 != null)
			{
				bezierPoint2.Rotation = bezierPoint.Rotation;
				return bezierPoint2;
			}
			num = Mathf.Clamp01(2f * (length - config.FullLength) / config.LastSegLength);
			return new TransformPoint
			{
				Position = Vector3.Lerp(vector, position3, num),
				Rotation = bezierPoint.Rotation
			};
		}

		private static TransformPoint GetBezierPoint(IList<Mass> masses, float length)
		{
			Mass mass = null;
			Mass mass2 = null;
			Mass mass3 = null;
			float num = 0f;
			float num2 = 0f;
			Mass mass4 = null;
			bool flag = false;
			int i = 0;
			while (i < masses.Count)
			{
				Mass mass5 = masses[i];
				if (flag)
				{
					mass3 = mass5;
					break;
				}
				if (mass4 == null)
				{
					goto IL_7C;
				}
				num2 = (mass5.Position - mass4.Position).magnitude;
				if (num + num2 < length)
				{
					num += num2;
					goto IL_7C;
				}
				mass = mass4;
				mass2 = mass5;
				flag = true;
				IL_80:
				i++;
				continue;
				IL_7C:
				mass4 = mass5;
				goto IL_80;
			}
			if (mass == null || mass3 == null)
			{
				return null;
			}
			Vector3 vector = 0.5f * (mass.Position + mass2.Position);
			Vector3 position = mass2.Position;
			Vector3 vector2 = 0.5f * (mass2.Position + mass3.Position);
			float num3 = (length - num) / num2;
			float num4 = 1f - num3;
			TransformPoint transformPoint = new TransformPoint
			{
				Position = num4 * num4 * vector + 2f * num4 * num3 * position + num3 * num3 * vector2
			};
			Vector3 vector3 = 2f * (1f - num3) * (position - vector) + 2f * num3 * (vector2 - position);
			transformPoint.Rotation = Quaternion.LookRotation(vector3);
			return transformPoint;
		}

		public static int BuildCatmullRom(List<Vector3> points, float alpha, Vector3[] spline, int verticesPerSegment = 10, int startVertexIndex = 0, float noiseAmp = 0f)
		{
			return SplineBuilder.BuildCatmullRom(2f * points[0] - points[1], 2f * points[points.Count - 1] - points[points.Count - 2], points, noiseAmp, alpha, spline, verticesPerSegment, startVertexIndex);
		}

		public static int BuildCatmullRom(Vector3 extraLeft, Vector3 extraRight, List<Vector3> points, float noiseAmp, float alpha, Vector3[] spline, int verticesPerSegment = 10, int startVertexIndex = 0)
		{
			int num = Math.Min(SplineBuilder.catmullRomPs.Length, points.Count);
			int num2 = num + 2;
			SplineBuilder.catmullRomPs[0] = extraLeft;
			for (int i = 0; i < num; i++)
			{
				float num3 = Mathf.Pow((float)i / (float)num, 0.1f);
				SplineBuilder.catmullRomPs[i + 1] = points[i] + SplineBuilder.PerlinVector(points[i], 4f) * num3 * noiseAmp;
			}
			SplineBuilder.catmullRomPs[num2 - 1] = extraRight;
			SplineBuilder.catmullRomts[0] = 0f;
			for (int j = 0; j < num2 - 1; j++)
			{
				SplineBuilder.catmullRomts[j + 1] = SplineBuilder.catmullRomts[j] + Mathf.Pow((SplineBuilder.catmullRomPs[j + 1] - SplineBuilder.catmullRomPs[j]).sqrMagnitude, alpha * 0.5f);
			}
			int num4 = num2 - 2 + (num2 - 3) * verticesPerSegment;
			if (num4 + startVertexIndex >= 1000)
			{
				verticesPerSegment = (1000 - startVertexIndex - (num2 - 2)) / (num2 - 3);
				num4 = num2 - 2 + (num2 - 3) * verticesPerSegment;
			}
			int num5 = startVertexIndex;
			for (int k = 0; k < num2 - 3; k++)
			{
				spline[num5] = SplineBuilder.catmullRomPs[k + 1];
				Vector3 vector = SplineBuilder.catmullRomPs[k];
				Vector3 vector2 = SplineBuilder.catmullRomPs[k + 1];
				Vector3 vector3 = SplineBuilder.catmullRomPs[k + 2];
				Vector3 vector4 = SplineBuilder.catmullRomPs[k + 3];
				float num6 = SplineBuilder.catmullRomts[k];
				float num7 = SplineBuilder.catmullRomts[k + 1];
				float num8 = SplineBuilder.catmullRomts[k + 2];
				float num9 = SplineBuilder.catmullRomts[k + 3];
				for (int l = 1; l <= verticesPerSegment; l++)
				{
					num5++;
					if (Mathf.Approximately(num6, num7) || Mathf.Approximately(num7, num8) || Mathf.Approximately(num8, num9))
					{
						spline[num5] = vector2;
					}
					else
					{
						float num10 = num7 + (float)l * (num8 - num7) / (float)(verticesPerSegment + 1);
						Vector3 vector5 = vector * (num7 - num10) / (num7 - num6) + vector2 * (num10 - num6) / (num7 - num6);
						Vector3 vector6 = vector2 * (num8 - num10) / (num8 - num7) + vector3 * (num10 - num7) / (num8 - num7);
						Vector3 vector7 = vector3 * (num9 - num10) / (num9 - num8) + vector4 * (num10 - num8) / (num9 - num8);
						Vector3 vector8 = vector5 * (num8 - num10) / (num8 - num6) + vector6 * (num10 - num6) / (num8 - num6);
						Vector3 vector9 = vector6 * (num9 - num10) / (num9 - num7) + vector7 * (num10 - num7) / (num9 - num7);
						Vector3 vector10 = vector8 * (num8 - num10) / (num8 - num7) + vector9 * (num10 - num7) / (num8 - num7);
						spline[num5] = vector10;
					}
				}
				num5++;
			}
			spline[num5] = SplineBuilder.catmullRomPs[num2 - 2];
			return num4;
		}

		public static int BuildBezier5CatmullRomCompositeFirstSection(List<Vector3> points, float noiseAmp, float alpha, Vector3[] spline, int bezierPoints, int catmullPointsPerSegment)
		{
			if (points.Count < 2)
			{
				return 0;
			}
			int num;
			if (points.Count < 8)
			{
				num = SplineBuilder.BuildCatmullRom(points, alpha, spline, catmullPointsPerSegment, 0, 0f);
			}
			else
			{
				BezierCurve bezierCurve = new BezierCurve(5);
				for (int i = 0; i <= 5; i++)
				{
					bezierCurve.AnchorPoints[i] = points[i];
				}
				num = bezierPoints;
				for (int j = 0; j < bezierPoints; j++)
				{
					float num2 = (float)j / (float)bezierPoints;
					bezierCurve.SetT(num2);
					Vector3 vector = bezierCurve.Point();
					spline[j] = vector + SplineBuilder.PerlinVector(vector, 4f) * Mathf.Pow(num2, 0.5f) * noiseAmp;
				}
			}
			return num;
		}

		public static int BuildBezier5CatmullRomComposite(List<Vector3> points, float noiseAmp, float alpha, Vector3[] spline, int bezierPoints, int catmullPointsPerSegment)
		{
			if (points.Count < 2)
			{
				return 0;
			}
			return SplineBuilder.BuildCatmullRom(points, alpha, spline, catmullPointsPerSegment, 0, noiseAmp);
		}

		public static Vector3 PerlinVector(Vector3 point, float frequency = 1f)
		{
			Vector3 vector = point * frequency;
			return new Vector3(Mathf.PerlinNoise(vector.x, vector.y), Mathf.PerlinNoise(vector.y, vector.z), Mathf.PerlinNoise(vector.z, vector.x)) * 2f - Vector3.one;
		}

		private const float HighSplineLength = 100f;

		private const float HighSplineSegmentLength = 0.05f;

		private const int LowSplineSegmentCount = 2;

		private static ushort[] BuildHermite_countsBuffer = new ushort[512];

		private static TransformPoint priorOriginalBezierPoint;

		private static Vector3[] catmullRomPs = new Vector3[400];

		private static float[] catmullRomts = new float[400];

		public const int MaxBezier5CatmullRomCompositePoints = 1000;
	}
}
