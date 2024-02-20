using System;
using System.Collections.Generic;
using ObjectModel;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class Fish1stBehaviour : FishBehaviour, IFishController
{
	public Fish1stBehaviour(FishController owner, Fish fishTemplate, GameFactory.RodSlot rodSlot, RodOnPodBehaviour.TransitionData td = null)
		: base(owner, fishTemplate, rodSlot)
	{
		this._fishTemplate = fishTemplate;
		base.Mass = fishTemplate.Weight;
		if (fishTemplate.InstanceId == null)
		{
			throw new DbConfigException("InstanceId is NULL for attacking fish");
		}
		base.InstanceGuid = fishTemplate.InstanceId.Value;
		this.Behavior = FishBehavior.Undefind;
		if (td != null)
		{
			GameFactory.FishSpawner.Replace(this);
			this.initialState = td.fishInitialState;
			this.initialStateType = this.initialState.GetType();
			this.Behavior = td.fishInitialBehavior;
			base.tpmId = td.fishTpmId;
		}
		else
		{
			GameFactory.FishSpawner.Add(this);
			this.initialStateType = typeof(FishSwim);
		}
		if (fishTemplate.Force == null)
		{
			throw new DbConfigException("Force is NULL for attacking fish");
		}
		this.Force = fishTemplate.Force.Value * 9.81f;
		if (fishTemplate.Speed == null)
		{
			throw new DbConfigException("Speed is NULL for attacking fish");
		}
		this.Speed = fishTemplate.Speed.Value;
		float? activity = fishTemplate.Activity;
		this.Activity = ((activity == null) ? 1f : activity.Value);
		float? stamina = fishTemplate.Stamina;
		this.Stamina = ((stamina == null) ? 1f : stamina.Value);
		this.Portrait = fishTemplate.Portrait;
		float? attackLure = fishTemplate.AttackLure;
		base.AttackLure = ((attackLure == null) ? 0.5f : attackLure.Value);
		if (fishTemplate.BiteTime != null)
		{
			this.BiteTime = fishTemplate.BiteTime.Value;
		}
		if (fishTemplate.AttackDelay != null)
		{
			this.PredatorAttackDelay = fishTemplate.AttackDelay.Value;
		}
		if (fishTemplate.HoldDelay != null)
		{
			this.PredatorHoldDelay = fishTemplate.HoldDelay.Value;
		}
		this.Rod = base.RodSlot.Rod as Rod1stBehaviour;
		this.Tackle = base.RodSlot.Tackle;
		this.Tackle.AttackingFish = this;
		this.fishAnimation = this._owner.GetComponent<FishAnimationController>();
		this._renderers = RenderersHelper.GetAllRenderersForObject<Renderer>(base.transform);
		this.SetVisibility(!GameFactory.Is3dViewVisible);
		this.IsOnPod = !base.RodSlot.IsInHands;
		this.Player = ((!this.IsOnPod) ? GameFactory.Player : null);
		this.fishMeshRenderer.enabled = !this.IsOnPod;
	}

	public FishBehavior Behavior
	{
		get
		{
			return this.behavior;
		}
		set
		{
			this.behavior = value;
			if (base.RodSlot.Tackle != null)
			{
				base.RodSlot.Tackle.AttackingFish = null;
			}
		}
	}

	public int SlotId
	{
		get
		{
			return base.RodSlot.Index;
		}
	}

	public float PredatorAttackDelay { get; private set; }

	public float PredatorHoldDelay { get; private set; }

	public Fish FishTemplate
	{
		get
		{
			return this._fishTemplate;
		}
	}

	public PlayerController Player { get; private set; }

	public void SetVisibility(bool flag)
	{
		for (int i = 0; i < this._renderers.Count; i++)
		{
			this._renderers[i].enabled = flag;
		}
		this.fishAnimation.SetVisibility(flag);
	}

	public bool IsOnPod { get; private set; }

	public void SetOnPod(bool onPod)
	{
		this.Player = ((!onPod) ? GameFactory.Player : null);
		this.IsOnPod = onPod;
		this.fishMeshRenderer.enabled = !onPod;
		this.CreateSim(this.DestroySim(true));
		this.ai.RefreshFishObject(this.fishObject);
	}

	public TackleBehaviour Tackle { get; set; }

	public Rod1stBehaviour Rod { get; private set; }

	public Type State
	{
		get
		{
			if (this.fsm != null)
			{
				return this.fsm.CurrentStateType;
			}
			return null;
		}
	}

	public FsmBaseState<IFishController> StateInstance
	{
		get
		{
			return this.fsm.CurrentState;
		}
	}

	public float BiteTime { get; set; }

	public bool IsTasting
	{
		get
		{
			return this.ai.IsMouthMagnetAttracting;
		}
	}

	public float DistanceToTackle
	{
		get
		{
			Transform transform = this.Tackle.Hook.transform;
			float magnitude = (this._owner.mouth.position - transform.position).magnitude;
			float magnitude2 = (this._owner.mouth.position - this._owner.root.position).magnitude;
			float magnitude3 = (transform.position - this._owner.root.position).magnitude;
			if (magnitude3 > magnitude2)
			{
				return magnitude;
			}
			return -magnitude;
		}
	}

	public float RetreatThreshold { get; private set; }

	public bool IsBig
	{
		get
		{
			return base.Mass > 0.906f;
		}
	}

	public bool IsHandsHoldCondition
	{
		get
		{
			return this.Length >= 0.85f;
		}
	}

	public bool IsInWater
	{
		get
		{
			return base.transform.position.y < 0f;
		}
	}

	public bool IsPassive
	{
		get
		{
			return this.ai.IsPassive;
		}
	}

	public bool IsGoingTo
	{
		get
		{
			if (this.ai.CurrentBehaviorDirection == FishBehaviorDirection.To)
			{
				return true;
			}
			Vector3? targetPosition = this.ai.TargetPosition;
			if (targetPosition == null)
			{
				return false;
			}
			float magnitude = (base.Mouth.position - GameFactory.Player.transform.position).magnitude;
			float magnitude2 = (this.ai.TargetPosition.Value - GameFactory.Player.transform.position).magnitude;
			return magnitude > magnitude2;
		}
	}

	public bool IsShowing
	{
		get
		{
			return this.State == typeof(FishShowBig) || this.State == typeof(FishShowSmall);
		}
	}

	public float AppliedForce
	{
		get
		{
			if (!this.isHooked)
			{
				return 0f;
			}
			Mass mass = this.fishObject.Masses[0];
			return this.appliedForceAverager.UpdateAndGet(mass.AvgForce.magnitude);
		}
	}

	public float CurrentForce
	{
		get
		{
			return this.ai.CurrentForce;
		}
	}

	public float CurrentRelativeForce
	{
		get
		{
			return this.ai.CurrentRelativeForce;
		}
		set
		{
			this.ai.CurrentRelativeForce = value;
		}
	}

	public Fish CaughtFish { get; set; }

	public bool isHooked { get; private set; }

	public bool IsFrozen { get; set; }

	public Mass HeadMass
	{
		get
		{
			return this.fishObject.Mouth;
		}
	}

	public Mass TailMass
	{
		get
		{
			return this.fishObject.Root;
		}
	}

	public Vector3 HeadRight
	{
		get
		{
			return (base.FishObject as AbstractFishBody).GetSegmentBendAxis(0);
		}
	}

	public Vector3 MouthPosition
	{
		get
		{
			return base.Mouth.position;
		}
	}

	public Vector3 ThroatPosition
	{
		get
		{
			return (!(base.Throat != null)) ? base.Mouth.position : base.Throat.position;
		}
	}

	public Vector3 GripPosition
	{
		get
		{
			return base.Grip.position;
		}
	}

	public global::FishAi ai { get; private set; }

	public FishAiBehavior FishAIBehaviour
	{
		get
		{
			return (this.ai == null) ? FishAiBehavior.None : this.ai.CurrentBehavior;
		}
	}

	public bool FishAiIsBiteTemplate
	{
		get
		{
			return this.ai != null && this.ai.IsBiteTemplate;
		}
	}

	public float FishAiMaxBiteTime
	{
		get
		{
			return (this.ai == null) ? 0f : this.ai.MaxBiteTime;
		}
	}

	public bool FishAiIsBiting
	{
		get
		{
			return this.ai != null && this.ai.IsBiting;
		}
	}

	public bool FishAiEndOfBite
	{
		get
		{
			return this.ai != null && this.ai.EndOfBite;
		}
	}

	public override void Start()
	{
		base.Start();
		if (this._owner.mouth == null)
		{
			throw new PrefabConfigException("FishController.mouth is null");
		}
		if (this._owner.root == null)
		{
			throw new PrefabConfigException("FishController.root is null");
		}
		if (this._owner.gill == null)
		{
			throw new PrefabConfigException("FishController.gill is null");
		}
		this.RetreatThreshold = (base.transform.position - this._owner.mouth.position).magnitude;
		this.fsm = new Fsm<IFishController>("Fish", this, true);
		this.fsm.RegisterState<FishSwim>();
		this.fsm.RegisterState<FishPredatorSwim>();
		this.fsm.RegisterState<FishPredatorAttack>();
		this.fsm.RegisterState<FishBite>();
		this.fsm.RegisterState<FishSwimAway>();
		this.fsm.RegisterState<FishAttack>();
		this.fsm.RegisterState<FishEscape>();
		this.fsm.RegisterState<FishHooked>();
		this.fsm.RegisterState<FishShowSmall>();
		this.fsm.RegisterState<FishShowBig>();
		this.fsm.RegisterState<FishDestroy>();
		this.ai = new global::FishAi(this.Portrait, this.fishAnimation, base.RodSlot.Sim.TackleTipMass, base.RodSlot)
		{
			Activity = this.Activity,
			Stamina = this.Stamina,
			Force = this.Force,
			Speed = this.Speed,
			Mass = base.Mass,
			RetreatThreshold = this.RetreatThreshold,
			AttackLure = base.AttackLure
		};
		this.waterDisturber = new FishWaterDiturber(base.transform, this._owner.modelSize, true);
		float num = 8f;
		float num2 = 40f;
		this._owner.LocomotionWaveAmp *= Mathf.Clamp((num2 - base.Mass) / (num2 - num), 0.1f, 1f);
		this.CreateSim(null);
		base.initChord();
		Vector3? target = this.Target;
		if (target != null)
		{
			this.ai.TargetPosition = this.Target;
			this.ai.CurrentBehavior = FishAiBehavior.Swim;
		}
		if (this._owner.ShakeMaxBendAngle < 1f)
		{
			float num3 = 0.2f;
			float num4 = 8f;
			float num5 = 0.7f;
			float num6 = 1.45f;
			float num7 = Mathf.Clamp01((base.Mass - num3) / (num4 - num3));
			this._owner.ShakeMaxBendAngle = (num6 - num7 * (num6 - num5)) * this._owner.ShakeMaxBendAngle;
		}
		this.fishObject.SetBendParameters(this._owner.ShakeMaxBendAngle, this._owner.ShakeBendReboundPoint, this._owner.ShakeStiffnessMultiplier);
		this.ai.Start(this.fishObject);
		this.ai.CurrentBehavior = FishAiBehavior.None;
		this.fsm.EnterState(this.initialStateType, this.initialState);
	}

	public override void Update()
	{
		if (GameFactory.FishSpawner.IsGamePaused)
		{
			return;
		}
		if (GameFactory.Player == null || base.RodSlot.Rod == null || base.RodSlot.Sim == null)
		{
			this.Destroy();
			return;
		}
		base.Update();
		this.ai.Speed = this.Speed;
		if (!this.IsShowing && this.Tackle != null)
		{
			this.ai.Update(Time.deltaTime, base.RodSlot.Sim.RodAppliedForce.magnitude, GameFactory.Player.transform.position, this.Tackle.IsBeingPulled);
		}
		this.CreateWaterDisturbance();
		this.FadeIn();
		if (base.RodSlot.Sim.HookedFishObject == this.fishObject)
		{
			base.RodSlot.Sim.IsHorizontalStabilizationOn = !this.ai.IsManeuver && this.isHooked;
		}
		this.UpdateAnimations();
		if (this.furMaterial != null && (this.State == typeof(FishShowBig) || this.State == typeof(FishShowSmall)))
		{
			this.UpdateFurMovement();
		}
		this.fsm.Update();
		this.UpdateBeforeSim();
		Vector3? target = this.Target;
		if (target != null)
		{
			Vector3? initial = this.Initial;
			if (initial != null)
			{
				Vector3? targetPosition = this.ai.TargetPosition;
				if (targetPosition != null && this.ai.IsPathCompleted)
				{
					if (this.ai.TargetPosition.Value == this.Target.Value)
					{
						this.ai.TargetPosition = this.Initial;
						this.ai.CurrentBehavior = FishAiBehavior.Swim;
					}
					else
					{
						this.ai.TargetPosition = new Vector3?(this.Target.Value);
						this.ai.CurrentBehavior = FishAiBehavior.Swim;
					}
				}
			}
		}
	}

	public override void LateUpdate()
	{
		if (this.Tackle != null && !this.Tackle.IsShowing)
		{
			base.LateUpdate();
			this.SyncWithSim();
		}
	}

	public override void OnDisable()
	{
	}

	public void CreateSim(Mass mouthMass = null)
	{
		if (!this.IsOnPod)
		{
			this.fishObject = base.RodSlot.Sim.AddVerletFish(this._owner.mouth.position, this.Length, base.Mass, this._owner.LocomotionWaveAmp, this._owner.LocomotionWaveFreq, this._owner.LocomotionWaveAxis);
			this.fishObject.ComputeWaterDrag(this.Force, this.Speed);
		}
		else
		{
			this.fishObject = new SimpleFishBody(base.RodSlot.Sim, base.Mass, this._owner.mouth.position, (this._owner.mouth.position - this._owner.root.position).normalized, this.Length);
			this.fishObject.ComputeWaterDrag(this.Force, this.Speed);
		}
		if (this.isHooked)
		{
			base.RodSlot.Sim.ReplacedHookedFishObject(this.fishObject);
			if (mouthMass != null)
			{
				Mass mass = this.fishObject.ReplaceFirstMass(mouthMass);
				base.RodSlot.Sim.RemoveMass(mass);
			}
		}
	}

	public void ReinitializeSimulation(Vector3 newPosition)
	{
		this._owner.transform.position += newPosition - this._owner.mouth.position;
		this.CreateSim(null);
	}

	public Mass DestroySim(bool keepMouthMass = false)
	{
		if (this.fishObject == null)
		{
			return null;
		}
		Mass mass = null;
		if (keepMouthMass)
		{
			mass = this.fishObject.Mouth;
			this.fishObject.Masses.Remove(mass);
		}
		VibrationModule.StopVibrate();
		if (base.RodSlot.Sim != null)
		{
			base.RodSlot.Sim.DestroyFish(this.fishObject);
		}
		this.fishObject = null;
		return mass;
	}

	private void UpdateBeforeSim()
	{
		if (this.fishObject == null)
		{
			return;
		}
		if (this.fishObject.Masses[0].Position.y <= this.fishObject.Masses[0].WaterHeight + 0.25f)
		{
			this.fishObject.SetLocomotionWaveMult(1f);
		}
		else
		{
			this.fishObject.SetLocomotionWaveMult(0f);
		}
		this.fishObject.SetBendParameters(base.Owner.ShakeMaxBendAngle, base.Owner.ShakeBendReboundPoint, base.Owner.ShakeStiffnessMultiplier);
		bool flag = this.fishObject.IsLying && this.fishObject.Root.Position.y > -this.fishObject.Edge;
		if (flag)
		{
			Vector3 segmentBendDirection = this.fishObject.GetSegmentBendDirection(0);
			this.fishObject.VisualChordRotation(this.fishObject.ChordRotation + segmentBendDirection.y * 10f);
		}
		else
		{
			this.fishObject.VisualChordRotation(this.fishObject.ChordRotation * Mathf.Exp(-Time.deltaTime * 3f));
		}
		if (this.State != typeof(FishShowSmall) && this.State != typeof(FishShowBig))
		{
			if (this.fishObject.Masses[0].Position.y <= this.fishObject.Masses[0].WaterHeight + 0.25f && !flag)
			{
				this.fishObject.RollStabilizer(Vector3.up, this._owner.RollStabilizerMultiplier);
			}
			else
			{
				this.fishObject.RollStabilizer(Vector3.up, 0f);
			}
		}
		else
		{
			this.fishObject.VisualChordRotation(this.fishObject.ChordRotation * Mathf.Exp(-Time.deltaTime * 3f));
			this.fishObject.SetLocomotionWaveMult(0f);
			this.fishObject.RollStabilizer(Vector3.up, 0f);
		}
		if (this.State == typeof(FishShowSmall) || this.State == typeof(FishShowBig))
		{
			this.fishObject.HookState = AbstractFishBody.HookStateEnum.HookedShowing;
		}
		else
		{
			this.fishObject.HookState = ((!this.isHooked) ? AbstractFishBody.HookStateEnum.NotHooked : AbstractFishBody.HookStateEnum.HookedSwimming);
		}
	}

	public void SyncWithSim()
	{
		if (this.fishObject == null)
		{
			return;
		}
		base.transform.position = this.fishObject.Masses[0].Position;
		if (this.IsOnPod)
		{
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			this.bezierCurve.RightAxis[i] = this.fishObject.GetSegmentRight(i);
		}
		this.bezierCurve.AnchorPoints[0] = base.transform.InverseTransformPoint(this.fishObject.Masses[0].Position);
		this.bezierCurve.AnchorPoints[1] = base.transform.InverseTransformPoint(this.fishObject.Masses[4].Position);
		this.bezierCurve.AnchorPoints[2] = base.transform.InverseTransformPoint(this.fishObject.Masses[8].Position);
		this.bezierCurve.AnchorPoints[3] = base.transform.InverseTransformPoint(this.fishObject.Masses[12].Position);
		this.bezierCurve.AnchorPoints[4] = base.transform.InverseTransformPoint(this.fishObject.Masses[16].Position);
		this.bezierCurve.AnchorPoints[5] = base.transform.InverseTransformPoint((this.fishObject.Masses[17].Position + this.fishObject.Masses[18].Position + this.fishObject.Masses[19].Position) / 3f);
		base.updateTransformableBones();
		if (this.Player != null && this.State == typeof(FishShowBig) && this.Player.Grip != null && this.Player.Grip.IsVisible)
		{
			base.transform.position += GameFactory.Player.Grip.Fish.position - this.GripPosition;
		}
		else if (this.Tackle.IsShowing)
		{
			base.transform.position += this.Rod.PositionCorrection(this.fishObject.Masses[0], this.Tackle.IsShowingComplete) - this.MouthPosition;
		}
		else
		{
			base.transform.position += this.fishObject.Masses[0].Position - this.MouthPosition;
		}
		Vector3 normalized = (this.bezierCurve.AnchorPoints[1] - this.bezierCurve.AnchorPoints[0]).normalized;
		Vector3 normalized2 = (this.bezierCurve.AnchorPoints[5] - this.bezierCurve.AnchorPoints[3]).normalized;
		Vector3 vector = this.bezierCurve.RightAxis[0];
		FishStateBase fishStateBase = this.fsm.CurrentState as FishStateBase;
		GameFactory.Player.UpdateFish(base.tpmId, base.transform.position, normalized, normalized2, vector, fishStateBase.State);
	}

	private void SyncWithSimOld()
	{
		if (this.IsFrozen)
		{
			return;
		}
		if (this.fishObject == null)
		{
			return;
		}
		Mass mass = this.fishObject.Masses[0];
		Mass mass2 = this.fishObject.Masses[1];
		Vector3 vector = mass.Position - mass2.Position;
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, vector);
		float num = Mathf.Abs(base.transform.localScale.y * this._owner.modelSize);
		if (num > 0.8f && this.Tackle is FloatBehaviour && base.transform.position.y < 0f && (this.State == typeof(FishBite) || this.State == typeof(FishSwimAway)))
		{
			quaternion = Quaternion.Euler(0f, quaternion.y, quaternion.z);
		}
		if (base.transform.position.y < 0f)
		{
			this.zRotationValue = this.UpdateRotationWhenShocked();
			base.transform.rotation = Quaternion.Euler(quaternion.eulerAngles.x, quaternion.eulerAngles.y, this.zRotationValue);
		}
		else
		{
			this.zRotationValue = this.UpdateRotationWhenHanging();
			base.transform.rotation = quaternion * Quaternion.Euler(0f, 0f, this.zRotationValue);
		}
		base.transform.position += mass.Position - this._owner.mouth.position;
	}

	public float ZRotationValue
	{
		get
		{
			return this.zRotationValue;
		}
	}

	private float UpdateRotationWhenHanging()
	{
		float num = 1f + 0.2f * Mathf.Abs(Mathfx.Bounce(Mathf.Sin(Time.time)));
		return -180f * (Mathf.Sin(Time.time) * num);
	}

	private float UpdateRotationWhenShocked()
	{
		if (!this.isShocked && this.ai.IsShocked)
		{
			this.zRotationInitial = this.zRotationValue;
			this.zRotationTarget = Mathf.Clamp(this.zRotationTarget, -45f, 45f);
			this.timeValue = 0f;
			this.isShocked = true;
		}
		if (this.isShocked && !this.ai.IsShocked)
		{
			this.zRotationInitial = this.zRotationValue;
			this.zRotationTarget = 0f;
			this.timeValue = 0f;
			this.isShocked = false;
		}
		if (this.zRotationTarget == this.zRotationValue)
		{
			return this.zRotationValue;
		}
		this.timeValue += Time.deltaTime;
		if (this.zRotationTarget == 0f)
		{
			return Mathf.Lerp(this.zRotationInitial, this.zRotationTarget, this.timeValue * 2f);
		}
		return Mathf.Lerp(this.zRotationInitial, this.zRotationTarget, this.timeValue / 5f);
	}

	private void UpdateFurMovement()
	{
		this.furMaterial.SetVector(this.furDisplacementPropertyID, new Vector3(0.1f, 0f, 0f));
	}

	public bool IsPathCompleted
	{
		get
		{
			return this.ai.IsPathCompleted;
		}
	}

	public bool IsAttackDelayed
	{
		get
		{
			return this.ai.IsAttackDelayed;
		}
	}

	public void Attack()
	{
		this.ai.Attack(this.BiteTime, base.RodSlot.Sim.isLureTackle);
	}

	public void Bite()
	{
		this.magnet = base.RodSlot.Sim.AddMagnet(this.fishObject);
		this.ai.Bite(this.magnet, base.RodSlot.Sim.isLureTackle);
	}

	public void Hook(float poolPeriod)
	{
		this.Tackle.AttackingFish = null;
		if (this.magnet != null)
		{
			base.RodSlot.Sim.DestroyMagnet(this.magnet);
			this.magnet = null;
		}
		if (this.isHooked)
		{
			return;
		}
		this.isHooked = true;
		base.RodSlot.Sim.HookFish(this.fishObject);
		this.ai.Hook(poolPeriod);
	}

	public void KeepHookInMouth(float poolPeriod)
	{
		if (this.magnet != null)
		{
			base.RodSlot.Sim.DestroyMagnet(this.magnet);
			this.magnet = null;
		}
		if (this.isHooked)
		{
			return;
		}
		this.isHooked = true;
		base.RodSlot.Sim.HookFish(this.fishObject);
		this.ai.KeepInMouth(poolPeriod);
	}

	public void Show()
	{
		this.fishObject.Collision = global::Phy.Mass.CollisionType.None;
		this.ai.Show();
	}

	public void Escape()
	{
		if (this.magnet != null)
		{
			base.RodSlot.Sim.DestroyMagnet(this.magnet);
			this.magnet = null;
		}
		base.RodSlot.Sim.EscapeFish(this.fishObject);
		this.Behavior = FishBehavior.Go;
		this.isHooked = false;
		this.ai.Escape();
		base.RodSlot.Reel.SetDragMode();
	}

	public void Swim()
	{
		if (this.ai != null)
		{
			this.ai.Swim();
		}
	}

	public void PredatorSwim()
	{
		if (this.ai != null)
		{
			this.ai.PredatorSwim();
		}
	}

	public void PredatorAttack()
	{
		if (this.ai != null)
		{
			this.ai.PredatorAttack();
		}
	}

	public void TurnAiOff()
	{
		this.ai.Stop();
	}

	private void FadeIn()
	{
		if (this.fadeInTime > 2f)
		{
			return;
		}
		this.fadeInTime += Time.deltaTime;
	}

	private void UpdateAnimations()
	{
		float num = this.ai.CurrentRelativeForce * 1.5f;
		float num2 = ((!this.ai.IsFishSwiming) ? 0f : 1f);
		bool isShowing = this.IsShowing;
		if (isShowing)
		{
			num = 1f;
			num2 = 0f;
		}
		if (!this.ai.IsManeuver || isShowing)
		{
			this.fishAnimation.SetValues(num, num2, isShowing, this.ai.IsShocked, this.ai.IsRight, this.ai.IsLeft);
		}
		this.fishAnimation.OnUpdate();
	}

	private void CreateWaterDisturbance()
	{
		if (GameFactory.Water == null)
		{
			return;
		}
		this.waterDisturber.Update();
	}

	public override void Destroy()
	{
		this.ai.Stop();
		this.DestroySim(false);
		if (GameFactory.FishSpawner != null)
		{
			GameFactory.FishSpawner.Remove(this);
		}
		Object.Destroy(base.gameObject);
	}

	private FishBehavior behavior;

	private List<Renderer> _renderers;

	private readonly Fish _fishTemplate;

	private Type initialStateType;

	private FsmBaseState<IFishController> initialState;

	private readonly Averager appliedForceAverager = new Averager(5);

	[HideInInspector]
	public Vector3? Initial;

	[HideInInspector]
	public Vector3? Target;

	private Vector3? priorTailPosition;

	private Vector3? priorTailSpeed;

	private Vector3? priorMouthPosition;

	private const bool ShowMarkers = false;

	private const float HandsHoldFishMass = 5f;

	private const float ChordAutoRotationSpeedFactor = 3f;

	private const float FurDisplacementMovementWeight = 30f;

	private const float FurDisplacementNoiseWeight = 0.7f;

	private const float FurDisplacementGravityWeight = 1f;

	private const float FurDisplacementDampingMultiplier = 5f;

	private Fsm<IFishController> fsm;

	private FishAnimationController fishAnimation;

	private FishWaterDiturber waterDisturber;

	private Magnet magnet;

	private Vector3 furDisplacement;

	private List<GameObject> markers = new List<GameObject>();

	private float zRotationValue;

	private float timeValue;

	private bool isShocked;

	private float zRotationTarget;

	private float zRotationInitial;

	private float fadeInTime;

	private const float FadeInTimeout = 2f;
}
