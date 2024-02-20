using System;
using System.Linq;
using ObjectModel;
using Phy;
using UnityEngine;

public class Float1stBehaviour : FloatBehaviour, IFloatBehaviour, ITackleBehaviour
{
	public Float1stBehaviour(FloatController owner, AssembledRod rodAssembly, RodOnPodBehaviour.TransitionData transitionData = null)
		: base(owner, rodAssembly, transitionData)
	{
		base.UserSetLeaderLength = rodAssembly.Rod.LeaderLength;
		Bobber bobber = rodAssembly.Bobber;
		if (bobber.Weight == null)
		{
			throw new DbConfigException("Bobber weight is null!");
		}
		this.BobberWeight = (float)bobber.Weight.Value;
		base.Resistance = bobber.ResistanceForce;
		base.Buoyancy = bobber.Buoyancy;
		base.SinkerMass = bobber.SinkerMass;
		base.Sensitivity = bobber.Sensitivity;
		this.InitHook(rodAssembly);
		this.throwData = new TackleThrowData
		{
			Windage = base.Windage,
			RodLength = (float)rodAssembly.Rod.Length.Value,
			MaxCastLength = rodAssembly.Rod.MaxCastLength
		};
		this._bobberRenderer = RenderersHelper.GetRenderer(base.transform);
		this._hookRenderer = RenderersHelper.GetRendererForObject(this.hookObject.transform);
		this._baitRenderer = RenderersHelper.GetRendererForObject(this.baitObject.transform);
		if (base.IsUncuttable)
		{
			base.AddUncuttableLeader(base.Hook, base.Hook.lineAnchor.position, Quaternion.Euler(-90f, 0f, 0f));
		}
		this.fsm = new Fsm<IFloatBehaviour>("Float", this, true);
		this.fsm.RegisterState<FloatOnTip>();
		this.fsm.RegisterState<FloatIdlePitch>();
		this.fsm.RegisterState<FloatFlying>();
		this.fsm.RegisterState<FloatPitching>();
		this.fsm.RegisterState<FloatFloating>();
		this.fsm.RegisterState<FloatSwallowed>();
		this.fsm.RegisterState<FloatHanging>();
		this.fsm.RegisterState<FloatShowItem>();
		this.fsm.RegisterState<FloatShowSmallFish>();
		this.fsm.RegisterState<FloatShowBigFish>();
		this.fsm.RegisterState<FloatHitched>();
		this.fsm.RegisterState<FloatBroken>();
		this.fsm.RegisterState<FloatHidden>();
		if (transitionData == null)
		{
			this.initialState = typeof(FloatHidden);
		}
		else
		{
			base.IsAttackFinished = transitionData.tackleAttackFinished;
			base.IsFinishAttackRequested = transitionData.tackleAttackFinishedRequested;
			base.IsHitched = transitionData.tackleInitialState == typeof(FloatHitched);
			this.initialState = transitionData.tackleInitialState;
		}
	}

	public override void Init()
	{
		base.Init();
		this.fsm.EnterState(this.initialState, null);
	}

	public override void SetVisibility(bool isVisible)
	{
		base.SetVisibility(isVisible);
		this._hookRenderer.enabled = isVisible;
		this._baitRenderer.enabled = isVisible;
		this._bobberRenderer.enabled = isVisible;
		if (this._stopperRenderer != null)
		{
			this._stopperRenderer.enabled = isVisible;
		}
		if (base.RodSlot.Line != null)
		{
			base.RodSlot.Line.SetVisibility(isVisible);
		}
	}

	public override void Destroy()
	{
		this.fsm = null;
		base.Hook = null;
		base.Destroy();
	}

	public float BobberWeight { get; protected set; }

	public override float LeaderLength
	{
		set
		{
			base.LeaderLength = value;
			base.RodSlot.Sim.SetLeaderLength(value);
		}
	}

	public override bool IsActive
	{
		get
		{
			return this.fsm.CurrentStateType != typeof(FloatHidden);
		}
	}

	public override bool IsSyncActive
	{
		get
		{
			Type currentStateType = this.fsm.CurrentStateType;
			return currentStateType == typeof(FloatFloating) || currentStateType == typeof(FloatHitched) || currentStateType == typeof(FloatSwallowed) || currentStateType == typeof(FloatHanging);
		}
	}

	public override bool HasEnteredShowingState
	{
		get
		{
			return this.State == typeof(FloatShowSmallFish) || this.State == typeof(FloatShowBigFish) || this.State == typeof(FloatShowItem);
		}
	}

	public SpringDrivenObject FloatObject
	{
		get
		{
			return this.floatObject;
		}
	}

	public override void CreateTackle(Vector3 direction)
	{
		base.transform.localScale = Vector3.one;
		base.TackleType = FishingRodSimulation.TackleType.Float;
		base.RodSlot.Sim.AddBobber((base.Owner.waterMark.position - this._owner.bottomLineAnchor.position).magnitude, (base.Hook.lineAnchor.position - base.Hook.hookAnchor.position).magnitude, this.BobberWeight, base.Buoyancy, base.SinkerMass, (base.Hook.BaitItem.Weight != null) ? ((float)base.Hook.BaitItem.Weight.Value) : 0f, -1f, base.Sensitivity, direction);
		this.RefreshObjectsFromSim();
		base.RodSlot.Line.SinkerMass = base.SinkerMass;
	}

	public void RefreshObjectsFromSim()
	{
		this.floatObject = (SpringDrivenObject)base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Bobber);
		this.leaderObject = (SpringDrivenObject)base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Leader);
		this.leashObject = (SpringDrivenObject)base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Leash);
		base.Hook.HookObject = (SpringDrivenObject)base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Hook);
	}

	public override bool IsOnTip
	{
		get
		{
			return this.fsm.CurrentStateType == typeof(FloatOnTip) || this.fsm.CurrentStateType == typeof(FloatIdlePitch);
		}
	}

	public override bool IsFlying
	{
		get
		{
			return this.fsm.CurrentStateType == typeof(FloatFlying);
		}
	}

	public override bool IsInWater
	{
		get
		{
			return this.fsm.CurrentStateType == typeof(FloatFloating) || this.fsm.CurrentStateType == typeof(FloatSwallowed);
		}
	}

	public bool IsHanging
	{
		get
		{
			return this.fsm.CurrentStateType == typeof(FloatHanging);
		}
	}

	public Vector3 HookLineDirection
	{
		get
		{
			return (base.Hook.lineAnchor.position - this._owner.bottomLineAnchor.position).normalized;
		}
	}

	public override Type State
	{
		get
		{
			return this.fsm.CurrentStateType;
		}
	}

	public override void OnFsmUpdate()
	{
		if (this.fsm == null)
		{
			return;
		}
		this.fsm.Update();
	}

	public override void OnLateUpdate()
	{
		this.SyncWithSim();
		base.Hook.OnLateUpdate();
		if (base.RodSlot.IsInHands)
		{
			this._owner.PlayerController.UpdateTackle(base.transform, base.Hook.transform);
		}
	}

	private void SyncWithSim()
	{
		if (this.floatObject == null)
		{
			return;
		}
		Mass mass = this.floatObject.Masses[0];
		Mass mass2 = this.floatObject.Masses[1];
		base.transform.localScale = FloatBehaviour.GetBobberScale(mass.Position);
		if (base.RodSlot.Rod != null)
		{
			bool flag = base.RodSlot.Tackle.IsShowingComplete || (!base.RodSlot.Tackle.FishIsShowing && !base.RodSlot.Tackle.ItemIsShowing);
			Vector3 vector = base.RodSlot.Rod.PositionCorrection(mass, flag);
			Vector3 vector2 = base.RodSlot.Rod.PositionCorrection(mass2, flag);
			Vector3 normalized = (vector - vector2).normalized;
			if (normalized.sqrMagnitude > 0f)
			{
				base.transform.rotation = Quaternion.FromToRotation(Vector3.up, normalized);
			}
			base.transform.position += vector - base.Owner.waterMark.position;
		}
		this.floatObject.SyncSwivel();
	}

	public override PhyObject phyObject
	{
		get
		{
			return this.floatObject;
		}
	}

	public override bool IsKinematic
	{
		get
		{
			return this.isKinematic;
		}
		set
		{
			this.isKinematic = value;
			this.floatObject.IsKinematic = value;
		}
	}

	public override void Hitch(Vector3 position)
	{
		base.HitchPosition = new Vector3?(position);
		Mass mass = base.Hook.HookObject.Masses[1];
		mass.IsKinematic = true;
		mass.StopMass();
		mass.Position = base.HitchPosition.Value;
	}

	public override void UnHitch()
	{
		Mass mass = base.Hook.HookObject.Masses[1];
		mass.IsKinematic = false;
		mass.StopMass();
		base.HitchPosition = null;
		base.RodSlot.Sim.HorizontalRealignLineMasses();
	}

	public override Vector3 Position
	{
		get
		{
			return base.Hook.transform.position;
		}
	}

	public override bool IsLying
	{
		get
		{
			Mass mass = base.Hook.HookObject.Masses[1];
			return mass != null && mass.IsLying;
		}
	}

	public float DragQuality
	{
		get
		{
			return 0f;
		}
	}

	public override bool IsIdle
	{
		get
		{
			return this.State == typeof(FloatOnTip) || this.State == typeof(FloatIdlePitch);
		}
	}

	public void ResetDrag()
	{
	}

	public Mass GetKinematicLeaderMass(int n = 0)
	{
		int num = this.leaderObject.Masses.Count - 1;
		if (n > 0 && n <= num)
		{
			num -= n;
		}
		Mass mass = this.leaderObject.Masses[num];
		mass.IsKinematic = true;
		mass.StopMass();
		return mass;
	}

	public Mass GetKinematicLeashMass(int n = 0)
	{
		Mass mass;
		if (n < 0)
		{
			mass = this.leashObject.Masses[0];
		}
		else
		{
			int num = this.leashObject.Masses.Count - 1;
			if (n > 0 && n <= num)
			{
				num -= n;
			}
			mass = this.leashObject.Masses[num];
		}
		mass.IsKinematic = true;
		mass.StopMass();
		return mass;
	}

	public Spring GetKinematicLeaderMassSpring()
	{
		return this.leaderObject.Springs[this.leaderObject.Springs.Count - 5];
	}

	protected override void CreateTackleInSound(Vector3 position, float size)
	{
		RandomSounds.PlaySoundAtPoint("Sounds/Actions/Lure/Bobber_Fall", position, size * 1.5f, false);
	}

	public bool CheckPitchIsTooShort()
	{
		this.isPitchTooShort = base.RodSlot.Line.FullLineLength <= base.RodSlot.Line.MinLineLengthWithFish || base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLength;
		if (this.isPitchTooShort)
		{
		}
		return this.isPitchTooShort;
	}

	public override bool IsPitchTooShort
	{
		get
		{
			return this.isPitchTooShort;
		}
		set
		{
			this.isPitchTooShort = value;
		}
	}

	protected override void OnHookEnterWater()
	{
		if (base.Thrown)
		{
			base.Thrown = false;
			base.TackleIn(base.Hook.transform.position, 2f);
		}
	}

	public override bool IsLiveBait
	{
		get
		{
			return base.Hook.BaitItem.IsLive;
		}
	}

	private void InitHook(AssembledRod rodAssembly)
	{
		HookController component = this.hookObject.GetComponent<HookController>();
		if (component == null)
		{
			throw new PrefabConfigException(string.Format("Prefab {0} has no HookController on it", rodAssembly.Hook.Asset));
		}
		if (component.baitAnchor == null)
		{
			throw new PrefabConfigException(string.Format("Prefab {0} has HookController with empty baitAnchor", rodAssembly.Hook.Asset));
		}
		component.Hook = rodAssembly.Hook;
		base.Hook = component;
		component.Tackle = this;
		float hookScaling = FloatBehaviour.GetHookScaling(rodAssembly.Hook.HookSize, component.hookModelSize);
		this.hookObject.transform.localScale *= hookScaling;
		component.Bait = this.baitObject;
		component.BaitItem = rodAssembly.Bait;
		if (this.IsLiveBait)
		{
			this.baitObject.transform.localScale /= hookScaling;
		}
		component.IsBaitShown = rodAssembly.Bait.Count > 0;
		component.Init();
		base.Hook.OnEnterWater += this.OnHookEnterWater;
	}

	public void HookSetFreeze(bool freeze)
	{
		if (freeze)
		{
			base.Hook.Freeze(false);
		}
		else
		{
			base.Hook.Unfreeze();
		}
	}

	public Mass GetHookMass(int index)
	{
		return base.Hook.HookObject.Masses[index];
	}

	public Mass GetBobberMainMass
	{
		get
		{
			return this.phyObject.Masses[1];
		}
	}

	public void SetHookMass(int index, Mass m)
	{
		base.Hook.HookObject.Masses[index] = m;
	}

	public bool HookIsIdle
	{
		get
		{
			return base.Hook.IsIdle;
		}
	}

	public Transform HookTransform
	{
		get
		{
			return base.Hook.transform;
		}
	}

	public Transform BaitTransform
	{
		get
		{
			return base.Hook.Bait.transform;
		}
	}

	public Bait BaitItem
	{
		get
		{
			return base.Hook.BaitItem;
		}
	}

	public override bool IsThrowing
	{
		get
		{
			return this.throwData.IsThrowing;
		}
		set
		{
			this.throwData.IsThrowing = value;
		}
	}

	public override bool IsBaitShown
	{
		get
		{
			return base.Hook.IsBaitShown;
		}
	}

	private Renderer _bobberRenderer;

	private SkinnedMeshRenderer _hookRenderer;

	private SkinnedMeshRenderer _baitRenderer;

	private Renderer _stopperRenderer;

	public const float MinVisibleLeaderLength = 0.3f;

	public const float MaxVisibleLeaderLength = 0.75f;

	private Fsm<IFloatBehaviour> fsm;

	private SpringDrivenObject floatObject;

	private SpringDrivenObject leaderObject;

	private SpringDrivenObject leashObject;

	private bool isKinematic;

	private bool isPitchTooShort;
}
