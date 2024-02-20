using System;
using System.Collections.Generic;
using UnityEngine;

namespace FPWorldStreamer
{
	public class LayersPrefab : MonoBehaviour
	{
		public List<LayerData> Layers = new List<LayerData>();

		[HideInInspector]
		public int LoadingScenesCellX;

		[HideInInspector]
		public int LoadingScenesCellZ;

		public Vector3 worldMovement;

		public List<Transform> staticObjects = new List<Transform>();
	}
}
