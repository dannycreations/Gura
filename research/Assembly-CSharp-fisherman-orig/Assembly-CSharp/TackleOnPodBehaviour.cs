using System;
using UnityEngine;

public class TackleOnPodBehaviour : TackleBehaviour
{
	public TackleOnPodBehaviour(TackleControllerBase owner, IAssembledRod rodAssembly, Transform anchor, RodOnPodBehaviour.TransitionData transitionData)
		: base(owner, rodAssembly, transitionData)
	{
		this.anchor = anchor;
		this.throwData = new TackleThrowData();
		base.IsAttackFinished = transitionData.tackleAttackFinished;
		base.IsFinishAttackRequested = transitionData.tackleAttackFinishedRequested;
		base.UnderwaterItemAsyncRequest = transitionData.underwaterItemAsyncRequest;
		if (transitionData.underwaterItemObject != null)
		{
			base.UnderwaterItem = transitionData.underwaterItemObject.GetComponent<UnderwaterItemController>();
			base.UnderwaterItem.DeleteBehaviour();
		}
	}

	protected RodOnPodBehaviour rodOnPod
	{
		get
		{
			return (RodOnPodBehaviour)this._owner.Behaviour.Rod;
		}
	}

	public override Vector3 Position
	{
		get
		{
			return this.rodOnPod.RodOnPodObject.hookMass.Position;
		}
	}

	public override Type State
	{
		get
		{
			if (this.rodOnPod.IsFloatTackle)
			{
				if (this.rodOnPod.FishState != RodOnPodBehaviour.PodFishState.Hooked)
				{
					return typeof(FloatFloating);
				}
				return typeof(FloatSwallowed);
			}
			else
			{
				if (this.rodOnPod.FishState != RodOnPodBehaviour.PodFishState.Hooked)
				{
					return typeof(LureFloating);
				}
				return typeof(LureSwallowed);
			}
		}
	}

	public override bool IsThrowing
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public override bool IsInWater
	{
		get
		{
			return this.rodOnPod.RodOnPodObject.tackleMass.Position.y <= 0f;
		}
	}

	public override bool IsLying
	{
		get
		{
			return !base.IsMovingWobbler && this.rodOnPod.RodOnPodObject.hookMass.IsLying;
		}
	}

	public override void CreateTackle(Vector3 direction)
	{
		base.TackleType = this.rodOnPod.TackleType;
	}

	public override void OnLateUpdate()
	{
		base.transform.position += this.rodOnPod.RodOnPodObject.tackleMass.Position - this.anchor.position;
		Vector3 forward = this.rodOnPod.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		base.transform.rotation = Quaternion.LookRotation((this.rodOnPod.CurrentTipPosition - this.rodOnPod.RodOnPodObject.tackleMass.Position).normalized, forward);
	}

	public override void Hitch(Vector3 position)
	{
		this.rodOnPod.RodOnPodObject.hookMass.IsKinematic = true;
		this.rodOnPod.RodOnPodObject.hookMass.StopMass();
		this.rodOnPod.RodOnPodObject.hookMass.Position = position;
	}

	public override void UnHitch()
	{
		this.rodOnPod.RodOnPodObject.hookMass.StopMass();
		this.rodOnPod.RodOnPodObject.hookMass.IsKinematic = false;
	}

	private Transform anchor;
}
