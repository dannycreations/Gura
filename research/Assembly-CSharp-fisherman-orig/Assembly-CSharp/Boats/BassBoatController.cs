using System;
using System.Collections.Generic;
using Cayman;
using FSMAnimator;
using ObjectModel;
using Phy;
using UnityEngine;

namespace Boats
{
	public class BassBoatController : BoatController<BassBoatController.BBStates, BassBoatController.BBSignals, BassBoatMouseController, BassBoat>, IBoatWithEngine
	{
		protected BassBoatController(Transform spawn, BoatSettings settings, BassBoat serverSettings, bool isOwner, ushort factoryID)
			: base(spawn, settings, serverSettings, isOwner, factoryID)
		{
			this._boatAnimation = settings.gameObject.GetComponent<Animation>();
			this._engine = this._settings.engines[0].Settings;
			this._engine.transform.position = this._settings.engines[0].MountRoot.position;
			if (this._settings.engines.Length > 1)
			{
				this._trollingEngineAnimations = this._settings.engines[1].Settings.transform.GetComponent<Animation>();
				if (this._trollingEngineAnimations != null)
				{
					this._trollingEngineAnimations.Play("SetOff");
				}
				this._trollingEngineSound = this._settings.engines[1].Settings.transform.GetComponent<AudioSource>();
			}
			this.ParseServerSettings();
			this.InitializeSimulation();
			this.engineObject = new EngineObject(this._engine, this._settings, this._serverSettings, this);
			this.engineObject.SetFullFuel();
			this._engineSound = this._engine.gameObject.GetComponent<RealisticEngineSound>();
			if (this._engine.SoundPosition != null)
			{
				this._engineSound = this._engine.SoundPosition.GetComponent<RealisticEngineSound>();
			}
			if (this._engineSound != null)
			{
				this._engineSoundVolume = this._engineSound.masterVolume;
				this._engineSound.masterVolume = 0f;
			}
		}

		public static BassBoatController Create(Transform spawn, BoatSettings settings, BassBoat serverSettings, bool isOwner, ushort factoryID)
		{
			BassBoatController bassBoatController = new BassBoatController(spawn, settings, serverSettings, isOwner, factoryID);
			bassBoatController.InitFSM();
			bassBoatController.InitPhysics();
			return bassBoatController;
		}

		protected override bool IsLookOnEngine
		{
			get
			{
				Vector3 localEulerAngles = GameFactory.Player.CameraController.Camera.transform.localEulerAngles;
				return Mathf.Abs(Math3d.ClampAngleTo180(localEulerAngles.x)) < 30f && Mathf.Abs(Math3d.ClampAngleTo180(localEulerAngles.y)) < 30f;
			}
		}

		protected override bool IsCanLeaveBoat
		{
			get
			{
				return (((this._playerFsm.CurStateID == BassBoatController.BBStates.EngineIdle || this._playerFsm.CurStateID == BassBoatController.BBStates.EngineOff) && !this.IsLookOnEngine) || (this._playerFsm.CurStateID == BassBoatController.BBStates.Fishing && this._driver.CanLeaveBoat)) && base.IsCanLeaveBoat;
			}
		}

		public override bool CantOpenInventory
		{
			get
			{
				return base.CantOpenInventory || (this._playerFsm.CurStateID != BassBoatController.BBStates.Fishing && this._playerFsm.CurStateID != BassBoatController.BBStates.EngineIdle && this._playerFsm.CurStateID != BassBoatController.BBStates.EngineOff);
			}
		}

		protected override bool IsAffectFOV
		{
			get
			{
				return this._playerFsm.CurStateID == BassBoatController.BBStates.EngineIdle || this._playerFsm.CurStateID == BassBoatController.BBStates.InGameMap;
			}
		}

		protected override BassBoatController.BBSignals BoardingSignal
		{
			get
			{
				return BassBoatController.BBSignals.Boarded;
			}
		}

		protected override BassBoatController.BBSignals UnboardingSignal
		{
			get
			{
				return BassBoatController.BBSignals.Unboarding;
			}
		}

		protected override BassBoatController.BBSignals HiddenUnboardingSignal
		{
			get
			{
				return BassBoatController.BBSignals.HiddenUnboarding;
			}
		}

		protected override BassBoatController.BBSignals CloseMapSignal
		{
			get
			{
				return (!this._wasEngineStarted) ? BassBoatController.BBSignals.CloseMapNoEngine : BassBoatController.BBSignals.CloseMapWithEngine;
			}
		}

		protected override BassBoatController.BBSignals RollSignal
		{
			get
			{
				return BassBoatController.BBSignals.Roll;
			}
		}

		protected override BassBoatController.BBSignals RollFisherSignal
		{
			get
			{
				return BassBoatController.BBSignals.RollRecovering;
			}
		}

		protected override bool IsEnterFishingPromtActive
		{
			get
			{
				return this._enterFishingPromptTill > Time.time;
			}
		}

		protected override bool IsBackToNavigationPromptActive
		{
			get
			{
				return this._enterDrivingPromptTill > Time.time;
			}
		}

		public override float Stamina
		{
			get
			{
				return this._rpmFactor;
			}
		}

		public override bool IsEngineForceActive
		{
			get
			{
				return !Mathf.Approximately(this._rpmFactor, 0f) || (this._phyTrollingPropeller != null && !Mathf.Approximately(this._phyTrollingPropeller.ThrustVelocity, 0f));
			}
		}

		public override bool IsTrollingPossible
		{
			get
			{
				return this._rpmFactor <= this._serverSettings.TrollingMaxRevs;
			}
		}

		public bool GlidingBoat { get; private set; }

		public float GlidingOnSpeed { get; private set; }

		public float GlidingOffSpeed { get; private set; }

		public float GlidingAcceleration { get; private set; }

		public override Animation BoatAnimation
		{
			get
			{
				return this._boatAnimation;
			}
		}

		public override bool IsActiveMovement
		{
			get
			{
				return this._wasEngineStarted;
			}
		}

		public override void InitFSM()
		{
			this._playerFsm = new TFSM<BassBoatController.BBStates, BassBoatController.BBSignals>("BassBoat", BassBoatController.BBStates.None);
			this._playerFsm.AddState(BassBoatController.BBStates.None, new StateSwitchingDelegate(this.ReleaseControll), null, null, 0f, BassBoatController.BBSignals.None, null, null);
			this._playerFsm.AddState(BassBoatController.BBStates.EngineOff, delegate
			{
				base.SetServerBoatNavigation(true);
				this._enterFishingPromptFrom = Time.time + 5f;
				ShowHudElements.Instance.SailingHndl.SetEngineIndicatorOn(false);
				this._enterFishingPromptTill = -1f;
				this._driver.PlayHandAnimation("BassIdleEmpty", 1f, 0f, 0f);
			}, delegate
			{
				this._enterFishingPromptFrom = -1f;
				this._enterFishingPromptTill = -1f;
			}, delegate
			{
				if (base.SwitchingRodHandler())
				{
					this._playerFsm.SendSignal(BassBoatController.BBSignals.Standup);
				}
				else if (base.IsOpenMap)
				{
					this._playerFsm.SendSignal(BassBoatController.BBSignals.UseMap);
				}
				else if (!this._wasEngineStarted && this.IsLookOnEngine)
				{
					ShowHudElements.Instance.ShowZodiacEnterIgnitionState();
				}
			}, 0f, BassBoatController.BBSignals.None, null, null);
			this._playerFsm.AddState(BassBoatController.BBStates.EngineStart, delegate
			{
				ShowHudElements.Instance.SailingHndl.SetEngineIndicatorOn(true);
				if (this._engineSound != null)
				{
					this._engineSound.masterVolume = this._engineSoundVolume;
					this._engineSound.PlayStartFail();
				}
				this._phyEngine.StarterEngaged = true;
				this._phyEngine.Clutch = 0f;
				this._phyEngine.ConnectionNeedSyncMark();
				base.PlayTransitAnimation("BassStartEngine", BassBoatController.BBSignals.EngineStarted, 2f, 1);
				this._startEngineSondAt = Time.time + 0.5f;
			}, null, delegate
			{
				if (this._startEngineSondAt > 0f && this._startEngineSondAt < Time.time)
				{
					this._startEngineSondAt = -1f;
					if (this._engineSound != null)
					{
						this._engineSound.masterVolume = this._engineSoundVolume;
					}
				}
			}, 0f, BassBoatController.BBSignals.None, null, null);
			this._playerFsm.AddState(BassBoatController.BBStates.EngineIdle, delegate
			{
				this._wheelHandsAnimator.Start();
				if (!this._wasEngineStarted)
				{
					this._wasEngineStarted = true;
					if (CaymanGenerator.Instance != null)
					{
						CaymanGenerator.Instance.OnScareAction();
					}
					PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.BoatEngineStarted, true, null);
					this.engineObject.Start();
					if (this._engineSound != null)
					{
						this._engineSound.masterVolume = this._engineSoundVolume;
					}
					this._phyEngine.FuelRate = this._engine.EngineModelIdleFuelRate;
					this._phyEngine.StarterEngaged = false;
					this._phyEngine.ConnectionNeedSyncMark();
					this.engineObject.TurnOn();
				}
			}, delegate
			{
				this._enterFishingPromptTill = -1f;
				this._driver.transform.localRotation = Quaternion.identity;
				this._wheelHandsAnimator.Stop();
			}, delegate
			{
				if (base.SwitchingRodHandler())
				{
					this._playerFsm.SendSignal(BassBoatController.BBSignals.Standup);
				}
				else if (base.IsOpenMap)
				{
					this._playerFsm.SendSignal(BassBoatController.BBSignals.UseMap);
				}
			}, 0f, BassBoatController.BBSignals.None, null, null);
			this._playerFsm.AddState(BassBoatController.BBStates.EngineStop, delegate
			{
				base.PlayTransitAnimation("BassStopEngine", BassBoatController.BBSignals.EngineStopped, 1f, 1);
			}, null, null, 0f, BassBoatController.BBSignals.None, null, null);
			this._playerFsm.AddState(BassBoatController.BBStates.Unboarding, delegate
			{
				base.SetServerBoatNavigation(false);
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsSailing, false);
				this.StopEngine();
				this._mouseController.PrepareDriverForUnboarding();
			}, null, null, 0f, BassBoatController.BBSignals.Unboarded, null, null);
			this._playerFsm.AddState(BassBoatController.BBStates.PrepareFishing, delegate
			{
				base.SetServerBoatNavigation(false);
				if (this._trollingEngineAnimations != null)
				{
					this._trollingEngineAnimations.Play("SetOn");
				}
				this._mouseController.EnterFishingMode();
				base.IsFishing = true;
				this.StopEngine();
			}, delegate
			{
				if (this._trollingEngineAnimations != null)
				{
					this._activateTrollingsTurnsAt = Time.time + 2.2f;
				}
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsBoatFishing, true);
				this._driver.PrepareFishingFromBoat();
			}, null, 0f, BassBoatController.BBSignals.ReadyToFish, null, null);
			this._playerFsm.AddState(BassBoatController.BBStates.Fishing, delegate
			{
				this._enterDrivingPromptFrom = Time.time + 5f;
				base.CheckForRequestedRod();
				if (this._settings.engines.Length > 1)
				{
					this._phyTrollingPropeller.Area = this._settings.engines[1].Settings.PropellerDragFactor;
					this._phyTrollingPropeller.ConnectionNeedSyncMark();
				}
				if (this._trollingEngineSound != null)
				{
					this._trollingEngineSound.pitch = 0f;
					this._trollingEngineSound.Play();
				}
			}, delegate
			{
				this._putForTrollingPromptTill = -1f;
				this._takeTrollingPromptTill = -1f;
				this._enterDrivingPromptTill = -1f;
				if (this._settings.engines.Length > 1)
				{
					this._phyTrollingPropeller.ThrustVelocity = 0f;
					this._phyTrollingPropeller.Area = 0f;
					this._phyTrollingPropeller.ConnectionNeedSyncMark();
				}
				if (this._trollingEngineSound != null)
				{
					this._trollingEngineSound.Stop();
				}
			}, null, 0f, BassBoatController.BBSignals.None, null, delegate
			{
				this._putForTrollingPromptTill = -1f;
				this._takeTrollingPromptTill = -1f;
				base.IsFishing = false;
				this._driver.FinishFishingFromBoat();
				this._mouseController.ClearFishingMode();
				if (this._trollingEngineSound != null)
				{
					this._trollingEngineSound.Stop();
				}
			});
			this._playerFsm.AddState(BassBoatController.BBStates.EndFishing, delegate
			{
				if (this._trollingEngineAnimations != null)
				{
					this._trollingEngineTurns.Stop();
					this._trollingEngineAnimations.Play("SetOff");
				}
				this._driver.InitFishingToDriving(delegate
				{
					this._mouseController.LeaveFishingMode(delegate
					{
						this._playerFsm.SendSignal((!this._wasEngineStarted) ? BassBoatController.BBSignals.FishingFinished : BassBoatController.BBSignals.FishingFinishedToIdle);
					});
				});
			}, delegate
			{
				this._enterDrivingPromptTill = -1f;
				base.IsFishing = false;
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsBoatFishing, false);
				this._driver.FinishFishingFromBoat();
			}, null, 0f, BassBoatController.BBSignals.None, null, null);
			this._playerFsm.AddState(BassBoatController.BBStates.UnboardFisher, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsSailing, false);
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsBoatFishing, false);
			}, delegate
			{
				base.IsFishing = false;
				this._mouseController.ClearFishingMode();
			}, null, 0f, BassBoatController.BBSignals.Unboarded, null, null);
			this._playerFsm.AddState(BassBoatController.BBStates.StartRecovery, delegate
			{
				base.SetServerBoatNavigation(false);
				BlackScreenHandler.Show(true, null);
				GameActionAdapter.Instance.TurnOver();
				this._rpmFactor = 0f;
			}, new StateSwitchingDelegate(base.RecoveryBoat), null, 0.5f, BassBoatController.BBSignals.RollRecovering, null, null);
			this._playerFsm.AddState(BassBoatController.BBStates.StartRecoveryEngineOff, delegate
			{
				base.SetServerBoatNavigation(false);
				BlackScreenHandler.Show(true, null);
				GameActionAdapter.Instance.TurnOver();
				this._rpmFactor = 0f;
			}, new StateSwitchingDelegate(base.RecoveryBoat), null, 0.5f, BassBoatController.BBSignals.RollRecovering, null, null);
			this._playerFsm.AddState(BassBoatController.BBStates.StartRecoveryFishing, delegate
			{
				BlackScreenHandler.Show(true, null);
				if (this._acceptRollBackFrom > 0f && this._acceptRollBackFrom < Time.time)
				{
					GameActionAdapter.Instance.TurnOver();
				}
			}, new StateSwitchingDelegate(base.RecoveryBoat), null, 0.5f, BassBoatController.BBSignals.RollRecovering, null, null);
			this._playerFsm.AddState(BassBoatController.BBStates.InGameMap, delegate
			{
				base.SetServerBoatNavigation(false);
				this._mouseController.EnterMapMode();
				this._driver.PrepareOpenMap();
			}, delegate
			{
				this._mouseController.LeaveMapMode();
			}, null, 0f, BassBoatController.BBSignals.None, null, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.None, BassBoatController.BBStates.EngineOff, BassBoatController.BBSignals.Boarded, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EngineOff, BassBoatController.BBStates.EngineStart, BassBoatController.BBSignals.StartEngine, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EngineStart, BassBoatController.BBStates.EngineIdle, BassBoatController.BBSignals.EngineStarted, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EngineIdle, BassBoatController.BBStates.EngineStop, BassBoatController.BBSignals.StopEngine, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EngineStop, BassBoatController.BBStates.EngineOff, BassBoatController.BBSignals.EngineStopped, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EngineOff, BassBoatController.BBStates.Unboarding, BassBoatController.BBSignals.Unboarding, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EngineIdle, BassBoatController.BBStates.Unboarding, BassBoatController.BBSignals.Unboarding, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.Unboarding, BassBoatController.BBStates.None, BassBoatController.BBSignals.Unboarded, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.Fishing, BassBoatController.BBStates.UnboardFisher, BassBoatController.BBSignals.Unboarding, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.UnboardFisher, BassBoatController.BBStates.None, BassBoatController.BBSignals.Unboarded, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EngineOff, BassBoatController.BBStates.PrepareFishing, BassBoatController.BBSignals.Standup, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EngineIdle, BassBoatController.BBStates.PrepareFishing, BassBoatController.BBSignals.Standup, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.PrepareFishing, BassBoatController.BBStates.Fishing, BassBoatController.BBSignals.ReadyToFish, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.Fishing, BassBoatController.BBStates.EndFishing, BassBoatController.BBSignals.Sitdown, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EndFishing, BassBoatController.BBStates.EngineOff, BassBoatController.BBSignals.FishingFinished, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EndFishing, BassBoatController.BBStates.EngineIdle, BassBoatController.BBSignals.FishingFinishedToIdle, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EngineIdle, BassBoatController.BBStates.StartRecovery, BassBoatController.BBSignals.Roll, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.StartRecovery, BassBoatController.BBStates.EngineIdle, BassBoatController.BBSignals.RollRecovering, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.Fishing, BassBoatController.BBStates.StartRecoveryFishing, BassBoatController.BBSignals.Roll, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.StartRecoveryFishing, BassBoatController.BBStates.Fishing, BassBoatController.BBSignals.RollRecovering, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EngineOff, BassBoatController.BBStates.StartRecoveryEngineOff, BassBoatController.BBSignals.Roll, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.StartRecoveryEngineOff, BassBoatController.BBStates.EngineOff, BassBoatController.BBSignals.RollRecovering, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EngineOff, BassBoatController.BBStates.InGameMap, BassBoatController.BBSignals.UseMap, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.InGameMap, BassBoatController.BBStates.EngineOff, BassBoatController.BBSignals.CloseMapNoEngine, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.EngineIdle, BassBoatController.BBStates.InGameMap, BassBoatController.BBSignals.UseMap, null);
			this._playerFsm.AddTransition(BassBoatController.BBStates.InGameMap, BassBoatController.BBStates.EngineIdle, BassBoatController.BBSignals.CloseMapWithEngine, null);
			this._playerFsm.AddAnyStateTransition(BassBoatController.BBStates.None, BassBoatController.BBSignals.HiddenUnboarding, true);
		}

		protected override void ToDrivingPrompt()
		{
			ShowHudElements.Instance.GoNavigationModeHint();
		}

		public override void OnTakeRodFromPod()
		{
			this._rpmFactor = 0f;
			this._phyEngine.Clutch = 0f;
			this._phyEngine.ConnectionNeedSyncMark();
		}

		public override bool HiddenLeave()
		{
			this.StopEngine();
			return base.HiddenLeave();
		}

		protected override void StopBoat()
		{
			base.StopBoat();
			this.StopEngine();
			this._playerFsm.SendSignal(BassBoatController.BBSignals.StopEngine);
		}

		private void StopEngine()
		{
			if (this._wasEngineStarted)
			{
				this._wasEngineStarted = false;
				PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.BoatEngineStarted, false, null);
				this._rpmFactor = 0f;
				this._phyPropeller.ThrustVelocity = 0f;
				this._phyPropeller.ConnectionNeedSyncMark();
				this._wheelHandsAnimator.Stop();
				this.engineObject.Stop();
				this.engineObject.TurnOff();
				this._phyEngine.FuelRate = 0f;
				this._phyEngine.ConnectionNeedSyncMark();
				if (this._engineSound != null)
				{
					this._engineSound.PlayShutdown();
				}
			}
		}

		protected override void ResetSimulation()
		{
			base.ResetSimulation();
			this.InitializeSimulation();
			base.InitPhysics();
		}

		private void InitializeSimulation()
		{
			this._engines.Clear();
			for (int i = 0; i < this._settings.engines.Length; i++)
			{
				PointOnRigidBody pointOnRigidBody = new PointOnRigidBody(this.boatSim, base.Phyboat, this._settings.Barycenter.InverseTransformPoint(this._settings.engines[i].Settings.ThrustPivot.position), Mass.MassType.Unknown)
				{
					MassValue = this._settings.engines[i].Settings.Weight,
					IgnoreEnvForces = true
				};
				this._engines.Add(pointOnRigidBody);
				this.boatSim.Masses.Add(pointOnRigidBody);
			}
			this._phyPropeller = new PointToRigidBodyResistanceTransmission(this._engines[0])
			{
				Area = this._engine.PropellerDragFactor,
				Radius = 0.25f,
				BodyResistance = this._settings.LongitudalResistance,
				WingFactor = 100f,
				SimulatedThrustVelocity = true
			};
			this._phyEngine = new EngineModel(this._engines[0])
			{
				ShaftMass = 1f,
				Radius = 0.5f,
				StarterVelocity = 100f,
				StarterTqFactor = 100f,
				CombustionTqGain = 50f,
				CombustionTqMax = 100f,
				LossesTqLowGain = 60f,
				LossesTqLowVelocity = 10f,
				LossesTqHighFactor = 0.1f,
				InputMultiplier = this._engine.EngineModelInputLoadMultiplier,
				OutputMultiplier = BoatWithEngineController.CalculateOutputMultiplier(this._engine.MinThrustVelocity, this._engine.EngineModelNominalMaxAngularSpeed, 0.5f),
				Transmission = this._phyPropeller
			};
			this.boatSim.Connections.Add(this._phyPropeller);
			this.boatSim.Connections.Add(this._phyEngine);
			if (this._engines.Count > 1)
			{
				this._phyTrollingPropeller = new PointToRigidBodyResistanceTransmission(this._engines[1])
				{
					Area = 0f,
					Radius = 0.1f,
					BodyResistance = this._settings.LongitudalResistance,
					SimulatedThrustVelocity = false
				};
				this.boatSim.Connections.Add(this._phyTrollingPropeller);
			}
			this.refreshBoatSettings();
		}

		protected override void refreshBoatSettings()
		{
			base.refreshBoatSettings();
			this._phyPropeller.Area = this._engine.PropellerDragFactor;
			this._phyPropeller.BodyResistance = this._settings.LongitudalResistance;
			this._phyPropeller.ConnectionNeedSyncMark();
			this._phyEngine.InputMultiplier = this._engine.EngineModelInputLoadMultiplier;
			this._phyEngine.ConnectionNeedSyncMark();
			base.Phyboat.RollFactor = this._engine.RollFactor;
			base.Phyboat.IsRollLimit = !Mathf.Approximately(this._engine.RollAngle, 0f);
			if (base.Phyboat.IsRollLimit)
			{
				base.Phyboat.RollAngle = Mathf.Clamp(this._engine.RollAngle, 0f, 180f) * 0.5f;
			}
			this.GlidingBoat = this._engine.GlidingOnSpeed > 0f;
			base.Phyboat.GlidingBoat = this.GlidingBoat;
			if (this.GlidingBoat)
			{
				this.GlidingOnSpeed = this._engine.GlidingOnSpeed;
				if (this._engine.GlidingOffSpeed <= 0f || this._engine.GlidingOffSpeed > this.GlidingOnSpeed)
				{
					this.GlidingOffSpeed = this.GlidingOnSpeed;
				}
				else
				{
					this.GlidingOffSpeed = this._engine.GlidingOffSpeed;
				}
				this.GlidingOnSpeed /= 3.6f;
				this.GlidingOffSpeed /= 3.6f;
				this.GlidingAcceleration = this._engine.GlidingAcceleration;
				base.Phyboat.GlidingOnSpeed = this.GlidingOnSpeed;
				base.Phyboat.GlidingOffSpeed = this.GlidingOffSpeed;
				base.Phyboat.LiftFactor = this._engine.LiftFactor;
				base.Phyboat.TangageFactor = this._engine.TangageFactor;
			}
			if (this.engineObject != null)
			{
				this.engineObject.RefreshBoatSettings();
			}
		}

		private void ParseServerSettings()
		{
			if (Mathf.Approximately(this._serverSettings.MinThrustVelocity, 0f))
			{
				this._engine.MinThrustVelocity = this._serverSettings.MaxThrustVelocity / 3.6f * (this._engine.EngineModelNominalMinAngularSpeed / this._engine.EngineModelNominalMaxAngularSpeed);
			}
			else
			{
				this._engine.MinThrustVelocity = this._serverSettings.MinThrustVelocity / 3.6f;
			}
			this._engine.MaxThrustVelocity = this._serverSettings.MaxThrustVelocity / 3.6f;
			this._engine.MaxThrustGainTime = this._serverSettings.MaxThrustGainTime;
			this._engine.PropellerDragFactor = this._serverSettings.PropellerDragFactor;
			this._engine.AcceleratedForceMultiplier = this._serverSettings.AcceleratedForceMultiplier;
			this._engine.RollFactor = ((this._serverSettings.RollFactor > 0f) ? this._serverSettings.RollFactor : 0f);
			this._engine.RollAngle = this._serverSettings.RollAngle;
			this._engine.GlidingOnSpeed = this._serverSettings.GlidingOnSpeed;
			this._engine.GlidingOnSpeed = this._serverSettings.GlidingOffSpeed;
			this._engine.GlidingAcceleration = this._serverSettings.GlidingAcceleration;
			this._engine.LiftFactor = this._serverSettings.LiftFactor;
			this._engine.TangageFactor = this._serverSettings.TangageFactor;
			this._engine.TurnAngle = this._serverSettings.TurnAngle;
			this._engine.HighSpeed = this._serverSettings.HighSpeed;
			this._engine.TurnFactor = this._serverSettings.TurnFactor;
		}

		protected override AnimationState PlayAnimation(string name, float animSpeed = 1f, int animWeight = 1)
		{
			return this._driver.PlayHandAnimation(name, animSpeed, 0f, 0f);
		}

		protected override void Board()
		{
			base.Board();
			if (this._wheelHandsAnimator == null)
			{
				this._wheelHandsAnimator = new TwoAxisSequences<TPMFloats>("BassBoatWheelTurnsController", new Animation[]
				{
					this._boatAnimation,
					this._driver.ArmsAnimation
				}, 1, "BassIdle", "BassSteerLeft", "BassSteerLeftIdle", "BassSteerRight", "BassSteerRightIdle", "BassFullPower", "BassFullBackPower", TPMFloats.Horizontal, TPMFloats.Vertical);
			}
			if (this._trollingEngineAnimations != null)
			{
				this._trollingEngineTurns = new AnimAxisSequence("BassBoatTrollingTurnsController", new AnimAxisSequence.PlayAnimationF(this.PlayTrollingAnimation), "Idle", "TurnLeft", "TurnLeftIdle", "TurnRight", "TurnRightIdle");
			}
		}

		private AnimationState PlayTrollingAnimation(string clipName, float animSpeed = 1f, float time = 0f, float blendTime = 0f)
		{
			AnimationState animationState = this._trollingEngineAnimations[clipName];
			if (animationState != null)
			{
				if (animSpeed < 0f && time < 0f)
				{
					time = animationState.length;
				}
				animationState.speed = animSpeed;
				animationState.time = time;
				this._trollingEngineAnimations.CrossFade(clipName, blendTime);
			}
			else
			{
				LogHelper.Error("Can't find animation {0}", new object[] { clipName });
			}
			return animationState;
		}

		protected void SetEngineZeroThrust()
		{
			this._rpmFactor = 0f;
			this._enterFishingPromptFrom = Time.time + 5f;
			this._enterFishingPromptTill = -1f;
			this._phyEngine.Clutch = 0f;
			this._phyEngine.ConnectionNeedSyncMark();
		}

		protected override void UpdateInput()
		{
			base.UpdateInput();
			BassBoatController.BBStates curStateID = this._playerFsm.CurStateID;
			if (curStateID != BassBoatController.BBStates.EngineIdle)
			{
				if (curStateID != BassBoatController.BBStates.EngineOff)
				{
					if (curStateID == BassBoatController.BBStates.Fishing)
					{
						if (this._activateTrollingsTurnsAt > 0f && this._activateTrollingsTurnsAt < Time.time)
						{
							this._activateTrollingsTurnsAt = -1f;
							this._trollingEngineTurns.Start();
						}
						base.UpdateDrivingPrompt();
						base.UpdateTrollingPrompts();
						if ((ControlsController.ControlsActions.StartFishing.WasPressed || this._driver.IsPutTrollingRod) && (this._driver.State == typeof(PlayerIdle) || this._driver.State == typeof(PlayerIdlePitch) || this._driver.State == typeof(PlayerEmpty) || this._driver.State == typeof(HandIdle)))
						{
							this._driver.IsPutTrollingRod = false;
							this._playerFsm.SendSignal(BassBoatController.BBSignals.Sitdown);
						}
						if (this._driver.State != typeof(PlayerPhotoMode))
						{
							this.updateTrollingEngine();
						}
					}
				}
				else
				{
					base.UpdateFishingPrompt();
					this._phyEngine.FuelRate = 0f;
					if (ControlsController.ControlsActions.UseAnchor.WasPressed)
					{
						this.SwitchAnchored();
					}
					if (ControlsController.ControlsActions.StartFishing.WasPressed)
					{
						this._playerFsm.SendSignal(BassBoatController.BBSignals.Standup);
					}
					else if (ControlsController.ControlsActions.StartStopBoatEngine.WasClicked && this.IsLookOnEngine && !this.IsCanLeaveBoat)
					{
						this._playerFsm.SendSignal(BassBoatController.BBSignals.StartEngine);
					}
				}
			}
			else
			{
				if (ControlsController.ControlsActions.StartStopBoatEngine.WasClicked && this.IsLookOnEngine)
				{
					this.StopEngine();
					this._playerFsm.SendSignal(BassBoatController.BBSignals.StopEngine);
					return;
				}
				base.UpdateFishingPrompt();
				if (!Mathf.Approximately(this.Vertical, 0f) && Time.time > this._throttleLockTimeStamp)
				{
					float num = Mathf.Sign(this.Vertical);
					float num2 = ((!ControlsController.ControlsActions.RunHotkey.IsPressed) ? 1f : this._engine.AcceleratedForceMultiplier);
					if (this._rpmFactor < 0f)
					{
						num2 = BoatController<BassBoatController.BBStates, BassBoatController.BBSignals, BassBoatMouseController, BassBoat>.IncreaseReverseThrustFactor;
					}
					float num3 = this._rpmFactor + this.Vertical * Time.deltaTime * num2 / this._engine.MaxThrustGainTime;
					if (Mathf.Sign(num3) != Mathf.Sign(this._rpmFactor) && this._rpmFactor != 0f)
					{
						this.SetEngineZeroThrust();
						this._throttleLockTimeStamp = Time.time + 0.25f;
					}
					else
					{
						if (this._engineSound != null && Mathf.Approximately(this._rpmFactor, 0f))
						{
							this._engineSound.PlayGearShift();
						}
						if (!RodPodHelper.IsFreeAllRodStands)
						{
							this._rpmFactor = Mathf.Clamp(num3, -1f, this._serverSettings.TrollingMaxRevs);
						}
						else
						{
							this._rpmFactor = Mathf.Clamp(num3, -1f, 1f);
						}
						this._phyEngine.Clutch = 1f;
						this._phyEngine.OutputMultiplier = BoatWithEngineController.CalculateOutputMultiplier(this._engine.MaxThrustVelocity * ((this._rpmFactor >= 0f) ? 1f : this.engineObject.reverseFactor), this._engine.EngineModelNominalMaxAngularSpeed, this._phyEngine.Radius);
						this._phyEngine.ConnectionNeedSyncMark();
					}
				}
				this._engine.PropellerPivot.localRotation = Quaternion.Euler(0f, 0f, this._engine.PropellerPivot.localRotation.eulerAngles.z + 6f * this._engine.PropellerMaxRPM * this._rpmFactor * Time.deltaTime);
				this._engine.HorizontalRotationPivot.localRotation = Quaternion.Euler(0f, Mathf.Clamp(-Math3d.ClampAngleTo180(this.engineObject.ThrustAngle), -this._engine.RotationRange, this._engine.RotationRange), 0f);
				if (ControlsController.ControlsActions.StartFishing.WasPressed)
				{
					if (this.IsTrollingPossible)
					{
						this._playerFsm.SendSignal(BassBoatController.BBSignals.Standup);
						return;
					}
					ShowHudElements.Instance.ShowTrollingSpeedTooHigh();
				}
				if (ControlsController.ControlsActions.UseAnchor.WasPressed)
				{
					this.SwitchAnchored();
				}
				this._axisInput[0] = -base.Horizontal;
				this._axisInput[1] = this._rpmFactor;
				this._wheelHandsAnimator.Update(this._axisInput);
				this.engineObject.Update(this._rpmFactor, base.BoatVelocity, base.Horizontal);
				this._phyPropeller.ThrustVelocity = this.engineObject.ThrustVelocity;
				this._phyPropeller.Area = this.engineObject.ThrustArea;
				float num4 = this.engineObject.Revolutions / this.engineObject.MaxRevolutions;
				this._phyEngine.FuelRate = this._engine.EngineModelIdleFuelRate + (this._engine.EngineModelMaxFuelRate - this._engine.EngineModelIdleFuelRate) * num4;
				float num5 = BoatWithEngineController.CalculateOutputMultiplier(this._engine.MinThrustVelocity, this._engine.EngineModelNominalMinAngularSpeed, this._phyEngine.Radius);
				float num6 = BoatWithEngineController.CalculateOutputMultiplier(this._engine.MaxThrustVelocity * this.engineObject.VelocityFactor, this._phyEngine.shaftVelocity, this._phyEngine.Radius);
				float num7 = ((this._rpmFactor < 0f) ? this.engineObject.reverseFactor : 1f);
				this._phyEngine.OutputMultiplier = num7 * Mathf.Lerp(num5, num6, num4);
				this._phyEngine.ConnectionNeedSyncMark();
				float thrustAngle = this.engineObject.ThrustAngle;
				float num8 = (thrustAngle - 90f) * 0.017453292f;
				this._phyPropeller.ThrustLocalDirection = new Vector3(Mathf.Cos(num8), 0f, Mathf.Sin(num8)) * Mathf.Sign(this._rpmFactor);
				this._phyPropeller.ConnectionNeedSyncMark();
				this._driver.Update3dCharMecanimParameter(TPMMecanimFParameter.LeftWeight, -Math3d.ClampAngleTo180(thrustAngle) / this._engine.RotationRange);
				this._driver.Update3dCharMecanimParameter(TPMMecanimFParameter.CenterWeight, this._rpmFactor);
			}
		}

		protected override bool CouldShowFishingPrompt
		{
			get
			{
				return this.IsTrollingPossible;
			}
		}

		private void updateTrollingEngine()
		{
			if (this._phyTrollingPropeller != null)
			{
				Vector2 vector;
				vector..ctor(ControlsController.ControlsActions.Move.X, ControlsController.ControlsActions.Move.Y);
				if (this._trollingEngineAnimations != null)
				{
					this._trollingEngineTurns.Update(-vector.x);
				}
				float num = Mathf.Min(vector.magnitude, 1f);
				this._phyTrollingPropeller.ThrustVelocity = this._settings.engines[1].Settings.MaxThrustVelocity * num;
				float num2 = -Mathf.Atan2(vector.y, -vector.x);
				this._phyTrollingPropeller.ThrustLocalDirection = new Vector3(Mathf.Cos(num2), 0f, Mathf.Sin(num2));
				this._phyTrollingPropeller.ConnectionNeedSyncMark();
				if (this._trollingEngineSound != null)
				{
					this.trollingEngineSoundPitch = Mathf.Lerp(num, this.trollingEngineSoundPitch, Mathf.Exp(-Time.deltaTime * 10f));
					this._trollingEngineSound.pitch = this.trollingEngineSoundPitch;
					this._trollingEngineSound.volume = GlobalConsts.BgVolume;
				}
			}
		}

		private void updateGauges()
		{
			if (this._settings.SpeedometerArrow != null)
			{
				Vector3 localEulerAngles = this._settings.SpeedometerArrow.localEulerAngles;
				localEulerAngles.x = MathHelper.PiecewiseLinear(this._settings.SpeedometerScale, base.BoatVelocity);
				this._settings.SpeedometerArrow.localEulerAngles = localEulerAngles;
			}
			if (this._settings.TachometerArrow != null)
			{
				Vector3 localEulerAngles2 = this._settings.TachometerArrow.localEulerAngles;
				localEulerAngles2.x = MathHelper.PiecewiseLinear(this._settings.TachometerScale, 60f * this._phyEngine.shaftVelocity / 6.2831855f);
				this._settings.TachometerArrow.localEulerAngles = localEulerAngles2;
			}
		}

		public override void LateUpdate()
		{
			this._phyEngine.ConnectionNeedSyncMark();
			base.LateUpdate();
			if (this._engineSound != null)
			{
				float num = this._phyEngine.shaftVelocity;
				if (this._rpmFactor < 0f)
				{
					num = this._engine.EngineModelNominalMinAngularSpeed + (this._phyEngine.shaftVelocity - this._engine.EngineModelNominalMinAngularSpeed) * 0.5f;
				}
				this._engineSound.engineCurrentRPM = 60f * num / 6.2831855f;
			}
			if (this._phyEngine.shaftVelocity < 0.01f && !this._phyEngine.StarterEngaged)
			{
				this.StopEngine();
				this._playerFsm.SendSignal(BassBoatController.BBSignals.StopEngine);
			}
			this.updateGauges();
			this.engineObject.updateWaterDisturbance();
		}

		public override void DisableSound(bool flag)
		{
			if (this._wasEngineStarted && this._engineSound != null)
			{
				this._engineSound.masterVolume = ((!flag) ? this._engineSoundVolume : 0f);
			}
		}

		public override void OnPlayerEnabled()
		{
			if (this._playerFsm.CurStateID != BassBoatController.BBStates.Fishing)
			{
				this._wheelHandsAnimator.RefreshAnimation();
			}
		}

		float IBoatWithEngine.get_BoatVelocity()
		{
			return base.BoatVelocity;
		}

		FloatingSimplexComposite IBoatWithEngine.get_Phyboat()
		{
			return base.Phyboat;
		}

		private bool _wasEngineStarted;

		private float _rpmFactor;

		private float _throttleLockTimeStamp;

		protected List<PointOnRigidBody> _engines = new List<PointOnRigidBody>();

		private PointToRigidBodyResistanceTransmission _phyPropeller;

		private PointToRigidBodyResistanceTransmission _phyTrollingPropeller;

		private EngineSettings _engine;

		private EngineModel _phyEngine;

		private EngineObject engineObject;

		private RealisticEngineSound _engineSound;

		private float _engineSoundVolume;

		private AudioSource _trollingEngineSound;

		private TwoAxisSequences<TPMFloats> _wheelHandsAnimator;

		private Animation _boatAnimation;

		private Animation _trollingEngineAnimations;

		private AnimAxisSequence _trollingEngineTurns;

		private float _activateTrollingsTurnsAt = -1f;

		private float _startEngineSondAt = -1f;

		private float trollingEngineSoundPitch;

		public enum BBStates
		{
			None,
			EngineIdle,
			PrepareFishing,
			Fishing,
			EndFishing,
			UnboardFisher,
			Unboarding,
			EngineOff,
			EngineStart,
			EngineStop,
			InGameMap,
			StartRecovery,
			StartRecoveryFishing,
			StartRecoveryEngineOff
		}

		public enum BBSignals
		{
			None,
			Boarded,
			Standup,
			ReadyToFish,
			Sitdown,
			ReadyToDrive,
			Unboarding,
			HiddenUnboarding,
			Unboarded,
			StartEngine,
			EngineStarted,
			StopEngine,
			EngineStopped,
			FishingFinished,
			UseMap,
			CloseMapNoEngine,
			CloseMapWithEngine,
			FishingFinishedToIdle,
			RollRecovering,
			Roll
		}
	}
}
