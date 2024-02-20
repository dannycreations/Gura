using System;
using Phy;

public class PlantController : UnderwaterItemController
{
	protected override UnderwaterItemBehaviour CreateBehaviour(UserBehaviours behaviourType, FishingRodSimulation sim)
	{
		if (this._behaviour != null)
		{
			return this._behaviour;
		}
		if (behaviourType == UserBehaviours.FirstPerson)
		{
			return new Plant1stBehaviour(this, sim);
		}
		return new UnderwaterItem3rdBehaviour(this);
	}

	public float SpringConstant = 100f;

	public float BendConstant = 30f;

	public float FrictionConstant = 3f;

	public bool AddHitchMass;
}
