using System;
using System.Collections.Generic;
using System.Linq;
using Malee;
using ObjectModel;
using UnityEngine;

namespace BiteEditor
{
	public class TimedMaps : ProbabilityMap
	{
		public override float[,] CalcMapsOnCpu(Rect worldBound, int widthResolution, int heightResolution, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f)
		{
			ProbabilityMap probabilityMap = this.FindRecord(progress);
			return probabilityMap.CalcMapsOnCpu(worldBound, widthResolution, heightResolution, progress, wind, isInversed, rangeMin, rangeMax);
		}

		public override RenderTexture CalcMapsOnGpu(Rect worldBound, int widthResolution, int heightResolution, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f)
		{
			ProbabilityMap probabilityMap = this.FindRecord(progress);
			return probabilityMap.CalcMapsOnGpu(worldBound, widthResolution, heightResolution, progress, wind, isInversed, rangeMin, rangeMax);
		}

		private ProbabilityMap FindRecord(int progress)
		{
			List<TimedMaps.TimeRecord> list = this._maps.OrderBy((TimedMaps.TimeRecord r) => r.Till).ToList<TimedMaps.TimeRecord>();
			int num = list.FindIndex((TimedMaps.TimeRecord r) => r.Till * 60 > progress);
			if (num > 0)
			{
				return list[num - 1].Map;
			}
			if (num == -1)
			{
				return list[this._maps.Count - 1].Map;
			}
			return list[0].Map;
		}

		[Reorderable]
		[SerializeField]
		private TimedMaps.MapsList _maps;

		[Serializable]
		private class TimeRecord
		{
			public int Till
			{
				get
				{
					return this._till;
				}
			}

			public ProbabilityMap Map
			{
				get
				{
					return this._map;
				}
			}

			[Tooltip("From previous hour to this. First record start from 5am")]
			[SerializeField]
			private int _till;

			[SerializeField]
			private ProbabilityMap _map;
		}

		[Serializable]
		private class MapsList : ReorderableArray<TimedMaps.TimeRecord>
		{
		}
	}
}
