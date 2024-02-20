using System;
using System.Collections.Generic;
using Cayman;
using FSMAnimator;
using ObjectModel;
using Phy;
using UnityEngine;

namespace Boats
{
	public class BoatWithEngineController : BoatController<BoatWithEngineController.BWEStates, BoatWithEngineController.BWESignals, ZodiacMouseController, MotorBoat>, IBoatWithEngine
	{
		protected BoatWithEngineController(Transform spawn, BoatSettings settings, MotorBoat serverSettings, bool isOwner, ushort factoryID)
			: base(spawn, settings, serverSettings, isOwner, factoryID)
		{
			LogHelper.Log("BoatWithEngineController.ctror(1): {0}", new object[] { this._settings });
			this._engine = this._settings.engines[0].Settings;
			this._engine.transform.position = this._settings.engines[0].MountRoot.position;
			this._boatAnimation = this._engine.gameObject.GetComponent<Animation>();
			LogHelper.Log("BoatWithEngineController.ctror(2): {0}, {1}, {2}", new object[]
			{
				this._boatAnimation,
				this._engine.AnimationsBank,
				(!(this._engine.AnimationsBank != null)) ? null : this._engine.AnimationsBank.Clips
			});
			if (this._engine.AnimationsBank != null)
			{
				LogHelper.Log("BoatWithEngineController.ctror(2.1): {0}", new object[] { this._engine.AnimationsBank.Clips.Length });
			}
			for (int i = 0; i < this._engine.AnimationsBank.Clips.Length; i++)
			{
				AnimationClip animationClip = this._engine.AnimationsBank.Clips[i];
				this._boatAnimation.AddClip(animationClip, animationClip.name);
			}
			LogHelper.Log("BoatWithEngineController.ctror(3): before ParseServerSettings");
			this.ParseServerSettings();
			this.InitializeSimulation();
			this.LiftUpEngine();
			this.engineObject = new EngineObject(this._engine, this._settings, this._serverSettings, this);
			this.engineObject.SetFullFuel();
			this._engineSound = this._engine.gameObject.GetComponent<RealisticEngineSound>();
			LogHelper.Log("BoatWithEngineController.ctror(4): {0}", new object[] { this._engineSound });
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

		public static BoatWithEngineController Create(Transform spawn, BoatSettings settings, MotorBoat serverSettings, bool isOwner, ushort factoryID)
		{
			BoatWithEngineController boatWithEngineController = new BoatWithEngineController(spawn, settings, serverSettings, isOwner, factoryID);
			boatWithEngineController.InitFSM();
			boatWithEngineController.InitPhysics();
			return boatWithEngineController;
		}

		protected override bool IsCanLeaveBoat
		{
			get
			{
				return (((this._playerFsm.CurStateID == BoatWithEngineController.BWEStates.EngineIdle || this._playerFsm.CurStateID == BoatWithEngineController.BWEStates.EngineOff) && !this.IsLookOnEngine) || (this._playerFsm.CurStateID == BoatWithEngineController.BWEStates.Fishing && this._driver.CanLeaveBoat)) && base.IsCanLeaveBoat;
			}
		}

		public override bool CantOpenInventory
		{
			get
			{
				return base.CantOpenInventory || (this._playerFsm.CurStateID != BoatWithEngineController.BWEStates.Fishing && this._playerFsm.CurStateID != BoatWithEngineController.BWEStates.EngineIdle && this._playerFsm.CurStateID != BoatWithEngineController.BWEStates.EngineOff);
			}
		}

		public override bool IsReadyForRod
		{
			get
			{
				return base.IsReadyForRod && this.IsTrollingPossible;
			}
		}

		public override Animation BoatAnimation
		{
			get
			{
				return this._boatAnimation;
			}
		}

		protected override BoatWithEngineController.BWESignals BoardingSignal
		{
			get
			{
				return BoatWithEngineController.BWESignals.Boarded;
			}
		}

		protected override BoatWithEngineController.BWESignals UnboardingSignal
		{
			get
			{
				return BoatWithEngineController.BWESignals.Unboarding;
			}
		}

		protected override BoatWithEngineController.BWESignals HiddenUnboardingSignal
		{
			get
			{
				return BoatWithEngineController.BWESignals.HiddenUnboarding;
			}
		}

		protected override BoatWithEngineController.BWESignals CloseMapSignal
		{
			get
			{
				return (!this._wasEngineStarted) ? BoatWithEngineController.BWESignals.CloseMapNoEngine : BoatWithEngineController.BWESignals.CloseMapWithEngine;
			}
		}

		protected override BoatWithEngineController.BWESignals RollSignal
		{
			get
			{
				return BoatWithEngineController.BWESignals.Roll;
			}
		}

		protected override BoatWithEngineController.BWESignals RollFisherSignal
		{
			get
			{
				return BoatWithEngineController.BWESignals.RollRecovering;
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

		protected override bool IsTakeAnchorPromptActive
		{
			get
			{
				return this._takeAnchorPromptTill > Time.time;
			}
		}

		public override bool IsEngineForceActive
		{
			get
			{
				return !Mathf.Approximately(this._rpmFactor, 0f);
			}
		}

		public override float Stamina
		{
			get
			{
				return this._rpmFactor;
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

		public override bool IsTrolling
		{
			get
			{
				return !Mathf.Approximately(this._rpmFactor, 0f) && (!RodPodHelper.IsFreeAllRodStands || this._playerFsm.CurStateID == BoatWithEngineController.BWEStates.Fishing);
			}
		}

		protected override bool IsAffectFOV
		{
			get
			{
				return this._playerFsm.CurStateID == BoatWithEngineController.BWEStates.EngineIdle || this._playerFsm.CurStateID == BoatWithEngineController.BWEStates.InGameMap;
			}
		}

		public override bool IsActiveMovement
		{
			get
			{
				return this._wasEngineStarted;
			}
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

		public static float CalculateOutputMultiplier(float thrustVelocity, float maxAngularVelocity, float radius)
		{
			return thrustVelocity / (radius * 3.1415927f * 2f * maxAngularVelocity);
		}

		protected override void ResetSimulation()
		{
			base.ResetSimulation();
			this.InitializeSimulation();
			base.InitPhysics();
		}

		protected override bool IsLookOnEngine
		{
			get
			{
				Transform transfrom = this.engineObject.transfrom;
				Transform transform = GameFactory.Player.CameraController.Camera.transform;
				Vector3 vector = Math3d.ProjectOXZ(transfrom.position) - Math3d.ProjectOXZ(transform.position);
				return vector.magnitude <= 2f && Vector3.Dot(transform.forward, vector.normalized) > 0.7f;
			}
		}

		public override void InitFSM()
		{
			this._playerFsm = new TFSM<BoatWithEngineController.BWEStates, BoatWithEngineController.BWESignals>("MotorBoat", BoatWithEngineController.BWEStates.None);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.None, new StateSwitchingDelegate(this.ReleaseControll), null, null, 0f, BoatWithEngineController.BWESignals.None, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.EngineOff, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.EngineStarted, false);
				base.SetServerBoatNavigation(true);
				this._enterFishingPromptFrom = Time.time + 5f;
				this._enterFishingPromptTill = -1f;
				this._driver.Update3dCharMecanimParameter(TPMMecanimFParameter.CenterWeight, 0f);
				this._driver.Update3dCharMecanimParameter(TPMMecanimFParameter.LeftWeight, 0f);
				if ((int)this._transitionToFishingSlot >= 0)
				{
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.Standup);
				}
				else if (this._wasEngineStarted && this.engineObject.IsFuel)
				{
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.SuccessEngineStart);
				}
				else
				{
					this._driver.PlayHandAnimation("empty", 1f, 0f, 0f);
				}
			}, delegate
			{
				this._enterFishingPromptFrom = -1f;
				this._enterFishingPromptTill = -1f;
			}, delegate
			{
				if (base.IsOpenMap)
				{
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.UseMap);
					return;
				}
				if (!this._wasEngineStarted && this.IsLookOnEngine)
				{
					ShowHudElements.Instance.ShowZodiacEnterIgnitionState();
				}
				if (base.SwitchingRodHandler())
				{
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.Standup);
				}
			}, 0f, BoatWithEngineController.BWESignals.None, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.EngineStartInit, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.EIMode, true);
				base.SetServerBoatNavigation(false);
				this._minigameView.SetActiveMinigame(true);
				this._mouseController.SetLookAtMode(this._driver.Root);
				base.PlayTransitAnimation("RMBMotorIgnitionIn", BoatWithEngineController.BWESignals.ReadyStartEngine, 1f, 1);
				this._attemptNumber = 0;
			}, null, null, 0f, BoatWithEngineController.BWESignals.None, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.EngineStartIdle, delegate
			{
				if (this._attemptNumber == 0)
				{
					ShowHudElements.Instance.ShowZodiacIgnitionIdleState();
				}
				if (this._attemptNumber == 0)
				{
					this._minigameView.FinishEngineIgnition();
				}
				else
				{
					this._minigameView.FinishEngineIgnitionWithHighlight();
				}
				if (this._attemptNumber > 0)
				{
				}
				if (this._wasEngineStarted)
				{
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.FinishEngineStart);
				}
				else if (this._attemptNumber == this._engine.IgnitionAttemptsCount)
				{
					this._attemptNumber = 0;
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.IgnitionFailSeries);
				}
				else
				{
					this._driver.PlayHandAnimation("RMBMotorIgnitionIdle", 1f, 0f, 0f);
				}
			}, null, delegate
			{
				if (base.SwitchingRodHandler())
				{
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.FinishEngineStart);
				}
			}, 0f, BoatWithEngineController.BWESignals.None, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.EngineStartForward, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.EIStart, true);
				if (this._engineSound != null)
				{
					this._engineSound.masterVolume = 0f;
					this._engineSound.PlayStartFail();
				}
				this._phyEngine.StarterEngaged = true;
				this._phyEngine.Clutch = 0f;
				this._phyEngine.ConnectionNeedSyncMark();
				base.PlayTransitAnimation("RMBMotorIgnitionForward", BoatWithEngineController.BWESignals.IgnitionBackward, this._engine.IgnitionForwardAnimK, 1);
				float num = ((this._attemptNumber >= this._engine.IgnitionAttemptSucessPrc.Length) ? this._engine.IgnitionAttemptSucessPrc[this._engine.IgnitionAttemptSucessPrc.Length - 1] : this._engine.IgnitionAttemptSucessPrc[this._attemptNumber]);
				this._attemptNumber++;
				this._minigameView.SetupForEngineIgnition(num);
				this._wasEngineStartFailed = false;
			}, null, null, 0f, BoatWithEngineController.BWESignals.None, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.EngineStartBackward, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.EIStart, false);
				this._movingBackwardNotFail = true;
				this._minigameView.ShakeEngine(this._wasEngineStarted);
				base.PlayTransitAnimation("RMBMotorIgnitionBackward", BoatWithEngineController.BWESignals.IgnitionFinished, this._engine.IgnitionBackwardAnimK, 1);
			}, null, null, 0f, BoatWithEngineController.BWESignals.None, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.EngineStartFinishing, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.EIMode, false);
				this._minigameView.SetActiveMinigame(false);
				this._phyEngine.StarterEngaged = false;
				this._phyEngine.ConnectionNeedSyncMark();
				base.PlayTransitAnimation("RMBMotorIgnitionOut", BoatWithEngineController.BWESignals.EngineStartFinished, 1f, 1);
			}, delegate
			{
				this._mouseController.ClearLookAtMode();
			}, null, 0f, BoatWithEngineController.BWESignals.None, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.EngineOffToIdle, delegate
			{
				base.PlayTransitAnimation("RMBTakeThrottle", BoatWithEngineController.BWESignals.EngineOffToIdleFinished, 1f, 1);
			}, null, null, 0f, BoatWithEngineController.BWESignals.None, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.DamnEngine, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.EIStroke, true);
				base.PlayTransitAnimation("RMBMotorBeat", BoatWithEngineController.BWESignals.IgnitionFailSeriesFinished, this._engine.BeatEngineAnimK, 1);
			}, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.EIStroke, false);
			}, null, 0f, BoatWithEngineController.BWESignals.None, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.EngineIdle, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.EngineStarted, true);
				this._engineAnimator.Start();
				this.engineObject.Start();
			}, delegate
			{
				this._enterFishingPromptTill = -1f;
				this._driver.transform.localRotation = Quaternion.identity;
				this._engineAnimator.Stop();
			}, delegate
			{
				if (base.IsOpenMap)
				{
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.UseMap);
					return;
				}
				if (this.IsLookOnEngine && this._firstLookOnEngine)
				{
					this._firstLookOnEngine = false;
					ShowHudElements.Instance.ShowZodiacStopEngine();
				}
				if (!this._firstLookOnEngine && !this.IsLookOnEngine)
				{
					this._firstLookOnEngine = true;
				}
				if (base.SwitchingRodHandler())
				{
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.Standup);
				}
			}, 0f, BoatWithEngineController.BWESignals.None, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.Unboarding, delegate
			{
				base.SetServerBoatNavigation(false);
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsSailing, false);
				this.StopEngine();
				this._mouseController.PrepareDriverForUnboarding();
			}, null, null, 0f, BoatWithEngineController.BWESignals.Unboarded, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.PrepareFishing, delegate
			{
				base.SetServerBoatNavigation(false);
				this._mouseController.EnterFishingMode();
				base.IsFishing = true;
			}, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsBoatFishing, true);
				this._driver.PrepareFishingFromBoat();
				this._attemptNumber = 0;
				if (!this._wasEngineStarted)
				{
					this.LiftUpEngine();
				}
			}, null, 0f, BoatWithEngineController.BWESignals.ReadyToFish, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.Fishing, delegate
			{
				this._enterDrivingPromptFrom = Time.time + 5f;
				base.CheckForRequestedRod();
			}, delegate
			{
				this._enterDrivingPromptTill = -1f;
				this._putForTrollingPromptTill = -1f;
				this._takeTrollingPromptTill = -1f;
			}, null, 0f, BoatWithEngineController.BWESignals.None, null, delegate
			{
				base.IsFishing = false;
				this._putForTrollingPromptTill = -1f;
				this._takeTrollingPromptTill = -1f;
				this._driver.FinishFishingFromBoat();
				this._mouseController.ClearFishingMode();
			});
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.EndFishing, delegate
			{
				this._driver.InitFishingToDriving(delegate
				{
					this._mouseController.LeaveFishingMode(delegate
					{
						this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.FishingFinished);
					});
				});
			}, delegate
			{
				base.IsFishing = false;
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsBoatFishing, false);
				this._driver.FinishFishingFromBoat();
				if (!this._wasEngineStarted)
				{
					this.LiftDownEngine();
				}
			}, null, 0f, BoatWithEngineController.BWESignals.None, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.UnboardFisher, delegate
			{
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsSailing, false);
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsBoatFishing, false);
			}, delegate
			{
				base.IsFishing = false;
				this._mouseController.ClearFishingMode();
			}, null, 0f, BoatWithEngineController.BWESignals.Unboarded, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.StartRecovery, delegate
			{
				base.SetServerBoatNavigation(false);
				BlackScreenHandler.Show(true, null);
				GameActionAdapter.Instance.TurnOver();
				this._rpmFactor = 0f;
			}, new StateSwitchingDelegate(base.RecoveryBoat), null, 0.5f, BoatWithEngineController.BWESignals.RollRecovering, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.StartRecoveryEngineOff, delegate
			{
				base.SetServerBoatNavigation(false);
				BlackScreenHandler.Show(true, null);
				GameActionAdapter.Instance.TurnOver();
				this._rpmFactor = 0f;
			}, new StateSwitchingDelegate(base.RecoveryBoat), null, 0.5f, BoatWithEngineController.BWESignals.RollRecovering, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.StartRecoveryFishing, delegate
			{
				BlackScreenHandler.Show(true, null);
				if (this._acceptRollBackFrom > 0f && this._acceptRollBackFrom < Time.time)
				{
					GameActionAdapter.Instance.TurnOver();
				}
			}, new StateSwitchingDelegate(base.RecoveryBoat), null, 0.5f, BoatWithEngineController.BWESignals.RollRecovering, null, null);
			this._playerFsm.AddState(BoatWithEngineController.BWEStates.InGameMap, delegate
			{
				base.SetServerBoatNavigation(false);
				this._mouseController.EnterMapMode();
				this._driver.PrepareOpenMap();
			}, delegate
			{
				this._mouseController.LeaveMapMode();
				if (this._wasEngineStarted)
				{
					this._driver.PlayHandAnimation("RMBTakeThrottle", 1f, 0f, 0f);
				}
			}, null, 0f, BoatWithEngineController.BWESignals.None, null, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.None, BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWESignals.Boarded, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWEStates.EngineStartInit, BoatWithEngineController.BWESignals.PreStartEngine, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineStartInit, BoatWithEngineController.BWEStates.EngineStartIdle, BoatWithEngineController.BWESignals.ReadyStartEngine, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineStartIdle, BoatWithEngineController.BWEStates.EngineStartForward, BoatWithEngineController.BWESignals.IgnitionForward, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineStartIdle, BoatWithEngineController.BWEStates.DamnEngine, BoatWithEngineController.BWESignals.IgnitionFailSeries, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.DamnEngine, BoatWithEngineController.BWEStates.EngineStartIdle, BoatWithEngineController.BWESignals.IgnitionFailSeriesFinished, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineStartForward, BoatWithEngineController.BWEStates.EngineStartBackward, BoatWithEngineController.BWESignals.IgnitionBackward, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineStartBackward, BoatWithEngineController.BWEStates.EngineStartIdle, BoatWithEngineController.BWESignals.IgnitionFinished, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineStartIdle, BoatWithEngineController.BWEStates.EngineStartFinishing, BoatWithEngineController.BWESignals.FinishEngineStart, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineStartFinishing, BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWESignals.EngineStartFinished, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWEStates.EngineOffToIdle, BoatWithEngineController.BWESignals.SuccessEngineStart, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineOffToIdle, BoatWithEngineController.BWEStates.EngineIdle, BoatWithEngineController.BWESignals.EngineOffToIdleFinished, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWEStates.Unboarding, BoatWithEngineController.BWESignals.Unboarding, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineIdle, BoatWithEngineController.BWEStates.Unboarding, BoatWithEngineController.BWESignals.Unboarding, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.Unboarding, BoatWithEngineController.BWEStates.None, BoatWithEngineController.BWESignals.Unboarded, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.Fishing, BoatWithEngineController.BWEStates.UnboardFisher, BoatWithEngineController.BWESignals.Unboarding, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.UnboardFisher, BoatWithEngineController.BWEStates.None, BoatWithEngineController.BWESignals.Unboarded, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWEStates.PrepareFishing, BoatWithEngineController.BWESignals.Standup, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineIdle, BoatWithEngineController.BWEStates.PrepareFishing, BoatWithEngineController.BWESignals.Standup, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.PrepareFishing, BoatWithEngineController.BWEStates.Fishing, BoatWithEngineController.BWESignals.ReadyToFish, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.Fishing, BoatWithEngineController.BWEStates.EndFishing, BoatWithEngineController.BWESignals.Sitdown, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EndFishing, BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWESignals.FishingFinished, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineIdle, BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWESignals.FuelIsOut, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineIdle, BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWESignals.TurnOffEngine, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineIdle, BoatWithEngineController.BWEStates.StartRecovery, BoatWithEngineController.BWESignals.Roll, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.StartRecovery, BoatWithEngineController.BWEStates.EngineIdle, BoatWithEngineController.BWESignals.RollRecovering, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.Fishing, BoatWithEngineController.BWEStates.StartRecoveryFishing, BoatWithEngineController.BWESignals.Roll, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.StartRecoveryFishing, BoatWithEngineController.BWEStates.Fishing, BoatWithEngineController.BWESignals.RollRecovering, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWEStates.StartRecoveryEngineOff, BoatWithEngineController.BWESignals.Roll, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.StartRecoveryEngineOff, BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWESignals.RollRecovering, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWEStates.InGameMap, BoatWithEngineController.BWESignals.UseMap, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.InGameMap, BoatWithEngineController.BWEStates.EngineOff, BoatWithEngineController.BWESignals.CloseMapNoEngine, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.EngineIdle, BoatWithEngineController.BWEStates.InGameMap, BoatWithEngineController.BWESignals.UseMap, null);
			this._playerFsm.AddTransition(BoatWithEngineController.BWEStates.InGameMap, BoatWithEngineController.BWEStates.EngineIdle, BoatWithEngineController.BWESignals.CloseMapWithEngine, null);
			this._playerFsm.AddAnyStateTransition(BoatWithEngineController.BWEStates.None, BoatWithEngineController.BWESignals.HiddenUnboarding, true);
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
			this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.TurnOffEngine);
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
				this._engineAnimator.Stop();
				this.engineObject.Stop();
				this.engineObject.TurnOff();
				this._minigameView.SetEngineIndicatorOn(this._wasEngineStarted);
				this._phyEngine.FuelRate = 0f;
				this._phyEngine.ConnectionNeedSyncMark();
				if (this._engineSound != null)
				{
					this._engineSound.PlayShutdown();
				}
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
			this._transitionToFishingSlot = -1;
			this._minigameView = ShowHudElements.Instance.SailingHndl;
			if (this._engineAnimator == null)
			{
				this._engineAnimator = new TwoAxisSequences<TPMFloats>("engineTurnsController", new Animation[]
				{
					this._boatAnimation,
					this._driver.ArmsAnimation
				}, 1, "RMBIdle", "RMBTurnLeft", "RMBTurnLeftIdle", "RMBTurnRight", "RMBTurnRightIdle", "RMBThrottleUp", "RMBThrottleDown", TPMFloats.Horizontal, TPMFloats.Vertical);
			}
			this.LiftDownEngine();
			this._attemptNumber = 0;
		}

		protected override void ReleaseControll()
		{
			base.ReleaseControll();
			this.LiftUpEngine();
		}

		private void LiftDownEngine()
		{
			this._engine.VerticalRotationPivot.localRotation = Quaternion.Euler(this._engine.EngineReadyPitch, 0f, 0f);
			this._engine.HandleRotationPivot.localRotation = Quaternion.Euler(-this._engine.EngineReadyPitch, 0f, 0f);
		}

		private void LiftUpEngine()
		{
			this._engine.VerticalRotationPivot.localRotation = Quaternion.Euler(this._engine.EngineStoppedPitch, 0f, 0f);
			this._engine.HandleRotationPivot.localRotation = Quaternion.Euler(-this._engine.EngineStoppedPitch, 0f, 0f);
			this._engines[0].WaterMotor = Vector3.zero;
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
			BoatWithEngineController.BWEStates curStateID = this._playerFsm.CurStateID;
			switch (curStateID)
			{
			case BoatWithEngineController.BWEStates.EngineOff:
				base.UpdateFishingPrompt();
				this._phyEngine.FuelRate = 0f;
				if (ControlsController.ControlsActions.UseAnchor.WasPressed)
				{
					this.SwitchAnchored();
				}
				if (ControlsController.ControlsActions.StartFishing.WasClicked)
				{
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.Standup);
				}
				else if (ControlsController.ControlsActions.StartStopBoatEngine.WasClicked && !this.IsCanLeaveBoat && this.IsLookOnEngine)
				{
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.PreStartEngine);
				}
				break;
			default:
				switch (curStateID)
				{
				case BoatWithEngineController.BWEStates.EngineIdle:
				{
					if (ControlsController.ControlsActions.StartStopBoatEngine.WasClicked && this.IsLookOnEngine)
					{
						this.StopEngine();
						this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.TurnOffEngine);
						return;
					}
					if (base.IsAnchored)
					{
						this._takeAnchorPromptTill = Time.time + 1f;
					}
					else if (this._takeAnchorPromptTill > -1f && this._takeAnchorPromptTill < Time.time)
					{
						this._takeAnchorPromptTill = -1f;
					}
					base.UpdateFishingPrompt();
					if (!Mathf.Approximately(this.Vertical, 0f) && Time.time > this._throttleLockTimeStamp)
					{
						float num = Mathf.Sign(this.Vertical);
						float num2 = ((!ControlsController.ControlsActions.RunHotkey.IsPressed) ? 1f : this._engine.AcceleratedForceMultiplier);
						if (this._rpmFactor < 0f)
						{
							num2 = BoatController<BoatWithEngineController.BWEStates, BoatWithEngineController.BWESignals, ZodiacMouseController, MotorBoat>.IncreaseReverseThrustFactor;
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
					if (ControlsController.ControlsActions.StartFishing.WasClicked)
					{
						if (this.IsTrollingPossible)
						{
							this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.Standup);
							return;
						}
						ShowHudElements.Instance.ShowTrollingSpeedTooHigh();
					}
					if (ControlsController.ControlsActions.UseAnchor.WasPressed)
					{
						this.SwitchAnchored();
					}
					this._axisInput[0] = base.Horizontal;
					this._axisInput[1] = this._rpmFactor;
					this._engineAnimator.Update(this._axisInput);
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
					this._driver.Update3dCharMecanimParameter(TPMMecanimFParameter.LeftWeight, Math3d.ClampAngleTo180(thrustAngle) / this._engine.RotationRange);
					this._driver.Update3dCharMecanimParameter(TPMMecanimFParameter.CenterWeight, this._rpmFactor);
					if (!this.engineObject.IsFuel)
					{
						this.StopEngine();
						this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.FuelIsOut);
					}
					break;
				}
				case BoatWithEngineController.BWEStates.Fishing:
					base.UpdateDrivingPrompt();
					base.UpdateTrollingPrompts();
					if ((ControlsController.ControlsActions.StartFishing.WasClicked || this._driver.IsPutTrollingRod) && (this._driver.State == typeof(PlayerIdle) || this._driver.State == typeof(PlayerIdlePitch) || this._driver.State == typeof(PlayerEmpty) || this._driver.State == typeof(HandIdle)))
					{
						this._driver.IsPutTrollingRod = false;
						this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.Sitdown);
					}
					break;
				}
				break;
			case BoatWithEngineController.BWEStates.EngineStartIdle:
				if (ControlsController.ControlsActions.StartFishing.WasClicked)
				{
					this._transitionToFishingSlot = 0;
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.FinishEngineStart);
				}
				if (ControlsController.ControlsActions.IgnitionForward.IsPressed)
				{
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.IgnitionForward);
				}
				else if (ControlsController.ControlsActions.StartStopBoatEngine.WasClicked || Input.GetKeyUp(27))
				{
					this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.FinishEngineStart);
				}
				break;
			case BoatWithEngineController.BWEStates.EngineStartForward:
				if (!this._wasEngineStartFailed)
				{
					this._lastValue = this._engine.IgnitionForwardSpeed.Evaluate(base.TransitAnimationPrc);
				}
				this._minigameView.SetValueAndIndication(this._lastValue);
				this.UpdateMinigame();
				break;
			case BoatWithEngineController.BWEStates.EngineStartBackward:
				this._minigameView.SetValue((!this._wasEngineStartFailed) ? this._engine.IgnitionBackwardSpeed.Evaluate(base.TransitAnimationPrc) : 0f);
				this.UpdateMinigame();
				if (!this.IsMinigameFinished && !this._minigameView.IsEngineIgnitionSuccess && this._movingBackwardNotFail)
				{
					this._movingBackwardNotFail = false;
					this._minigameView.IndicateFail();
				}
				break;
			}
		}

		protected override bool CouldShowFishingPrompt
		{
			get
			{
				return this.IsTrollingPossible;
			}
		}

		private bool IsMinigameFinished
		{
			get
			{
				return this._wasEngineStarted || this._wasEngineStartFailed;
			}
		}

		private void UpdateMinigame()
		{
			if (!this.IsMinigameFinished && ControlsController.ControlsActions.IgnitionForward.WasReleased)
			{
				bool wasEngineStarted = this._wasEngineStarted;
				this._wasEngineStarted = this._minigameView.IsEngineIgnitionSuccess && this.engineObject.IsFuel;
				if (this._wasEngineStarted && CaymanGenerator.Instance != null)
				{
					CaymanGenerator.Instance.OnScareAction();
				}
				if (!wasEngineStarted && this._wasEngineStarted)
				{
					PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.BoatEngineStarted, true, null);
				}
				this._wasEngineStartFailed = !this._wasEngineStarted;
				if (this._wasEngineStarted)
				{
					if (this._engineSound != null)
					{
						this._engineSound.masterVolume = this._engineSoundVolume;
					}
					this._phyEngine.FuelRate = this._engine.EngineModelIdleFuelRate;
					this._phyEngine.ConnectionNeedSyncMark();
					this.engineObject.TurnOn();
					this._minigameView.SetEngineIndicatorOn(this._wasEngineStarted);
				}
				else
				{
					this._minigameView.IndicateFail();
				}
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
				this._playerFsm.SendSignal(BoatWithEngineController.BWESignals.TurnOffEngine);
			}
			this.engineObject.updateWaterDisturbance();
		}

		public override void Destroy()
		{
			base.Destroy();
			if (this._engineSound != null)
			{
				this._engineSound.masterVolume = 0f;
			}
			this.engineObject.Destroy();
		}

		public override void OnPlayerEnabled()
		{
			if (this._playerFsm.CurStateID != BoatWithEngineController.BWEStates.Fishing)
			{
				this._engineAnimator.RefreshAnimation();
			}
		}

		public override void DisableSound(bool flag)
		{
			if (this._wasEngineStarted && this._engineSound != null)
			{
				this._engineSound.masterVolume = ((!flag) ? this._engineSoundVolume : 0f);
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

		protected List<PointOnRigidBody> _engines = new List<PointOnRigidBody>();

		private float _takeAnchorPromptTill = -1f;

		private EngineSettings _engine;

		private Animation _boatAnimation;

		private RealisticEngineSound _engineSound;

		private float _engineSoundVolume;

		private float _forcePrc;

		private float _rpmFactor;

		private float _throttleLockTimeStamp;

		private float _engineYaw;

		private EngineModel _phyEngine;

		private PointToRigidBodyResistanceTransmission _phyPropeller;

		private TwoAxisSequences<TPMFloats> _engineAnimator;

		private bool _wasEngineStarted;

		private bool _wasEngineStartFailed;

		private int _attemptNumber;

		private IBoatWithEngineMinigameView _minigameView;

		protected EngineObject engineObject;

		private bool _firstLookOnEngine = true;

		private bool _movingBackwardNotFail;

		private float _lastValue;

		public enum BWEStates
		{
			None,
			EngineIdle,
			PrepareFishing,
			Fishing,
			EndFishing,
			UnboardFisher,
			Unboarding,
			EngineOff,
			EngineStartInit,
			EngineStartIdle,
			EngineStartForward,
			EngineStartBackward,
			DamnEngine,
			EngineStartFinishing,
			EngineOffToIdle,
			InGameMap,
			StartRecovery,
			StartRecoveryFishing,
			StartRecoveryEngineOff
		}

		public enum BWESignals
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
			PreStartEngine,
			ReadyStartEngine,
			IgnitionForward,
			IgnitionBackward,
			IgnitionFinished,
			FinishEngineStart,
			EngineStartFinished,
			SuccessEngineStart,
			EngineOffToIdleFinished,
			IgnitionFailSeries,
			IgnitionFailSeriesFinished,
			FuelIsOut,
			TurnOffEngine,
			FishingFinished,
			UseMap,
			CloseMapNoEngine,
			CloseMapWithEngine,
			Roll,
			RollRecovering
		}
	}
}
