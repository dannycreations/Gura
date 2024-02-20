using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI._2D.GlobalMap
{
	public class GlobePondsCoordinates
	{
		public static Vector3 GetCoordinate(int pondId)
		{
			return GlobePondsCoordinates.Get(pondId, GlobePondsCoordinates.Data, "GlobePondsCoordinates - can't found coordinate for pondId:{0}");
		}

		public static Vector3 GetRegionRotation(GlobeRegions region)
		{
			return GlobePondsCoordinates.Get((int)region, GlobePondsCoordinates.GlobeRegionsRotations, "GetRegionRotation - can't found rotation for region:{0}");
		}

		private static Vector3 Get(int pondId, Dictionary<int, Vector3> data, string error)
		{
			bool flag = data.ContainsKey(pondId);
			if (!flag)
			{
				Debug.LogErrorFormat(error, new object[] { pondId });
			}
			return (!flag) ? Vector3.zero : data[pondId];
		}

		private static readonly Dictionary<int, Vector3> Data = new Dictionary<int, Vector3>
		{
			{
				2,
				new Vector3(0.7075279f, 0.2084292f, 0.6752498f)
			},
			{
				113,
				new Vector3(0.8941216f, -0.1375292f, 0.4261839f)
			},
			{
				111,
				new Vector3(0.7059591f, -0.1777212f, 0.6855932f)
			},
			{
				115,
				new Vector3(0.7033303f, -0.05972374f, 0.7083506f)
			},
			{
				114,
				new Vector3(0.6082693f, 0.4008057f, 0.6851013f)
			},
			{
				123,
				new Vector3(0.867952f, 0.003886718f, 0.4966335f)
			},
			{
				121,
				new Vector3(0.3415057f, 0.3733827f, 0.8625312f)
			},
			{
				119,
				new Vector3(0.8391563f, 0.1412392f, 0.5252324f)
			},
			{
				102,
				new Vector3(0.7900837f, -0.01056035f, 0.6129086f)
			},
			{
				106,
				new Vector3(0.7948847f, -0.1246869f, 0.5938115f)
			},
			{
				109,
				new Vector3(0.4732483f, 0.352295f, 0.8074191f)
			},
			{
				118,
				new Vector3(0.4889108f, 0.2040566f, 0.848132f)
			},
			{
				100,
				new Vector3(0.7059605f, 0.2539124f, 0.6611725f)
			},
			{
				150,
				new Vector3(-0.2356136f, -0.6073645f, 0.7586796f)
			},
			{
				160,
				new Vector3(-0.07906972f, -0.5864796f, 0.8060956f)
			}
		};

		private static readonly Dictionary<int, Vector3> GlobeRegionsRotations = new Dictionary<int, Vector3> { 
		{
			2,
			new Vector3(-10.699f, 80.291f, -22.431f)
		} };
	}
}
