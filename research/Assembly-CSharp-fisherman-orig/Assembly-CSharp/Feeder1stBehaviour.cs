using System;
using System.Linq;
using ObjectModel;
using Phy;
using UnityEngine;

public class Feeder1stBehaviour : FeederBehaviour
{
	public Feeder1stBehaviour(FeederController owner, AssembledRod rodAssembly, RodOnPodBehaviour.TransitionData transitionData = null)
		: base(owner, rodAssembly, transitionData)
	{
		base.UserSetLeaderLength = rodAssembly.Rod.LeaderLength;
		this.InitHook(rodAssembly);
		this.isBottom = rodAssembly.IsBottom;
		this.throwData = new TackleThrowData
		{
			Windage = base.Windage,
			RodLength = (float)rodAssembly.Rod.Length.Value,
			MaxCastLength = rodAssembly.Rod.MaxCastLength
		};
		if (base.IsUncuttable)
		{
			base.AddUncuttableLeader(base.Hook, base.Hook.lineAnchor.position, Quaternion.Euler(-90f, 0f, 0f));
		}
		this.fsm = new Fsm<IFeederBehaviour>("Feeder", this, true);
		this.fsm.RegisterState<FeederOnTip>();
		this.fsm.RegisterState<FeederIdlePitch>();
		this.fsm.RegisterState<FeederFlying>();
		this.fsm.RegisterState<FeederPitching>();
		this.fsm.RegisterState<FeederFloating>();
		this.fsm.RegisterState<FeederSwallowed>();
		this.fsm.RegisterState<FeederHanging>();
		this.fsm.RegisterState<FeederShowItem>();
		this.fsm.RegisterState<FeederShowSmallFish>();
		this.fsm.RegisterState<FeederShowBigFish>();
		this.fsm.RegisterState<FeederHitched>();
		this.fsm.RegisterState<FeederBroken>();
		this.fsm.RegisterState<FeederHidden>();
		if (transitionData == null)
		{
			this.initialState = typeof(FeederHidden);
		}
		else
		{
			base.IsAttackFinished = transitionData.tackleAttackFinished;
			base.IsFinishAttackRequested = transitionData.tackleAttackFinishedRequested;
			base.IsHitched = transitionData.tackleInitialState == typeof(FeederHitched);
			this.initialState = transitionData.tackleInitialState;
		}
	}

	public new Fish1stBehaviour Fish
	{
		get
		{
			return (Fish1stBehaviour)this.fish;
		}
		set
		{
			this.fish = value;
			if (value != null)
			{
				this.AttackingFish = null;
			}
		}
	}

	public override bool HasEnteredShowingState
	{
		get
		{
			return this.State == typeof(FeederShowSmallFish) || this.State == typeof(FeederShowBigFish) || this.State == typeof(FeederShowItem);
		}
	}

	public new Fish1stBehaviour AttackingFish { get; set; }

	public override void Init()
	{
		base.Init();
		this.fsm.EnterState(this.initialState, null);
		Chum chum = FeederHelper.FindPreparedChumActiveRodAll().FirstOrDefault<Chum>();
	}

	public override void SetVisibility(bool isVisibility)
	{
		base.SetVisibility(isVisibility);
		if (base.RodSlot.Line != null)
		{
			base.RodSlot.Line.SetVisibility(isVisibility);
		}
	}

	public override void Destroy()
	{
		this.fsm = null;
		base.Hook = null;
		base.Destroy();
	}

	public override bool IsActive
	{
		get
		{
			return this.fsm.CurrentStateType != typeof(FeederHidden);
		}
	}

	public override bool IsSyncActive
	{
		get
		{
			Type currentStateType = this.fsm.CurrentStateType;
			return currentStateType == typeof(FeederFloating) || currentStateType == typeof(FeederHitched) || currentStateType == typeof(FeederSwallowed) || currentStateType == typeof(FeederHanging);
		}
	}

	public override void CreateTackle(Vector3 direction)
	{
		if (base.RodTemplate == RodTemplate.Bottom)
		{
			base.TackleType = FishingRodSimulation.TackleType.Feeder;
			base.RodSlot.Sim.AddFeeder(base.Size, (base.Hook.lineAnchor.position - base.Hook.hookAnchor.position).magnitude, (base.Hook.BaitItem.Weight != null) ? ((float)base.Hook.BaitItem.Weight.Value) : 0f, (!base.Hook.BaitItem.IsLive) ? base.Hook.BaitItem.Buoyancy : 0f, direction, base.Controller);
		}
		else if (base.RodTemplate == RodTemplate.ClassicCarp)
		{
			base.TackleType = FishingRodSimulation.TackleType.CarpClassic;
			base.RodSlot.Sim.AddFeeder(base.Size, (base.Hook.lineAnchor.position - base.Hook.hookAnchor.position).magnitude, (base.Hook.BaitItem.Weight != null) ? ((float)base.Hook.BaitItem.Weight.Value) : 0f, (!base.Hook.BaitItem.IsLive) ? base.Hook.BaitItem.Buoyancy : 0f, direction, base.Controller);
		}
		else if (base.RodTemplate == RodTemplate.Spod)
		{
			base.TackleType = FishingRodSimulation.TackleType.Spod;
			base.RodSlot.Sim.AddSpod(base.Size, (float)(this._rodAssembly.ChumInterface as Chum).Weight.Value, (this._rodAssembly.ChumInterface as Chum).Buoyancy, direction, base.Controller);
			base.Hook.transform.parent = base.Controller.transform;
			base.Hook.SetVisible(false);
		}
		else if (base.RodTemplate == RodTemplate.MethodCarp)
		{
			base.TackleType = FishingRodSimulation.TackleType.CarpMethod;
			base.RodSlot.Sim.AddCarpMethod(base.Size, (base.Hook.lineAnchor.position - base.Hook.hookAnchor.position).magnitude, (base.Hook.BaitItem.Weight != null) ? ((float)base.Hook.BaitItem.Weight.Value) : 0f, (!base.Hook.BaitItem.IsLive) ? base.Hook.BaitItem.Buoyancy : 0f, direction, base.Controller);
		}
		else if (base.RodTemplate == RodTemplate.PVACarp && (this._rodAssembly.FeederInterface as PvaFeeder).Form == PvaFeederForm.Stick)
		{
			base.TackleType = FishingRodSimulation.TackleType.CarpPVAStick;
			base.RodSlot.Sim.AddCarpPVAStick(base.Size, (base.Hook.lineAnchor.position - base.Hook.hookAnchor.position).magnitude, (base.Hook.BaitItem.Weight != null) ? ((float)base.Hook.BaitItem.Weight.Value) : 0f, (!base.Hook.BaitItem.IsLive) ? base.Hook.BaitItem.Buoyancy : 0f, direction, base.Controller);
		}
		else if (base.RodTemplate == RodTemplate.PVACarp && (this._rodAssembly.FeederInterface as PvaFeeder).Form == PvaFeederForm.Bag)
		{
			base.TackleType = FishingRodSimulation.TackleType.CarpPVABag;
			Chum chum = FeederHelper.FindPreparedChumActiveRodAll().FirstOrDefault<Chum>();
			if (this.initialState != typeof(FeederFloating) && this.initialState != typeof(FeederSwallowed) && this.initialState != typeof(FeederHitched) && chum != null && chum.Weight != null && !Mathf.Approximately((float)chum.Weight.Value, 0f))
			{
				base.RodSlot.Sim.AddCarpPVABag(base.Size, (base.Hook.lineAnchor.position - base.Hook.hookAnchor.position).magnitude, (base.Hook.BaitItem.Weight != null) ? ((float)base.Hook.BaitItem.Weight.Value) : 0f, (!base.Hook.BaitItem.IsLive) ? base.Hook.BaitItem.Buoyancy : 0f, direction, base.Controller);
			}
			else
			{
				base.TackleType = FishingRodSimulation.TackleType.CarpClassic;
				base.RodSlot.Sim.AddFeeder(base.Size, (base.Hook.lineAnchor.position - base.Hook.hookAnchor.position).magnitude, (base.Hook.BaitItem.Weight != null) ? ((float)base.Hook.BaitItem.Weight.Value) : 0f, (!base.Hook.BaitItem.IsLive) ? base.Hook.BaitItem.Buoyancy : 0f, direction, base.Controller);
			}
		}
		this.RefreshObjectsFromSim();
	}

	public void RefreshObjectsFromSim()
	{
		this.feederObject = (FeederTackleObject)base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Feeder);
		this.leaderObject = (SpringDrivenObject)base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Leader);
		base.Hook.HookObject = (SpringDrivenObject)base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Hook);
	}

	public override bool IsOnTip
	{
		get
		{
			return this.fsm.CurrentStateType == typeof(FeederOnTip) || this.fsm.CurrentStateType == typeof(FeederIdlePitch);
		}
	}

	public override bool IsFlying
	{
		get
		{
			return this.fsm.CurrentStateType == typeof(FeederFlying);
		}
	}

	public override bool IsInWater
	{
		get
		{
			return this.fsm.CurrentStateType == typeof(FeederFloating) || this.fsm.CurrentStateType == typeof(FeederSwallowed);
		}
	}

	public bool IsHanging
	{
		get
		{
			return this.fsm.CurrentStateType == typeof(FeederHanging);
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

	public override bool CheckPitchIsTooShort()
	{
		this.isPitchTooShort = base.RodSlot.Line.FullLineLength <= base.RodSlot.Line.MinLineLengthWithFish || base.RodSlot.Line.SecuredLineLength <= base.RodSlot.Line.MinLineLength;
		if (this.isPitchTooShort)
		{
		}
		return this.isPitchTooShort;
	}

	public Spring GetKinematicLeaderMassSpring()
	{
		return this.leaderObject.Springs[this.leaderObject.Springs.Count - 5];
	}

	public override void SetActive(bool isActive)
	{
		base.SetActive(isActive);
		if (base.Controller.SecondaryTackleObject != null)
		{
			base.Controller.SecondaryTackleObject.SetActive(isActive);
		}
		if (base.TackleType == FishingRodSimulation.TackleType.Spod)
		{
			base.Hook.SetVisible(false);
		}
	}

	public override void UpdateTransitionData(RodOnPodBehaviour.TransitionData transitionData)
	{
		base.UpdateTransitionData(transitionData);
		if (base.Controller.SecondaryTackleObject != null)
		{
			transitionData.secondaryTackleObject = base.Controller.SecondaryTackleObject;
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
		if (this.feederObject == null)
		{
			return;
		}
		this.FeederObject.SyncTransform(base.transform, base.Controller.topLineAnchor, base.Controller.bottomLineAnchor, base.Controller.CenterTransform);
	}

	public override float LeaderLength
	{
		set
		{
			base.LeaderLength = value;
			base.RodSlot.Sim.SetLeaderLength(value);
		}
	}

	public override bool IsLying
	{
		get
		{
			Mass mass;
			if (base.Hook.HookObject != null)
			{
				mass = base.Hook.HookObject.Masses[1];
			}
			else
			{
				mass = this.RigidBody;
			}
			return mass != null && mass.IsLying;
		}
	}

	public override bool IsFeederLying
	{
		get
		{
			return this.RigidBody != null && this.RigidBody.IsLying;
		}
	}

	public override PhyObject phyObject
	{
		get
		{
			return this.feederObject;
		}
	}

	public override FeederTackleObject FeederObject
	{
		get
		{
			return this.feederObject;
		}
	}

	public override RigidBody RigidBody
	{
		get
		{
			return (this.feederObject == null) ? null : this.feederObject.RigidBody;
		}
	}

	public new bool IsKinematic
	{
		get
		{
			return this.isKinematic;
		}
		set
		{
			this.isKinematic = value;
			this.feederObject.IsKinematic = value;
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

	public override float TackleMass
	{
		get
		{
			return base.Mass + base.Hook.Mass + base.Hook.BaitMass;
		}
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

	public override bool IsLiveBait
	{
		get
		{
			return base.Hook.BaitItem.IsLive;
		}
	}

	public override bool IsBottom
	{
		get
		{
			return this.isBottom;
		}
	}

	public override Mass GetKinematicLeaderMass(int n = 0)
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
		component.Bait = this.baitObject;
		component.BaitItem = rodAssembly.Bait;
		component.IsBaitShown = rodAssembly.Bait.Count > 0;
		component.Init();
	}

	private Fsm<IFeederBehaviour> fsm;

	private SpringDrivenObject leaderObject;

	private FeederTackleObject feederObject;

	public new bool Thrown;

	public const float MinVisibleLeaderLength = 0.3f;

	public const float MaxVisibleLeaderLength = 0.75f;

	private bool isKinematic;

	private bool isPitchTooShort;

	private bool isBottom;
}
