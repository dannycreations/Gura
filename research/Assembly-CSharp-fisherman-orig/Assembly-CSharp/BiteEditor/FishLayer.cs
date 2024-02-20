using System;
using System.Collections.Generic;
using UnityEngine;

namespace BiteEditor
{
	public class FishLayer : MonoBehaviour
	{
		public Depth Depth
		{
			get
			{
				return this._depth;
			}
		}

		public List<FishForm> Forms
		{
			get
			{
				return this._forms;
			}
		}

		public ProbabilityMap Map
		{
			get
			{
				return this._map;
			}
		}

		public TimeChart TimeChart
		{
			get
			{
				return this._timeChart;
			}
		}

		public bool UseDefaultChart
		{
			get
			{
				return this._useDefaultChart;
			}
		}

		[SerializeField]
		private Depth _depth;

		[SerializeField]
		private List<FishForm> _forms;

		[SerializeField]
		private ProbabilityMap _map;

		[SerializeField]
		private float _mapModifier = 1f;

		[SerializeField]
		private TimeChart _timeChart;

		[SerializeField]
		private bool _useDefaultChart = true;
	}
}
