using System;
using ObjectModel;
using Phy;
using UnityEngine;

public class FishOnPodBehaviour : IFishController
{
	public FishOnPodBehaviour(RodOnPodBehaviour owner, Vector3 position, Fish fishTemplate, RodOnPodBehaviour.TransitionData transitionData = null)
	{
		this.fsm = new Fsm<IFishController>("FishOnPod", this, true);
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
		this.RodOnPod = owner;
		this.Tackle = this.RodOnPod.Tackle;
		float num = fishTemplate.Force.Value * 9.81f;
		if (transitionData != null)
		{
			position = transitionData.fishPosition;
		}
		RodOnPodBehaviour.RodPodSim.AddFish(owner.RodOnPodObject, position, fishTemplate.Weight, num, fishTemplate.Speed.Value, fishTemplate.Length);
		this.FishTemplate = fishTemplate;
		this.spawnTimestamp = Time.time;
		global::FishAi fishAi = new global::FishAi(fishTemplate.Portrait, new DummyFishAnimationController(), owner.TackleTipMass, this.RodSlot);
		global::FishAi fishAi2 = fishAi;
		float? activity = fishTemplate.Activity;
		fishAi2.Activity = ((activity == null) ? 1f : activity.Value);
		global::FishAi fishAi3 = fishAi;
		float? stamina = fishTemplate.Stamina;
		fishAi3.Stamina = ((stamina == null) ? 1f : stamina.Value);
		fishAi.Force = num;
		fishAi.Speed = fishTemplate.Speed.Value;
		global::FishAi fishAi4 = fishAi;
		float? attackLure = fishTemplate.AttackLure;
		fishAi4.AttackLure = ((attackLure == null) ? 0.5f : attackLure.Value);
		fishAi.RetreatThreshold = fishTemplate.Length * 0.5f;
		fishAi.Mass = fishTemplate.Weight;
		this.ai = fishAi;
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
		this.ai.Start(owner.RodOnPodObject.fishObject);
		this.InstanceGuid = this.FishTemplate.InstanceId.Value;
		this.ai.CurrentBehavior = FishAiBehavior.None;
		if (transitionData != null)
		{
			GameFactory.FishSpawner.Replace(this);
			this.fsm.EnterState(transitionData.fishInitialState.GetType(), transitionData.fishInitialState);
			this.Behavior = transitionData.fishInitialBehavior;
			this.tpmId = transitionData.fishTpmId;
		}
		else
		{
			GameFactory.FishSpawner.Add(this);
			this.fsm.EnterState(typeof(FishSwim), null);
		}
		this.RodSlot.OnResetFishBiteConfirmRequest();
	}

	public TackleBehaviour Tackle { get; set; }

	public Mass HeadMass
	{
		get
		{
			return this.RodOnPod.RodOnPodObject.fishObject.Mouth;
		}
	}

	public Mass TailMass
	{
		get
		{
			return this.RodOnPod.RodOnPodObject.fishObject.Root;
		}
	}

	public PhyObject FishObject
	{
		get
		{
			return this.RodOnPod.RodOnPodObject.fishObject;
		}
	}

	public FishAiBehavior FishAIBehaviour
	{
		get
		{
			return (this.ai == null) ? FishAiBehavior.None : this.ai.CurrentBehavior;
		}
	}

	public Vector3 HeadRight
	{
		get
		{
			return Vector3.right;
		}
	}

	public Vector3 MouthPosition
	{
		get
		{
			return this.HeadMass.Position;
		}
	}

	public Vector3 ThroatPosition
	{
		get
		{
			return this.HeadMass.Position;
		}
	}

	public Fish CaughtFish { get; set; }

	public Fish FishTemplate { get; private set; }

	public RodOnPodBehaviour RodOnPod { get; private set; }

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
			return this.RodOnPod.RodSlot;
		}
	}

	public bool IsPassive
	{
		get
		{
			return this.ai.IsPassive;
		}
	}

	public bool IsBig
	{
		get
		{
			return this.FishTemplate.Weight > 0.906f;
		}
	}

	public bool IsGoingTo
	{
		get
		{
			return false;
		}
	}

	public FishController Owner
	{
		get
		{
			return null;
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

	public Type State
	{
		get
		{
			return this.fsm.CurrentStateType;
		}
	}

	public FsmBaseState<IFishController> StateInstance
	{
		get
		{
			return this.fsm.CurrentState;
		}
	}

	public bool IsTasting
	{
		get
		{
			return this.ai.IsMouthMagnetAttracting;
		}
	}

	public Guid InstanceGuid { get; set; }

	public bool IsHandsHoldCondition
	{
		get
		{
			return false;
		}
	}

	public bool IsShowing
	{
		get
		{
			return false;
		}
	}

	public int tpmId { get; set; }

	public float PredatorAttackDelay { get; private set; }

	public float PredatorHoldDelay { get; private set; }

	public float BiteTime { get; private set; }

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

	public bool IsPathCompleted
	{
		get
		{
			return this.ai.IsPathCompleted || this.State == typeof(FishEscape);
		}
	}

	public bool IsAttackDelayed
	{
		get
		{
			return this.ai.IsAttackDelayed;
		}
	}

	public bool IsFrozen { get; set; }

	public FishBehavior Behavior
	{
		get
		{
			return this._behavior;
		}
		set
		{
			this._behavior = value;
		}
	}

	public float DistanceToTackle
	{
		get
		{
			Vector3 position = this.RodOnPod.RodOnPodObject.hookMass.Position;
			Vector3 position2 = this.RodOnPod.RodOnPodObject.fishObject.Mouth.Position;
			Vector3 position3 = this.RodOnPod.RodOnPodObject.fishObject.Root.Position;
			float magnitude = (position2 - position).magnitude;
			float magnitude2 = (position2 - position3).magnitude;
			float magnitude3 = (position - position3).magnitude;
			if (magnitude3 > magnitude2)
			{
				return magnitude;
			}
			return -magnitude;
		}
	}

	public bool fishIsHooked { get; private set; }

	public global::FishAi ai { get; private set; }

	public float CurrentForce
	{
		get
		{
			return this.ai.CurrentForce;
		}
	}

	public float AttackLure
	{
		get
		{
			float? attackLure = this.FishTemplate.AttackLure;
			return (attackLure == null) ? 0.5f : attackLure.Value;
		}
	}

	public void Show()
	{
		throw new NotImplementedException("FishOnPod cannot be shown");
	}

	public void SetOnPod(bool isOnPod)
	{
	}

	public void SetVisibility(bool flag)
	{
	}

	public void Hook(float poolPeriod)
	{
		this.fishIsHooked = true;
		RodOnPodBehaviour.RodPodSim.HookFish(this.RodOnPod.RodOnPodObject);
		this.ai.Hook(poolPeriod);
		this.Behavior = FishBehavior.Hook;
	}

	public void Escape()
	{
		this.Behavior = FishBehavior.Go;
		RodOnPodBehaviour.RodPodSim.EscapeFish(this.RodOnPod.RodOnPodObject);
	}

	public void Bite()
	{
		this.magnet = RodOnPodBehaviour.RodPodSim.AddMagnet(this.RodOnPod.RodOnPodObject.hookMass, this.RodOnPod.RodOnPodObject.fishObject);
		this.ai.Bite(this.magnet, this.RodOnPod.TackleType == FishingRodSimulation.TackleType.Lure);
	}

	public void Swim()
	{
		this.ai.Swim();
	}

	public void PredatorSwim()
	{
		this.ai.PredatorSwim();
	}

	public void PredatorAttack()
	{
		this.ai.PredatorAttack();
	}

	public void Attack()
	{
		this.ai.Attack(this.BiteTime, this.RodOnPod.IsLureTackle);
	}

	public void KeepHookInMouth(float poolPeriod)
	{
		RodOnPodBehaviour.RodPodSim.HookFish(this.RodOnPod.RodOnPodObject);
		this.ai.KeepInMouth(poolPeriod);
	}

	public void Destroy()
	{
		GameFactory.FishSpawner.Remove(this);
		this.ai.Stop();
		this.Clean();
		this.CaughtFish = null;
		this.FishTemplate = null;
		this.RodOnPod.OnFishDestroy();
	}

	public void Clean()
	{
		if (RodOnPodBehaviour.RodPodSim != null)
		{
			RodOnPodBehaviour.RodPodSim.RemoveFish(this.RodOnPod.RodOnPodObject);
			if (this.magnet != null)
			{
				RodOnPodBehaviour.RodPodSim.DestroyMagnet(this.magnet);
				this.magnet = null;
			}
		}
	}

	public void SyncWithSim()
	{
	}

	public void Update()
	{
		this.ai.Update(Time.deltaTime, this.RodOnPod.RodOnPodObject.TipMass.AvgForce.magnitude, GameFactory.Player.transform.position, false);
		this.fsm.Update();
	}

	private Fsm<IFishController> fsm;

	private FishBehavior _behavior;

	private float spawnTimestamp;

	private bool confirmBiteSent;

	private bool strikeSent;

	private Magnet magnet;
}
