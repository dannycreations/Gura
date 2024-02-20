using System;
using BiteEditor.ObjectModel;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BiteEditor
{
	public class BiteEditor : MonoBehaviour
	{
		public static string SceneName
		{
			get
			{
				string name = SceneManager.GetActiveScene().name;
				return (!name.Contains("_settings")) ? string.Format("{0}_settings", SceneManager.GetActiveScene().name) : name;
			}
		}

		public static string ScenePath
		{
			get
			{
				return string.Format("{0}{1}.unity", "Assets\\BiteSystem\\Settings\\", BiteEditor.SceneName);
			}
		}

		public static string GetImagePath(BiteMapPatch patch)
		{
			return string.Format("{0}{1}_{2}.png", Settings.GetImagesFolder(BiteEditor.SceneName), patch.transform.parent.name, patch.name);
		}

		public Vector3 MainPos
		{
			get
			{
				return this._mainPos;
			}
		}

		public Vector2 MainSize
		{
			get
			{
				return this._mainSize;
			}
		}

		public MapResolution MainWidthResolution
		{
			get
			{
				return this._mainWidthResolution;
			}
		}

		public MapResolution MainHeightResolution
		{
			get
			{
				return this._mainHeightResolution;
			}
		}

		public Terrain Terrain
		{
			get
			{
				return this._terrain;
			}
		}

		public float MapOpacity
		{
			get
			{
				return this._mapOpacity;
			}
		}

		public float PaintedMapOpacity
		{
			get
			{
				return this._paintedMapOpacity;
			}
		}

		public Color AltimeterColor
		{
			get
			{
				return this._altimeterColor;
			}
		}

		public AttractorsGroup AttractorsPrefab
		{
			get
			{
				return this._attractorsPrefab;
			}
		}

		public Transform Weather
		{
			get
			{
				return this._weather;
			}
		}

		public Transform Maps
		{
			get
			{
				return this._maps;
			}
		}

		public Transform TimeCharts
		{
			get
			{
				return this._timeCharts;
			}
		}

		public Transform Simulators
		{
			get
			{
				return this._simulators;
			}
		}

		public HeightMap HeightMap
		{
			get
			{
				return this._heightMap;
			}
		}

		public GameObject FocusObject
		{
			get
			{
				return this._focusObject;
			}
		}

		public static BiteEditor Instance
		{
			get
			{
				return (!(BiteEditor._instance != null)) ? Object.FindObjectOfType<BiteEditor>() : BiteEditor._instance;
			}
		}

		public const string PREFABS_PATH = "Assets\\BiteSystem\\Prefabs\\";

		public const string SETTINGS_PATH = "Assets\\BiteSystem\\Settings\\";

		[SerializeField]
		private bool _isUsedToGenerateFish = true;

		[SerializeField]
		private Vector3 _mainPos;

		[SerializeField]
		private Vector2 _mainSize;

		[SerializeField]
		private MapResolution _mainWidthResolution = MapResolution._1024;

		[SerializeField]
		private MapResolution _mainHeightResolution = MapResolution._1024;

		[SerializeField]
		private bool _isVerticalFlip;

		[SerializeField]
		private SplatMap.LayerName[] _splatLayers;

		[SerializeField]
		private Terrain _terrain;

		[SerializeField]
		private float _mapOpacity = 0.3f;

		[SerializeField]
		private float _paintedMapOpacity = 1f;

		[SerializeField]
		private Color _altimeterColor = Color.yellow;

		[HideInInspector]
		public string OldSystemJSon;

		[HideInInspector]
		public float WaterEdgePointsDistance = 4f;

		[HideInInspector]
		public float WaterEdgeDeltaAngle = 5f;

		[HideInInspector]
		public float WaterDepth;

		[HideInInspector]
		public Color WallColor;

		[HideInInspector]
		[SerializeField]
		private BiteMap _biteMapPrefab;

		[HideInInspector]
		[SerializeField]
		private MapProcessor _mapProcessorPrefab;

		[HideInInspector]
		[SerializeField]
		private WindyMaps _windyMapsPrefab;

		[HideInInspector]
		[SerializeField]
		private TimedMaps _timedMapsPrefab;

		[HideInInspector]
		[SerializeField]
		private BiteSimulator _simulatorPrefab;

		[HideInInspector]
		[SerializeField]
		private TimeChart _timeChartPrefab;

		[HideInInspector]
		[SerializeField]
		private FishGroup _fishGroupPrefab;

		[HideInInspector]
		[SerializeField]
		private FishDescription _fishDescriptionPrefab;

		[HideInInspector]
		[SerializeField]
		private Curve _curvePrefab;

		[HideInInspector]
		[SerializeField]
		private AttractorsGroup _attractorsPrefab;

		[HideInInspector]
		[SerializeField]
		private Transform _weather;

		[HideInInspector]
		[SerializeField]
		private Transform _maps;

		[HideInInspector]
		[SerializeField]
		private Transform _timeCharts;

		[HideInInspector]
		[SerializeField]
		private Transform _simulators;

		[HideInInspector]
		[SerializeField]
		private Transform _fishGroups;

		[HideInInspector]
		[SerializeField]
		private Transform _curves;

		[HideInInspector]
		[SerializeField]
		private Transform _fishDB;

		[HideInInspector]
		[SerializeField]
		private HeightMap _heightMap;

		[HideInInspector]
		[SerializeField]
		private GameObject _focusObject;

		private static BiteEditor _instance;

		[Serializable]
		public class BrushSettings
		{
			public bool IsGradient;

			public bool IsInvertedGradient;

			public byte MinProbability = 10;

			public byte Probability = 10;

			public int BrushIndex = 3;

			public byte BrushScale = 3;
		}

		[Serializable]
		public class FillSettings
		{
			public bool IsHeightMapFilling;

			public bool IsInvertedGradient;

			public bool IsReplaceProbability;

			public byte MinProbability = 10;

			public byte Probability = 10;

			public float FillingDepth = 0.1f;

			public float FillingRaduis = 10f;
		}
	}
}
