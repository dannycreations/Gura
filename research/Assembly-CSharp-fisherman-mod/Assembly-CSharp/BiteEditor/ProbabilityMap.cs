using System;
using ObjectModel;
using UnityEngine;

namespace BiteEditor
{
	public abstract class ProbabilityMap : MonoBehaviour
	{
		public abstract float[,] CalcMapsOnCpu(Rect worldBound, int widthResolution, int heightResolution, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f);

		public abstract RenderTexture CalcMapsOnGpu(Rect worldBound, int widthResolution, int heightResolution, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f);

		public static Rect GetBounds(Vector3 center, float width, float height)
		{
			return new Rect(center.x - width / 2f, center.z - height / 2f, width, height);
		}

		public int ExportId;
	}
}
