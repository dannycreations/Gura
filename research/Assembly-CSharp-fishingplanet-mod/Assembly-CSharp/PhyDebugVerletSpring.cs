using System;
using Phy;
using Phy.Verlet;
using UnityEngine;

public class PhyDebugVerletSpring : PhyDebugConnection
{
	public override void Init(ConnectionBase phyConnection)
	{
		this.phyVerletSpring = phyConnection as VerletSpring;
		base.Init(phyConnection);
		base.name = "VerletSpring";
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (this.phyVerletSpring != null)
		{
			this.length = this.phyVerletSpring.length;
			this.friction = this.phyVerletSpring.friction.X;
			this.compressible = this.phyVerletSpring.compressible;
		}
	}

	public override void OnUpdateReplay(int cUID, int UID1, int UID2, PhyReplay.ReplaySegment.SpringData springData)
	{
		base.OnUpdateReplay(cUID, UID1, UID2, springData);
		this.length = Mathf.Abs(springData.Length);
		this.compressible = springData.Length > 0f;
		this.friction = springData.Friction;
		this.DeltaLength = (this.Mass1.transform.position - this.Mass2.transform.position).magnitude - this.length;
	}

	private void Start()
	{
	}

	private void Update()
	{
		this.OnUpdate();
	}

	private VerletSpring phyVerletSpring;

	public float length;

	public float friction;

	public bool compressible;

	public float DeltaLength;
}
