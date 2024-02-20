using System;
using UnityEngine;

namespace Boats
{
	public class TPMBoatSettings : MonoBehaviour
	{
		public Renderer[] Renderers
		{
			get
			{
				return this._renderers;
			}
		}

		public Transform PlayerPivot
		{
			get
			{
				return this._playerPivot;
			}
		}

		public Transform AnglerPivot
		{
			get
			{
				return this._anglerPivot;
			}
		}

		public PaddleSettings Oar
		{
			get
			{
				return this._oar;
			}
		}

		public Transform OarAnchor
		{
			get
			{
				return this._oarAnchor;
			}
		}

		public Transform[] WaterDisturbers
		{
			get
			{
				return this._waterDisturbers;
			}
		}

		public byte TicksBetweenDisturbs
		{
			get
			{
				return this._ticksBetweenDisturbs;
			}
		}

		public float DisturbanceRadius
		{
			get
			{
				return this._disturbanceRadius;
			}
		}

		public float DisturbanceMaxForceAtSpeed
		{
			get
			{
				return this._disturbanceMaxForceAtSpeed;
			}
		}

		public WaterDisturbForce DisturbanceMaxForce
		{
			get
			{
				return this._disturbanceMaxForce;
			}
		}

		public float GroundCorrection
		{
			get
			{
				return this._groundCorrection;
			}
		}

		public TPMEngineSettings Engine
		{
			get
			{
				return this._engine;
			}
		}

		public Animation TrollingEngine
		{
			get
			{
				return this._trollingEngine;
			}
		}

		public GameObject Sonar
		{
			get
			{
				return this._sonar;
			}
		}

		public Transform ThrustPivot
		{
			get
			{
				return this._thrustPivot;
			}
		}

		public BoatShaderParametersController.BoatMaskType MaskType
		{
			get
			{
				return this._maskType;
			}
		}

		[SerializeField]
		private Renderer[] _renderers;

		[SerializeField]
		private Transform _playerPivot;

		[SerializeField]
		private Transform _anglerPivot;

		[SerializeField]
		private PaddleSettings _oar;

		[SerializeField]
		private Transform _oarAnchor;

		[SerializeField]
		private Transform[] _waterDisturbers;

		[SerializeField]
		private byte _ticksBetweenDisturbs = 3;

		[SerializeField]
		private float _disturbanceRadius = 0.5f;

		[SerializeField]
		private float _disturbanceMaxForceAtSpeed = 5f;

		[SerializeField]
		private WaterDisturbForce _disturbanceMaxForce = WaterDisturbForce.Medium;

		[SerializeField]
		private float _groundCorrection;

		[SerializeField]
		private TPMEngineSettings _engine;

		[SerializeField]
		private Animation _trollingEngine;

		[SerializeField]
		private GameObject _sonar;

		[SerializeField]
		private Transform _thrustPivot;

		[SerializeField]
		private BoatShaderParametersController.BoatMaskType _maskType;
	}
}
