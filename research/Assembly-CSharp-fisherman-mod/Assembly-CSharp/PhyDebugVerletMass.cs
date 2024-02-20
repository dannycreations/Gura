using System;
using Mono.Simd.Math;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class PhyDebugVerletMass : PhyDebugMass
{
	public override void Init(Mass phyMass, SimulationThread simThread, PhyDebugTool tool)
	{
		base.Init(phyMass, simThread, tool);
		base.name = "VerletMass_" + phyMass.UID;
	}

	private void Start()
	{
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		VerletMass verletMass = this.phyMass as VerletMass;
		this.VerletPositionDelta = verletMass.PositionDelta4f.AsVector3();
	}

	private void Update()
	{
		this.OnUpdate();
	}

	public override void DrawGizmos()
	{
		base.DrawGizmos();
	}

	public Vector3 VerletPositionDelta;
}
