using System;
using UnityEngine;

namespace FPWorldStreamer
{
	public class LayerData : MonoBehaviour, ICell
	{
		public string LayerName
		{
			get
			{
				return this._name;
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

		public int LoadingCellsR
		{
			get
			{
				return this._loadingCellsR;
			}
		}

		public float UnloadingBorderDist
		{
			get
			{
				return this._unloadingBorderDist;
			}
		}

		public Color Color
		{
			get
			{
				return this._color;
			}
		}

		public string[] SceneNames
		{
			get
			{
				return this._sceneNames;
			}
		}

		public void Init(LayerSettings layer, string[] sceneNames)
		{
			this._name = layer.layerName;
			this._xSize = layer.xSize;
			this._zSize = layer.zSize;
			this._loadingCellsR = layer.loadingCellsR;
			this._unloadingBorderDist = layer.unloadingBorderDist;
			this._color = layer.color;
			this._sceneNames = sceneNames;
		}

		[SerializeField]
		private string _name;

		[SerializeField]
		private float _xSize;

		[SerializeField]
		private float _zSize;

		[SerializeField]
		private int _loadingCellsR;

		[SerializeField]
		private float _unloadingBorderDist;

		[SerializeField]
		private Color _color;

		[SerializeField]
		private string[] _sceneNames;

		[HideInInspector]
		public int LoadingScenesCellMinR;

		[HideInInspector]
		public int LoadingScenesCellMaxR = 1;
	}
}
