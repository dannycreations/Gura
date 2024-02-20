using System;
using Phy;
using UnityEngine;

public class RigidItemController : UnderwaterItemController
{
	protected override UnderwaterItemBehaviour CreateBehaviour(UserBehaviours behaviourType, FishingRodSimulation sim = null)
	{
		if (this._behaviour != null)
		{
			return this._behaviour;
		}
		if (behaviourType == UserBehaviours.FirstPerson)
		{
			return new RigidItem1stBehaviour(this, sim);
		}
		return new UnderwaterItem3rdBehaviour(this);
	}

	private void Start()
	{
		int num = 0;
		while (this.CenterTransform == null && num < base.transform.childCount)
		{
			Transform child = base.transform.GetChild(num++);
			if (child.name == "center")
			{
				this.CenterTransform = child;
			}
		}
		if (this._behaviour != null)
		{
			this._behaviour.Start();
		}
	}

	public float FrictionConstant = 0.3f;

	public float TorsionalFrictionConstant = 0.07f;

	public float Width = 0.1f;

	public float Depth = 0.1f;

	public Transform CenterTransform;
}
