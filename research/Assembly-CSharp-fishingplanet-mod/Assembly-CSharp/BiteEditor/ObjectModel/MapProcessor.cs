using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ObjectModel;

namespace BiteEditor.ObjectModel
{
	public class MapProcessor : ProbabilityMap
	{
		public MapProcessor(int id, string name, Vector2f position2D, int initialMapId, bool isInverted, float rangeMin, float rangeMax, float width, float height)
			: base(id, name, position2D)
		{
			this._initialMapId = initialMapId;
			this._isInverted = isInverted;
			this._rangeMin = rangeMin;
			this._rangeMax = rangeMax;
			this._actions = new List<MapProcessor.Action>();
			this._width = width;
			this._height = height;
		}

		public MapProcessor()
		{
		}

		[JsonProperty]
		public override string Type
		{
			get
			{
				return "MapProcessor";
			}
		}

		public override void FinishInitialization(Pond pond)
		{
			this._initialMap = pond.FindMap(this._initialMapId);
			for (int i = 0; i < this._actions.Count; i++)
			{
				this._actions[i].FinishInitialization(pond);
			}
		}

		public void AddAction(Operation operation, int operandMapId, float rangeMin, float rangeMax)
		{
			this._actions.Add(new MapProcessor.Action(operation, operandMapId, rangeMin, rangeMax));
		}

		public override float GetProbability(Vector2f pos, int progress = 0, WindDirection wind = WindDirection.None, bool isInversed = false, float rangeMin = 0f, float rangeMax = 1f)
		{
			float num = this._initialMap.GetProbability(pos, progress, wind, this._isInverted, this._rangeMin, this._rangeMax);
			for (int i = 0; i < this._actions.Count; i++)
			{
				MapProcessor.Action action = this._actions[i];
				float probability = action.OperandMap.GetProbability(pos, progress, wind, false, action.RangeMin, action.RangeMax);
				switch (action.Operation)
				{
				case Operation.Add:
					num = Math.Min(num + probability, 1f);
					break;
				case Operation.Sub:
					num = Math.Max(num - probability, 0f);
					break;
				case Operation.Mul:
					num = Math.Min(num * probability, 1f);
					break;
				case Operation.InvAdd:
					num = Math.Min(num + 1f - probability, 1f);
					break;
				case Operation.InvSub:
					num = Math.Max(num - (1f - probability), 0f);
					break;
				}
			}
			return (!isInversed) ? ((1f - num) * rangeMin + num * rangeMax) : (num * rangeMin + (1f - num) * rangeMax);
		}

		[JsonProperty]
		private int _initialMapId;

		[JsonIgnore]
		private ProbabilityMap _initialMap;

		[JsonProperty]
		private bool _isInverted;

		[JsonProperty]
		private float _rangeMin;

		[JsonProperty]
		private float _rangeMax;

		[JsonProperty]
		private List<MapProcessor.Action> _actions;

		[JsonProperty]
		private float _width;

		[JsonProperty]
		private float _height;

		public class Action
		{
			public Action(Operation operation, int operandMapId, float rangeMin, float rangeMax)
			{
				this.Operation = operation;
				this.OperandMapId = operandMapId;
				this.RangeMin = rangeMin;
				this.RangeMax = rangeMax;
				this.OperandMap = null;
			}

			[JsonProperty]
			public Operation Operation { get; private set; }

			[JsonProperty]
			public int OperandMapId { get; private set; }

			[JsonIgnore]
			public ProbabilityMap OperandMap { get; private set; }

			[JsonProperty]
			public float RangeMin { get; private set; }

			[JsonProperty]
			public float RangeMax { get; private set; }

			public void FinishInitialization(Pond pond)
			{
				this.OperandMap = pond.FindMap(this.OperandMapId);
			}
		}
	}
}
