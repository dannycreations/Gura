using System;
using UnityEngine;

namespace FPWorldStreamer
{
	[ExecuteInEditMode]
	public class LayerSettings : MonoBehaviour, ICell
	{
		public string LayerName
		{
			get
			{
				return this.layerName;
			}
		}

		public float XSize
		{
			get
			{
				return this.xSize;
			}
		}

		public float ZSize
		{
			get
			{
				return this.zSize;
			}
		}

		public string layerName = "NewLayer";

		public Transform objectsRoot;

		public float xSize = 10f;

		public float zSize = 10f;

		public float yViewSize = 200f;

		public int loadingCellsR = 1;

		public float unloadingBorderDist = 10f;

		public Color color = Color.red;

		[HideInInspector]
		public bool collapsed;
	}
}
