using System;
using UnityEngine;

namespace FPWorldStreamer
{
	public class TerrainSplitter : MonoBehaviour
	{
		public Material customMaterial;

		public Transform destinationRoot;

		public float minimalY;

		public string groupTag = "StreamerGroup";

		public int eachDirectionSectorsCount = 8;

		public int detailResolutionPerPatch = 8;
	}
}
