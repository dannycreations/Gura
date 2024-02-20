using System;
using System.Collections.Generic;
using System.Linq;
using TPM;
using UnityEngine;

namespace Boats
{
	public class TPMBoat
	{
		public TPMBoat(Transform handWithOar = null)
		{
			this._handWithOar = handWithOar;
		}

		public Transform BoatModel
		{
			get
			{
				return (!(this._boat != null)) ? null : this._boat.transform;
			}
		}

		public Vector3 PlayerGroundCorrection
		{
			get
			{
				return Vector3.zero;
			}
		}

		public Transform PlayerPivot
		{
			get
			{
				return this._settings.PlayerPivot;
			}
		}

		public Transform AnglerPivot
		{
			get
			{
				return this._settings.AnglerPivot;
			}
		}

		public Vector3 GroundCorrection
		{
			get
			{
				return new Vector3(0f, this._settings.GroundCorrection, 0f);
			}
		}

		public BoatShaderParametersController.BoatMaskType MaskType
		{
			get
			{
				return this._settings.MaskType;
			}
		}

		public void SetVisibility(bool flag)
		{
			this._isVisible = flag;
			if (this._boat != null)
			{
				for (int i = 0; i < this._settings.Renderers.Length; i++)
				{
					this._settings.Renderers[i].enabled = flag;
				}
				if (this._settings.Oar != null)
				{
					this._settings.Oar.gameObject.SetActive(flag);
				}
				if (this._settings.Sonar != null)
				{
					this._settings.Sonar.SetActive(flag);
				}
				for (int j = 0; j < this._additionalRenderers.Count; j++)
				{
					this._additionalRenderers[j].enabled = flag;
				}
			}
		}

		public void ServerUpdate(ThirdPersonData data, float dtPrc, Vector3 velocity)
		{
			if (data.Boat == null)
			{
				if (this._boat != null)
				{
					this._factoryID = null;
					this.DestroyModel();
					this._wasInitialized = false;
				}
			}
			else
			{
				if (this._factoryID != null)
				{
					ushort? factoryID = this._factoryID;
					if (((factoryID == null) ? null : new int?((int)factoryID.Value)) != (int)data.Boat.FactoryID)
					{
						this.DestroyModel();
					}
				}
				this._factoryID = new ushort?(data.Boat.FactoryID);
				this._isOarInHands = !data.BoolParameters[8];
				if (this._boat == null)
				{
					this._settings = GameFactory.BoatDock.GetTPMBoat(this._factoryID.Value);
					this._boat = this._settings.gameObject;
					this._boat.GetComponent<Collider>().enabled = data.IsPhotoMode;
					if (data.IsPhotoMode)
					{
						this._boat.layer = GlobalConsts.PhotoModeLayer;
					}
					if (this._settings.Oar != null)
					{
						this._oarWaterDisturber = new OarWaterDisturber(this._settings.Oar);
					}
					if (this._settings.Engine != null)
					{
						this._engine = new TPMEngineController(this._settings.Engine);
					}
					if (this._settings.TrollingEngine != null)
					{
						this._trollingEngine = new TPMTrollingEngine(this._settings.TrollingEngine);
					}
					this._additionalRenderers = RenderersHelper.GetAllRenderersForObject<Renderer>(this._settings.transform);
					this._additionalRenderers.RemoveAll((Renderer r) => this._settings.Renderers.Contains(r));
					this.SetVisibility(this._isVisible);
					this.initWaterSplashesPool(60);
				}
				if (this._engine != null)
				{
					this._engine.ServerUpdate(data);
				}
				if (this._trollingEngine != null)
				{
					this._trollingEngine.ServerUpdate(data);
				}
				this.UpdateOarParent();
				if (this._wasInitialized)
				{
					this._prevPosition = Vector3.Lerp(this._prevPosition, this._targetPosition, dtPrc);
					this._prevRotation = Quaternion.Slerp(this._prevRotation, this._targetRotation, dtPrc);
				}
				else
				{
					this._wasInitialized = true;
					this._prevPosition = data.Boat.Position;
					this._prevRotation = data.Boat.Rotation;
				}
				this._targetPosition = data.Boat.Position;
				this._targetRotation = data.Boat.Rotation;
				this._boatVelocity = velocity;
			}
		}

		public void SyncUpdate(float dtPrc)
		{
			if (this._boat != null)
			{
				if (this._engine != null)
				{
					this._engine.SyncUpdate(dtPrc);
				}
				if (this._isVisible)
				{
					this._boat.transform.rotation = Quaternion.Slerp(this._prevRotation, this._targetRotation, dtPrc);
					this._boat.transform.position = Vector3.Lerp(this._prevPosition, this._targetPosition, dtPrc);
					if (this._oarWaterDisturber != null)
					{
						this._oarWaterDisturber.Update();
					}
					float magnitude = this._boatVelocity.magnitude;
					if (magnitude > 0f)
					{
						if (this._disturbsCounter == 0)
						{
							this._disturbsCounter = this._settings.TicksBetweenDisturbs;
							for (int i = 0; i < this._settings.WaterDisturbers.Length; i++)
							{
								float num = Mathf.Clamp01(magnitude / this._settings.DisturbanceMaxForceAtSpeed);
								GameFactory.Water.AddWaterDisturb(this._settings.WaterDisturbers[i].position, Mathf.Lerp(0f, this._settings.DisturbanceRadius, num * 2f), Mathf.Lerp(0f, (float)((byte)this._settings.DisturbanceMaxForce), num));
							}
						}
						else
						{
							this._disturbsCounter -= 1;
						}
					}
					this.updateWaterDisturbance();
				}
				else
				{
					this.updateWaterSplashes();
				}
			}
		}

		public void OnDestroy()
		{
			if (this._boat != null)
			{
				this.DestroyModel();
			}
		}

		private void UpdateOarParent()
		{
			if (this._settings.Oar != null)
			{
				if (this._isOarInHands)
				{
					this._settings.Oar.transform.parent = this._handWithOar;
					this._settings.Oar.transform.localRotation = Quaternion.identity;
					this._settings.Oar.transform.localPosition = Vector3.zero;
				}
				else
				{
					this._settings.Oar.transform.parent = this._boat.transform;
					this._settings.Oar.transform.localRotation = this._settings.OarAnchor.localRotation;
					this._settings.Oar.transform.localPosition = this._settings.OarAnchor.localPosition;
				}
			}
		}

		private void DestroyModel()
		{
			if (this._settings.Oar != null)
			{
				this._oarWaterDisturber.OnDestroy();
				this._oarWaterDisturber = null;
				Object.Destroy(this._settings.Oar.gameObject);
				this._settings = null;
			}
			if (this._engine != null)
			{
				this._engine.OnDestroy();
				this._engine = null;
			}
			Object.Destroy(this._boat);
			this._boat = null;
			this._additionalRenderers = null;
			this.destroyWaterSplashPool();
		}

		private void initWaterSplashesPool(int count)
		{
			if (this._settings.ThrustPivot != null)
			{
				this.engineSplashes = new List<TPMBoat.MovingEngineSplash>();
				this._engineSplashEmitter = new TPMBoat.WaterSplashEmitter();
				this._engineSplashEmitter.transform = this._settings.ThrustPivot;
				this._engineSplashEmitter.size = new Vector2(1.5f, 3.5f);
				this._engineSplashEmitter.rate = new Vector2(10f, 30f);
				this._engineSplashEmitter.spread = new Vector2(0.2f, 0.6f);
				this._engineSplashEmitter.initialVelocityFactor = 0.6f;
				this._engineSplashEmitter.normalAttenuation = 0f;
				if ("2D/Splashes/pMotorSplash".Length > 0)
				{
					this.waterSplashPool = new GameObject("EngineWaterSplashPool");
					for (int i = 0; i < count; i++)
					{
						TPMBoat.MovingEngineSplash movingEngineSplash = new TPMBoat.MovingEngineSplash("2D/Splashes/pMotorSplash", 2f);
						movingEngineSplash.gameobject.transform.parent = this.waterSplashPool.transform;
						this.engineSplashes.Add(movingEngineSplash);
					}
				}
			}
		}

		private void destroyWaterSplashPool()
		{
			this.engineSplashes = null;
			if (this.waterSplashPool != null)
			{
				Object.Destroy(this.waterSplashPool);
			}
			this.waterSplashPool = null;
			this._engineSplashEmitter = null;
		}

		private TPMBoat.MovingEngineSplash getWaterSplash()
		{
			return this.engineSplashes.Find((TPMBoat.MovingEngineSplash x) => x.deactivationTimestamp <= Time.time);
		}

		private void updateWaterSplashes()
		{
			if (this.engineSplashes != null)
			{
				for (int i = 0; i < this.engineSplashes.Count; i++)
				{
					this.engineSplashes[i].Update();
				}
			}
		}

		public void updateWaterDisturbance()
		{
			float magnitude = this._boatVelocity.magnitude;
			TPMBoat.WaterSplashEmitter engineSplashEmitter = this._engineSplashEmitter;
			if (engineSplashEmitter != null)
			{
				Vector3 vector = engineSplashEmitter.transform.position;
				vector.y = 0f;
				float num = Mathf.Clamp01(magnitude / 10f);
				num = Mathf.Lerp(num, num * Mathf.Max(0f, Vector3.Dot(engineSplashEmitter.transform.forward, this._boatVelocity.normalized)), engineSplashEmitter.normalAttenuation);
				float num2 = 1f / Mathf.Lerp(engineSplashEmitter.rate[0], engineSplashEmitter.rate[1], num);
				if ("2D/Splashes/pMotorSplash".Length != 0 && engineSplashEmitter.disturbanceTimestamp + num2 < Time.time && num > 0.02f)
				{
					float num3 = Mathf.Lerp(engineSplashEmitter.size[0], engineSplashEmitter.size[1], num);
					float num4 = Mathf.Lerp(engineSplashEmitter.spread[0], engineSplashEmitter.spread[1], num);
					engineSplashEmitter.disturbanceTimestamp = Time.time;
					vector += Random.insideUnitSphere * num4;
					vector.y = 0f;
					TPMBoat.MovingEngineSplash waterSplash = this.getWaterSplash();
					if (waterSplash != null)
					{
						waterSplash.Activate(vector, this._boatVelocity * engineSplashEmitter.initialVelocityFactor, num3);
					}
				}
				this.updateWaterSplashes();
			}
		}

		private TPMBoatSettings _settings;

		private TPMEngineController _engine;

		private OarWaterDisturber _oarWaterDisturber;

		private Vector3 _prevPosition;

		private Vector3 _targetPosition;

		private Quaternion _prevRotation;

		private Quaternion _targetRotation;

		private ushort? _factoryID;

		private GameObject _boat;

		private bool _wasInitialized;

		private bool _isVisible;

		private bool _isOarInHands;

		private readonly Transform _handWithOar;

		private byte _disturbsCounter;

		private Vector3 _boatVelocity;

		private List<Renderer> _additionalRenderers;

		private TPMTrollingEngine _trollingEngine;

		private const int WaterSplashesPoolSize = 60;

		private TPMBoat.WaterSplashEmitter _engineSplashEmitter;

		private const string WaterSplashPrefab = "2D/Splashes/pMotorSplash";

		private const float WaterSplashDuration = 2f;

		private List<TPMBoat.MovingEngineSplash> engineSplashes;

		private GameObject waterSplashPool;

		[Serializable]
		public class WaterSplashEmitter
		{
			public Transform transform;

			public Vector2 size;

			public Vector2 spread;

			public Vector2 rate;

			public float initialVelocityFactor;

			public float normalAttenuation;

			[HideInInspector]
			public float disturbanceTimestamp;
		}

		private class MovingEngineSplash
		{
			public MovingEngineSplash(string prefabName, float duration)
			{
				this.gameobject = DynWaterParticlesController.CreateSplash(GameFactory.Player.CameraController.transform, Vector3.down * 1000f, prefabName, 1f, 1f, false, false, 0);
				this.duration = duration;
				this.gameobject.SetActive(false);
				this.particleSys = this.gameobject.GetComponentsInChildren<ParticleSystem>();
			}

			public GameObject gameobject { get; private set; }

			public float deactivationTimestamp { get; private set; }

			public float duration { get; private set; }

			public void Activate(Vector3 position, Vector3 velocity, float size)
			{
				this.gameobject.SetActive(true);
				this.gameobject.transform.position = position;
				this.deactivationTimestamp = Time.time + this.duration;
				foreach (ParticleSystem particleSystem in this.particleSys)
				{
					particleSystem.time = 0f;
					particleSystem.startSize = size;
					particleSystem.Clear();
					particleSystem.Play();
				}
				this.velocity = velocity;
			}

			public void Update()
			{
				if (Time.time > this.deactivationTimestamp)
				{
					this.gameobject.SetActive(false);
				}
				else
				{
					this.gameobject.transform.position += this.velocity * Time.deltaTime;
				}
			}

			public Vector3 velocity;

			private ParticleSystem[] particleSys;
		}
	}
}
