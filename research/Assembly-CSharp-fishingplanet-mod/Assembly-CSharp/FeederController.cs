using System;
using System.Collections.Generic;
using Phy;
using TPM;
using UnityEngine;

public class FeederController : TackleControllerBase
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

	public RigidBody RigidBody
	{
		get
		{
			IFeederBehaviour feederBehaviour = base.Behaviour as IFeederBehaviour;
			return (feederBehaviour == null) ? null : feederBehaviour.RigidBody;
		}
	}

	public override TackleBehaviour SetBehaviour(UserBehaviours behaviourType, IAssembledRod rodAssembly, GameFactory.RodSlot slot, RodOnPodBehaviour.TransitionData transitionData = null)
	{
		base.SetBehaviour(behaviourType, rodAssembly, slot, transitionData);
		if (this._behaviour == null)
		{
			if (behaviourType == UserBehaviours.FirstPerson)
			{
				this._behaviour = new Feeder1stBehaviour(this, rodAssembly as AssembledRod, transitionData);
			}
			else if (behaviourType == UserBehaviours.ThirdPerson)
			{
				this._behaviour = new Feeder3rdBehaviour(this, rodAssembly);
			}
			else if (behaviourType == UserBehaviours.RodPod)
			{
				this._behaviour = new FeederOnPodBehaviour(this, rodAssembly, this.topLineAnchor, transitionData);
			}
		}
		return this._behaviour;
	}

	protected override void OnUpdate()
	{
		if (GameFactory.GameIsPaused)
		{
			return;
		}
		if (this.RigidBody != null)
		{
			this.RigidBody.UpdateCollisions(this.CurrentCollisions, GlobalConsts.RigidBodyMask);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject != null && collision.gameObject.layer != GlobalConsts.PhotoModeLayer)
		{
			this.CurrentCollisions[RigidBody.CollisionHash(collision)] = collision;
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject != null && collision.gameObject.layer != GlobalConsts.PhotoModeLayer)
		{
			this.CurrentCollisions[RigidBody.CollisionHash(collision)] = collision;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject != null && collision.gameObject.layer != GlobalConsts.PhotoModeLayer)
		{
			List<int> list = new List<int>();
			foreach (int num in this.CurrentCollisions.Keys)
			{
				if (this.CurrentCollisions[num].gameObject == collision.gameObject)
				{
					list.Add(num);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				this.CurrentCollisions.Remove(list[i]);
			}
		}
	}

	protected void StopFeeder()
	{
		if (this.RigidBody != null)
		{
			this.RigidBody.CollisionPlanesCount = 0;
			if (this.CurrentCollisions != null)
			{
				this.CurrentCollisions.Clear();
			}
			this.RigidBody.StopMass();
			this.RigidBody.Reset();
		}
	}

	public float FrictionConstant = 0.3f;

	public float TorsionalFrictionConstant = 0.07f;

	public float Width = 0.1f;

	public float Depth = 0.1f;

	public float MassValue = 0.1f;

	public float SolidFactor = 1f;

	public float Buoyancy = -1f;

	public float BounceFactor;

	public float ExtrudeFactor = 0.01f;

	public float CollisionFrictionFactor = 0.1f;

	public float FeederLineLength = 0.2f;

	public Vector3 AxialDragFactors = new Vector3(15f, 15f, 10f);

	public bool DebugChangeMassValue;

	public readonly Vector2 MassToSolidFactorLow = new Vector2(0.015f, 0.017f);

	public readonly Vector2 MassToSolidFactorHigh = new Vector2(0.25f, 0.1f);

	public Transform CenterTransform;

	public GameObject ChumObject;

	public GameObject SecondaryTackleObject;

	public Dictionary<int, Collision> CurrentCollisions = new Dictionary<int, Collision>();
}
