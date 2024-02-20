using System;
using System.Linq;
using ObjectModel;
using Phy;
using UnityEngine;

public class FeederOnPodBehaviour : TackleOnPodBehaviour, IFeederBehaviour, IFloatBehaviour, ITackleBehaviour
{
	public FeederOnPodBehaviour(TackleControllerBase owner, IAssembledRod rodAssembly, Transform anchor, RodOnPodBehaviour.TransitionData transitionData)
		: base(owner, rodAssembly, anchor, transitionData)
	{
		this.BaitItem = rodAssembly.BaitInterface as Bait;
		this.IsBaitShown = this.BaitItem.Count > 0;
		this.fsm = new Fsm<IFeederBehaviour>("FeederOnPod", this, true);
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
		this.initialState = transitionData.tackleInitialState;
		base.IsHitched = transitionData.tackleInitialState == typeof(FeederHitched);
		this.isBottom = rodAssembly.SinkerInterface != null && !(rodAssembly.SinkerInterface is Feeder);
	}

	public FeederController Controller
	{
		get
		{
			return this._owner as FeederController;
		}
	}

	public override void Init()
	{
		base.Init();
		this.fsm.EnterState(this.initialState, null);
	}

	public RigidBody RigidBody
	{
		get
		{
			return base.rodOnPod.RodOnPodObject.feederBody;
		}
	}

	public FeederTackleObject FeederObject
	{
		get
		{
			return null;
		}
	}

	public Feeding[] Feedings { get; protected set; }

	public IChum Chum
	{
		get
		{
			return this._rodAssembly.ChumInterface;
		}
	}

	public IChum[] ChumAll
	{
		get
		{
			return this._rodAssembly.ChumInterfaceAll;
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

	public override bool IsLiveBait
	{
		get
		{
			return this.BaitItem.IsLive;
		}
	}

	public override bool IsBottom
	{
		get
		{
			return this.isBottom;
		}
	}

	public bool IsFeederLying
	{
		get
		{
			return this.RigidBody != null && this.RigidBody.IsLying;
		}
	}

	public Mass GetBobberMainMass
	{
		get
		{
			return base.rodOnPod.RodOnPodObject.tackleMass;
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

	public override bool IsIdle
	{
		get
		{
			return this.State == typeof(FeederOnTip) || this.State == typeof(FeederIdlePitch);
		}
	}

	public Feeding[] InitFeeding()
	{
		if (this.ChumAll != null)
		{
			if (this._rodAssembly.RodTemplate == RodTemplate.Spod)
			{
				return null;
			}
			this.Feedings = this.ChumAll.Select((IChum c) => new Feeding
			{
				IsNew = false,
				ItemId = c.InstanceId.Value,
				IsDestroyed = false,
				IsExpired = c.IsExpired
			}).ToArray<Feeding>();
			foreach (Feeding feeding in this.Feedings)
			{
				GameFactory.Player.AddFeeding(feeding);
			}
		}
		return this.Feedings;
	}

	public void UpdateFeeding(Vector3 position)
	{
		if (this.Feedings != null)
		{
			foreach (Feeding feeding in this.Feedings)
			{
				feeding.IsNew = false;
				feeding.Position = new Point3(position.x, position.y, position.z);
				GameFactory.Player.AddFeeding(feeding);
			}
		}
	}

	public void SetFilled(bool isFilled)
	{
		this.IsFilled = isFilled;
	}

	public bool IsFilled { get; private set; }

	public void DestroyFeeding()
	{
		if (this.Feedings != null)
		{
			foreach (Feeding feeding in this.Feedings)
			{
				feeding.IsNew = false;
				feeding.IsDestroyed = true;
				GameFactory.Player.AddFeeding(feeding);
			}
			this.Feedings = null;
		}
	}

	public override void OnFsmUpdate()
	{
		this.fsm.Update();
	}

	public override void OnLateUpdate()
	{
		this.HookIsIdle = HookController.DetectHookMotion(base.rodOnPod.RodOnPodObject.tackleMass.Position, base.rodOnPod.RodOnPodObject.hookMass.Position, ref this.idleCycles, ref this.priorShift);
		base.transform.rotation = this.RigidBody.Rotation;
		base.transform.position += this.RigidBody.Position - this.Controller.CenterTransform.position;
	}

	private Fsm<IFeederBehaviour> fsm;

	private new Type initialState;

	private bool isBottom;

	private float priorShift;

	private int idleCycles;
}
