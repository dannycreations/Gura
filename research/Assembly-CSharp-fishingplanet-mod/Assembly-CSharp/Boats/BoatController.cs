using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using I2.Loc;
using Mono.Simd;
using Mono.Simd.Math;
using ObjectModel;
using Phy;
using SimplexGeometry;
using UnityEngine;

namespace Boats
{
	public abstract class BoatController<TStateEnum, TSignalEnum, TCamera, TServerSettings> : IBoatController, IBoatData where TStateEnum : struct, IConvertible, IComparable where TSignalEnum : struct, IConvertible, IComparable where TCamera : BoatMouseController where TServerSettings : Boat
	{
		protected BoatController(Transform spawn, BoatSettings settings, TServerSettings serverSettings, bool isOwner, ushort factoryID)
		{
			this._mouseController = GameFactory.Player.CameraController.Camera.GetComponent<TCamera>();
			this._settings = settings;
			for (int i = 0; i < this._settings.transform.childCount; i++)
			{
				this._rodSlots = this._settings.transform.GetChild(i).GetComponent<RodPodController>();
				if (this._rodSlots != null)
				{
					this._rodSlots.SetBoat(serverSettings);
					this._rodSlots.OnPut(false);
					break;
				}
			}
			this._serverSettings = serverSettings;
			this.ParseServerSettings();
			this._factoryID = factoryID;
			this.State = ((!isOwner) ? BoatState.FOR_RENT : BoatState.MY);
			if (settings.FakeLegs != null)
			{
				this._fakeLegs = new FakeLegsController(settings.FakeLegs);
			}
			this.boatSim = new FishingRodSimulation("Boat.RodSource", true);
			this.boatSimThread = new SimulationThread("Boat", this.boatSim, new FishingRodSimulation("Boat.RodThread", false));
			this.boatSim.PhyActionsListener = this.boatSimThread;
			this.boatSimThread.OnThreadException += this.OnSimThreadException;
			this.boatSimThread.Start();
			this.InitializeSimulation(spawn.position, spawn.rotation);
			if (this._settings.paddle != null)
			{
				this._oarWaterDisturber = new OarWaterDisturber(this._settings.paddle);
			}
			this._boatCollider = this.Transform.gameObject.GetComponent<BoatCollider>();
			this._boatMeshCollider = new GameObject("BoatMeshCollider")
			{
				transform = 
				{
					parent = this.Transform,
					localPosition = Vector3.zero,
					localRotation = Quaternion.identity
				},
				layer = LayerMask.NameToLayer("Boat")
			}.AddComponent<MeshCollider>();
			this._boatMeshCollider.convex = true;
			this._boatMeshCollider.inflateMesh = true;
			this._boatMeshCollider.skinWidth = this._boatCollider.InflateWidth;
			this._boatMeshCollider.sharedMesh = this._boatCollider.CollisionMesh;
			this._boatCollider.BoatMeshCollider = this._boatMeshCollider;
			this._renderers = RenderersHelper.GetAllRenderersForObject<Renderer>(this._settings.transform);
			this._azCollider = this._settings.GetComponent<BoxCollider>();
			if (this._azCollider == null || this._azCollider.gameObject.layer != GlobalConsts.BoatAZLayer)
			{
				LogHelper.Error("{0} boat has no collider with layer BoatActionZone", new object[] { this._settings.name });
			}
			this.autoAnchorTimestamp = new float?(Time.time + 1f);
		}

		protected bool Boarded
		{
			get
			{
				return this._driver != null && !this._unboarding;
			}
		}

		public ushort FactoryID
		{
			get
			{
				return this._factoryID;
			}
		}

		public Vector3 Position
		{
			get
			{
				return this.Transform.position;
			}
		}

		public Vector3 DriverCameraLocalPosition
		{
			get
			{
				return this._mouseController.DriverCameraLocalPosition;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return this.Phyboat.Rotation;
			}
		}

		public BoatState State { get; protected set; }

		public FloatingSimplexComposite Phyboat { get; protected set; }

		public bool IsAnchored
		{
			get
			{
				return this.setAnchorCalled || this.anchorMassFront != null;
			}
			set
			{
				if (value)
				{
					this.SetAnchor();
				}
				else
				{
					this.ReleaseAnchor();
				}
			}
		}

		public float BoatVelocity { get; private set; }

		public Vector3 BoatCollisionImpulse
		{
			get
			{
				return this.Phyboat.collisionImpulse.AsVector3();
			}
		}

		public virtual float Stamina
		{
			get
			{
				return 1f;
			}
		}

		public virtual bool IsRowing
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsEngineForceActive
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsTrolling
		{
			get
			{
				return false;
			}
		}

		public bool IsOarPresent
		{
			get
			{
				return this._settings.paddle != null;
			}
		}

		public bool IsEnginePresent
		{
			get
			{
				return this._settings.engines.Length > 0;
			}
		}

		public virtual bool IsTrollingPossible
		{
			get
			{
				return false;
			}
		}

		public bool IsBeingLookedAt(Collider collider)
		{
			return this._azCollider == collider;
		}

		public virtual Animation BoatAnimation
		{
			get
			{
				return null;
			}
		}

		public virtual bool CantOpenInventory
		{
			get
			{
				return this._mouseController != null && this._mouseController.IsTransitionActive;
			}
		}

		public ConnectedBodiesSystem Sim
		{
			get
			{
				return this.boatSim;
			}
		}

		public SimulationThread SimThread
		{
			get
			{
				return this.boatSimThread;
			}
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> FishingModeSwitched = delegate(bool a)
		{
		};

		protected bool IsFishing
		{
			get
			{
				return this._isFishing;
			}
			set
			{
				this._isFishing = value;
				this.FishingModeSwitched(this._isFishing);
			}
		}

		public bool IsInTransitionState
		{
			get
			{
				return this._mouseController.IsTransitionActive;
			}
		}

		public virtual Transform DriverPivot
		{
			get
			{
				return this._settings.DriverPivot;
			}
		}

		public virtual Transform AnglerPivot
		{
			get
			{
				return this._settings.AnglerPivot;
			}
		}

		public virtual Transform ShadowPivot
		{
			get
			{
				return (!this._isFishing) ? this._settings.DriverShadowPivot : this._settings.AnglerShadowPivot;
			}
		}

		public float Width
		{
			get
			{
				return this._settings.Width;
			}
		}

		public float Length
		{
			get
			{
				return this._settings.BowLength + this._settings.MiddleLength + this._settings.SternLength;
			}
		}

		protected virtual bool IsLookOnEngine
		{
			get
			{
				return false;
			}
		}

		protected virtual bool IsAffectFOV
		{
			get
			{
				return false;
			}
		}

		public bool IsOpenMap
		{
			get
			{
				return ControlsController.ControlsActions.OpenMap.WasClicked && !this.IsInTransitionState && this._unboardingPosition == null;
			}
		}

		public Vector3 TPMPlayerPosition
		{
			get
			{
				return this.DriverPivot.position;
			}
		}

		protected virtual bool IsCanLeaveBoat
		{
			get
			{
				return this._unboardingPosition != null && !this.IsInTransitionState;
			}
		}

		public ItemSubTypes Category
		{
			get
			{
				return this._serverSettings.ItemSubType;
			}
		}

		protected BoatType Type
		{
			get
			{
				return GameFactory.BoatDock.GetTypeByItemType(this._serverSettings.ItemSubType);
			}
		}

		public virtual bool IsReadyForRod
		{
			get
			{
				return !this.CantOpenInventory;
			}
		}

		protected abstract TSignalEnum BoardingSignal { get; }

		protected abstract TSignalEnum UnboardingSignal { get; }

		protected abstract TSignalEnum HiddenUnboardingSignal { get; }

		protected abstract TSignalEnum CloseMapSignal { get; }

		protected abstract TSignalEnum RollSignal { get; }

		protected abstract TSignalEnum RollFisherSignal { get; }

		protected abstract bool IsEnterFishingPromtActive { get; }

		protected virtual bool IsBackToNavigationPromptActive
		{
			get
			{
				return false;
			}
		}

		protected virtual bool IsTakeAnchorPromptActive
		{
			get
			{
				return false;
			}
		}

		public sbyte SavedFishingSlot
		{
			get
			{
				return this._transitionToFishingSlot;
			}
		}

		public Transform Transform
		{
			get
			{
				return this._settings.transform;
			}
		}

		protected float Horizontal
		{
			get
			{
				return ControlsController.ControlsActions.Move.X;
			}
		}

		protected virtual float Vertical
		{
			get
			{
				if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
				{
					return ControlsController.ControlsActions.BoatThrottle.Value - ControlsController.ControlsActions.BoatThrottleNegative.Value;
				}
				return ControlsController.ControlsActions.Move.Y;
			}
		}

		public abstract bool IsActiveMovement { get; }

		public RodPodController RodSlots
		{
			get
			{
				return this._rodSlots;
			}
		}

		public void Teleport(Vector3 position, Quaternion rotation)
		{
			this.Phyboat.Rotation = rotation;
			this.Phyboat.Position = position;
			this.Transform.position = position;
			this.Transform.rotation = rotation;
			this.Transform.position += this.Phyboat.Position - this._settings.Barycenter.position;
			this.freezePhySync = true;
		}

		public void SetServerBoatNavigation(bool rowing)
		{
			if (BoatController<TStateEnum, TSignalEnum, TCamera, TServerSettings>.Rowing != rowing)
			{
				BoatController<TStateEnum, TSignalEnum, TCamera, TServerSettings>.Rowing = rowing;
				MonoBehaviour.print("boat navigation: " + rowing);
				if (!StaticUserData.IS_IN_TUTORIAL)
				{
					PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.BoatNavigation, rowing, null);
				}
			}
		}

		private Transform CreateDebugSphere(string name)
		{
			Transform transform = GameObject.CreatePrimitive(0).transform;
			transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			transform.name = name;
			transform.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			transform.GetComponent<Collider>().enabled = false;
			return transform;
		}

		private void InitializeSimulation(Vector3 spawnPosition, Quaternion spawnRotation)
		{
			BoatHull boatHull = null;
			FloatingBodyType bodyType = this._settings.BodyType;
			if (bodyType != FloatingBodyType.Triangular)
			{
				if (bodyType == FloatingBodyType.Trapezoid)
				{
					boatHull = new TrapezoidBoatHull(this._settings.BowLength, this._settings.BowWidth, this._settings.MiddleLength, this._settings.Width, this._settings.Height, this._settings.BottomWidth, this._settings.Barycenter.localPosition.AsVector4f() - this._settings.VolumeBodyAnchor.localPosition.AsVector4f(), this._settings.Weight, false);
				}
			}
			else
			{
				boatHull = new TriangularBoatHull(this._settings.BowLength, this._settings.MiddleLength, this._settings.SternLength, this._settings.Width, this._settings.Height, this._settings.Weight);
			}
			this.Phyboat = new FloatingSimplexComposite(this.boatSim, this._settings.Weight, spawnPosition, Mass.MassType.Boat)
			{
				Rotation = spawnRotation,
				WaterY = -1000f,
				IgnoreEnvForces = true,
				VolumeBody = boatHull,
				RotationDamping = 0.2f,
				WaterNoise = this._settings.WaterNoise,
				BuoyancySpeedMultiplier = 0f,
				WaterDragConstant = 0f,
				LateralWaterResistance = this._settings.LateralResistance,
				LongitudalWaterResistance = this._settings.LongitudalResistance,
				SlidingFrictionFactor = 0.01f,
				BounceFactor = 0.001f,
				DynamicTangageStabilizer = this._settings.DynamicTangageStabilizer,
				DynamicYawStabilizer = this._settings.DynamicYawStabilizer,
				DynamicRollStabilizer = this._settings.DynamicRollStabilizer,
				Collision = Mass.CollisionType.RigidbodyContacts,
				AxialWaterDragEnabled = false,
				Dimensions = new Vector3(this._settings.Width, this._settings.Height, this._settings.BowLength + this._settings.BowWidth + this._settings.MiddleLength)
			};
			this.Phyboat.UpdateBody();
			this.Phyboat.Position = new Vector3(this.Phyboat.Position.x, -this.Phyboat.waterlineY, this.Phyboat.Position.z);
			this.Transform.rotation = spawnRotation;
			this.Transform.position = this.Phyboat.Position;
			this.Transform.position += this.Phyboat.Position - this._settings.Barycenter.position;
			this._bowForcePoint = new PointOnRigidBody(this.boatSim, this.Phyboat, boatHull.BowPoint.AsVector3(), Mass.MassType.Unknown)
			{
				IgnoreEnvForces = true
			};
			this._sternForcePoint = new PointOnRigidBody(this.boatSim, this.Phyboat, boatHull.SternPoint.AsVector3(), Mass.MassType.Unknown)
			{
				IgnoreEnvForces = true
			};
			this.boatSim.Masses.Add(this._bowForcePoint);
			this.boatSim.Masses.Add(this._sternForcePoint);
			this.rodForcePoint = new PointOnRigidBody(this.boatSim, this.Phyboat, Vector3.zero, Mass.MassType.Unknown)
			{
				IgnoreEnvForces = true
			};
			this.boatSim.Masses.Add(this.rodForcePoint);
			this.rodCounterForcePoint = new PointOnRigidBody(this.boatSim, this.Phyboat, Vector3.zero, Mass.MassType.Unknown)
			{
				IgnoreEnvForces = true
			};
			this.boatSim.Masses.Add(this.rodCounterForcePoint);
			this.boatSim.Masses.Add(this.Phyboat);
			Vector3 vector;
			vector..ctor(spawnPosition.x, 0f, spawnPosition.z);
			this.boatSim.VisualPositionOffset = vector;
			this.boatSimThread.VisualPositionOffsetGlobalChanged(vector);
		}

		protected virtual void refreshBoatSettings()
		{
			this.Phyboat.VolumeBody.SetMass(this._settings.Weight);
			this.Phyboat.MassValue = this._settings.Weight;
			this.Phyboat.WaterNoise = this._settings.WaterNoise;
			this.Phyboat.LateralWaterResistance = this._settings.LateralResistance;
			this.Phyboat.LongitudalWaterResistance = this._settings.LongitudalResistance;
			this.Phyboat.DynamicRollStabilizer = this._settings.DynamicRollStabilizer;
			this.Phyboat.DynamicTangageStabilizer = this._settings.DynamicTangageStabilizer;
			this.Phyboat.DynamicYawStabilizer = this._settings.DynamicYawStabilizer;
			this.Phyboat.UpdateBody();
		}

		public void OnSimThreadException()
		{
			this.SimThreadExceptionThrown = true;
		}

		protected virtual void ResetSimulation()
		{
			this.SimThreadExceptionThrown = false;
			Vector3? vector = this.lastValidBoatPosition;
			if (vector != null)
			{
				this.InitializeSimulation(this.lastValidBoatPosition.Value, this.lastValidBoatRotation.Value);
			}
			this.InitPhysics();
			this.StopBoat();
		}

		public void SetVisibility(bool flag)
		{
			for (int i = 0; i < this._renderers.Count; i++)
			{
				this._renderers[i].enabled = flag;
			}
			if (this._settings.FishFinderPivots != null)
			{
				for (int j = 0; j < this._settings.FishFinderPivots.Length; j++)
				{
					this._settings.FishFinderPivots[j].gameObject.SetActive(flag);
				}
			}
		}

		private void ParseServerSettings()
		{
			if (this._serverSettings.Weight == null)
			{
				this._settings.Weight += this._serverSettings.WeightModifier;
			}
			else
			{
				this._settings.Weight = (float)this._serverSettings.Weight.Value + this._serverSettings.WeightModifier;
			}
			this._settings.LongitudalResistance *= this._serverSettings.LongitudalResistanceMultiplier;
			this._settings.LateralResistance *= this._serverSettings.LateralResistanceMultiplier;
			if (this._settings.paddle != null)
			{
				this._settings.paddle.RowingVelocity = this._serverSettings.Velocity;
				if (!Mathf.Approximately(this._serverSettings.Velocity, 0f))
				{
					this._settings.paddle.FastSpeedK = (this._serverSettings.Velocity + this._serverSettings.AccelertatedVelocity) / this._serverSettings.Velocity;
				}
			}
		}

		protected void InitPhysics()
		{
			this.boatSim.RefreshObjectArrays(true);
		}

		public abstract void InitFSM();

		protected bool SwitchingRodHandler()
		{
			if (this._driver.RequestedRod != null)
			{
				this._isRodPodTransition = false;
				this._transitionToFishingSlot = (sbyte)this._driver.RequestedRod.Slot;
				this._driver.RequestedRod = null;
				return true;
			}
			int requestedSlotId = this._driver.GetRequestedSlotId();
			if (requestedSlotId != -1)
			{
				this._isRodPodTransition = ControlsController.ControlsActions.RodStandStandaloneHotkeyModifier.IsPressed || ControlsController.ControlsActions.RodPanel.IsPressed;
				this._transitionToFishingSlot = (sbyte)requestedSlotId;
				return true;
			}
			return false;
		}

		protected void CheckForRequestedRod()
		{
			if ((int)this._transitionToFishingSlot > 0)
			{
				if (this._isRodPodTransition)
				{
					this._driver.TryToTakeRodFromPodSlot((int)this._transitionToFishingSlot - 1);
				}
				else
				{
					this._driver.TryToTakeRodFromSlot((int)this._transitionToFishingSlot, true);
				}
			}
			this._transitionToFishingSlot = -1;
		}

		public virtual void OnTakeRodFromPod()
		{
		}

		public void TakeControll(PlayerController driver)
		{
			this._wasFishingPromptUsed = false;
			this._wasDrivingPromptUsed = false;
			this._wasPutForTrollingPromptUsed = false;
			this._wasTakeTrollingPromptUsed = false;
			this._unboardingPosition = null;
			this._unboarding = false;
			driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsSailing, true);
			driver.Update3dCharMecanimParameter(TPMMecanimIParameter.SubState, (byte)this.Type);
			this._driver = driver;
			this.Board();
			this._playerFsm.SendSignal(this.BoardingSignal);
		}

		public void CameraSetActive(bool flag)
		{
			this._mouseController.SetActive(flag);
		}

		public void FreezeCamera(bool flag)
		{
			this._mouseController.FreezeCamera(flag);
		}

		public void SetFishPhotoMode(bool flag)
		{
			this._mouseController.SetFishPhotoMode(flag);
		}

		public void InitTransitionToZeroXRotation()
		{
			this._mouseController.InitTransitionToZeroXRotation();
		}

		public void TransitToZeroXRotation()
		{
			this._mouseController.TransitToZeroXRotation();
		}

		public void FinalizeTransitionToZeroXRotation()
		{
			this._mouseController.FinalizeTransitionToZeroXRotation();
		}

		public void SetExternalGlobalRotation(Quaternion rotation)
		{
			this._mouseController.SetGlobalRotation(rotation);
		}

		public void OnPlayerExternalControllReleased()
		{
			this._mouseController.OnPlayerExternalControllReleased();
		}

		protected virtual void Board()
		{
			GameActionAdapter.Instance.Board(this.Category);
			this._driver.Collider.position = this.DriverPivot.position;
			this._hand = TransformHelper.FindDeepChild(this._driver.Collider, "loc_rod_right");
			this._hand2 = TransformHelper.FindDeepChild(this._driver.Collider, "loc_rod_left");
			this._mouseController.TakeControll(this._driver.CameraController, this);
			this.IsAnchored = false;
			this.autoAnchorTimestamp = null;
			this.PlaySound(BoatController<TStateEnum, TSignalEnum, TCamera, TServerSettings>.KayakSound.BOARDING, this._settings.transform.position);
		}

		protected virtual void ReleaseControll()
		{
			GameFactory.Player.TargetFov = 60f;
			if (this._driver != null)
			{
				this.PlaySound(BoatController<TStateEnum, TSignalEnum, TCamera, TServerSettings>.KayakSound.UNBOARDING, this._settings.transform.position);
				GameActionAdapter.Instance.UnBoard(this.Category);
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsSailing, false);
				this._driver.Update3dCharMecanimParameter(TPMMecanimBParameter.IsBoatFishing, false);
				this._driver.ResetZoneEnabler = true;
				Vector3 position = this._driver.Collider.position;
				Quaternion rotation = this._driver.Collider.rotation;
				this._driver.Collider.parent.rotation = Quaternion.identity;
				if (!this._isHiddenLeave)
				{
					this._driver.Collider.rotation = rotation;
					this._driver.MoveFromBoat(position, this._unboardingPosition.Value);
				}
				else
				{
					Transform lastPin = this._driver.LastPin;
					this._driver.Collider.rotation = lastPin.rotation;
					this._driver.Collider.position = lastPin.position;
					this._driver.OnMoveFromBoatFinished();
				}
				this._driver = null;
				this._mouseController.ReleaseControll();
				this.StopBoat();
				this.IsAnchored = true;
			}
			this._isHiddenLeave = false;
		}

		public virtual bool HiddenLeave()
		{
			this.ReleaseAnchor();
			GameActionAdapter.Instance.RestoreBoatPosition();
			Transform spawnPos = GameFactory.BoatDock.GetSpawnSettings(this._serverSettings.ItemSubType).spawnPos;
			bool flag = (spawnPos.position - this.Phyboat.Position).magnitude > 1f;
			if (flag)
			{
				GameFactory.Message.ShowBoatsMoved();
			}
			this.Teleport(new Vector3(spawnPos.position.x, -this.Phyboat.waterlineY, spawnPos.position.z), spawnPos.rotation);
			this.SetAnchor();
			if (this._driver != null)
			{
				this._isHiddenLeave = true;
				this._playerFsm.SendSignal(this.HiddenUnboardingSignal);
			}
			return flag;
		}

		protected virtual bool CouldShowFishingPrompt
		{
			get
			{
				return true;
			}
		}

		protected void UpdateFishingPrompt()
		{
			if (!this._wasFishingPromptUsed && this.CouldShowFishingPrompt && this._enterFishingPromptFrom > 0f)
			{
				if (this._enterFishingPromptFrom < Time.time)
				{
					this._enterFishingPromptFrom = -1f;
					this._wasFishingPromptUsed = true;
					this._enterFishingPromptTill = Time.time + 3f;
				}
				else if (!Mathf.Approximately(this.Vertical, 0f) || !Mathf.Approximately(this.Horizontal, 0f))
				{
					this._enterFishingPromptFrom = Time.time + 5f;
				}
			}
			if (this._enterFishingPromptTill < Time.time)
			{
				this._enterFishingPromptTill = -1f;
			}
		}

		protected void UpdateDrivingPrompt()
		{
			if (!this._wasDrivingPromptUsed && this._enterDrivingPromptFrom > 0f && this._enterDrivingPromptFrom < Time.time)
			{
				this._enterDrivingPromptFrom = -1f;
				this._wasDrivingPromptUsed = true;
				this._enterDrivingPromptTill = Time.time + 3f;
			}
			if (this._enterDrivingPromptTill > 0f && this._enterDrivingPromptTill < Time.time)
			{
				this._enterDrivingPromptTill = -1f;
			}
		}

		protected void UpdateTrollingPrompts()
		{
			if (this._driver.IsTrollingRodThrown && !this._wasPutForTrollingPromptUsed)
			{
				this._wasPutForTrollingPromptUsed = true;
				this._putForTrollingPromptTill = Time.time + 3f;
			}
			if (this._putForTrollingPromptTill > 0f && this._putForTrollingPromptTill < Time.time)
			{
				this._putForTrollingPromptTill = -1f;
			}
			if (this._driver.IsPutTrollingRod && !this._wasTakeTrollingPromptUsed)
			{
				this._wasTakeTrollingPromptUsed = true;
				this._takeTrollingPromptTill = Time.time + 3f;
			}
			if (this._takeTrollingPromptTill > 0f && this._takeTrollingPromptTill < Time.time)
			{
				this._takeTrollingPromptTill = -1f;
			}
		}

		protected virtual void StopBoat()
		{
			this.Phyboat.CollisionPlanesCount = 0;
			if (this._boatCollider != null && this._boatCollider.CurrentCollisions != null)
			{
				this._boatCollider.CurrentCollisions.Clear();
			}
			this.Phyboat.StopMass();
			this.Phyboat.Reset();
		}

		protected void SetAnchor()
		{
			this.StopBoat();
			this.setAnchorCalled = true;
		}

		protected virtual void addAnchorLate()
		{
			this.setAnchorCalled = false;
			if (this.anchorMassFront == null)
			{
				Vector3 vector = this._settings.AnchorFrontPivot.position + this.Transform.forward;
				vector.y = this._settings.AnchorDepth;
				Vector3 vector2 = this._settings.AnchorBackPivot.position - this.Transform.forward;
				vector2.y = this._settings.AnchorDepth;
				this.anchorMassFront = new Mass(this.boatSim, 0f, vector, Mass.MassType.Unknown)
				{
					IsKinematic = true
				};
				this.anchorMassBack = new Mass(this.boatSim, 0f, vector2, Mass.MassType.Unknown)
				{
					IsKinematic = true
				};
				this.boatSim.Masses.Add(this.anchorMassFront);
				this.boatSim.Masses.Add(this.anchorMassBack);
				this.anchorSpringFront = new MassToRigidBodySpring(this.anchorMassFront, this.Phyboat, 0.0001f, (this._settings.AnchorFrontPivot.position - vector).magnitude * 1.05f, 0.001f, this._settings.Barycenter.InverseTransformPoint(this._settings.AnchorFrontPivot.position));
				this.boatSim.Connections.Add(this.anchorSpringFront);
				this.anchorSpringBack = new MassToRigidBodySpring(this.anchorMassBack, this.Phyboat, 0.0001f, (this._settings.AnchorBackPivot.position - vector2).magnitude * 1.05f, 0.001f, this._settings.Barycenter.InverseTransformPoint(this._settings.AnchorBackPivot.position));
				this.boatSim.Connections.Add(this.anchorSpringBack);
				this.boatSim.RefreshObjectArrays(true);
				this.PriorVelocityLimit = this.Phyboat.CurrentVelocityLimit;
				this.Phyboat.CurrentVelocityLimit = 0.5f;
			}
		}

		protected void ReleaseAnchor()
		{
			this.setAnchorCalled = false;
			if (this.anchorMassFront != null)
			{
				this.boatSim.RemoveMass(this.anchorMassFront);
				this.boatSim.RemoveConnection(this.anchorSpringFront);
				this.boatSim.RemoveMass(this.anchorMassBack);
				this.boatSim.RemoveConnection(this.anchorSpringBack);
				this.boatSim.RefreshObjectArrays(true);
				this.anchorMassFront = null;
				this.anchorSpringFront = null;
				this.anchorMassBack = null;
				this.anchorSpringBack = null;
				this.Phyboat.CurrentVelocityLimit = 50f;
			}
		}

		public void RecalcCargoInertia()
		{
			Vector4f vector4f = this.Phyboat.VolumeBody.Barycenter.Negative();
			this.Phyboat.VolumeBody.UpdateCargo();
			vector4f += this.Phyboat.VolumeBody.Barycenter;
			this.Phyboat.VolumeBody.Translate(vector4f.Negative());
			this.Phyboat.Position4f += vector4f;
			this.Phyboat.MassValue = this.Phyboat.VolumeBody.MassValue;
			this.Phyboat.InertiaTensor = this.Phyboat.VolumeBody.InertiaTensor;
			if (this.boatSim.PhyActionsListener != null)
			{
				this.boatSim.PhyActionsListener.BoatInertiaChanged(this.Phyboat.UID);
			}
		}

		private void UpdateUnboardingPosition()
		{
			if ((this._unboardingFramesCounter += 1) <= 10)
			{
				return;
			}
			this._unboardingFramesCounter = 0;
			Vector3 position = this._driver.CameraController.Camera.transform.position;
			Vector3 vector = Math3d.ProjectOXZ(this._driver.CameraController.Camera.transform.forward);
			RaycastHit[] array = Physics.RaycastAll(position, vector, 10000f, GlobalConsts.WallsMask);
			if (array.Length > 0)
			{
				List<RaycastHit> list = array.ToList<RaycastHit>();
				list.Sort((RaycastHit v1, RaycastHit v2) => v1.distance.CompareTo(v2.distance));
				int i = 0;
				while (i < list.Count - 1)
				{
					if (Mathf.Approximately(list[i + 1].distance, list[i].distance))
					{
						list.RemoveAt(i + 1);
					}
					else
					{
						i++;
					}
				}
				float num = 0.6f;
				if (list.Count % 2 == 0)
				{
					RaycastHit raycastHit = list[0];
					RaycastHit raycastHit2 = list[1];
					if (raycastHit.distance <= 5f && raycastHit2.distance - raycastHit.distance >= 1.2f)
					{
						float num2 = num;
						if (raycastHit.distance + num <= 3f)
						{
							float num3 = Mathf.Min(raycastHit2.distance - num, 3f);
							num2 = num3 - raycastHit.distance;
						}
						this.SetUnboardingPosition(raycastHit.point, vector, raycastHit2.point, num2);
						return;
					}
				}
				else
				{
					RaycastHit raycastHit3 = list[0];
					float num4 = raycastHit3.distance - num;
					if (num4 > 3f)
					{
						this.SetUnboardingPosition(position, vector, raycastHit3.point, 3f);
						return;
					}
					if (num4 >= 0f)
					{
						this.SetUnboardingPosition(position, vector, raycastHit3.point, num4);
						return;
					}
					RaycastHit maskedRayHit = Math3d.GetMaskedRayHit(position, position - 5f * vector, GlobalConsts.WallsMask);
					if (!(maskedRayHit.collider != null))
					{
						this.SetUnboardingPosition(position, -vector, raycastHit3.point, 3f - num);
						return;
					}
					if ((maskedRayHit.point - raycastHit3.point).magnitude >= num)
					{
						this.SetUnboardingPosition(maskedRayHit.point, vector, raycastHit3.point, num);
						return;
					}
				}
			}
			this._unboardingPosition = null;
		}

		private void SetUnboardingPosition(Vector3 from, Vector3 dir, Vector3 second, float dist)
		{
			Vector3 vector = from + dist * dir;
			Vector3? groundCollision = Math3d.GetGroundCollision(vector);
			if (groundCollision != null && groundCollision.Value.y > -0.5f && groundCollision.Value.y - vector.y < 1f)
			{
				this._unboardingPosition = ((groundCollision == null) ? null : new Vector3?(groundCollision.GetValueOrDefault() + new Vector3(0f, 1.7f, 0f)));
			}
		}

		[Conditional("BOAT_UNBOARDING")]
		private void UpdateGizmos(Vector3 first, Vector3 second, Vector3 player)
		{
		}

		protected void ToFishingPrompt()
		{
			ShowHudElements.Instance.ShowBoatIdlePrompt(this.IsAnchored);
		}

		protected abstract void ToDrivingPrompt();

		public void Update()
		{
			if (this.Boarded)
			{
				this.UpdateUnboardingPosition();
			}
			if (ShowHudElements.Instance.CurrentState != ShowHudStates.HideAll && this._driver != null)
			{
				if (this.IsEnterFishingPromtActive)
				{
					this.ToFishingPrompt();
				}
				else if (this.IsBackToNavigationPromptActive)
				{
					this.ToDrivingPrompt();
				}
				else if (this._putForTrollingPromptTill > 0f)
				{
					ShowHudElements.Instance.PlaceTackleOnPodHint();
				}
				else if (this._takeTrollingPromptTill > 0f)
				{
					ShowHudElements.Instance.TakeTackleFromPodHint();
				}
				else if (this._exitZoneCounter > 0)
				{
					ShowHudElements.Instance.ShowBoatExitZone();
				}
				else if (this.IsCanLeaveBoat)
				{
					ShowHudElements.Instance.ShowBoatUnBoarding();
				}
				else if (this.IsTakeAnchorPromptActive)
				{
					ShowHudElements.Instance.ShowTakeAnchorPrompt();
				}
			}
			this.UpdateInput();
			if (this._boatCollider != null)
			{
				this.Phyboat.UpdateCollisions(this._boatCollider.CurrentCollisions, GlobalConsts.BoatMask);
			}
			this.UpdateFSM();
			BoatShaderParametersController.SetBoatParameters(this._settings.MaskType, this.Transform);
		}

		public virtual void LateUpdate()
		{
			this.UpdatePhysics();
			this.UpdateRodAppliedForce();
			this.boatSimThread.SyncMain();
			if (!this.freezePhySync)
			{
				this.Transform.rotation = this.Phyboat.Rotation;
				this.Transform.position = this.Phyboat.Position - this.Phyboat.Rotation * this._settings.Barycenter.localPosition;
			}
			else
			{
				this.freezePhySync = false;
			}
			if (this._driver != null)
			{
				this._driver.Collider.parent.rotation = this.DriverPivot.rotation;
				if (this._settings.AnglerToDriverToCurve != null)
				{
					float num = Mathf.Lerp(this.AnglerPivot.position.y, this.DriverPivot.position.y, this._mouseController.CameraLocalPositionPrc);
					Vector2 vector = this._settings.AnglerToDriverToCurve.Evaluate(this._mouseController.CameraLocalPositionPrc);
					float num2 = Mathf.Lerp(this.AnglerPivot.position.x, this.DriverPivot.position.x, vector.x);
					float num3 = Mathf.Lerp(this.AnglerPivot.position.z, this.DriverPivot.position.z, vector.y);
					this._driver.Collider.position = new Vector3(num2, num, num3);
				}
				else
				{
					this._driver.Collider.position = Vector3.Lerp(this.AnglerPivot.position, this.DriverPivot.position, this._mouseController.CameraLocalPositionPrc);
				}
				float num4 = Mathf.Clamp01(this.BoatVelocity / 20f);
				if (this.IsAffectFOV)
				{
					GameFactory.Player.TargetFov = Mathf.Lerp(60f, 70f, num4);
				}
			}
			if (this.BoatVelocity > 0f)
			{
				if (this._disturbsCounter == 0)
				{
					this._disturbsCounter = this._settings.TicksBetweenDisturbs;
					for (int i = 0; i < this._settings.WaterDisturbers.Length; i++)
					{
						float num5 = Mathf.Clamp01(this.BoatVelocity / this._settings.DisturbanceMaxForceAtSpeed);
						GameFactory.Water.AddWaterDisturb(this._settings.WaterDisturbers[i].position, Mathf.Lerp(0f, this._settings.DisturbanceRadius, num5 * 2f), Mathf.Lerp(0f, (float)((byte)this._settings.DisturbanceMaxForce), num5));
					}
				}
				else
				{
					this._disturbsCounter -= 1;
				}
			}
			if (this._oarWaterDisturber != null && this._settings.paddle.gameObject.activeInHierarchy)
			{
				this._oarWaterDisturber.Update();
			}
			if (this.setAnchorCalled)
			{
				this.addAnchorLate();
			}
			float? num6 = this.autoAnchorTimestamp;
			if (num6 != null && Time.time >= this.autoAnchorTimestamp.Value)
			{
				this.autoAnchorTimestamp = null;
				this.addAnchorLate();
			}
		}

		protected virtual void UpdatePhysics()
		{
			if (Mathf.Abs(Vector3.Angle(Vector3.up, Math3d.GetUpVector(this.Phyboat.Rotation))) > this._settings.StartRollBackWhenRoll)
			{
				this._playerFsm.SendSignal(this.RollSignal);
			}
			if (this.SimThreadExceptionThrown)
			{
				this.ResetSimulation();
			}
			if (Mathf.Abs(this.Phyboat.Position4f.X) > 100f || Mathf.Abs(this.Phyboat.Position4f.Z) > 100f)
			{
				Vector3 vector;
				vector..ctor(this.Transform.position.x, 0f, this.Transform.position.z);
				this.boatSim.VisualPositionOffset = vector;
				this.boatSimThread.VisualPositionOffsetGlobalChanged(vector);
			}
			if (GameFactory.WaterFlow != null && this.Boarded)
			{
				Vector3 streamSpeed = GameFactory.WaterFlow.GetStreamSpeed(this.Phyboat.Position - Vector3.up * 0.25f);
				this.Phyboat.FlowVelocity = streamSpeed.AsVector4f();
			}
			else
			{
				this.Phyboat.FlowVelocity = Vector4f.Zero;
			}
			this.BoatVelocity = Mathf.Lerp(this.Phyboat.Velocity.magnitude, this.BoatVelocity, Mathf.Exp(-Time.deltaTime * 10f));
			if (!Mono.Simd.Math.Vector3Extension.IsNaN(this.Phyboat.Position) && this.Phyboat.Position.y < 1f)
			{
				this.lastValidBoatPosition = new Vector3?(this.Phyboat.Position);
				this.lastValidBoatRotation = new Quaternion?(this.Phyboat.Rotation);
			}
		}

		protected virtual void UpdateInput()
		{
			if (ControlsController.ControlsActions.InteractObject.WasClicked && this.Boarded && this.IsCanLeaveBoat && this._playerFsm.SendSignal(this.UnboardingSignal))
			{
				this._unboarding = true;
			}
			if (this.Boarded && !GameFactory.GameIsPaused)
			{
				GameActionAdapter.Instance.TravelByBoat();
			}
		}

		protected virtual void UpdateFSM()
		{
			this._playerFsm.Update();
			if (this._curAnimationState == null)
			{
				return;
			}
			if ((this._curAnimationState.speed > 0f && this._curAnimationState.time >= this._curAnimationState.length) || (this._curAnimationState.speed < 0f && this._curAnimationState.time <= 0f))
			{
				this.FinishTransition();
			}
		}

		protected void UpdateRodAppliedForce()
		{
			if (this.Boarded && GameFactory.Player.Rod != null && GameFactory.Player.Rod.Segment.firstTransform != null && GameFactory.Player.Rod.RodObject != null && GameFactory.Player.Rod.Tackle.IsInWater && !GameFactory.Player.Rod.Tackle.IsShowing && Time.time > this.ignoreAppliedRodForceTimestamp)
			{
				Vector3 position = GameFactory.Player.Rod.RodObject.TipMass.Position;
				position.y *= 0.1f;
				Vector3 vector = GameFactory.Player.RodSlot.Sim.RodAppliedForce;
				this.rodForcePoint.LocalPosition = this.Phyboat.WorldToLocal(position);
				float magnitude = vector.magnitude;
				if (magnitude >= 20f && magnitude < 30f)
				{
					vector *= (magnitude - 20f) / 10f;
				}
				else if (magnitude < 20f)
				{
					vector = Vector3.zero;
				}
				this.rodForcePoint.Motor = -vector * this.rodAppliedForceFactor;
				float num = Vector3.Dot(this.Transform.forward, Vector3.Cross(this.Transform.up, Vector3.up));
				this.rodCounterForce = Mathf.Lerp(num, this.rodCounterForce, Mathf.Exp(-Time.deltaTime * 0.5f));
				this.rodCounterForcePoint.LocalPosition = Vector3.left * this.rodCounterForce * 10f;
				this.rodCounterForcePoint.Motor = Vector3.down * 9.81f * this.Phyboat.MassValue * 0.15f * Mathf.Abs(this.rodCounterForce);
				this.rodAppliedForceFactor = Mathf.Lerp(1f, this.rodAppliedForceFactor, Mathf.Exp(-Time.deltaTime * 0.2f));
			}
			else
			{
				this.rodForcePoint.Motor = Vector3.zero;
				this.rodCounterForcePoint.LocalPosition = Vector3.zero;
				this.rodCounterForcePoint.Motor = Vector3.zero;
			}
		}

		public void DampenRodForce()
		{
			this.rodAppliedForceFactor = 0f;
			this.ignoreAppliedRodForceTimestamp = Time.time + 0.5f;
		}

		protected void RecoveryBoat()
		{
			BlackScreenHandler.Hide();
			this.Phyboat.Rotation = Quaternion.Euler(0f, this.Phyboat.Rotation.eulerAngles.y, 0f);
			this.Phyboat.StopMass();
			this.rodAppliedForceFactor = 0f;
		}

		protected float TransitAnimationPrc
		{
			get
			{
				return (!(this._curAnimationState != null)) ? 0f : (this._curAnimationState.time / this._curAnimationState.length);
			}
		}

		protected bool InitTransitionAnimation(AnimationState animation, TSignalEnum transitionSignal)
		{
			this._curAnimationState = animation;
			this._curAnimationTransition = transitionSignal;
			if (this._curAnimationState == null)
			{
				this.FinishTransition();
				return false;
			}
			return true;
		}

		protected void PlayTransitAnimation(string name, TSignalEnum transitionSignal, float animSpeed = 1f, int animWeight = 1)
		{
			AnimationState animationState = this.PlayAnimation(name, animSpeed, animWeight);
			if (!this.InitTransitionAnimation(animationState, transitionSignal))
			{
				LogHelper.Error("Animation {0} was not found. Start transition immediately", new object[] { name });
			}
		}

		protected abstract AnimationState PlayAnimation(string name, float animSpeed = 1f, int animWeight = 1);

		private void FinishTransition()
		{
			this._curAnimationState = null;
			this._playerFsm.SendSignal(this._curAnimationTransition);
		}

		public void Rent()
		{
			this.State = BoatState.RENTED;
		}

		public void OnRentExpired()
		{
			this.State = BoatState.FOR_RENT;
			if (this._driver != null)
			{
				this.HiddenLeave();
			}
		}

		public virtual void Destroy()
		{
			this.SetServerBoatNavigation(false);
			this.boatSimThread.OnThreadException -= this.OnSimThreadException;
			this.boatSimThread.ForceStop();
			this._hand = null;
			this._driver = null;
			if (this._settings != null && this._settings.gameObject != null)
			{
				if (this._oarWaterDisturber != null)
				{
					this._oarWaterDisturber.OnDestroy();
				}
				Object.Destroy(this._settings.gameObject);
				this._settings = null;
			}
		}

		public void SetBoatInExitZone(bool flag)
		{
			if (flag)
			{
				this._exitZoneCounter++;
			}
			else
			{
				this._exitZoneCounter--;
			}
			if (this._exitZoneCounter < 0)
			{
				LogHelper.Error("Invalid exit zone counter = {0}", new object[] { this._exitZoneCounter });
			}
		}

		public virtual void OnTimeChanged()
		{
		}

		public void OnCloseMap()
		{
			this._playerFsm.SendSignal(this.CloseMapSignal);
		}

		public virtual void DisableSound(bool flag)
		{
		}

		public virtual void OnPlayerEnabled()
		{
		}

		protected virtual void SwitchAnchored()
		{
			this.PlaySound((!this.IsAnchored) ? BoatController<TStateEnum, TSignalEnum, TCamera, TServerSettings>.KayakSound.ANCHOR_DROP : BoatController<TStateEnum, TSignalEnum, TCamera, TServerSettings>.KayakSound.ANCHOR_LIFT, this._settings.AnchorFrontPivot.position);
			this.IsAnchored = !this.IsAnchored;
			if (this.IsAnchored)
			{
				StaticUserData.ChatController.AddMessage(ScriptLocalization.Get("AnchorDropped"), MessageChatType.System, true);
			}
			else
			{
				StaticUserData.ChatController.AddMessage(ScriptLocalization.Get("AnchorWeighed"), MessageChatType.System, true);
			}
			this.SetServerIsAnchored(this.IsAnchored);
		}

		protected virtual void SetServerIsAnchored(bool anchored)
		{
			if (!StaticUserData.IS_IN_TUTORIAL)
			{
				PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.AnchorDown, anchored, null);
			}
		}

		protected virtual void PlaySound(BoatController<TStateEnum, TSignalEnum, TCamera, TServerSettings>.KayakSound sound, Vector3 position)
		{
			string[] array = this._sounds[(int)sound];
			string text = array[Random.Range(0, array.Length)];
			RandomSounds.PlaySoundAtPoint(text, position, 0.5f, false);
		}

		public static float IncreaseReverseThrustFactor = 2f;

		protected const float FISHING_PROMPT_DELAY = 5f;

		protected const float FISHING_PROMPT_DURATION = 3f;

		protected const float DRIVING_PROMPT_DELAY = 5f;

		protected const float DRIVING_PROMPT_DURATION = 3f;

		protected const float PROMPT_DURATION = 3f;

		protected const float ANIM_BLEND_TIME = 0.5f;

		protected const float STOP_ROWING_DELAY = 0.3f;

		protected const float AUTO_ANCHOR_DELAY = 1f;

		protected const float THROTTLE_LOCK_DURATION = 0.25f;

		protected const float WaterFlowProbeDepth = 0.25f;

		protected const float AncoredVelocityLimit = 0.5f;

		protected float PriorVelocityLimit;

		protected const float UnboardingMinY = -0.5f;

		protected const float UnboardingMaxYLift = 1f;

		protected const float SOUND_SIZE = 0.5f;

		private const float _maxFOVAtSpeed = 20f;

		private const float _minFOV = 60f;

		private const float _maxFOV = 70f;

		protected static bool Rowing;

		protected bool _wasFishingPromptUsed;

		protected bool _wasDrivingPromptUsed;

		protected bool _wasPutForTrollingPromptUsed;

		protected bool _wasTakeTrollingPromptUsed;

		protected float _enterFishingPromptFrom = -1f;

		protected float _enterFishingPromptTill = -1f;

		protected float _enterDrivingPromptFrom = -1f;

		protected float _enterDrivingPromptTill = -1f;

		protected float _putForTrollingPromptTill = -1f;

		protected float _takeTrollingPromptTill = -1f;

		protected float _acceptRollBackFrom = -1f;

		private bool _unboarding;

		private Collider _azCollider;

		protected TCamera _mouseController;

		protected BoatSettings _settings;

		protected PointOnRigidBody[] constraints;

		protected PointOnRigidBody rodForcePoint;

		protected PointOnRigidBody rodCounterForcePoint;

		protected PointOnRigidBody _bowForcePoint;

		protected PointOnRigidBody _sternForcePoint;

		protected float rodAppliedForceFactor;

		protected float ignoreAppliedRodForceTimestamp;

		protected float[] _axisInput = new float[2];

		private bool _isFishing;

		protected FishingRodSimulation boatSim;

		protected SimulationThread boatSimThread;

		protected Vector3? _unboardingPosition;

		protected bool _isRodPodTransition;

		protected sbyte _transitionToFishingSlot = -1;

		private AnimationState _curAnimationState;

		private TSignalEnum _curAnimationTransition;

		private Mass anchorMassFront;

		private Mass anchorMassBack;

		private MassToRigidBodySpring anchorSpringFront;

		private MassToRigidBodySpring anchorSpringBack;

		private int _exitZoneCounter;

		private byte _disturbsCounter;

		private OarWaterDisturber _oarWaterDisturber;

		protected float rodCounterForce;

		protected bool freezePhySync;

		protected PlayerController _driver;

		protected Transform _hand;

		protected Transform _hand2;

		protected TFSM<TStateEnum, TSignalEnum> _playerFsm;

		protected FakeLegsController _fakeLegs;

		protected BoatCollider _boatCollider;

		protected MeshCollider _boatMeshCollider;

		protected TServerSettings _serverSettings;

		protected ushort _factoryID;

		protected const float CollisionBounceFactor = 0.001f;

		protected const float CollisionFrictionFactor = 0.01f;

		protected const float CounterWeightReactionFactor = 0.5f;

		protected const float CounterWeightGrossWeightRatio = 0.15f;

		protected const float AppliedForceThresholdLow = 20f;

		protected const float AppliedForceThresholdHigh = 30f;

		protected const float BoatRecenterRadius = 100f;

		protected RodPodController _rodSlots;

		private bool _isHiddenLeave;

		private float? autoAnchorTimestamp;

		protected string[][] _sounds = new string[][]
		{
			new string[] { "Sounds/Actions/Kayak/sfx_boat_anchor_drop" },
			new string[] { "Sounds/Actions/Kayak/sfx_boat_anchor_lift" },
			new string[] { "Sounds/Actions/Kayak/sfx_boat_boarding" },
			new string[] { "Sounds/Actions/Kayak/sfx_boat_leave" }
		};

		private bool SimThreadExceptionThrown;

		private Vector3? lastValidBoatPosition;

		private Quaternion? lastValidBoatRotation;

		private List<Renderer> _renderers;

		private bool setAnchorCalled;

		private const float UNDOARDING_MAX_CAST_DISTANCE = 10000f;

		private const byte UNBOARDING_FRAMES_TO_CAST = 10;

		private byte _unboardingFramesCounter;

		protected enum KayakSound : byte
		{
			ANCHOR_DROP,
			ANCHOR_LIFT,
			BOARDING,
			UNBOARDING
		}
	}
}
