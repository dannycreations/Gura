using System;
using TPM;
using UnityEngine;

public class ReelController : MonoBehaviour
{
	public ReelBehaviour Behaviour
	{
		get
		{
			return this._behaviour;
		}
	}

	public ReelBehaviour SetBehaviour(UserBehaviours behaviourType, IAssembledRod rodAssembly, GameFactory.RodSlot slot, RodOnPodBehaviour.TransitionData transitionData)
	{
		if (this._behaviour == null)
		{
			if (behaviourType == UserBehaviours.FirstPerson)
			{
				this._behaviour = new Reel1stBehaviour(this, rodAssembly, slot, transitionData);
			}
			else if (behaviourType == UserBehaviours.ThirdPerson)
			{
				this._behaviour = new Reel3rdBehaviour(this, rodAssembly, slot);
			}
			else if (behaviourType == UserBehaviours.None)
			{
				this._behaviour = new ReelBehaviour(this, rodAssembly, slot, transitionData);
			}
		}
		return this._behaviour;
	}

	private void Start()
	{
		if (this._behaviour != null)
		{
			this._behaviour.Start(this);
		}
	}

	private void Update()
	{
		if (this._behaviour != null)
		{
			this._behaviour.Update();
		}
	}

	public float minFriction;

	public float maxFriction = 1f;

	public int numFrictionSections = 6;

	public float speed = 0.5f;

	public int numReelSpeedSections = 4;

	public float maxLoad;

	public const int FrictionSpeed6 = 6;

	public const int FrictionSpeed8 = 8;

	public const int FrictionSpeed12 = 12;

	public const int DefaultFrictionSectionCount = 6;

	public const float ForceIncreaseOnReeling = 1f;

	public readonly float LoosenFrictionForce = 0.1f;

	private ReelBehaviour _behaviour;
}
