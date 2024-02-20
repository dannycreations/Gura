using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ObjectModel
{
	public class ChumPiece
	{
		[JsonIgnore]
		public Guid OwnerId
		{
			get
			{
				return this._ownerId;
			}
		}

		[JsonIgnore]
		public float PowerFillTime
		{
			get
			{
				return this._powerFillTime;
			}
		}

		[JsonIgnore]
		public float MaxPowerDuration
		{
			get
			{
				return this._maxPowerDuration;
			}
		}

		[JsonIgnore]
		public float PowerReleaseTime
		{
			get
			{
				return this._powerReleaseTime;
			}
		}

		[JsonIgnore]
		public Vector3f Position
		{
			get
			{
				return this._position;
			}
		}

		[JsonIgnore]
		public Vector3f FromPosition
		{
			get
			{
				return this._fromPosition;
			}
		}

		[JsonIgnore]
		public float AttractionR
		{
			get
			{
				return this._attractionR;
			}
		}

		[JsonIgnore]
		public float AttractionH
		{
			get
			{
				return this._attractionH;
			}
		}

		[JsonIgnore]
		public float AddedAt
		{
			get
			{
				return this._addedAt;
			}
		}

		[JsonIgnore]
		public List<FishTypeAttractivity> FishTypeAttractivity
		{
			get
			{
				return this._fishTypeAttractivity;
			}
		}

		[JsonIgnore]
		public float TemperatureK
		{
			get
			{
				return this._temperatureK;
			}
		}

		[JsonIgnore]
		public float EndAt
		{
			get
			{
				return this._addedAt + this._powerFillTime + this._maxPowerDuration + this._powerReleaseTime;
			}
		}

		[JsonIgnore]
		public Vector3f FlowV
		{
			get
			{
				return this._flowV;
			}
		}

		[JsonProperty]
		private Guid _ownerId;

		[JsonProperty]
		private float _powerFillTime;

		[JsonProperty]
		private float _maxPowerDuration;

		[JsonProperty]
		private float _powerReleaseTime;

		[JsonProperty]
		private Vector3f _position;

		[JsonProperty]
		private Vector3f _fromPosition;

		[JsonProperty]
		private float _attractionR;

		[JsonProperty]
		private float _attractionH;

		[JsonProperty]
		private float _addedAt;

		[JsonProperty]
		private List<FishTypeAttractivity> _fishTypeAttractivity;

		[JsonProperty]
		private float _temperatureK;

		[JsonProperty]
		private Vector3f _flowV;
	}
}
