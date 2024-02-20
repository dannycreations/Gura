using System;
using System.Collections.Generic;
using UnityEngine;

namespace FPWorldStreamer
{
	[ExecuteInEditMode]
	public class SceneSplitter : MonoBehaviour
	{
		public List<LayerSettings> SceneLayers
		{
			get
			{
				return this._sceneLayers;
			}
		}

		public string groupTag = "StreamerGroup";

		[HideInInspector]
		public string scenesPath = "Assets/SplitScenes/";

		[HideInInspector]
		public bool isLayersCollapsed;

		private List<LayerSettings> _sceneLayers = new List<LayerSettings>();
	}
}
