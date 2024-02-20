using System;
using ObjectModel;
using Phy;
using UnityEngine;

public class LureOnPodBehaviour : TackleOnPodBehaviour, ILureBehaviour, ITackleBehaviour
{
	public LureOnPodBehaviour(TackleControllerBase owner, IAssembledRod rodAssembly, Transform anchor, RodOnPodBehaviour.TransitionData transitionData)
		: base(owner, rodAssembly, anchor, transitionData)
	{
		this.fsm = new Fsm<ILureBehaviour>("LureOnPod", this, true);
		this.fsm.RegisterState<LureOnTip>();
		this.fsm.RegisterState<LureIdlePitch>();
		this.fsm.RegisterState<LureFlying>();
		this.fsm.RegisterState<LurePitching>();
		this.fsm.RegisterState<LureFloating>();
		this.fsm.RegisterState<LureSwallowed>();
		this.fsm.RegisterState<LureHanging>();
		this.fsm.RegisterState<LureShowItem>();
		this.fsm.RegisterState<LureShowSmallFish>();
		this.fsm.RegisterState<LureShowBigFish>();
		this.fsm.RegisterState<LureHitched>();
		this.fsm.RegisterState<LureBroken>();
		this.fsm.RegisterState<LureHidden>();
		this.initialState = transitionData.tackleInitialState;
		base.IsHitched = transitionData.tackleInitialState == typeof(LureHitched);
		this.LureItem = rodAssembly.HookInterface as Lure;
	}

	public Lure LureItem { get; private set; }

	public override void Init()
	{
		base.Init();
		this.fsm.EnterState(this.initialState, null);
	}

	public override Type State
	{
		get
		{
			return this.fsm.CurrentStateType;
		}
	}

	protected LureController Owner
	{
		get
		{
			return this._owner as LureController;
		}
	}

	public Vector3 DirectionVector
	{
		get
		{
			return Vector3.up;
		}
	}

	public void OnEnterHangingState()
	{
		throw new NotImplementedException();
	}

	public void OnExitHangingState()
	{
		throw new NotImplementedException();
	}

	public void RealignMasses()
	{
		throw new NotImplementedException();
	}

	public void ResetDrag()
	{
	}

	public void SetFreeze(bool freeze)
	{
		throw new NotImplementedException();
	}

	public void SetHookMass(Mass m)
	{
		throw new NotImplementedException();
	}

	public void SetLureMasses(Mass m)
	{
		throw new NotImplementedException();
	}

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

	public override bool IsIdle
	{
		get
		{
			return this.State == typeof(LureOnTip) || this.State == typeof(LureIdlePitch);
		}
	}

	public override void OnFsmUpdate()
	{
		this.fsm.Update();
	}

	public override void OnLateUpdate()
	{
		base.OnLateUpdate();
		Vector3 velocity = base.rodOnPod.RodOnPodObject.tackleMass.Velocity;
		velocity.y = 0f;
		this.averageVelocity = Mathf.Lerp(this.averageVelocity, velocity.magnitude, Mathf.Exp(-Time.deltaTime * 10f));
		this.IsMoving = this.averageVelocity > 0.3f;
	}

	private Fsm<ILureBehaviour> fsm;

	private float averageVelocity;
}
