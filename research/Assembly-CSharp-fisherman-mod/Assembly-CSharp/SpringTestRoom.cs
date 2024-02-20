using System;
using System.Collections.Generic;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class SpringTestRoom : PhyTestRoom
{
	public override void InitSim()
	{
		List<Mass> list = new List<Mass>();
		List<Spring> list2 = new List<Spring>();
		this.rootMass = new VerletMass(this.sim, this.SegmentMass, this.rootPosition.position, Mass.MassType.Line)
		{
			IsKinematic = true
		};
		list.Add(this.rootMass);
		this.sim.Masses.Add(this.rootMass);
		Mass mass = this.rootMass;
		for (int i = 0; i < this.SegmentsCount; i++)
		{
			Mass mass2;
			if (i < this.SegmentsCount - 1)
			{
				mass2 = new Mass(this.sim, this.SegmentMass, mass.Position, Mass.MassType.Line)
				{
					AirDragConstant = 0f
				};
			}
			else
			{
				mass2 = new VerletMass(this.sim, this.LoadMass, mass.Position, Mass.MassType.Line)
				{
					AirDragConstant = 0f
				};
			}
			list.Add(mass2);
			this.sim.Masses.Add(mass2);
			Spring spring = new Spring(mass, mass2, 500f, this.SegmentLength, 0.002f);
			list2.Add(spring);
			this.sim.Connections.Add(spring);
			mass = mass2;
		}
		if (this.KinematicLoadTransform != null)
		{
			mass.IsKinematic = true;
		}
		base.InitSim();
	}

	public override void UpdateSim()
	{
		base.UpdateSim();
	}

	private void Start()
	{
		base.OnStart();
	}

	private void LateUpdate()
	{
		base.OnLateUpdate();
	}

	private void OnApplicationQuit()
	{
		base.OnQuit();
	}

	public int SegmentsCount;

	public float SegmentMass;

	public float SegmentLength;

	public float LoadMass;

	public Transform KinematicLoadTransform;

	protected VerletMass verletRoot;

	protected Vector3 voffset = Vector3.right;
}
