using System;
using Phy;
using UnityEngine;

public abstract class UnderwaterItemController : MonoBehaviour
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

	public UnderwaterItemBehaviour Behaviour
	{
		get
		{
			return this._behaviour;
		}
	}

	public GameFactory.RodSlot RodSlot { get; private set; }

	public int ItemId
	{
		get
		{
			return this._itemId;
		}
	}

	public int TpmId
	{
		get
		{
			return this._tpmId;
		}
	}

	public void SetTpmId(int id)
	{
		this._tpmId = id;
	}

	public void SetItemId(int itemId)
	{
		this._itemId = itemId;
	}

	public virtual UnderwaterItemBehaviour Init(int itemId, Vector3 additionVector, UserBehaviours behaviourType, GameFactory.RodSlot rodSlot, FishingRodSimulation sim = null)
	{
		this._itemId = itemId;
		base.transform.position += additionVector;
		this.RodSlot = rodSlot;
		this._behaviour = this.CreateBehaviour(behaviourType, sim);
		if (this._behaviour != null && behaviourType == UserBehaviours.FirstPerson)
		{
			Mass mass = this.RodSlot.Sim.TackleTipMass;
			if (mass is PointOnRigidBody)
			{
				mass = (mass as PointOnRigidBody).RigidBody.PriorSpring.Mass1;
			}
			else
			{
				mass = mass.PriorSpring.Mass1;
			}
			Spring spring = new Spring(mass, this._behaviour.phyObject.ForHookMass, 1000f, 0.005f, 0.07f);
			this._behaviour.phyObject.RodTipMass = this.RodSlot.Sim.RodTipMass;
			this._behaviour.phyObject.Connections.Add(spring);
			this.RodSlot.Sim.Connections.Add(spring);
		}
		return this._behaviour;
	}

	protected abstract UnderwaterItemBehaviour CreateBehaviour(UserBehaviours behaviourType, FishingRodSimulation sim);

	private void Start()
	{
		int num = 0;
		while (this.HookTransform == null && num < base.transform.childCount)
		{
			Transform child = base.transform.GetChild(num++);
			if (child.name == "hook")
			{
				this.HookTransform = child;
			}
		}
		if (this._behaviour != null)
		{
			this._behaviour.Start();
		}
	}

	private void LateUpdate()
	{
		if (this.NeedUpdate && this._behaviour != null)
		{
			this._behaviour.LateUpdate();
		}
	}

	private void OnDestroy()
	{
		if (this._behaviour != null)
		{
			this._behaviour.OnDestroy();
		}
	}

	public void DeleteBehaviour()
	{
		this._behaviour = null;
	}

	public const float ForHookLineLength = 0.005f;

	public const float ForHookLineFrictionConstant = 0.07f;

	public Transform HookTransform;

	public float MassValue = 0.3f;

	public float Buoyancy = -0.2f;

	public float AirDragConstant = 0.02f;

	public bool NeedUpdate = true;

	protected UnderwaterItemBehaviour _behaviour;

	protected int _itemId;

	private int _tpmId;
}
