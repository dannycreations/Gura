using System;
using ObjectModel;
using Phy;
using UnityEngine;

public class FloatOnPodBehaviour : TackleOnPodBehaviour, IFloatBehaviour, ITackleBehaviour
{
	public FloatOnPodBehaviour(TackleControllerBase owner, IAssembledRod rodAssembly, Transform anchor, RodOnPodBehaviour.TransitionData transitionData)
		: base(owner, rodAssembly, anchor, transitionData)
	{
		this.BaitItem = rodAssembly.BaitInterface as Bait;
		this.IsBaitShown = this.BaitItem.Count > 0;
		base.Size = (this.Owner.waterMark.position - this._owner.bottomLineAnchor.position).magnitude;
		this.fsm = new Fsm<IFloatBehaviour>("FloatOnPod", this, true);
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
		this.initialState = transitionData.tackleInitialState;
		base.IsHitched = transitionData.tackleInitialState == typeof(FloatHitched);
		Vector3 vector = transitionData.tackleRotation * Vector3.up;
		vector.y = 0f;
		if (!Mathf.Approximately(0f, vector.magnitude))
		{
			this.originalTiltAxis = Vector3.Cross(Vector3.up, vector.normalized).normalized;
		}
		else
		{
			this.originalTiltAxis = Vector3.right;
		}
	}

	public override void Init()
	{
		base.Init();
		this.fsm.EnterState(this.initialState, null);
		this.originalDepth = base.rodOnPod.RodOnPodObject.tackleMass.Position.y - base.rodOnPod.RodOnPodObject.hookMass.Position.y;
	}

	protected FloatController Owner
	{
		get
		{
			return this._owner as FloatController;
		}
	}

	public float UserSetLeaderLength
	{
		get
		{
			return base.rodOnPod.LeaderLength;
		}
		set
		{
		}
	}

	public float LeaderLength
	{
		get
		{
			return base.rodOnPod.LeaderLength;
		}
		set
		{
		}
	}

	public Mass GetBobberMainMass
	{
		get
		{
			return base.rodOnPod.RodOnPodObject.tackleMass;
		}
	}

	public override bool IsLiveBait
	{
		get
		{
			return this.BaitItem.IsLive;
		}
	}

	public bool HookIsIdle { get; private set; }

	public Transform HookTransform
	{
		get
		{
			return null;
		}
	}

	public Transform BaitTransform
	{
		get
		{
			return null;
		}
	}

	public Bait BaitItem { get; private set; }

	public override bool IsBaitShown { get; protected set; }

	public override Type State
	{
		get
		{
			return this.fsm.CurrentStateType;
		}
	}

	public bool CheckPitchIsTooShort()
	{
		throw new NotImplementedException();
	}

	public Mass GetHookMass(int index)
	{
		return base.rodOnPod.RodOnPodObject.hookMass;
	}

	public Mass GetKinematicLeaderMass(int index = 0)
	{
		throw new NotImplementedException();
	}

	public void HookSetFreeze(bool freeze)
	{
		throw new NotImplementedException();
	}

	public void SetHookMass(int index, Mass m)
	{
		throw new NotImplementedException();
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

	public override bool IsIdle
	{
		get
		{
			return this.State == typeof(FloatOnTip) || this.State == typeof(FloatIdlePitch);
		}
	}

	public override void OnFsmUpdate()
	{
		this.fsm.Update();
	}

	public override void OnLateUpdate()
	{
		base.OnLateUpdate();
		float num = base.rodOnPod.RodOnPodObject.tackleMass.Position.y - base.rodOnPod.RodOnPodObject.hookMass.Position.y;
		float num2 = 0f;
		if (num > 0f && num < base.rodOnPod.LeaderLength)
		{
			num2 = Mathf.Lerp(90f, 0f, num / base.rodOnPod.LeaderLength);
		}
		Vector3 forward = base.rodOnPod.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		base.transform.rotation = Quaternion.LookRotation(forward, (base.rodOnPod.RodOnPodObject.tackleMass.Position - base.rodOnPod.RodOnPodObject.hookMass.Position).normalized);
		base.transform.rotation = Quaternion.AngleAxis(num2, this.originalTiltAxis) * base.transform.rotation;
		this.HookIsIdle = HookController.DetectHookMotion(base.rodOnPod.RodOnPodObject.tackleMass.Position, base.rodOnPod.RodOnPodObject.hookMass.Position, ref this.idleCycles, ref this.priorShift);
	}

	private Fsm<IFloatBehaviour> fsm;

	private Vector3 originalTiltAxis;

	private float originalDepth;

	private float priorShift;

	private int idleCycles;
}
