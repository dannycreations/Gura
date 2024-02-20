using System;
using UnityEngine;

public class BellController : MonoBehaviour
{
	public Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
	}

	public Quaternion Rotation
	{
		get
		{
			return base.transform.rotation;
		}
	}

	public BellBehaviour Behaviour
	{
		get
		{
			return this.behaviour;
		}
	}

	public BellBehaviour SetBehaviour(UserBehaviours behaviourType, IAssembledRod rodAssembly, GameFactory.RodSlot rodSlot)
	{
		if (this.behaviour == null)
		{
			if (behaviourType == UserBehaviours.FirstPerson)
			{
				this.behaviour = new Bell1stBehaviour(this, rodAssembly, rodSlot, null);
			}
			else if (behaviourType == UserBehaviours.ThirdPerson)
			{
				this.behaviour = new Bell3rdBehaviour(this, rodSlot);
			}
			else if (behaviourType == UserBehaviours.RodPod)
			{
				this.behaviour = new BellOnPodBehaviour(this, rodSlot, rodAssembly);
			}
		}
		return this.behaviour;
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		if (this.behaviour != null)
		{
			this.behaviour.Destroy();
		}
		this.behaviour = null;
	}

	public int BellPosFromTip = 1;

	public int JingleCountThreshold = 3;

	public float MinJinglePeriod = 0.1f;

	public float MaxJinglePeriod = 0.2f;

	public float NormalSensitivity = 1f;

	public float HighSensitivity = 10f;

	public float[] FactorClips;

	public AudioClip[] Clips;

	public Transform Root;

	public Transform SpringOne;

	public Transform SpringTwo;

	public Transform BallOne;

	public Transform BallTwo;

	private BellBehaviour behaviour;
}
