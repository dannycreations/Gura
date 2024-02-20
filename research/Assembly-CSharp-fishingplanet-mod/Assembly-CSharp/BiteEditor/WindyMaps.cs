using System;
using System.Collections.Generic;
using System.Linq;
using ObjectModel;
using UnityEngine;

namespace BiteEditor
{
	public class WindyMaps : ProbabilityMap
	{
		public override float[,] CalcMapsOnCpu(Rect worldBound, int widthResolution, int heightResolution, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f)
		{
			ProbabilityMap probabilityMap = this.FindMap(wind);
			return probabilityMap.CalcMapsOnCpu(worldBound, widthResolution, heightResolution, progress, wind, isInversed, rangeMin, rangeMax);
		}

		public override RenderTexture CalcMapsOnGpu(Rect worldBound, int widthResolution, int heightResolution, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f)
		{
			ProbabilityMap probabilityMap = this.FindMap(wind);
			return probabilityMap.CalcMapsOnGpu(worldBound, widthResolution, heightResolution, progress, wind, isInversed, rangeMin, rangeMax);
		}

		private ProbabilityMap FindMap(WindDirection wind)
		{
			WindyMaps.WindRecord windRecord = this._maps.FirstOrDefault((WindyMaps.WindRecord r) => r.Winds.Contains(wind));
			if (windRecord != null)
			{
				return windRecord.Map;
			}
			windRecord = this._maps.FirstOrDefault((WindyMaps.WindRecord r) => r.Winds.Count == 0);
			if (windRecord != null)
			{
				return windRecord.Map;
			}
			LogHelper.Error("{0} can't find map for wind {1}", new object[] { base.name, wind });
			return this._maps[0].Map;
		}

		[SerializeField]
		private List<WindyMaps.WindRecord> _maps;

		[Serializable]
		private class WindRecord
		{
			public List<WindDirection> Winds
			{
				get
				{
					return this._winds;
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
			private List<WindDirection> _winds;

			[SerializeField]
			private ProbabilityMap _map;
		}
	}
}
