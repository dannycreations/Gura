using System;
using ObjectModel;
using Phy;
using TPM;
using UnityEngine;

public abstract class TackleBehaviour : ITackleBehaviour
{
	protected TackleBehaviour(TackleControllerBase owner, IAssembledRod rodAssembly, RodOnPodBehaviour.TransitionData transitionData = null)
	{
		this._owner = owner;
		this._rodAssembly = rodAssembly;
		this.Size = (this._owner.topLineAnchor.position - this._owner.bottomLineAnchor.position).magnitude;
		if (rodAssembly.Rod != null)
		{
			TacklePhysicalParams tacklePhysicalParams = RodHelper.GetTacklePhysicalParams(rodAssembly.Rod, null);
			if (tacklePhysicalParams == null)
			{
				throw new InvalidOperationException("Current tackle physical params are null!");
			}
			this.mass = tacklePhysicalParams.TackleMass;
			this.windage = tacklePhysicalParams.TackleWindage;
		}
		if (transitionData != null)
		{
			this.UnderwaterItemAsyncRequest = transitionData.underwaterItemAsyncRequest;
			this.UnderwaterItemId = transitionData.underwaterItemId;
			this.UnderwaterItemName = transitionData.underwaterItemName;
			this.UnderwaterItemCategory = transitionData.underwaterItemCategory;
			if (transitionData.underwaterItemObject != null)
			{
				this.UnderwaterItem = transitionData.underwaterItemObject.GetComponent<UnderwaterItemController>();
			}
			this.FishTemplate = transitionData.spawnedFish;
			this.StrikeTimeEnd = transitionData.tackleStrikeTimeEnd;
		}
		this.AttachedObject = null;
		Lure lure = rodAssembly.HookInterface as Lure;
		if (lure != null && lure.ItemSubType == ItemSubTypes.BuzzBait)
		{
			this.SurfaceFxWaterDisturbance = 2f;
			this.SurfaceFxSplashPeriod = 0.3f;
			this.SurfaceFxSplashSize = 0.8f;
		}
		this.Rod1st = this.Rod as Rod1stBehaviour;
	}

	public IAssembledRod RodAssembly
	{
		get
		{
			return this._rodAssembly;
		}
	}

	public RodTemplate RodTemplate
	{
		get
		{
			return this._rodAssembly.RodTemplate;
		}
	}

	public FishingRodSimulation.TackleType TackleType { get; protected set; }

	public bool IsLureTackle
	{
		get
		{
			return this.TackleType == FishingRodSimulation.TackleType.Lure || this.TackleType == FishingRodSimulation.TackleType.Topwater || this.TackleType == FishingRodSimulation.TackleType.Wobbler;
		}
	}

	public bool IsUncuttable
	{
		get
		{
			Leader leader = this._rodAssembly.LeaderInterface as Leader;
			return leader != null && leader.ItemSubType.IsUncuttableLeader();
		}
	}

	public bool IsMovingWobbler
	{
		get
		{
			return (this.TackleType == FishingRodSimulation.TackleType.Wobbler || this.TackleType == FishingRodSimulation.TackleType.Topwater) && this.IsMoving;
		}
	}

	public bool IsSlider
	{
		get
		{
			Bobber bobber = this._rodAssembly.BobberInterface as Bobber;
			return bobber != null && bobber.ItemSubType == ItemSubTypes.Slider;
		}
	}

	public bool IsWaggler
	{
		get
		{
			Bobber bobber = this._rodAssembly.BobberInterface as Bobber;
			return bobber != null && bobber.ItemSubType == ItemSubTypes.Waggler;
		}
	}

	public bool IsSwimbait
	{
		get
		{
			Lure lure = this._rodAssembly.HookInterface as Lure;
			return lure != null && lure.ItemSubType == ItemSubTypes.Swimbait;
		}
	}

	public int SlotId
	{
		get
		{
			return this.RodSlot.Index;
		}
	}

	public GameFactory.RodSlot RodSlot
	{
		get
		{
			return this._owner.RodSlot;
		}
	}

	public GameActionAdapter Adapter
	{
		get
		{
			return PhotonConnectionFactory.Instance.GetGameSlot(this.SlotId).Adapter;
		}
	}

	public virtual Vector3 Position
	{
		get
		{
			return this.transform.position;
		}
	}

	public GameObject gameObject
	{
		get
		{
			return (!(this._owner != null)) ? null : this._owner.gameObject;
		}
	}

	public Transform transform
	{
		get
		{
			return this._owner.gameObject.transform;
		}
	}

	public virtual bool IsBeingPulled
	{
		get
		{
			return false;
		}
	}

	public virtual PhyObject phyObject
	{
		get
		{
			return null;
		}
	}

	public Transform TopLineAnchor
	{
		get
		{
			return this._owner.topLineAnchor;
		}
	}

	public Transform BottomLineAnchor
	{
		get
		{
			return this._owner.bottomLineAnchor;
		}
	}

	public Vector3 LineEndPosition
	{
		get
		{
			if (this.lineEndAnchor != null)
			{
				return this.lineEndAnchor.position;
			}
			if (this.lineEnd == null)
			{
				return this._owner.topLineAnchor.position;
			}
			Rod1stBehaviour rod1stBehaviour = this.Rod as Rod1stBehaviour;
			if (rod1stBehaviour == null)
			{
				return this.lineEnd.Position;
			}
			return rod1stBehaviour.PositionCorrection(this.lineEnd, this.IsShowingComplete || (!this.FishIsShowing && !this.ItemIsShowing));
		}
	}

	public virtual Vector3 LeaderTopPosition
	{
		get
		{
			if (this.leaderHitchAnchor != null)
			{
				return this.leaderHitchAnchor.position;
			}
			if (this.leaderHitch == null)
			{
				return this._owner.bottomLineAnchor.position;
			}
			Rod1stBehaviour rod1stBehaviour = this.Rod as Rod1stBehaviour;
			if (rod1stBehaviour == null)
			{
				return this.leaderHitch.Position;
			}
			return rod1stBehaviour.PositionCorrection(this.leaderHitch, this.IsShowingComplete || (!this.FishIsShowing && !this.ItemIsShowing));
		}
	}

	public Transform LeaderEndAnchor
	{
		get
		{
			if (this.leaderEndAnchor != null)
			{
				return this.leaderEndAnchor;
			}
			if (this.Hook != null)
			{
				return this.Hook.lineAnchor;
			}
			return this.TopLineAnchor;
		}
	}

	public virtual Vector3 LeashTopPosition
	{
		get
		{
			if (this.leashHitchAnchor != null)
			{
				return this.leashHitchAnchor.position;
			}
			if (this.leashHitch == null)
			{
				return this._owner.bottomLineAnchor.position;
			}
			Rod1stBehaviour rod1stBehaviour = this.Rod as Rod1stBehaviour;
			if (rod1stBehaviour == null)
			{
				return this.leashHitch.Position;
			}
			return rod1stBehaviour.PositionCorrection(this.leashHitch, this.IsShowingComplete || (!this.FishIsShowing && !this.ItemIsShowing));
		}
	}

	public virtual Vector3 LeashEndPosition
	{
		get
		{
			if (this.leashEndAnchor != null)
			{
				return this.leashEndAnchor.position;
			}
			if (this.leashEnd == null)
			{
				return this.TopLineAnchor.position;
			}
			Rod1stBehaviour rod1stBehaviour = this.Rod as Rod1stBehaviour;
			if (rod1stBehaviour == null)
			{
				return this.leashEnd.Position;
			}
			return rod1stBehaviour.PositionCorrection(this.leashEnd, this.IsShowingComplete || (!this.FishIsShowing && !this.ItemIsShowing));
		}
	}

	public GameObject AttachedObject { get; protected set; }

	public GameObject BaitObject { get; protected set; }

	public Mass KinematicHandLineMass { get; set; }

	public virtual Type State
	{
		get
		{
			return null;
		}
	}

	public virtual Transform HookAnchor
	{
		get
		{
			return null;
		}
	}

	public virtual bool IsInWater
	{
		get
		{
			return false;
		}
	}

	public virtual bool IsLying
	{
		get
		{
			return false;
		}
	}

	public virtual bool IsOnTip
	{
		get
		{
			return false;
		}
	}

	public virtual float TackleMass
	{
		get
		{
			return this.Mass;
		}
	}

	public virtual bool IsActive
	{
		get
		{
			return false;
		}
	}

	public virtual bool IsMoving { get; protected set; }

	public virtual DragStyle DragStyle
	{
		get
		{
			return DragStyle.Undefined;
		}
	}

	public virtual bool IsIdle
	{
		get
		{
			return false;
		}
	}

	public virtual bool IsFlying
	{
		get
		{
			return false;
		}
	}

	public virtual bool IsSyncActive
	{
		get
		{
			return false;
		}
	}

	public virtual bool IsLiveBait
	{
		get
		{
			return false;
		}
	}

	public virtual bool IsBottom
	{
		get
		{
			return false;
		}
	}

	public virtual bool IsThrowing { get; set; }

	public virtual bool IsBaitShown
	{
		get
		{
			return false;
		}
		protected set
		{
		}
	}

	public bool IsStrikeTimedOut { get; set; }

	public float StrikeTimeEnd { get; set; }

	public virtual float LineStrain
	{
		get
		{
			return 0f;
		}
	}

	public virtual Transform TransformWithHook
	{
		get
		{
			return null;
		}
	}

	public bool IsInitialized { get; private set; }

	public virtual void Init()
	{
		this.IsInitialized = true;
	}

	public virtual void Destroy()
	{
		if (this.ShackleObject != null)
		{
			this.ShackleObject.SetActive(false);
			Object.Destroy(this.ShackleObject);
			this.ShackleObject = null;
		}
		if (this.SwivelObject != null)
		{
			this.SwivelObject.SetActive(false);
			Object.Destroy(this.SwivelObject);
			this.SwivelObject = null;
		}
		if (this._owner != null)
		{
			if (this.UnderwaterItem != null)
			{
				this.FinishWithItem();
			}
			this.gameObject.SetActive(false);
			Object.Destroy(this.gameObject);
			this._owner = null;
		}
	}

	public virtual void SetActive(bool isActive)
	{
		if (this.gameObject != null)
		{
			this.gameObject.SetActive(isActive);
		}
		if (this.Rod != null)
		{
			this.Rod.transform.gameObject.SetActive(isActive);
		}
		if (this.ShackleObject != null)
		{
			this.ShackleObject.SetActive(isActive);
		}
		if (this.SwivelObject != null)
		{
			this.SwivelObject.SetActive(isActive);
		}
	}

	public virtual void Start()
	{
	}

	public virtual void FixedUpdate()
	{
	}

	public virtual void Update()
	{
		this.tackleMoveDir = TackleMoveDir.None;
		if (this.RodSlot.Sim == null)
		{
			return;
		}
		Mass tackleTipMass = this.RodSlot.Sim.TackleTipMass;
		if (tackleTipMass != null)
		{
			if (tackleTipMass.Velocity.y > 0.05f)
			{
				this.tackleMoveDir = TackleMoveDir.Up;
			}
			else if (tackleTipMass.Velocity.y < -0.05f)
			{
				this.tackleMoveDir = TackleMoveDir.Down;
			}
			else if (tackleTipMass.Velocity.magnitude > 0.15f)
			{
				this.tackleMoveDir = TackleMoveDir.Forward;
			}
		}
	}

	public virtual void LateUpdate()
	{
	}

	public virtual void OnFsmUpdate()
	{
	}

	public virtual void OnLateUpdate()
	{
	}

	public virtual void CreateTackle(Vector3 direction)
	{
	}

	public virtual void FinishDragPeriod()
	{
	}

	public void ThrowTackle(Vector3 direction)
	{
		this.throwData.IsThrowing = true;
		this.throwData.Direction = direction;
	}

	protected Vector3? HitchPosition { get; set; }

	public bool FishIsLoading { get; set; }

	public IFishController Fish
	{
		get
		{
			return this.fish;
		}
		set
		{
			this.fish = value;
			this.FishTemplate = ((this.fish == null) ? null : this.fish.FishTemplate);
			this.FishIsLoading = false;
			if (value != null)
			{
				this.AttackingFish = null;
			}
		}
	}

	public Fish FishTemplate { get; private set; }

	public UnderwaterItemController UnderwaterItem { get; set; }

	public ResourceRequest UnderwaterItemAsyncRequest { get; protected set; }

	public bool HasUnderwaterItem
	{
		get
		{
			return this.UnderwaterItem != null && this.UnderwaterItem.Behaviour != null;
		}
	}

	public bool UnderwaterItemBehaviourIsNotInitialized
	{
		get
		{
			return this.UnderwaterItem != null && this.UnderwaterItem.Behaviour == null;
		}
	}

	public bool UnderwaterItemIsLoading
	{
		get
		{
			return this.UnderwaterItemAsyncRequest != null;
		}
	}

	public string UnderwaterItemName { get; set; }

	public string UnderwaterItemCategory { get; set; }

	public int UnderwaterItemId { get; private set; }

	public InventoryItem CaughtItem { get; set; }

	public IFishController AttackingFish { get; set; }

	public HookController Hook { get; set; }

	public RodBehaviour Rod
	{
		get
		{
			return this.RodSlot.Rod;
		}
	}

	public Rod1stBehaviour Rod1st { get; private set; }

	public RigidBodyController Sinker { get; set; }

	public RigidBodyController Swivel { get; protected set; }

	public RigidBodyController Shackle { get; protected set; }

	public Vector3 ShackleShift { get; protected set; }

	public GameObject SwivelObject { get; protected set; }

	public GameObject ShackleObject { get; protected set; }

	public float Mass
	{
		get
		{
			return this.mass;
		}
	}

	public float Windage
	{
		get
		{
			return this.windage;
		}
	}

	public float Resistance { get; protected set; }

	public float Buoyancy { get; protected set; }

	public float Size { get; protected set; }

	public float SpeedIncBuoyancy { get; protected set; }

	public float SinkerMass { get; protected set; }

	public float Sensitivity { get; protected set; }

	public virtual void SetVisibility(bool isVisible)
	{
		this._isVisible = isVisible;
		if (this._swivelRenderer != null)
		{
			this._swivelRenderer.enabled = isVisible;
		}
	}

	public virtual void SetOpaque(float prc)
	{
	}

	public virtual void RodSyncUpdate(Line3rdBehaviour lineBehaviour, float dtPrc)
	{
	}

	public virtual void ServerUpdate(TackleData tackleData, bool b, float f)
	{
	}

	public virtual void UpdateTransitionData(RodOnPodBehaviour.TransitionData transitionData)
	{
		if (this.UnderwaterItem != null)
		{
			transitionData.underwaterItemId = this.UnderwaterItem.ItemId;
			transitionData.underwaterItemName = this.UnderwaterItemName;
			transitionData.underwaterItemCategory = this.UnderwaterItemCategory;
			transitionData.underwaterItemObject = this.UnderwaterItem.gameObject;
			this.UnderwaterItem = null;
		}
	}

	public TackleThrowData ThrowData
	{
		get
		{
			return this.throwData;
		}
	}

	public bool IsShowing { get; set; }

	public virtual bool HasEnteredShowingState
	{
		get
		{
			return false;
		}
	}

	public bool IsLineInHand
	{
		get
		{
			return this.IsShowing && this.IsShowingComplete && (this.Fish == null || this.Fish.State == typeof(FishShowSmall));
		}
	}

	public static float ShowGripInAnimDuration(IFishController fish)
	{
		if (fish != null)
		{
			return 0.01337f * (GameFactory.Player.Grip.Fish.position - fish.HeadMass.Position).magnitude;
		}
		return 0.01337f;
	}

	public bool IsPulledOut
	{
		get
		{
			return this.transform.position.y > 0f;
		}
	}

	public bool IsFishHooked
	{
		get
		{
			return this.fish != null && this.fish.Behavior == FishBehavior.Hook;
		}
	}

	public void DisturbWater(Vector3 position, float radius, float force)
	{
		if (GameFactory.Water == null)
		{
			return;
		}
		GameFactory.Water.AddWaterDisturb(position, radius, force);
	}

	public bool IsHitched { get; set; }

	public bool IsClipStrike { get; set; }

	public virtual void Hitch(Vector3 position)
	{
	}

	public virtual void UnHitch()
	{
	}

	public bool IsAttackFinished { get; set; }

	public bool IsFinishAttackRequested { get; set; }

	public bool HasHitTheGround { get; set; }

	public bool IsOutOfTerrain { get; set; }

	public bool IsHangingAfterThrow
	{
		get
		{
			return this.Rod.CurrentTipPosition.y > this.RodSlot.Line.SecuredLineLength && this.IsPulledOut;
		}
	}

	public virtual bool IsPitchTooShort
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public bool Thrown { get; set; }

	public bool IsOnTipComplete { get; set; }

	public bool IsShowingComplete { get; set; }

	public virtual bool IsKinematic
	{
		get
		{
			return this.isKinematic;
		}
		set
		{
			this.isKinematic = value;
			this.phyObject.IsKinematic = value;
		}
	}

	public bool FishIsShowing
	{
		get
		{
			return this.Fish != null && (this.Fish.State == typeof(FishShowSmall) || this.Fish.State == typeof(FishShowBig));
		}
	}

	public bool ItemIsShowing { get; set; }

	public bool CheckGroundHit()
	{
		this.HasHitTheGround = false;
		this.IsOutOfTerrain = false;
		Vector3 position = this.RodSlot.Sim.TackleTipMass.Position;
		Vector3 vector;
		vector..ctor(position.x, position.y + 50f, position.z);
		RaycastHit raycastHit;
		if (Physics.Raycast(vector, Vector3.down, ref raycastHit, 100f, GlobalConsts.FishMask))
		{
			this.HasHitTheGround = raycastHit.point.y > 0f;
		}
		else
		{
			this.IsOutOfTerrain = true;
		}
		return this.HasHitTheGround || this.IsOutOfTerrain;
	}

	public void UpdateLureDepthStatus()
	{
		Mass tackleTipMass = this.Rod.TackleTipMass;
		if (tackleTipMass.IsLying)
		{
			this.tackleStatus = TackleStatus.OnBottom;
		}
		else if (tackleTipMass.Position.y > -0.1f)
		{
			this.tackleStatus = TackleStatus.OnSurface;
		}
		else if (tackleTipMass.Position.y > -0.5f)
		{
			this.tackleStatus = TackleStatus.NearSurface;
		}
		else if (tackleTipMass.Position.y - tackleTipMass.GroundHeight < 0.5f)
		{
			this.tackleStatus = TackleStatus.NearBottom;
		}
		else
		{
			this.tackleStatus = TackleStatus.MidWater;
		}
		if (this.lurePositionHandler == null)
		{
			this.lurePositionHandler = ShowHudElements.Instance.FishingHndl.LurePositionHandlerContinuous;
		}
		else if (tackleTipMass.Position.y > 0.05f)
		{
			this.lurePositionHandler.Refresh(1f, 0f);
		}
		else
		{
			Vector3 vector;
			if (tackleTipMass is PointOnRigidBody)
			{
				vector = (tackleTipMass as PointOnRigidBody).RigidBody.Rotation * Vector3.forward;
			}
			else
			{
				vector..ctor(1f, 0.5f * Mathf.Sign(tackleTipMass.Velocity.y) * Mathf.Sqrt(Mathf.Abs(tackleTipMass.Velocity.y * 4f)));
			}
			float num = Mathf.Clamp01((tackleTipMass.Position.y - tackleTipMass.GroundHeight) / Mathf.Abs(tackleTipMass.GroundHeight));
			float num2 = 0.42857f * (5.33333f * num * num * num - 8f * num * num + 5f * num);
			float num3 = Mathf.Atan2(vector.y, Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z));
			this.lurePositionHandler.Refresh(num2, num3);
		}
	}

	private void ShowTackleStatusChange()
	{
		FishingHandler fishingHndl = ShowHudElements.Instance.FishingHndl;
		switch (this.tackleStatus)
		{
		case TackleStatus.OnBottom:
			fishingHndl.LurePositionHandler.Refresh(LurePositionHandler.LurePositionEnum.OnBottom, this.tackleMoveDir);
			return;
		case TackleStatus.NearBottom:
			fishingHndl.LurePositionHandler.Refresh(LurePositionHandler.LurePositionEnum.NearBottom, this.tackleMoveDir);
			return;
		case TackleStatus.MidWater:
			fishingHndl.LurePositionHandler.Refresh(LurePositionHandler.LurePositionEnum.Middle, this.tackleMoveDir);
			return;
		case TackleStatus.NearSurface:
			fishingHndl.LurePositionHandler.Refresh(LurePositionHandler.LurePositionEnum.NearSurface, this.tackleMoveDir);
			return;
		case TackleStatus.OnSurface:
			fishingHndl.LurePositionHandler.Refresh(LurePositionHandler.LurePositionEnum.OnSurface, this.tackleMoveDir);
			return;
		default:
			return;
		}
	}

	public void EscapeFish()
	{
		this.AttackingFish = null;
		if (this.fish == null)
		{
			return;
		}
		this.fish.Escape();
		this.fish = null;
		this.FishTemplate = null;
		this.StrikeTimeEnd = 0f;
		this.Rod.ResetAppliedForce();
	}

	private protected bool EnterWater { protected get; private set; }

	protected bool HasSurfaceCollision()
	{
		bool flag = (double)this.transform.position.y < 0.01;
		this.EnterWater = flag;
		bool flag2 = this.wasInWater != flag;
		this.wasInWater = flag;
		return flag2;
	}

	private void updateSurfaceEffects()
	{
		if (this.Rod.TackleTipMass == null)
		{
			return;
		}
		float magnitude = (this.Rod.TackleTipMass.Position - this.prevTackleMassPos).magnitude;
		this.splashPhase += magnitude;
		float num = magnitude / Time.deltaTime;
		float num2 = Mathf.Abs(this.transform.position.y);
		if (num2 < this.SurfaceFxDepthDelta && num > this.SurfaceFxMinVelocity)
		{
			this.DisturbWater(this.transform.position, 0.3f, this.SurfaceFxWaterDisturbance * Mathf.Pow(num2 / this.SurfaceFxDepthDelta, 0.1f));
		}
		if (this.SurfaceFxSplashPeriod > 0f && this.SurfaceFxSplashSize > 0f && num > this.SurfaceFxMinVelocity && this.splashPhase > this.SurfaceFxSplashPeriod * Mathf.Lerp(0.8f, 1f, Random.value) && Mathf.Abs(this.transform.position.y) < this.SurfaceFxDepthDelta)
		{
			this.CreateTackleInSplash(this.transform.position + Random.insideUnitSphere * 0.05f, this.SurfaceFxSplashSize);
			this.splashPhase = 0f;
		}
		this.prevTackleMassPos = this.Rod.TackleTipMass.Position;
	}

	public void CheckSurfaceCollisions()
	{
		bool flag = this.HasSurfaceCollision();
		if (flag)
		{
			if (this.EnterWater)
			{
				RandomSounds.PlaySoundAtPoint("Sounds/Actions/Lure/Lure_Dipping", this.transform.position, 0.125f, false);
			}
			else
			{
				RandomSounds.PlaySoundAtPoint("Sounds/Actions/Lure/Lure_Out", this.transform.position, 0.125f, false);
			}
		}
		this.updateSurfaceEffects();
	}

	public void TackleIn(float size = 3f)
	{
		this.TackleIn(this.transform.position, size);
	}

	public void TackleOut(float size = 1f)
	{
		this.TackleOut(this.transform.position, size);
	}

	public void OnFinishThrowing()
	{
		this.TackleIn(this.transform.position, 3f);
	}

	public void TackleIn(Vector3 position, float size = 3f)
	{
		if (!this._isVisible)
		{
			return;
		}
		Vector3 vector;
		vector..ctor(position.x, 0f, position.z);
		this.CreateTackleInSplash(vector, size);
		this.CreateTackleInSound(vector, size);
	}

	public void TackleOut(Vector3 position, float size = 1f)
	{
		if (!this._isVisible)
		{
			return;
		}
		Vector3 vector;
		vector..ctor(position.x, 0f, position.z);
		this.CreateTackleOutSplash(vector, size);
		this.CreateTackleOutSound(vector, size);
	}

	protected virtual void CreateTackleInSplash(Vector3 position, float size)
	{
		if (GameFactory.Player != null)
		{
			DynWaterParticlesController.CreateSplash(GameFactory.Player.CameraController.transform, position, "2D/Splashes/pSplash_universal", size, 1f, true, true, 1);
		}
	}

	protected virtual void CreateTackleInSound(Vector3 position, float size)
	{
		RandomSounds.PlaySoundAtPoint("Sounds/Actions/Lure/Lure_Splash_Big", position, size, false);
	}

	protected virtual void CreateTackleOutSplash(Vector3 position, float size)
	{
		if (GameFactory.Player != null)
		{
			DynWaterParticlesController.CreateSplash(GameFactory.Player.CameraController.transform, position, "2D/Splashes/pSplash_universal", size, 1f, true, true, 1);
		}
	}

	protected virtual void CreateTackleOutSound(Vector3 position, float size)
	{
		RandomSounds.PlaySoundAtPoint("Sounds/Actions/Lure/Lure_Out", position, size * 0.25f, false);
	}

	public ResourceRequest AddItemAsync(int itemId, string asset, string name, string category)
	{
		this.IsHitched = false;
		this.UnderwaterItemName = name;
		this.UnderwaterItemCategory = category;
		this.UnderwaterItemId = itemId;
		this.Rod.Reel.SetFightMode();
		this.UnderwaterItemAsyncRequest = Resources.LoadAsync<GameObject>(asset);
		return this.UnderwaterItemAsyncRequest;
	}

	public void SpawnLoadedItem()
	{
		if (this.UnderwaterItem == null)
		{
			if (this.UnderwaterItemAsyncRequest == null || !this.UnderwaterItemAsyncRequest.isDone)
			{
				return;
			}
			if (this.UnderwaterItemId == 0)
			{
				Debug.LogError("Underwater item has not been added.");
				return;
			}
			this.UnderwaterItem = Object.Instantiate<GameObject>(this.UnderwaterItemAsyncRequest.asset as GameObject).GetComponent<UnderwaterItemController>();
		}
		this.UnderwaterItemAsyncRequest = null;
		if (this.UnderwaterItem != null)
		{
			this.UnderwaterItem.transform.position = Vector3.zero;
			this.UnderwaterItem.transform.rotation = Quaternion.identity;
			this.UnderwaterItem.Init(this.UnderwaterItemId, this.Rod.TackleTipMass.Position - this.UnderwaterItem.HookTransform.position, UserBehaviours.FirstPerson, this.RodSlot, null);
		}
	}

	public void AddItem(int itemId, string asset, string name, string category)
	{
		this.IsHitched = false;
		GameObject gameObject = Resources.Load<GameObject>(asset);
		this.UnderwaterItemName = name;
		this.UnderwaterItemCategory = category;
		this.UnderwaterItem = Object.Instantiate<GameObject>(gameObject).GetComponent<UnderwaterItemController>();
		this.UnderwaterItemId = itemId;
		if (this.UnderwaterItem != null)
		{
			this.UnderwaterItem.Init(itemId, this.RodSlot.Sim.TackleTipMass.Position - this.UnderwaterItem.HookTransform.position, UserBehaviours.FirstPerson, this.RodSlot, null);
		}
		this.RodSlot.Reel.SetFightMode();
	}

	public void AddItem(int itemId, GameObject itemObject, string name, string category, bool doInit = false)
	{
		this.UnderwaterItemName = name;
		this.UnderwaterItemCategory = category;
		this.UnderwaterItemId = itemId;
		itemObject.transform.rotation = Quaternion.identity;
		this.UnderwaterItem = Object.Instantiate<GameObject>(itemObject).GetComponent<UnderwaterItemController>();
		this.UnderwaterItem.SetItemId(itemId);
		if (!doInit)
		{
			this.UnderwaterItem.transform.parent = this.transform;
		}
		this.UnderwaterItem.transform.localPosition = Vector3.zero;
		this.UnderwaterItem.transform.rotation = Quaternion.identity;
		if (this.UnderwaterItem != null && doInit)
		{
			this.UnderwaterItem.Init(itemId, this.RodSlot.Sim.TackleTipMass.Position - this.UnderwaterItem.HookTransform.position, UserBehaviours.FirstPerson, this.RodSlot, null);
		}
	}

	public void FinishWithItem()
	{
		if (this.RodSlot.Reel != null)
		{
			this.RodSlot.Reel.SetDragMode();
		}
		this.IsShowing = false;
		Object.Destroy(this.UnderwaterItem.gameObject);
		this.UnderwaterItemCategory = null;
		this.CaughtItem = null;
		this.UnderwaterItemName = null;
		this.UnderwaterItem = null;
		this.UnderwaterItemId = 0;
	}

	public void SetForward(GameObject gameObject, Vector3 position, Vector3 direction)
	{
		if (gameObject != null)
		{
			gameObject.transform.position = position;
			Quaternion quaternion = default(Quaternion);
			quaternion.SetFromToRotation(gameObject.transform.forward, direction);
			gameObject.transform.rotation = quaternion * gameObject.transform.rotation;
		}
	}

	public void AddUncuttableLeader(MonoBehaviour controller, Vector3 position, Quaternion extraRotation)
	{
		GameObject gameObject = (GameObject)Resources.Load("Tackle/Sinkers/Swivels/pSwivel", typeof(GameObject));
		if (gameObject != null)
		{
			this.SwivelObject = Object.Instantiate<GameObject>(gameObject, position, controller.transform.rotation);
			this.Swivel = this.SwivelObject.GetComponent<RigidBodyController>();
			this._swivelRenderer = RenderersHelper.GetRendererForObject(this.SwivelObject.transform);
		}
		GameObject gameObject2 = (GameObject)Resources.Load("Tackle/Sinkers/Swivels/pShackle", typeof(GameObject));
		if (gameObject2 != null)
		{
			this.ShackleObject = Object.Instantiate<GameObject>(gameObject2, position, controller.transform.rotation);
			this.Shackle = this.ShackleObject.GetComponent<RigidBodyController>();
			this.ShackleShift = Vector3.Scale(this.Shackle.bottomLineAnchor.localPosition - this.Shackle.topLineAnchor.localPosition, this.Shackle.transform.localScale);
			this.AttachToTackle(this.ShackleObject, controller, position, extraRotation);
		}
	}

	public void AttachToTackle(GameObject obj, MonoBehaviour controller, Vector3 position, Quaternion extraRotation)
	{
		obj.transform.rotation = controller.transform.rotation;
		obj.transform.rotation *= extraRotation;
		obj.transform.position = position;
		Transform transform = obj.transform.Find("bottom");
		if (transform != null)
		{
			obj.transform.position += obj.transform.position - transform.position;
		}
		obj.transform.parent = controller.transform;
		this.AttachedObject = obj;
	}

	public void DetachFromTackle()
	{
		if (this.AttachedObject != null)
		{
			this.AttachedObject.transform.parent = null;
		}
		this.AttachedObject = null;
	}

	protected TackleControllerBase _owner;

	protected IAssembledRod _rodAssembly;

	public Mass lineEnd;

	public Transform lineEndAnchor;

	public Mass leaderHitch;

	public Transform leaderHitchAnchor;

	public Transform leaderEndAnchor;

	public bool IsVisibleLeader = true;

	public Mass leashHitch;

	public Transform leashHitchAnchor;

	public Mass leashEnd;

	public Transform leashEndAnchor;

	protected bool _isVisible = true;

	protected Type initialState;

	protected SkinnedMeshRenderer _swivelRenderer;

	internal TackleThrowData throwData;

	internal RandomSounds tackleSounds;

	protected float? MaxSpeed;

	protected IFishController fish;

	private float mass;

	private float windage;

	public const float ShowGripInAnimDurationFactor = 0.3f;

	public float SurfaceFxMinVelocity = 0.5f;

	public float SurfaceFxWaterDisturbance = 1f;

	public float SurfaceFxSplashSize;

	public float SurfaceFxSplashPeriod;

	public float SurfaceFxDepthDelta = 0.01f;

	private bool isKinematic;

	private const float MinVerticalSpeed = 0.05f;

	private const float MinMoveSpeed = 0.15f;

	private TackleStatus tackleStatus;

	private TackleMoveDir tackleMoveDir;

	private LurePositionHandlerContinuous lurePositionHandler;

	private bool wasInWater;

	private Vector3 prevTackleMassPos;

	private float splashPhase;
}
