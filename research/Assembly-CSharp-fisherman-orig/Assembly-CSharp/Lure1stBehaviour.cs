using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Simd.Math;
using ObjectModel;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class Lure1stBehaviour : LureBehaviour, ILureBehaviour, ITackleBehaviour
{
	public Lure1stBehaviour(LureController owner, AssembledRod rodAssembly, RodOnPodBehaviour.TransitionData transitionData = null)
		: base(owner, rodAssembly, transitionData)
	{
		base.LeaderLength = ((rodAssembly.Leader == null) ? 0f : rodAssembly.Rod.LeaderLength);
		base.SinkerMass = 0f;
		if (rodAssembly.RodTemplate == RodTemplate.Lure || rodAssembly.RodTemplate.IsLureBait())
		{
			Lure lure = rodAssembly.Hook as Lure;
			if (lure.Weight == null)
			{
				throw new DbConfigException("Lure weight is null!");
			}
			base.LureItem = lure;
			base.Resistance = lure.ResistanceForce;
			base.Buoyancy = lure.Buoyancy;
			base.SpeedIncBuoyancy = lure.SpeedIncBuoyancy;
		}
		else if (rodAssembly.RodTemplate.IsOffsetHook())
		{
			OffsetHook offsetHook = rodAssembly.Hook as OffsetHook;
			JigBait jigBait = rodAssembly.Bait as JigBait;
			Sinker sinker = rodAssembly.Sinker;
			if (offsetHook.Weight == null)
			{
				throw new DbConfigException("Hook weight is null!");
			}
			if (jigBait.Weight == null)
			{
				throw new DbConfigException("Bait weight is null!");
			}
			if (sinker == null)
			{
				base.Buoyancy = offsetHook.Buoyancy;
			}
			else
			{
				base.Buoyancy = sinker.Buoyancy;
				if (sinker.Weight != null)
				{
					base.SinkerMass = (float)sinker.Weight.Value;
				}
			}
			base.Resistance = jigBait.ResistanceForce;
			base.SpeedIncBuoyancy = jigBait.SpeedIncBuoyancy;
			if (base.Buoyancy >= 0f)
			{
				base.Buoyancy = -1f;
			}
		}
		else
		{
			JigHead jigHead = rodAssembly.Hook as JigHead;
			JigBait jigBait2 = rodAssembly.Bait as JigBait;
			if (jigBait2.Weight == null)
			{
				throw new DbConfigException("JigBait weight is null!");
			}
			if (jigHead.Weight == null)
			{
				throw new DbConfigException("Jig weight is null!");
			}
			base.Resistance = jigBait2.ResistanceForce;
			base.Buoyancy = jigHead.Buoyancy;
			base.SpeedIncBuoyancy = jigBait2.SpeedIncBuoyancy;
		}
		if (base.RodAssembly.RodTemplate.IsSinkerRig())
		{
			GameObject gameObject = (GameObject)Resources.Load("Tackle/Sinkers/DefaultSinker/pDefaultSinker", typeof(GameObject));
			if (gameObject != null)
			{
				base.SmallSinkerObject = Object.Instantiate<GameObject>(gameObject, base.TopLineAnchor.position, base.Owner.transform.rotation);
				base.SmallSinkerObject.transform.localScale = new Vector3(0.003f, 0.003f, 0.003f);
				this._smallSinkerRenderer = base.SmallSinkerObject.transform.GetComponent<MeshRenderer>();
			}
		}
		if (base.IsUncuttable)
		{
			base.AddUncuttableLeader(owner, base.TopLineAnchor.position, Quaternion.identity);
		}
		this.fsm = new Fsm<ILureBehaviour>("Lure", this, true);
		this.fsm.RegisterState<LureOnTip>();
		this.fsm.RegisterState<LureIdlePitch>();
		this.fsm.RegisterState<LureFlying>();
		this.fsm.RegisterState<LureFloating>();
		this.fsm.RegisterState<LureSwallowed>();
		this.fsm.RegisterState<LureHanging>();
		this.fsm.RegisterState<LureShowItem>();
		this.fsm.RegisterState<LureShowSmallFish>();
		this.fsm.RegisterState<LureShowBigFish>();
		this.fsm.RegisterState<LureHitched>();
		this.fsm.RegisterState<LureBroken>();
		this.fsm.RegisterState<LurePitching>();
		this.fsm.RegisterState<LureHidden>();
		this.throwData = new TackleThrowData
		{
			Windage = base.Windage,
			RodLength = (float)rodAssembly.Rod.Length.Value,
			MaxCastLength = rodAssembly.Rod.MaxCastLength
		};
		this._lureRenderers = RenderersHelper.GetAllRenderersForObject<Renderer>(base.transform);
		if (transitionData == null)
		{
			this.initialState = typeof(LureHidden);
		}
		else
		{
			base.IsAttackFinished = transitionData.tackleAttackFinished;
			base.IsFinishAttackRequested = transitionData.tackleAttackFinishedRequested;
			base.IsHitched = transitionData.tackleInitialState == typeof(LureHitched);
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
		for (int i = 0; i < this._lureRenderers.Count; i++)
		{
			this._lureRenderers[i].enabled = isVisible;
		}
		if (this._smallSinkerRenderer != null)
		{
			this._smallSinkerRenderer.enabled = isVisible;
		}
	}

	public TackleObject LureObject
	{
		get
		{
			return this.lureObject;
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

	public new Fish1stBehaviour AttackingFish { get; set; }

	public Transform WaterMark
	{
		get
		{
			return null;
		}
	}

	public override Type State
	{
		get
		{
			return this.fsm.CurrentStateType;
		}
	}

	public bool IsWobbler
	{
		get
		{
			return base.LureItem != null && base.LureItem.Wobbler != null;
		}
	}

	public bool IsPopper
	{
		get
		{
			return base.LureItem != null && base.LureItem.TopWaterType != null && base.LureItem.TopWaterType.Value == TopWaterType.Popper;
		}
	}

	public bool IsWalker
	{
		get
		{
			return base.LureItem != null && base.LureItem.TopWaterType != null && base.LureItem.TopWaterType.Value == TopWaterType.Walker;
		}
	}

	public bool IsJerkbait
	{
		get
		{
			return base.LureItem != null && base.LureItem.TopWaterType != null && base.LureItem.TopWaterType.Value == TopWaterType.Jerkbait;
		}
	}

	public bool IsAnimatedWobbler
	{
		get
		{
			return this.IsWobbler && base.LureItem.Wobbler.IsSelfAnimated;
		}
	}

	public override bool HasEnteredShowingState
	{
		get
		{
			return this.State == typeof(LureShowSmallFish) || this.State == typeof(LureShowBigFish) || this.State == typeof(LureShowItem);
		}
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

	public DragStyleSet CurrentDragStyleSet
	{
		get
		{
			if (base.LureItem != null)
			{
				if (base.LureItem.TopWaterType == null)
				{
					return DragStyleSet.NormalLure;
				}
				if (base.LureItem.TopWaterType == TopWaterType.Popper)
				{
					return DragStyleSet.Popper;
				}
				if (base.LureItem.TopWaterType == TopWaterType.Walker)
				{
					return DragStyleSet.Walker;
				}
			}
			return DragStyleSet.NormalLure;
		}
	}

	public override void CreateTackle(Vector3 direction)
	{
		if (base.LureItem == null)
		{
		}
		if (base.LureItem == null || (base.LureItem.TopWaterType == null && base.LureItem.Wobbler == null))
		{
			base.TackleType = FishingRodSimulation.TackleType.Lure;
			base.Size = (this._owner.topLineAnchor.position - this._owner.bottomLineAnchor.position).magnitude;
			if (base.RodTemplate.IsSinkerRig())
			{
				base.RodSlot.Sim.AddRig(base.Size, (this._owner.bottomLineAnchor.position - base.Owner.hookAnchor.position).magnitude, this.TackleMass, base.Buoyancy, base.SpeedIncBuoyancy, direction, base.LeaderLength, base.SinkerMass, base.RodTemplate, base.Sinker, base.Owner);
			}
			else
			{
				base.RodSlot.Sim.AddLure(base.Size, (this._owner.bottomLineAnchor.position - base.Owner.hookAnchor.position).magnitude, this.TackleMass, base.Buoyancy, base.SpeedIncBuoyancy, direction, base.LeaderLength, base.Owner);
			}
		}
		else
		{
			float num = 0.1f;
			if (base.LureItem.Length != null)
			{
				num = (float)base.LureItem.Length.Value;
			}
			base.Size = num;
			if ((base.LureItem.TopWaterType == null && base.LureItem.Wobbler != null) || base.LureItem.TopWaterType == TopWaterType.Wobbler)
			{
				base.TackleType = FishingRodSimulation.TackleType.Wobbler;
				base.RodSlot.Sim.AddWobbler(num, (this._owner.bottomLineAnchor.position - base.Owner.hookAnchor.position).magnitude, this.TackleMass, base.Buoyancy, base.SpeedIncBuoyancy, direction, base.LeaderLength, base.Owner, base.LureItem.Wobbler.SinkRate, base.LureItem.Wobbler.WorkingDepth, 0f, 0f);
			}
			else if (base.IsSwimbait)
			{
				base.TackleType = FishingRodSimulation.TackleType.Wobbler;
				base.RodSlot.Sim.AddSwimbait(num, (this._owner.bottomLineAnchor.position - base.Owner.hookAnchor.position).magnitude, this.TackleMass, base.Buoyancy, base.SpeedIncBuoyancy, direction, base.LeaderLength, base.Owner);
			}
			else if (base.LureItem.TopWaterType == TopWaterType.Jerkbait)
			{
				base.TackleType = FishingRodSimulation.TackleType.Wobbler;
				base.RodSlot.Sim.AddWobbler(num, (this._owner.bottomLineAnchor.position - base.Owner.hookAnchor.position).magnitude, this.TackleMass, base.Buoyancy, base.SpeedIncBuoyancy, direction, base.LeaderLength, base.Owner, base.LureItem.Wobbler.SinkRate, base.LureItem.Wobbler.WorkingDepth, base.LureItem.Wobbler.WalkingFactor, base.LureItem.Wobbler.ImbalanceRatio);
			}
			else if (base.LureItem.TopWaterType == TopWaterType.Walker)
			{
				base.TackleType = FishingRodSimulation.TackleType.Topwater;
				base.RodSlot.Sim.AddTopWater(num, this.TackleMass, direction, base.LeaderLength, base.Owner, (!Mathf.Approximately(base.LureItem.Wobbler.WalkingFactor, 0f)) ? base.LureItem.Wobbler.WalkingFactor : 1f, base.LureItem.Wobbler.ImbalanceRatio);
			}
			else if (base.LureItem.TopWaterType == TopWaterType.Popper)
			{
				base.TackleType = FishingRodSimulation.TackleType.Topwater;
				base.RodSlot.Sim.AddTopWater(num, this.TackleMass, direction, base.LeaderLength, base.Owner, base.LureItem.Wobbler.WalkingFactor, (!Mathf.Approximately(base.LureItem.Wobbler.ImbalanceRatio, 0f)) ? base.LureItem.Wobbler.ImbalanceRatio : 0.25f);
			}
			float magnitude = (this._owner.transform.InverseTransformPoint(this._owner.topLineAnchor.position) - this._owner.transform.InverseTransformPoint(this._owner.bottomLineAnchor.position)).magnitude;
			float num2 = num / magnitude;
			GameObject attachedObject = base.AttachedObject;
			base.DetachFromTackle();
			base.transform.localScale = new Vector3(num2, num2, num2);
			if (attachedObject != null)
			{
				base.AttachToTackle(attachedObject, base.Owner, base.TopLineAnchor.position, Quaternion.identity);
			}
		}
		if (base.SwimbaitOwner != null)
		{
			this.rootAnchor = base.SwimbaitOwner.rootAnchor;
		}
		else
		{
			this.rootAnchor = base.BottomLineAnchor.parent;
		}
		this.RefreshObjectsFromSim();
	}

	public void RefreshObjectsFromSim()
	{
		this.lureObject = (TackleObject)base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Lure);
		if (this.lureObject != null)
		{
			this.hookMass = this.lureObject.HookMass;
		}
		this.leaderObject = (SpringDrivenObject)base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Leader);
		this.leashObject = (SpringDrivenObject)base.RodSlot.Sim.Objects.FirstOrDefault((PhyObject i) => i.Type == PhyObjectType.Leash);
	}

	public override void Start()
	{
		base.Start();
		this.analyzer.Reset(this.CurrentDragStyleSet);
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
		float num = 0f;
		Vector2 vector;
		vector..ctor(base.transform.position.x, base.transform.position.z);
		Vector2? vector2 = this.priorPosition;
		if (vector2 != null && Time.deltaTime > 0f)
		{
			num = Vector2.Distance(vector, this.priorPosition.Value) / Time.deltaTime;
		}
		num = this.velocityAverager.UpdateAndGet(num, Time.deltaTime);
		this.priorPosition = new Vector2?(vector);
		this.IsMoving = num > 0.3f;
		if (base.RodSlot.IsInHands)
		{
			this._owner.PlayerController.UpdateTackle(base.transform, null);
		}
		if (this.State != typeof(LureFloating))
		{
			return;
		}
		if (base.Rod1st != null && base.Rod1st.Player != null)
		{
			if (this.IsPopper)
			{
				Vector3 vector3 = (this.lureObject.Masses[0].Position - this.prevTacklePosition) / Time.deltaTime;
				this.tackleFilteredVelocity = Vector3.Lerp(vector3, this.tackleFilteredVelocity, Mathf.Exp(-Time.deltaTime * 10f));
				this.prevTacklePosition = this.lureObject.Masses[0].Position;
				float num2 = ((!base.Rod1st.Player.IsStriking) ? 1.3f : 0.4f);
				if (Time.time > this.popperSplashTimestamp + 0.5f && (this.tackleFilteredVelocity - this.lureObject.Masses[0].FlowVelocity.AsVector3()).magnitude > Mathf.Max(base.RodSlot.Reel.CurrentSpeed * num2, 1f))
				{
					Vector3 position = this.lureObject.Masses[0].Position;
					position.y = 0f;
					this.popperSplashTimestamp = Time.time;
					DynWaterParticlesController.CreateSplash(base.Rod1st.Player.CameraController.transform, position, "2D/Splashes/pSplash_universal", 1f, 1f, true, true, 1);
					RandomSounds.PlaySoundAtPoint("Sounds/Actions/Popper/sfx_popper_splash_0" + Random.Range(1, 4), position, 3f, false);
				}
			}
			if ((this.IsAnimatedWobbler && !this.IsPopper && !this.IsWalker && !this.IsJerkbait) || base.RodSlot.Reel.CurrentReelSpeedSection == 4 || this.fftFillBufferTimer > 0f)
			{
				base.Rod1st.Player.HudFishingHandler.LurePositionHandlerContinuous.ShowDragStyleText(string.Empty, 0f);
				return;
			}
			this.analyzer.Update(new ActionDragStyleAnalyzer.ActionStatus
			{
				TacklePosition = base.transform.position,
				IsReeling = base.RodSlot.Reel.IsReeling,
				IsStriking = base.Rod1st.Player.IsPulling,
				HasFullStrike = base.Rod1st.Player.HasFullStrike,
				IsLying = this.IsLying,
				ReelingSpeed = ((!base.RodSlot.Reel.IsReeling) ? 0 : base.RodSlot.Reel.CurrentReelSpeedSection)
			});
			base.Rod1st.Player.HudFishingHandler.LurePositionHandlerContinuous.ShowDragStyleText(ActionDragStyleAnalyzer.GetFineDragStyleName(this.DragStyle), this.DragQuality);
		}
	}

	private void SyncWithSim()
	{
		if (this.lureObject == null)
		{
			return;
		}
		if (!this.IsFrozen)
		{
			this.LureObject.SyncTransform(base.transform, this._owner.topLineAnchor, base.Owner.hookAnchor, this.rootAnchor);
		}
		else
		{
			Mass mass = null;
			Vector3 vector = Vector3.zero;
			if (this.Fish != null)
			{
				AbstractFishBody abstractFishBody = this.Fish.FishObject as AbstractFishBody;
				vector = this.Fish.HeadRight;
				mass = abstractFishBody.Masses[0];
			}
			else if (base.UnderwaterItem != null && base.UnderwaterItem.Behaviour != null)
			{
				mass = (base.UnderwaterItem.Behaviour as UnderwaterItem1stBehaviour).phyObject.ForHookMass;
				vector = mass.Rotation * Vector3.right;
			}
			this.LureObject.SyncTransformFrozen(base.transform, this._owner.topLineAnchor, base.Owner.hookAnchor, this.rootAnchor, mass, vector);
		}
	}

	public override PhyObject phyObject
	{
		get
		{
			return this.lureObject;
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
			this.lureObject.IsKinematic = value;
		}
	}

	public bool IsFrozen { get; private set; }

	public override bool IsOnTip
	{
		get
		{
			return this.fsm.CurrentStateType == typeof(LureOnTip) || this.fsm.CurrentStateType == typeof(LureIdlePitch);
		}
	}

	public override bool IsFlying
	{
		get
		{
			return this.fsm.CurrentStateType == typeof(LureFlying);
		}
	}

	public override bool IsInWater
	{
		get
		{
			return this.fsm.CurrentStateType == typeof(LureFloating) || this.fsm.CurrentStateType == typeof(LureSwallowed);
		}
	}

	public bool IsHanging
	{
		get
		{
			return this.fsm.CurrentStateType == typeof(LureHanging);
		}
	}

	public override float TackleMass
	{
		get
		{
			return base.Mass;
		}
	}

	public override bool IsLying
	{
		get
		{
			return !base.IsMovingWobbler && this.hookMass != null && this.hookMass.IsLying;
		}
	}

	public Vector3 DirectionVector
	{
		get
		{
			return this.LureObject.DirectionVector;
		}
	}

	public override void Hitch(Vector3 position)
	{
		base.HitchPosition = new Vector3?(position);
		this.LureObject.Hitch(position);
	}

	public override void UnHitch()
	{
		this.LureObject.Unhitch();
		base.HitchPosition = null;
		base.RodSlot.Sim.HorizontalRealignLineMasses();
	}

	public void SetHookMass(Mass m)
	{
		RigidBodyTackleObject rigidBodyTackleObject = this.LureObject as RigidBodyTackleObject;
		if (rigidBodyTackleObject != null)
		{
			rigidBodyTackleObject.ExchangeFirstMass(m);
		}
	}

	public void SetLureMasses(Mass m)
	{
		if (!(this.LureObject is RigidBodyTackleObject))
		{
			this.LureObject.Masses[0] = m;
			this.LureObject.Masses[1] = m;
		}
	}

	public override DragStyle DragStyle
	{
		get
		{
			return this.analyzer.Style;
		}
	}

	public float DragQuality
	{
		get
		{
			return this.analyzer.Quality;
		}
	}

	public override bool IsIdle
	{
		get
		{
			return this.State == typeof(LureOnTip) || this.State == typeof(LureIdlePitch);
		}
	}

	public override bool IsActive
	{
		get
		{
			return this.fsm.CurrentStateType != typeof(LureHidden);
		}
	}

	public override bool IsBeingPulled
	{
		get
		{
			return base.RodSlot.Reel.IsReeling && base.RodSlot.Reel.CurrentReelSpeedSection > 0 && base.RodSlot.Line.IsTensioned;
		}
	}

	public override bool IsSyncActive
	{
		get
		{
			Type currentStateType = this.fsm.CurrentStateType;
			return currentStateType == typeof(LureFloating) || currentStateType == typeof(LureHitched) || currentStateType == typeof(LureSwallowed) || currentStateType == typeof(LureHanging);
		}
	}

	public void ResetDrag()
	{
		this.analyzer.Reset(this.CurrentDragStyleSet);
	}

	public override void FinishDragPeriod()
	{
		this.analyzer.FinishPeriod();
	}

	public void SetFreeze(bool freeze)
	{
		if (freeze)
		{
			this.Freeze(false);
		}
		else
		{
			this.Unfreeze();
		}
	}

	public Mass Freeze(bool makeLureKinematic)
	{
		this.IsFrozen = true;
		if (makeLureKinematic)
		{
			this.lureObject.IsKinematic = true;
		}
		return this.lureObject.Masses.First<Mass>();
	}

	public void RealignMasses()
	{
		this.LureObject.RealingMasses();
	}

	public void Unfreeze()
	{
		this.lureObject.IsKinematic = false;
		this.IsFrozen = false;
	}

	public void OnEnterHangingState()
	{
		this.LureObject.OnEnterHangingState();
	}

	public void OnExitHangingState()
	{
		this.LureObject.OnExitHangingState();
	}

	private List<Renderer> _lureRenderers;

	private MeshRenderer _smallSinkerRenderer;

	private Fsm<ILureBehaviour> fsm;

	private Vector2? priorPosition;

	public const float MinHorizontalVelocity = 0.3f;

	private const float PopperSplashCooldown = 0.5f;

	private const float RigidLureDefaultLength = 0.1f;

	private TackleObject lureObject;

	private SpringDrivenObject leaderObject;

	private SpringDrivenObject leashObject;

	private Mass hookMass;

	private ActionDragStyleAnalyzer analyzer = new ActionDragStyleAnalyzer();

	private float fftFillBufferTimer;

	private Transform rootAnchor;

	private float popperSplashTimestamp;

	private Vector3 prevTacklePosition;

	private Vector3 tackleFilteredVelocity;

	private TimedAverager velocityAverager = new TimedAverager(1f, 100);

	private bool isKinematic;
}
