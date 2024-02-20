using System;
using UnityEngine;

namespace FPWorldStreamer
{
	public class TerrainToMeshFixer : MonoBehaviour, ICell
	{
		public string GroupTag
		{
			get
			{
				return this._groupTag;
			}
		}

		public float XSize
		{
			get
			{
				return this._xSize;
			}
		}

		public float ZSize
		{
			get
			{
				return this._zSize;
			}
		}

		public string LayerName
		{
			get
			{
				return base.gameObject.name;
			}
		}

		[SerializeField]
		private string _groupTag = "StreamerGroup";

		[SerializeField]
		private float _xSize;

		[SerializeField]
		private float _zSize;

		[HideInInspector]
		public GUIStyle errorStyle = new GUIStyle();
	}
}
