using System;
using Phy;
using UnityEngine;

public class UnderwaterItemObject : PhyObject
{
	public UnderwaterItemObject(PhyObjectType type, ConnectedBodiesSystem sim = null)
		: base(type, sim)
	{
		this.ForHookMass = null;
		this.TailMass = null;
		this.RodTipMass = null;
		this.RootDirection = Vector3.zero;
	}

	public UnderwaterItemObject(ConnectedBodiesSystem sim, UnderwaterItemObject source)
		: base(sim, source)
	{
		if (source.ForHookMass != null && source.RodTipMass != null)
		{
			this.ForHookMass = sim.DictMasses[source.ForHookMass.UID];
			this.TailMass = sim.DictMasses[source.TailMass.UID];
			this.RodTipMass = sim.DictMasses[source.RodTipMass.UID];
		}
		this.RootDirection = source.RootDirection;
	}

	public Mass ForHookMass;

	public Mass TailMass;

	public Mass RodTipMass;

	public Vector3 RootDirection;
}
