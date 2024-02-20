using System;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class ElasticBranchController : UnderwaterItemController0
{
	public override void CreateSim(ConnectedBodiesSystem sim = null)
	{
		Debug.Log("CreateSim " + (sim ?? GameFactory.Player.RodSlot.Sim));
		if (base.phyObject != null)
		{
			return;
		}
		if (GameFactory.Player.RodSlot.Sim != null || sim != null)
		{
			base.phyObject = new VerletBendTree(sim ?? GameFactory.Player.RodSlot.Sim, this.root, this.TotalMass, this.Fritcion, this.RotationalStiffness, this.DisplacementStiffness, this.NormalStiffness);
			this.phyNodes = (base.phyObject as VerletBendTree).NodesList.ToArray();
			this.initialRotations = new Quaternion[this.phyNodes.Length];
			for (int i = 0; i < this.initialRotations.Length; i++)
			{
				this.initialRotations[i] = this.phyNodes[i].rotation;
			}
		}
	}

	public new void LateUpdate()
	{
		this.SyncWithSim();
	}

	public float Fritcion;

	public float RotationalStiffness;

	public float DisplacementStiffness;

	public float NormalStiffness;
}
