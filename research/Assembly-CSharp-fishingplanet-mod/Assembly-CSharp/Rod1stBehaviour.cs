using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mono.Simd.Math;
using ObjectModel;
using ObjectModel.Common;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class Rod1stBehaviour : RodBehaviour
{
	public Rod1stBehaviour(RodController controller, AssembledRod rodAssembly, GameFactory.RodSlot slot)
		: base(controller, rodAssembly, slot)
	{
		Rod rod = rodAssembly.Rod;
		if (rod.Length != null)
		{
			base.Length = (float)rod.Length.Value;
		}
		if (rod.Weight != null)
		{
			base.Segment.weight = Mathf.Clamp((float)rod.Weight.Value, 0.1f, 0.5f);
		}
		base.Segment.curveTest = rod.CurveTest;
		base.Segment.progressive = rod.Progressive;
		this.IsTelescopic = rod.ItemSubType == ItemSubTypes.TelescopicRod;
		CapsuleCollider capsuleCollider = base.gameObject.GetComponent<CapsuleCollider>();
		if (capsuleCollider == null)
		{
			capsuleCollider = base.gameObject.AddComponent<CapsuleCollider>();
		}
		capsuleCollider.isTrigger = true;
		capsuleCollider.direction = 2;
		capsuleCollider.radius = 0.2f;
		capsuleCollider.height = 2f;
		capsuleCollider.center = new Vector3(0f, 0f, 0.5f);
		capsuleCollider.enabled = false;
		base.gameObject.layer = LayerMask.NameToLayer("Default");
		this.SetOnPod(null, 0);
	}

	public Vector3 TipPosition
	{
		get
		{
			return this.PositionCorrection(base.RodObject.Masses[this._rodPoints.Count - 1], true);
		}
	}

	public bool IsUnEquiped { get; set; }

	public bool IsForced
	{
		get
		{
			return false;
		}
	}

	public override Mass TackleTipMass
	{
		get
		{
			return base.RodSlot.Sim.TackleTipMass;
		}
	}

	public bool IsOverloaded
	{
		get
		{
			return base.AppliedForce >= base.MaxLoad;
		}
	}

	public void SetVisibility(bool flag)
	{
		for (int i = 0; i < this._renderers.Count; i++)
		{
			this._renderers[i].enabled = flag;
		}
	}

	public void SetOnPod(RodPodController rodPod, int rodPodSlot)
	{
		bool flag = rodPod != null;
		this.Player = ((!flag) ? GameFactory.Player : null);
		GameFactory.FishSpawner.FishSetOnPod(base.RodSlot.Index, flag);
		if (base.Line != null)
		{
			(base.Line as Line1stBehaviour).SetOnPod(rodPod, rodPodSlot);
		}
		base.RodSlot.IsInHands = !flag;
		base.RodSlot.Sim.SetOnPod(flag);
		(base.RodSlot.SimThread.InternalSim as FishingRodSimulation).SetOnPod(flag);
		if (rodPod != null)
		{
			base.RodSlot.SimThread.MaxIterationsPerFrame = 150;
		}
		else
		{
			base.RodSlot.SimThread.MaxIterationsPerFrame = 150;
		}
	}

	public PlayerController Player { get; private set; }

	public override float AdjustedAppliedForce
	{
		get
		{
			float num = base.AdjustedAppliedForce;
			if (base.Tackle.IsHitched)
			{
				num *= Mathf.Lerp(0.2f, 1f, this.hitchDamperValue.value);
			}
			else if (this.Player != null && this.Player.IsStriking)
			{
				if (base.Tackle.Fish == null)
				{
					num *= Mathf.Lerp(0.3f, 1f, this.dragStrikeDamperValue.value);
				}
			}
			else if (base.IsFishHooked)
			{
				num *= Mathf.Lerp(0.3f, 1f, this.hookingDamperValue.value);
			}
			if (base.Reel.IsReeling)
			{
				num *= 1f;
			}
			if (this.Player != null && GameFactory.Player.StrikeMovesBack)
			{
				num *= 0.8f;
			}
			return num;
		}
	}

	public override Vector3 CurrentTipPosition
	{
		get
		{
			return base.RodSlot.Sim.RodTipMass.Position;
		}
	}

	public AssembledRod AssembledRod
	{
		get
		{
			return this.rodAssembly;
		}
	}

	public float MaxCastLength
	{
		get
		{
			return Mathf.Min(this.rodCaster.CurrentMaxCastLength, base.Line.AvailableLineLengthOnSpool - base.LineOnRodLength);
		}
	}

	public Vector3 ThrowRotation { get; set; }

	public float CastLength { get; set; }

	public float GetRotationalDamper()
	{
		Vector3 rodForce = this.GetRodForce();
		rodForce.y = 0f;
		rodForce.Normalize();
		float num = Vector3.Angle(this._owner.segment.firstTransform.forward, rodForce);
		base.ReelDamper = base.CalcReelDamper(num);
		num = Quaternion.FromToRotation(this._owner.segment.firstTransform.forward, rodForce).eulerAngles.y;
		if (num > 180f)
		{
			num -= 360f;
		}
		if (num > 90f)
		{
			num = 90f;
		}
		if (num < -90f)
		{
			num = -90f;
		}
		return Mathf.Sin(-num * 0.017453292f);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.RodSlot.Sim != null)
		{
			this.recalcFlag = true;
			this.dataOnTipQ.Enqueue(base.RodSlot.Sim.RodAppliedForce);
			this.dataOnHandQ.Enqueue(base.RodSlot.Sim.HandsAppliedForce);
			float num = base.RodSlot.Sim.RodAppliedForce.magnitude - this._owner.lastForceOnTip.magnitude;
			this.accelOnTipQ.Enqueue(num);
			this.accelOnTipFilter = Mathf.Lerp(num, this.accelOnTipFilter, Mathf.Exp(-Time.deltaTime * 10f));
			num = base.RodSlot.Sim.HandsAppliedForce.magnitude - this._owner.lastForceOnHand.magnitude;
			this.accelOnHandQ.Enqueue(num);
			this._owner.lastForceOnTip = base.RodSlot.Sim.RodAppliedForce;
			this._owner.lastForceOnHand = base.RodSlot.Sim.HandsAppliedForce;
		}
		if (this.dataOnTipQ.Count > 30)
		{
			this.dataOnTipQ.Dequeue();
		}
		if (this.dataOnHandQ.Count > 35)
		{
			this.dataOnHandQ.Dequeue();
		}
		if (this.accelOnTipQ.Count > 7)
		{
			this.accelOnTipQ.Dequeue();
		}
		if (this.accelOnHandQ.Count > 7)
		{
			this.accelOnHandQ.Dequeue();
		}
	}

	public Vector3 GetRodForce()
	{
		if (!this.recalcFlag)
		{
			return this._owner.cachedForceOnTip;
		}
		Vector3 vector;
		vector..ctor(0f, 0f, 0f);
		IEnumerator enumerator = this.dataOnTipQ.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Vector3 vector2 = (Vector3)obj;
				vector += vector2;
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		if (this.dataOnTipQ.Count > 0)
		{
			vector /= (float)this.dataOnTipQ.Count;
		}
		this._owner.cachedForceOnTip = vector;
		return vector;
	}

	public Vector3 GetRodForceHand()
	{
		if (!this.recalcFlag)
		{
			return this._owner.cachedForceOnHand;
		}
		Vector3 vector;
		vector..ctor(0f, 0f, 0f);
		IEnumerator enumerator = this.dataOnHandQ.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				Vector3 vector2 = (Vector3)obj;
				vector += vector2;
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		if (this.dataOnHandQ.Count > 0)
		{
			vector /= (float)this.dataOnHandQ.Count;
		}
		this._owner.cachedForceOnHand = vector;
		return vector;
	}

	public float GetRodAccel()
	{
		if (!this.recalcFlag)
		{
			return this._owner.cachedAccelOnTip;
		}
		float num = 0f;
		IEnumerator enumerator = this.accelOnTipQ.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				float num2 = (float)obj;
				num += num2;
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		if (this.accelOnTipQ.Count > 0)
		{
			num /= (float)this.accelOnTipQ.Count;
		}
		this._owner.cachedAccelOnTip = this.accelOnTipFilter;
		return this.accelOnTipFilter;
	}

	public float GetRodAccelHand()
	{
		if (!this.recalcFlag)
		{
			return this._owner.cachedAccelOnHand;
		}
		float num = 0f;
		IEnumerator enumerator = this.accelOnHandQ.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				float num2 = (float)obj;
				num += num2;
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		if (this.accelOnHandQ.Count > 0)
		{
			num /= (float)this.accelOnHandQ.Count;
		}
		this._owner.cachedAccelOnHand = num;
		return num;
	}

	public override void Start()
	{
		base.Start();
	}

	public bool IsInitialized { get; private set; }

	public bool SimulationValid { get; private set; }

	public void InvalidateSimulation()
	{
		this.SimulationValid = false;
	}

	public override void Init(RodOnPodBehaviour.TransitionData transitionData = null)
	{
		base.Init(transitionData);
		base.Start();
		this.rodAssembly = StaticUserData.RodInHand;
		this.rodCaster = new RodCaster();
		this.UpdateCaster();
		base.Line.MinLineLengthOnPitch = base.ModelLength * 0.85f;
		base.Line.MinLineLengthWithFish = base.ModelLength * 0.85f;
		this.ReinitializeSimulation(transitionData);
		if (base.RodSlot.SimThread != null)
		{
			base.RodSlot.SimThread.Start();
			base.RodSlot.SimThread.OnThreadException += this.OnSimThreadException;
		}
		base.InitProceduralBend(this.PreprocessBendPoints());
		if (transitionData != null && transitionData.spawnedFish != null)
		{
			this._owner.StartCoroutine(GameFactory.FishSpawner.SpawnAttackingFish(transitionData.spawnedFish, base.RodSlot, UserBehaviours.FirstPerson, transitionData));
		}
		this.IsInitialized = true;
	}

	private void UpdateCaster()
	{
		this.rodCaster.CastingForce = 1f;
		this.rodCaster.RodMaxCastLength = this.rodAssembly.Rod.MaxCastLength;
		this.rodCaster.RodCastWeightMin = this.rodAssembly.Rod.CastWeightMin;
		this.rodCaster.RodCastWeightMax = this.rodAssembly.Rod.CastWeightMax;
		this.rodCaster.RodLength = (float)this.rodAssembly.Rod.Length.Value;
		this.rodCaster.LineType = this.rodAssembly.Line.ItemSubType;
		this.rodCaster.LineThickness = this.rodAssembly.Line.Thickness;
		this.rodCaster.ReelFriction = this.rodAssembly.Reel.CastFriction;
		this.rodCaster.ReelCastWeightMin = this.rodAssembly.Reel.CastWeightMin;
		this.rodCaster.ReelBacklashProbability = this.rodAssembly.Reel.BacklashProbability;
		this.rodCaster.TackleWindage = base.Tackle.Windage;
		this.rodCaster.TackleWeight = base.Tackle.TackleMass;
	}

	public void ReinitializeSimulationOnLateUpdate(RodOnPodBehaviour.TransitionData transitionData = null)
	{
		this.doReinitializeSimulationOnLateUpdate = true;
		this.transitionDataBuffer = transitionData;
	}

	public void ReinitializeSimulation(RodOnPodBehaviour.TransitionData transitionData = null)
	{
		this.transitionDataBuffer = null;
		this.doReinitializeSimulationOnLateUpdate = false;
		if (this.groundInitThread != null)
		{
			this.groundInitThread.Join();
		}
		if (base.RodSlot.SimThread != null)
		{
			if (transitionData != null)
			{
				float num = transitionData.tacklePosition.y - 0.1f;
				this.collisionSwitchCounter = (int)Mathf.Pow(2f, 12f) + 250;
			}
			base.RodSlot.SimThread.Reset();
			this.groundInitStage = Rod1stBehaviour.GroundInitStage.Requested;
			this.collisionSwitchCounter = 0;
		}
		base.RodSlot.Sim.Clear();
		Vector3 vector;
		vector..ctor(this._owner.segment.firstTransform.position.x, 0f, this._owner.segment.firstTransform.position.z);
		base.RodSlot.Sim.VisualPositionOffset = vector;
		base.RodSlot.SimThread.VisualPositionOffsetGlobalChanged(vector);
		if (this.RodTransformOverride == null)
		{
			base.RodSlot.Sim.RodRotation = this._owner.segment.firstTransform.rotation;
		}
		else
		{
			base.RodSlot.Sim.RodRotation = this.RodTransformOverride.rotation;
		}
		base.RodSlot.Sim.prevRodRotation = base.RodSlot.Sim.RodRotation;
		base.RodSlot.Sim.AddRod(this._owner.segment, base.RodSlot);
		base.RodObject = base.RodSlot.Sim.Objects.First((PhyObject i) => i.Type == PhyObjectType.Rod) as RodObjectInHands;
		this._rodPoints = new List<Vector3>(base.RodPointsCount);
		for (int k = 0; k < base.RodPointsCount; k++)
		{
			this._rodPoints.Add(Vector3.zero);
		}
		this._tpmRodPoints = new List<Vector3>(base.RodPointsCount);
		for (int j = 0; j < base.RodPointsCount; j++)
		{
			this._tpmRodPoints.Add(Vector3.zero);
		}
		Vector3 vector2;
		Vector3 vector3;
		if (transitionData == null)
		{
			vector2 = this.CurrentTipPosition - Vector3.up * base.Line.MinLineLength;
			vector3 = Vector3.down;
		}
		else
		{
			vector2 = transitionData.tacklePosition;
			vector3 = Vector3.zero;
		}
		Line1stBehaviour line1stBehaviour = base.Line as Line1stBehaviour;
		line1stBehaviour.CreateLine(vector2, 0f);
		base.Tackle.CreateTackle(vector3);
		line1stBehaviour.RefreshObjectsFromSim();
		base.RodSlot.Sim.TurnLimitsOn(false);
		this.SimulationValid = true;
	}

	public void OnSimThreadException()
	{
		this.SimThreadExceptionThrown = true;
	}

	public void ResetSimulation()
	{
		this.SimThreadExceptionThrown = false;
		RodOnPodBehaviour.TransitionData transitionData = new RodOnPodBehaviour.TransitionData();
		Vector3? vector = this.lastValidTacklePosition;
		if (vector != null)
		{
			transitionData.tacklePosition = this.lastValidTacklePosition.Value;
		}
		else
		{
			transitionData.tacklePosition = base.Segment.firstTransform.position;
		}
		Vector3? vector2 = this.lastValidFishPosition;
		if (vector2 != null)
		{
			transitionData.fishPosition = this.lastValidFishPosition.Value;
			transitionData.fishPosition.y = Mathf.Min(0f, transitionData.fishPosition.y);
		}
		else
		{
			transitionData.fishPosition = transitionData.tacklePosition;
		}
		transitionData.fishInitialState = ((base.Tackle.Fish == null) ? null : base.Tackle.Fish.StateInstance);
		this.ReinitializeSimulation(transitionData);
		Fish1stBehaviour fish1stBehaviour = base.Tackle.Fish as Fish1stBehaviour;
		if (fish1stBehaviour != null)
		{
			fish1stBehaviour.ReinitializeSimulation(transitionData.fishPosition);
			fish1stBehaviour.ai.RefreshFishObject(base.Tackle.Fish.FishObject as AbstractFishBody);
			if (transitionData.fishInitialState is FishHooked)
			{
				base.RodSlot.Sim.HookFish(base.Tackle.Fish.FishObject);
			}
		}
	}

	public override void Update()
	{
		base.Update();
		if (base.Tackle != null && base.Tackle.IsHitched)
		{
			if (!this.wasHitched)
			{
				this.hitchDamperValue.value = 0f;
			}
			this.hitchDamperValue.update(Time.deltaTime);
			this.wasHitched = true;
		}
		else if (this.wasHitched)
		{
			this.wasHitched = false;
			this.hitchDamperValue.value = 0f;
		}
		if (base.Tackle != null && base.Tackle.Fish == null)
		{
			if (this.Player != null && this.Player.IsStriking)
			{
				if (!this.wasStriking)
				{
					this.dragStrikeDamperValue.value = 0f;
				}
				this.dragStrikeDamperValue.update(Time.deltaTime);
				this.wasStriking = true;
			}
			else if (this.wasStriking)
			{
				this.wasStriking = false;
				this.dragStrikeDamperValue.value = 0f;
			}
		}
		if (base.IsFishHooked)
		{
			if (!this.wasHooked)
			{
				this.hookingDamperValue.value = 0f;
			}
			this.hookingDamperValue.update(Time.deltaTime);
			this.wasHooked = true;
		}
		else if (this.wasHooked)
		{
			this.wasHooked = false;
			this.hookingDamperValue.value = 0f;
		}
	}

	public override void OnLateUpdate()
	{
		if (this.SimThreadExceptionThrown)
		{
			this.ResetSimulation();
		}
		if (this.doReinitializeSimulationOnLateUpdate)
		{
			this.ReinitializeSimulation(this.transitionDataBuffer);
		}
		if (!this.IsInitialized)
		{
			return;
		}
		if (base.RodSlot.Sim.Objects.Count == 0)
		{
			return;
		}
		FishingRodSimulation sim = base.RodSlot.Sim;
		sim.prevRodPosition = sim.RodPosition;
		sim.prevRodRotation = sim.RodRotation;
		if (this.RodTransformOverride == null)
		{
			sim.RodPosition = this._owner.segment.firstTransform.position;
			sim.RodRotation = this._owner.segment.firstTransform.rotation;
		}
		else
		{
			sim.RodPosition = this.RodTransformOverride.position;
			sim.RodRotation = this.RodTransformOverride.rotation;
		}
		if (base.RodSlot.Sim.RodTransformResetFlag)
		{
			sim.prevRodPosition = sim.RodPosition;
			sim.prevRodRotation = sim.RodRotation;
		}
		if (base.RodSlot.Sim.RodKinematicSplineMovement != null)
		{
			Quaternion quaternion = Quaternion.Slerp(base.RodSlot.Sim.RodRotationDelta, base.RodSlot.Sim.RodRotation * Quaternion.Inverse(base.RodSlot.Sim.prevRodRotation), 0.25f);
			base.RodSlot.Sim.RodKinematicSplineMovement.SetNextPointAndRotation(base.RodSlot.Sim.RodPosition, quaternion * base.RodSlot.Sim.RodRotation);
			base.RodSlot.Sim.RodRotationDelta = quaternion;
			if (base.RodSlot.Sim.RodTransformResetFlag)
			{
				base.RodSlot.Sim.RodKinematicSplineMovement.Stop();
			}
		}
		else
		{
			base.RodSlot.Sim.Masses[0].Position = base.RodSlot.Sim.RodPosition;
			base.RodSlot.Sim.Masses[0].Rotation = base.RodSlot.Sim.RodRotation;
		}
		base.RodSlot.Sim.RodTransformResetFlag = false;
		this.SimulatePhysics();
		this.SyncWithSimRotateOnly();
		Vector3 vector = base.RodObject.RootMass.Rotation * Vector3.forward;
		vector.y = 0f;
		vector.Normalize();
		Vector3 forward = this._owner.segment.firstTransform.forward;
		forward.y = 0f;
		forward.Normalize();
		this.rodActualVisualRotationDelta = Quaternion.FromToRotation(vector, forward);
		if (base.RodSlot.SimThread != null)
		{
			base.RodSlot.SimThread.UpdateProceduralBend(this, base.RodObject.UID, this.Rings, this.FirstRingLocatorsUnbentPositions);
		}
		base.UpdateProceduralBend(this.PreprocessBendPoints());
		this.CalculateAppliedForce();
		if (base.Tackle.IsShowing)
		{
			if (base.Tackle.Fish != null)
			{
				base.Tackle.Fish.SyncWithSim();
			}
			if (base.Tackle.UnderwaterItem != null && base.Tackle.UnderwaterItem.Behaviour != null)
			{
				base.Tackle.UnderwaterItem.Behaviour.LateUpdate();
			}
		}
		base.Tackle.OnLateUpdate();
		base.Line.OnLateUpdate();
		if (base.Bell != null)
		{
			base.Bell.OnLateUpdate();
			base.Bell.SoundUpdate();
		}
		if (this._owner.DebugSnapshotLog && base.RodSlot.SimThread == null)
		{
			base.RodSlot.Sim.DebugSnapshotLog();
			this._owner.DebugSnapshotLog = false;
		}
		if ((base.Tackle.UnderwaterItemAsyncRequest != null || base.Tackle.UnderwaterItemBehaviourIsNotInitialized) && !sim.plannedHorizontalRealignMasses)
		{
			base.Tackle.SpawnLoadedItem();
		}
	}

	public override void CalculateAppliedForce()
	{
		base.CalculateAppliedForce();
		if (base.RodAssembly.IsRodDisassembled || base.Tackle.IsShowing)
		{
			base.AppliedForce = 0f;
			return;
		}
	}

	private void SimulatePhysics()
	{
		if (base.RodSlot.Sim.Objects.Count > 0)
		{
			base.Tackle.OnFsmUpdate();
			this.UpdateHeightMapChunks();
			this.UpdateColliders();
			this.UpdateStreamInfo();
			base.Tackle.phyObject.CheckLayer();
			this.UpdateWindInfo();
			if (base.Tackle.Fish != null && base.Line.SecuredLineLength < base.Line.MinLineLengthWithFish + 5f)
			{
				this.FixPhysics();
			}
			else if (base.Tackle.Fish != null)
			{
				this.FixPhysicsConditionally();
			}
			this.Simulate();
		}
		if (base.RodSlot.Sim.LineTipMass != null)
		{
			Vector3 position = base.RodSlot.Sim.LineTipMass.Position;
			if (position.y > base.RodSlot.Sim.LineTipMass.GroundHeight && !Vector3Extension.IsNaN(position) && !Vector3Extension.IsInfinity(position))
			{
				this.lastValidTacklePosition = new Vector3?(position);
			}
		}
		if (base.Tackle.Fish != null)
		{
			Vector3 mouthPosition = base.Tackle.Fish.MouthPosition;
			if (mouthPosition.y > base.Tackle.Fish.FishObject.Masses[0].GroundHeight && !Vector3Extension.IsNaN(mouthPosition) && Vector3Extension.IsInfinity(mouthPosition))
			{
				this.lastValidFishPosition = new Vector3?(mouthPosition);
			}
		}
	}

	private void FixPhysics()
	{
		if (this.isFixOn)
		{
			return;
		}
		base.RodSlot.Sim.TurnLimitsOn(false);
	}

	private void FixPhysicsConditionally()
	{
		int count = base.RodSlot.Sim.Masses.Count;
		Mass mass = base.RodSlot.Sim.Masses[count - 1];
		Mass mass2 = base.RodSlot.Sim.Masses[count - 2];
		bool flag = mass.IsLimitBreached && mass2.IsLimitBreached;
		if (this.isFixOn)
		{
			if (!flag)
			{
				this.isFixOn = false;
				float currentVelocityLimit = base.RodSlot.Sim.Masses[1].CurrentVelocityLimit;
				bool flag2 = currentVelocityLimit == 50f;
				if (flag2)
				{
					if (this.wasLimitApplied)
					{
						base.RodSlot.Sim.TurnLimitsOn(this.wasLimitWithFish);
					}
					else
					{
						base.RodSlot.Sim.TurnLimitsOff();
					}
				}
			}
		}
		else if (flag)
		{
			float currentVelocityLimit2 = base.RodSlot.Sim.Masses[1].CurrentVelocityLimit;
			this.wasLimitApplied = currentVelocityLimit2 > 0f;
			this.wasLimitWithFish = currentVelocityLimit2 == 100f;
			base.RodSlot.Sim.TurnLimitsOn(false);
			this.isFixOn = true;
		}
		foreach (Mass mass3 in base.RodSlot.Sim.Masses)
		{
			mass3.IsLimitBreached = false;
		}
	}

	private void UpdateHeightMapChunks()
	{
		if (Init3D.ActiveTerrain != null)
		{
			foreach (Mass mass in base.RodSlot.Sim.Masses)
			{
				Math3d.UpdateTerrainHeightChunk(mass.Position, Init3D.ActiveTerrain, mass.heightChunk);
			}
		}
	}

	private void UpdateColliders()
	{
		foreach (Mass mass in base.RodSlot.Sim.Masses)
		{
			if ((mass.sphereOverlapCenter - mass.Position).magnitude > 0.25f)
			{
				int num = Physics.OverlapSphereNonAlloc(mass.Position, 0.5f, this.sphereOverlapBuffer, GlobalConsts.FishMask);
				mass.sphereOverlapCenter = mass.Position;
				for (int i = 0; i < mass.colliders.Length; i++)
				{
					if (i < num)
					{
						Collider collider = this.sphereOverlapBuffer[i];
						if (collider != null)
						{
							int instanceID = collider.gameObject.GetInstanceID();
							bool flag = this.collidersCache.ContainsKey(instanceID);
							if (collider is BoxCollider)
							{
								BoxCollider boxCollider = collider as BoxCollider;
								BoxCollider boxCollider2;
								if (flag)
								{
									boxCollider2 = this.collidersCache[instanceID] as BoxCollider;
									boxCollider2.SyncPosition(boxCollider);
								}
								else
								{
									boxCollider2 = new BoxCollider(boxCollider);
									this.collidersCache[instanceID] = boxCollider2;
								}
								mass.colliders[i] = boxCollider2;
							}
							else if (collider is SphereCollider)
							{
								SphereCollider sphereCollider = collider as SphereCollider;
								SphereCollider sphereCollider2;
								if (flag)
								{
									sphereCollider2 = this.collidersCache[instanceID] as SphereCollider;
									sphereCollider2.SyncPosition(sphereCollider);
								}
								else
								{
									sphereCollider2 = new SphereCollider(sphereCollider);
									this.collidersCache[instanceID] = sphereCollider2;
								}
								mass.colliders[i] = sphereCollider2;
							}
							else if (collider is CapsuleCollider)
							{
								CapsuleCollider capsuleCollider = collider as CapsuleCollider;
								CapsuleCollider capsuleCollider2;
								if (flag)
								{
									capsuleCollider2 = this.collidersCache[instanceID] as CapsuleCollider;
									capsuleCollider2.SyncPosition(capsuleCollider);
								}
								else
								{
									capsuleCollider2 = new CapsuleCollider(capsuleCollider);
									this.collidersCache[instanceID] = capsuleCollider2;
								}
								mass.colliders[i] = capsuleCollider2;
							}
							else if (collider is MeshCollider)
							{
								if (flag)
								{
									mass.colliders[i] = this.collidersCache[instanceID];
								}
								else if (Init3D.meshCollidersCache.ContainsKey(instanceID))
								{
									this.collidersCache[instanceID] = Init3D.meshCollidersCache[instanceID];
								}
							}
						}
					}
					else
					{
						mass.colliders[i] = null;
					}
				}
			}
		}
	}

	private void UpdateStreamInfo()
	{
		foreach (Mass mass in base.RodSlot.Sim.Masses)
		{
			RodBehaviour.UpdateMassEnvironment(mass);
		}
	}

	private void UpdateWindInfo()
	{
	}

	private void Simulate()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		FishingRodSimulation sim = base.RodSlot.Sim;
		sim.ResetTransformCycle();
		if (base.RodSlot.SimThread != null)
		{
			base.RodSlot.SimThread.DebugTrigger = this._owner.SimThreadDebugTrigger || base.RodSlot.SimThread.DebugTrigger;
			this._owner.SimThreadDebugTrigger = false;
			base.RodSlot.SimThread.PrintPhySnapshotLog = this._owner.DebugSnapshotLog;
			base.RodSlot.SimThread.PrintActions = this.SimThreadPrintActions;
			base.RodSlot.SimThread.DisableSyncSim = this.SimThreadDisableSyncSim;
			base.RodSlot.SimThread.DisableStructuralUpdate = this.DisableStructuralUpdate;
			base.RodSlot.SimThread.ApplyMoveAndRotateToRod(this._owner.rootNode.position, this._owner.rootNode.rotation);
			sim.UpdateBeforeSync();
			if (!this.DisableMainSync)
			{
				base.RodSlot.SimThread.SyncMain();
			}
			sim.UpdateAfterSync();
		}
		else
		{
			sim.ApplyMoveAndRotateToRod(this._owner.rootNode.position, this._owner.rootNode.rotation);
			sim.Update(Time.deltaTime);
		}
		if (Time.time > this.freezeRodTipMoveSpeedTimestamp)
		{
			this.UpdateRodTipMoveSpeed(this._owner.segment.firstTransform.position, this._owner.segment.firstTransform.rotation);
		}
		else
		{
			base.RodTipMoveSpeed = 0f;
		}
	}

	public void FreezeRodTipMoveSpeedUpdate(float duration)
	{
		base.RodTipMoveSpeed = 0f;
		this.freezeRodTipMoveSpeedTimestamp = Time.time + duration;
	}

	private void UpdateRodTipMoveSpeed(Vector3 position, Quaternion rotation)
	{
		if (Time.deltaTime == 0f)
		{
			return;
		}
		Vector3 vector = position + rotation * Vector3.forward * base.ModelLength;
		base.RodTipMoveSpeed = Mathf.Lerp(Vector3.Distance(vector, this.priorTipPosition) / Time.deltaTime, base.RodTipMoveSpeed, Mathf.Exp(-Time.deltaTime * 10f));
		Vector3 position2 = base.Tackle.transform.position;
		float num = Vector3.Distance(position2, this.priorTipPosition);
		float num2 = Vector3.Distance(position2, vector);
		if (num2 > num)
		{
			base.TipMoveSpeedFromTackle = (num2 - num) / Time.deltaTime;
		}
		else
		{
			base.TipMoveSpeedFromTackle = 0f;
		}
		this.priorTipPosition = vector;
	}

	private void SyncWithSimRotateOnly()
	{
		float num = Vector3.Angle(this._owner.segment.firstTransform.forward, base.RodSlot.Sim.RodAppliedForce.normalized);
		base.ReelDamper = base.CalcReelDamper(num);
	}

	private void CreatePhysicalMarkers()
	{
		GameObject gameObject = (GameObject)Resources.Load("Tackle/cubeMarker", typeof(GameObject));
		for (int i = 0; i < base.RodObject.Masses.Count; i++)
		{
			Mass mass = base.RodObject.Masses[i];
			GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
			gameObject2.transform.position = mass.Position;
			gameObject2.transform.rotation = this.GetRodBoneRotation(base.RodObject.Masses, i);
			gameObject2.transform.localScale = Vector3.one * 0.01f;
			this.markers.Add(gameObject2);
		}
	}

	private void CreateBendMarkers()
	{
		GameObject gameObject = (GameObject)Resources.Load("Tackle/cubeMarker", typeof(GameObject));
		for (int i = 0; i < this._owner.segment.Nodes.Length; i++)
		{
			Transform transform = this._owner.segment.Nodes[i];
			GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject);
			gameObject2.transform.position = transform.position;
			gameObject2.transform.localScale = Vector3.one * 0.01f;
			this.markers.Add(gameObject2);
		}
	}

	public override Vector3 RootPositionCorrection(Vector3 point, float groundHeight)
	{
		if (this.Player == null)
		{
			return Vector3.zero;
		}
		float num = Mathf.Clamp01(1f - Mathf.Exp(10f * (Mathf.Max(groundHeight, 0f) - point.y)));
		if (base.Tackle != null && base.Tackle.IsLineInHand)
		{
			return this.Player.HandTransform.position - base.Tackle.KinematicHandLineMass.Position;
		}
		return (this._owner.segment.firstTransform.position - base.RodObject.RootMass.Position) * num;
	}

	public override Vector3 LineLocatorCorrection()
	{
		return base.Line.LastRingLinePosition - this.RootRotationCorrect(base.RodObject.TipMass.Position + this.RootPositionCorrection(base.RodObject.RootMass.Position, base.RodObject.RootMass.GroundHeight));
	}

	public override Vector3 RootRotationCorrect(Vector3 point)
	{
		Quaternion quaternion = this.rodActualVisualRotationDelta;
		float num = 0.03f * (point - base.RodObject.RootMass.Position).sqrMagnitude;
		float num2 = Mathf.Clamp01(1f - Mathf.Exp(-10f * point.y));
		return Vector3.Lerp(point, base.RodObject.RootMass.Position + quaternion * (point - base.RodObject.RootMass.Position), Mathf.Clamp01(num2 * Mathf.Exp(-num)));
	}

	public override Vector3 PositionCorrection(Mass m, bool needRotationCorrect = true)
	{
		Vector3 vector = m.Position + this.RootPositionCorrection(m.Position, m.GroundHeight);
		if (needRotationCorrect)
		{
			return this.RootRotationCorrect(vector);
		}
		return vector;
	}

	private List<Vector3> PreprocessBendPoints()
	{
		Vector3 vector = Vector3.zero;
		Vector3 forward = Vector3.forward;
		bool flag = false;
		for (int i = 0; i < this._rodPoints.Count; i++)
		{
			this._rodPoints[i] = base.gameObject.transform.InverseTransformPoint(this.PositionCorrection(base.RodObject.Masses[i], true));
			if (i == 0)
			{
				vector = this._rodPoints[i];
			}
			else if (base.RodObject.Masses[i].IsLying || flag)
			{
				flag = true;
				vector += forward * (base.RodObject.Masses[i].Position - base.RodObject.Masses[i - 1].Position).magnitude;
			}
			else
			{
				vector = this._rodPoints[i];
			}
			this._tpmRodPoints[i] = vector;
		}
		if (base.RodSlot.IsInHands)
		{
			this._owner.PlayerController.UpdateRod(base.transform, this._tpmRodPoints);
		}
		else
		{
			Line1stBehaviour line1stBehaviour = base.Line as Line1stBehaviour;
			GameFactory.Player.UpdateFakeRod(this.RodOnPodTpmId, base.transform, this._rodPoints, line1stBehaviour.ServerMainAndLeaderPoints, (line1stBehaviour.Sinkers.Count <= 0) ? Vector3.zero : line1stBehaviour.Sinkers[0].transform.position, line1stBehaviour.LeaderObj != null, (base.Tackle == null) ? null : base.Tackle.transform, (!(base.Tackle.Hook != null)) ? null : base.Tackle.Hook.transform);
		}
		return this._rodPoints;
	}

	private Quaternion GetRodBoneRotation(IList<Mass> masses, int index)
	{
		Mass mass = masses[index];
		if (index == 0 || index >= masses.Count - 1)
		{
			return mass.Rotation;
		}
		Mass mass2 = masses[index + 1];
		return Quaternion.LookRotation(mass2.Position - mass.Position);
	}

	public override void TriggerHapticPulseOnRod(float motor, float duration)
	{
		if (this.Player == null)
		{
			return;
		}
		if (!SettingsManager.Vibrate)
		{
			return;
		}
		VibrationModule.VibrateTimed(motor, duration);
	}

	private void OnDestroy()
	{
		if (base.RodSlot.SimThread != null)
		{
			base.RodSlot.SimThread.OnThreadException -= this.OnSimThreadException;
		}
	}

	private void OnApplicationQuit()
	{
		if (base.RodSlot.SimThread != null)
		{
			base.RodSlot.SimThread.OnThreadException -= this.OnSimThreadException;
			base.RodSlot.SimThread.ForceStop();
		}
	}

	private const float RodMinMass = 0.1f;

	private const float RodMaxMass = 0.5f;

	private const int MaxGroundRaycastsPerFrame = 10;

	private Quaternion rodActualVisualRotationDelta;

	private float[] MassesNormZ;

	private List<Vector3> _rodPoints;

	private List<Vector3> _tpmRodPoints;

	private AssembledRod rodAssembly;

	private bool recalcFlag;

	private const bool ShowPhysicalMarkers = false;

	private const bool ShowBendMarkers = false;

	internal Queue dataOnTipQ = new Queue();

	internal Queue dataOnHandQ = new Queue();

	internal Queue accelOnTipQ = new Queue();

	private const float accelOnTipFilterTau = 10f;

	private float accelOnTipFilter;

	internal Queue accelOnHandQ = new Queue();

	private RodCaster rodCaster;

	private Vector3 priorTipPosition;

	private Vector3? lastValidTacklePosition;

	private Vector3? lastValidFishPosition;

	private float freezeRodTipMoveSpeedTimestamp;

	private bool doReinitializeSimulationOnLateUpdate;

	private RodOnPodBehaviour.TransitionData transitionDataBuffer;

	private volatile bool SimThreadExceptionThrown;

	private bool wasStriking;

	private readonly ValueChanger dragStrikeDamperValue = new ValueChanger(0f, 1f, 0.75f, null);

	private bool wasHitched;

	private readonly ValueChanger hitchDamperValue = new ValueChanger(0f, 1f, 0.33f, null);

	private bool wasHooked;

	private readonly ValueChanger hookingDamperValue = new ValueChanger(0f, 1f, 0.25f, null);

	private bool isFixOn;

	private bool wasLimitApplied;

	private bool wasLimitWithFish;

	private const int lineGroundUpdatePeriod = 5;

	private int lineGroundUpdatePhase;

	private Dictionary<int, Rod1stBehaviour.GroundCollisionInfo> cachedCollisionInfo = new Dictionary<int, Rod1stBehaviour.GroundCollisionInfo>();

	private volatile int collisionSwitchCounter;

	private volatile Rod1stBehaviour.GroundInitStage groundInitStage;

	private Thread groundInitThread;

	private Collider[] sphereOverlapBuffer = new Collider[6];

	private Dictionary<int, ICollider> collidersCache = new Dictionary<int, ICollider>();

	private const int sphereOverlapMax = 6;

	private const float sphereOverlapRadius = 0.5f;

	public bool FreezeRodPivot;

	public bool SimThreadPrintActions;

	public bool SimThreadDisableSyncSim;

	public bool DisableStructuralUpdate;

	public bool DisableMainSync;

	public Transform RodTransformOverride;

	private List<GameObject> markers = new List<GameObject>();

	private class GroundCollisionInfo
	{
		public Vector3 point;

		public Vector3 normal;
	}

	private enum GroundInitStage
	{
		Requested,
		InitThread,
		InitHeight,
		Done
	}
}
