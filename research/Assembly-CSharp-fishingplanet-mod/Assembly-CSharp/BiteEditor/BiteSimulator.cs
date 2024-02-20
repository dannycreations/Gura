using System;
using System.Collections.Generic;
using Malee;
using UnityEngine;

namespace BiteEditor
{
	public class BiteSimulator : MonoBehaviour
	{
		public bool IsSimulationStarted
		{
			get
			{
				return this._lastUpdateAt > 0.0;
			}
		}

		public bool IsSimulationPaused
		{
			get
			{
				return this._isPaused;
			}
		}

		public void Setup()
		{
			Renderer component = base.GetComponent<Renderer>();
			component.sharedMaterial = new Material(this._displayMaterialPrefab);
		}

		private const int MINUTES_IN_DAY = 1440;

		private const int ATTRACTORS_MAX_COUNT_PER_BLIT = 20;

		[SerializeField]
		private Material _displayMaterialPrefab;

		[SerializeField]
		private Material _attractorsAddMaterial;

		[SerializeField]
		private Material _attractorsMulMaterial;

		[SerializeField]
		private Material _attractorsOverrideMaterial;

		[SerializeField]
		private MapResolution _widthResolution = MapResolution._1024;

		[SerializeField]
		private MapResolution _heightResolution = MapResolution._1024;

		[Reorderable]
		[SerializeField]
		private BiteSimulator.WeathersList _weathers;

		[SerializeField]
		private FishName _fish;

		[SerializeField]
		private FishForm _fishForm;

		[SerializeField]
		private Depth _range;

		[Tooltip("How many minutes in 1 sec")]
		[Range(1f, 120f)]
		[SerializeField]
		private byte _speed = 10;

		[Range(1f, 30f)]
		[SerializeField]
		private byte _fps = 1;

		[Range(1f, 1440f)]
		[Tooltip("Current day progress in minutes with possibility to change")]
		[SerializeField]
		private int _progress = 1;

		[Tooltip("Readonly property to display progress description when simulation started")]
		[SerializeField]
		private string _progressDescription;

		[SerializeField]
		private bool _supressTimeChart;

		[HideInInspector]
		[SerializeField]
		private double _lastUpdateAt = -1.0;

		[HideInInspector]
		[SerializeField]
		private bool _isPaused;

		[HideInInspector]
		[SerializeField]
		private int _dayIndex = -1;

		[HideInInspector]
		[SerializeField]
		private FishLayer _curLayer;

		[HideInInspector]
		[SerializeField]
		private List<AttractorsGroup> attractorGroups;

		[HideInInspector]
		[SerializeField]
		private RenderTexture[] _blitsBuffer;

		[HideInInspector]
		[SerializeField]
		private BiteSimulator.AttractorActor[] _attractorActors;

		private Texture2D _texture;

		[HideInInspector]
		[SerializeField]
		private Renderer _renderer;

		[Serializable]
		private struct AttractorActor
		{
			public AttractorActor(Material material, List<Vector4> positions, List<Vector4> options)
			{
				this.Material = material;
				this.Positions = positions;
				this.Options = options;
			}

			public readonly Material Material;

			public readonly List<Vector4> Positions;

			public readonly List<Vector4> Options;
		}

		[Serializable]
		public class WeathersList : ReorderableArray<Weather>
		{
		}
	}
}
