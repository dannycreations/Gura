using System;
using ObjectModel;
using UnityEngine;

namespace BiteEditor
{
	public class Attractor : MonoBehaviour, IAttractor
	{
		public Vector2f Position2D
		{
			get
			{
				return new Vector2f(base.transform.position, true);
			}
		}

		public float AttractionR
		{
			get
			{
				return (!this._isDetractor) ? this._pumpingR : this._supressionR;
			}
		}

		public float RandomR
		{
			get
			{
				return this._randomR;
			}
		}

		public float AttractionMinValue
		{
			get
			{
				return this._attractionMinValue;
			}
		}

		public float AttractionMaxValue
		{
			get
			{
				return this._attractionMaxValue;
			}
		}

		public int InitialPhase
		{
			get
			{
				return this._initialPhase;
			}
		}

		public int FillTime
		{
			get
			{
				return this._fillTime;
			}
		}

		public int MaxPowerDuration
		{
			get
			{
				return this._maxPowerDuration;
			}
		}

		public int MaxPowerRandomDuration
		{
			get
			{
				return this._maxPowerRandomDuration;
			}
		}

		public int SwitchDelay
		{
			get
			{
				return this._switchDelay;
			}
		}

		public ushort[] LinkedZones
		{
			get
			{
				return this._linkedZones;
			}
		}

		[SerializeField]
		private bool _isDetractor;

		[SerializeField]
		private float _attractionMinValue = 1f;

		[SerializeField]
		private float _attractionMaxValue = 2f;

		[SerializeField]
		private float _supressionR = 10f;

		[SerializeField]
		private float _pumpingR = 5f;

		[Tooltip("random movement")]
		[SerializeField]
		private float _randomR;

		[Tooltip("maximum time in minutes for movement to the past from initial time randomization")]
		[SerializeField]
		private int _initialPhase = 10;

		[Tooltip("how many minutes it takes to get the maximum|minimum value")]
		[SerializeField]
		private int _fillTime = 10;

		[Tooltip("duration of the maximum|minimum value")]
		[SerializeField]
		private int _maxPowerDuration = 10;

		[Tooltip("randomized part of maximum|minimum value. finalDuration = _maxPowerDuration + (0.._maxPowerRandomDuration)")]
		[SerializeField]
		private int _maxPowerRandomDuration = 30;

		[Tooltip("delay in minutes after phase finishing")]
		[SerializeField]
		private int _switchDelay;

		[SerializeField]
		private ushort[] _linkedZones;
	}
}
