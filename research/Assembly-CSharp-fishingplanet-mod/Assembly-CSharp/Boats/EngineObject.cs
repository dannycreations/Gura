using System;
using System.Collections.Generic;
using ObjectModel;
using UnityEngine;

namespace Boats
{
	public class EngineObject
	{
		public EngineObject(EngineSettings engine, BoatSettings boat, MotorBoat server, IBoatWithEngine controller)
		{
			this._boatController = controller;
			this._engineSettings = engine;
			this._boatSettings = boat;
			this.motorBoatSettings = server;
			this.ThrustVelocity = 0f;
			this.thrustAngle = 0f;
			this.ThrustArea = engine.PropellerDragFactor;
			this.nonglidingFactor = 1f;
			if (this._boatController.GlidingOffSpeed > 0f)
			{
				this.nonglidingFactor = 0.20000005f / this._boatController.GlidingOffSpeed / this._boatController.GlidingOffSpeed;
			}
			this.glidingFactor = 1f;
			if (this._boatController.GlidingOnSpeed > 0f)
			{
				this.glidingFactor = this._boatController.GlidingAcceleration / this._boatController.GlidingOnSpeed;
			}
			this.FuelCapacity = this._engineSettings.FuelCapacity;
			if (this.FuelCapacity <= 0f)
			{
				this.FuelCapacity = this.motorBoatSettings.FuelCapacity;
			}
			this.fuelConsumption = 0f;
			this.maxRevolutions = ((this.motorBoatSettings.MaxRevolutions >= 0f) ? this.motorBoatSettings.MaxRevolutions : 1f);
			this.minRevolutions = ((this.motorBoatSettings.MinRevolutions <= this.maxRevolutions) ? this.motorBoatSettings.MinRevolutions : (this.maxRevolutions * 0.1f));
			this.idleRevolutions = ((this.motorBoatSettings.IdleRevolutions <= this.minRevolutions) ? this.motorBoatSettings.IdleRevolutions : this.minRevolutions);
			this.accelerationRevolutions = ((this.motorBoatSettings.AccelerationRevolutions >= 0.1f) ? this.motorBoatSettings.AccelerationRevolutions : 0.1f);
			if (this._engineSettings.HighSpeed <= 0f)
			{
				this.highSpeed = this._engineSettings.MaxThrustVelocity * 0.5f;
			}
			else
			{
				this.highSpeed = this._engineSettings.HighSpeed / 3.6f;
			}
			this.dSpeed = this.highSpeed - 0.5f;
			if (Mathf.Approximately(this.motorBoatSettings.ReverseFactor, 0f))
			{
				this.reverseFactor = 1f;
			}
			else
			{
				this.reverseFactor = Mathf.Clamp(this.motorBoatSettings.ReverseFactor, 0f, 1f);
			}
			this.ranges = new float[5];
			this.ranges[0] = this.minRevolutions;
			this.ranges[1] = this.motorBoatSettings.ThrustVelocity1;
			this.ranges[2] = this.motorBoatSettings.ThrustVelocity2;
			this.ranges[3] = this.motorBoatSettings.ThrustVelocity3;
			this.ranges[4] = this.maxRevolutions;
			this.deltaRanges = (this.maxRevolutions - this.minRevolutions) / 4f;
			for (int i = 1; i < 4; i++)
			{
				if (this.ranges[i] <= 0f)
				{
					this.ranges[i] = this.minRevolutions + this.deltaRanges * (float)i;
				}
			}
			if (this._engineSettings.TurnAngle <= 0f)
			{
				this.turnAngle = 30f;
			}
			else if (this._engineSettings.TurnAngle > 180f)
			{
				this.turnAngle = 90f;
			}
			else
			{
				this.turnAngle = this._engineSettings.TurnAngle * 0.5f;
			}
			if (this._engineSettings.TurnFactor > 0f)
			{
				this.turnSpeed *= this._engineSettings.TurnFactor;
			}
			this.timeCollision = 0f;
			this.initWaterSplashesPool(60);
			EngineObject.WaterSplashEmitter waterSplashEmitter = new EngineObject.WaterSplashEmitter();
			waterSplashEmitter.transform = this._engineSettings.ThrustPivot;
			waterSplashEmitter.size = new Vector2(this._engineSettings.WaterSplashSizeMin, this._engineSettings.WaterSplashSizeMax);
			waterSplashEmitter.rate = new Vector2(this._engineSettings.WaterSplashRateMin, this._engineSettings.WaterSplashRateMax);
			waterSplashEmitter.spread = new Vector2(this._engineSettings.WaterSplashPositionSpreadMin, this._engineSettings.WaterSplashPositionSpreadMax);
			waterSplashEmitter.initialVelocityFactor = 0.6f;
			waterSplashEmitter.normalAttenuation = 0f;
			waterSplashEmitter.useRPM = true;
			if (this._boatSettings.SplashEmitters == null)
			{
				this._boatSettings.SplashEmitters = new List<EngineObject.WaterSplashEmitter>();
			}
			this._boatSettings.SplashEmitters.Add(waterSplashEmitter);
		}

		public Transform transfrom
		{
			get
			{
				return this._engineSettings.transform;
			}
		}

		public float reverseFactor { get; private set; }

		public float highSpeed { get; private set; }

		public float FuelCapacity { get; protected set; }

		public float Fuel
		{
			get
			{
				return this.fuel;
			}
			set
			{
				this.fuel = Mathf.Clamp(value, 0f, this.FuelCapacity);
				this.isFuel = this.fuel > 0f;
			}
		}

		public float Revolutions
		{
			get
			{
				if (!this.isOn)
				{
					return 0f;
				}
				if (this.isIdle)
				{
					return this.idleRevolutions;
				}
				return this.revolutions;
			}
		}

		public float IdleRevolutions
		{
			get
			{
				return this.idleRevolutions;
			}
		}

		public float MaxRevolutions
		{
			get
			{
				return this.maxRevolutions;
			}
		}

		public float ThrustVelocity { get; private set; }

		public float VelocityFactor { get; private set; }

		public float ThrustArea { get; private set; }

		public float ThrustAngle
		{
			get
			{
				return (this.thrustAngle >= 0f) ? this.thrustAngle : (this.thrustAngle + 360f);
			}
		}

		public bool IsOn
		{
			get
			{
				return this.isOn;
			}
		}

		public bool IsIdle
		{
			get
			{
				return this.isOn && this.isIdle;
			}
		}

		public bool IsFuel
		{
			get
			{
				return this.isFuel;
			}
		}

		public void RefreshBoatSettings()
		{
			this.ThrustArea = this._engineSettings.PropellerDragFactor;
			this.nonglidingFactor = 1f;
			if (this._boatController.GlidingOffSpeed > 0f)
			{
				this.nonglidingFactor = 0.20000005f / this._boatController.GlidingOffSpeed / this._boatController.GlidingOffSpeed;
			}
			this.glidingFactor = 1f;
			if (this._boatController.GlidingOnSpeed > 0f)
			{
				this.glidingFactor = this._boatController.GlidingAcceleration / this._boatController.GlidingOnSpeed;
			}
			if (this._engineSettings.HighSpeed <= 0f)
			{
				this.highSpeed = this._engineSettings.MaxThrustVelocity * 0.5f;
			}
			else
			{
				this.highSpeed = this._engineSettings.HighSpeed / 3.6f;
			}
			if (Mathf.Approximately(this.motorBoatSettings.ReverseFactor, 0f))
			{
				this.reverseFactor = 1f;
			}
			else
			{
				this.reverseFactor = Mathf.Clamp(this.motorBoatSettings.ReverseFactor, 0f, 1f);
			}
			if (this._engineSettings.TurnAngle <= 0f)
			{
				this.turnAngle = 30f;
			}
			else if (this._engineSettings.TurnAngle > 180f)
			{
				this.turnAngle = 90f;
			}
			else
			{
				this.turnAngle = this._engineSettings.TurnAngle * 0.5f;
			}
			if (this._engineSettings.TurnFactor > 0f)
			{
				this.turnSpeed = 30f * this._engineSettings.TurnFactor;
			}
		}

		public void SetFullFuel()
		{
			this.Fuel = this.FuelCapacity;
		}

		public void TurnOn()
		{
			if (this.isFuel)
			{
				this.isOn = true;
				this.revolutions = this.idleRevolutions;
			}
		}

		public void TurnOff()
		{
			this.isOn = false;
			this.revolutions = 0f;
		}

		public void Start()
		{
		}

		public void Update(float gas, float boatVelocity, float direction)
		{
			float longitudinalSpeed = this._boatController.Phyboat.LongitudinalSpeed;
			bool flag = gas < 0f;
			if (flag)
			{
				gas = -gas;
			}
			if (Mathf.Approximately(direction, 0f))
			{
				if (this.thrustAngle > 0f)
				{
					this.thrustAngle -= this.turnSpeed * Time.deltaTime;
					if (this.thrustAngle < 0f)
					{
						this.thrustAngle = 0f;
					}
				}
				else if (this.thrustAngle < 0f)
				{
					this.thrustAngle += this.turnSpeed * Time.deltaTime;
					if (this.thrustAngle > 0f)
					{
						this.thrustAngle = 0f;
					}
				}
			}
			else
			{
				if (this._boatController.Phyboat.IsCollision)
				{
					this.timeCollision = 1f;
				}
				else if (this.timeCollision > 0f)
				{
					this.timeCollision -= Time.deltaTime;
				}
				float num;
				if (this.timeCollision > 0f)
				{
					num = 10f;
				}
				else if (flag)
				{
					num = this.turnAngle;
				}
				else if (longitudinalSpeed < 0f)
				{
					num = 0f;
				}
				else if (longitudinalSpeed < 0.5f)
				{
					num = this.turnAngle;
				}
				else
				{
					num = this.turnAngle * this.dSpeed / (longitudinalSpeed + this.dSpeed - 0.5f);
					float num2 = (longitudinalSpeed - Mathf.Abs(this._boatController.Phyboat.LateralSpeed)) / longitudinalSpeed;
					if (num2 < 0.5f)
					{
						if (num2 <= 0f)
						{
							num *= 0.5f;
						}
						else
						{
							num *= 0.5f + num2;
						}
					}
				}
				if (direction < 0f)
				{
					this.thrustAngle -= this.turnSpeed * Time.deltaTime;
					if (this.thrustAngle < -num)
					{
						this.thrustAngle = -num;
					}
				}
				else
				{
					this.thrustAngle += this.turnSpeed * Time.deltaTime;
					if (this.thrustAngle > num)
					{
						this.thrustAngle = num;
					}
				}
			}
			float num3 = 0f;
			float num4 = this._engineSettings.PropellerDragFactor;
			if (this.isOn)
			{
				this.isIdle = this.revolutions < this.minRevolutions && gas < this.minRevolutions;
				if (this.fuelConsumption > 0f)
				{
					this.fuel -= ((!this.isIdle) ? gas : this.minRevolutions) * this.fuelConsumption * Time.deltaTime;
					if (this.fuel < 0f)
					{
						this.fuel = 0f;
						this.isFuel = false;
						this.TurnOff();
					}
				}
				if (this.isOn)
				{
					if (this.isIdle)
					{
						this.revolutions = this.idleRevolutions;
					}
					else
					{
						float num5 = gas;
						if (boatVelocity > 5f)
						{
							float num6 = 0.02f * (boatVelocity - 5f);
							if (num6 > 0.1f)
							{
								num6 = 0.1f;
							}
							num5 += num6;
						}
						if (num5 > this.maxRevolutions)
						{
							num5 = this.maxRevolutions;
						}
						float num7 = this.accelerationRevolutions * Time.deltaTime;
						if (num5 > this.revolutions)
						{
							this.revolutions += num7;
							if (this.revolutions > num5)
							{
								this.revolutions = num5;
							}
						}
						else
						{
							this.revolutions -= num7;
							if (this.revolutions < num5)
							{
								this.revolutions = num5;
							}
						}
						if (this.revolutions >= this.minRevolutions)
						{
							this.VelocityFactor = 1f;
							if (this.revolutions < this.maxRevolutions)
							{
								int num8 = (int)((this.revolutions - this.minRevolutions) / this.deltaRanges);
								this.VelocityFactor = this.ranges[num8] + (this.revolutions / this.deltaRanges - (float)num8) * (this.ranges[num8 + 1] - this.ranges[num8]);
							}
							float num9 = 1f + this.VelocityFactor * this.VelocityFactor * 0.20000005f;
							if (this._boatController.GlidingBoat)
							{
								if (this._boatController.Phyboat.IsGliding)
								{
									num9 = 1.2f;
									if (longitudinalSpeed > this._boatController.GlidingOnSpeed)
									{
										num9 -= (longitudinalSpeed - this._boatController.GlidingOnSpeed) * this.glidingFactor;
									}
									if (num9 < 0.8f)
									{
										num9 = 0.8f;
									}
								}
								else
								{
									float num10 = 1.2f;
									if (longitudinalSpeed < this._boatController.GlidingOffSpeed)
									{
										num10 = 1f + longitudinalSpeed * longitudinalSpeed * this.nonglidingFactor;
									}
									if (num10 > num9)
									{
										num9 = num10;
									}
								}
							}
							this.VelocityFactor /= num9;
							this.VelocityFactor = Mathf.Clamp(this.VelocityFactor, 0f, 2f);
							num3 = this._engineSettings.MaxThrustVelocity * this.VelocityFactor;
							if (flag)
							{
								num3 = -num3 * this.reverseFactor;
								num4 = this._engineSettings.PropellerDragFactor / this.reverseFactor;
							}
						}
					}
				}
			}
			this.ThrustVelocity = num3;
			this.ThrustArea = num4;
		}

		public void Stop()
		{
			this.revolutions = this.minRevolutions;
			this.ThrustVelocity = 0f;
		}

		public void Destroy()
		{
			this.destroyWaterSplashPool();
		}

		private void initWaterSplashesPool(int count)
		{
			this.waterSplashPool = new GameObject("EngineWaterSplashPool");
			for (int i = 0; i < count; i++)
			{
				EngineObject.MovingEngineSplash movingEngineSplash = new EngineObject.MovingEngineSplash(this._engineSettings.WaterSplashPrefab, this._engineSettings.WaterSplashDuration);
				movingEngineSplash.gameobject.transform.parent = this.waterSplashPool.transform;
				this.engineSplashes.Add(movingEngineSplash);
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
		}

		private EngineObject.MovingEngineSplash getWaterSplash()
		{
			return this.engineSplashes.Find((EngineObject.MovingEngineSplash x) => x.deactivationTimestamp <= Time.time);
		}

		private void updateWaterSplashes()
		{
			for (int i = 0; i < this.engineSplashes.Count; i++)
			{
				this.engineSplashes[i].Update();
			}
		}

		public void updateWaterDisturbance()
		{
			for (int i = 0; i < this._boatSettings.SplashEmitters.Count; i++)
			{
				EngineObject.WaterSplashEmitter waterSplashEmitter = this._boatSettings.SplashEmitters[i];
				Vector3 vector = waterSplashEmitter.transform.position;
				vector.y = 0f;
				float num;
				if (waterSplashEmitter.useRPM)
				{
					num = Mathf.Max(0f, (this.revolutions - this.minRevolutions) / (this.maxRevolutions - this.minRevolutions));
				}
				else
				{
					num = this._boatController.BoatVelocity / this._engineSettings.MaxThrustVelocity;
				}
				num = Mathf.Lerp(num, num * Mathf.Max(0f, Vector3.Dot(waterSplashEmitter.transform.forward, this._boatController.Phyboat.Velocity.normalized)), waterSplashEmitter.normalAttenuation);
				float num2 = 1f / Mathf.Lerp(waterSplashEmitter.rate[0], waterSplashEmitter.rate[1], num);
				if (this._engineSettings.WaterSplashPrefab.Length != 0 && waterSplashEmitter.disturbanceTimestamp + num2 < Time.time && num > 0.02f)
				{
					float num3 = Mathf.Lerp(waterSplashEmitter.size[0], waterSplashEmitter.size[1], num);
					float num4 = Mathf.Lerp(waterSplashEmitter.spread[0], waterSplashEmitter.spread[1], num);
					waterSplashEmitter.disturbanceTimestamp = Time.time;
					vector += Random.insideUnitSphere * num4;
					vector.y = 0f;
					EngineObject.MovingEngineSplash waterSplash = this.getWaterSplash();
					if (waterSplash != null)
					{
						waterSplash.Activate(vector, this._boatController.Phyboat.Velocity * waterSplashEmitter.initialVelocityFactor, num3);
					}
				}
			}
			this.updateWaterSplashes();
			Vector3 position = this._engineSettings.ThrustPivot.position;
			position.y = 0f;
			GameFactory.Water.AddWaterDisturb(position, Mathf.Min(this.ThrustVelocity, this._engineSettings.WaterDisturbanceRadius), this.ThrustVelocity);
		}

		private IBoatWithEngine _boatController;

		private EngineSettings _engineSettings;

		private BoatSettings _boatSettings;

		private MotorBoat motorBoatSettings;

		private bool isOn;

		private bool isIdle;

		private float fuel;

		private float fuelConsumption;

		private float revolutions;

		private float idleRevolutions = 0.09f;

		private float minRevolutions = 0.1f;

		private float maxRevolutions = 1f;

		private const int countRanges = 4;

		private float[] ranges;

		private float deltaRanges;

		private float accelerationRevolutions = 1f;

		private float turnSpeed = 30f;

		private float turnAngle;

		private const float highVelocity = 5f;

		private const float factorHighVelocity = 0.02f;

		private const float glidingResistance = 1.2f;

		private const float slowResistance = 1f;

		private const float deltaResistance = 0.20000005f;

		private const float fastResistance = 0.8f;

		private const float maxVelocityFactor = 2f;

		private const float smallSpeed = 0.5f;

		private float nonglidingFactor;

		private float glidingFactor;

		private bool isFuel;

		private float thrustAngle;

		private float timeCollision;

		private float dSpeed;

		private const int WaterSplashesPoolSize = 60;

		private List<EngineObject.MovingEngineSplash> engineSplashes = new List<EngineObject.MovingEngineSplash>();

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

			public bool useRPM;

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
