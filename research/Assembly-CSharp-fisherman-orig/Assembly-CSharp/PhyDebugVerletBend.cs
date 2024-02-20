using System;
using Phy;
using Phy.Verlet;

public class PhyDebugVerletBend : PhyDebugConnection
{
	public override void Init(ConnectionBase phyConnection)
	{
		this.phyVerletBend = phyConnection as VerletBend;
		base.Init(phyConnection);
		base.name = "VerletSpring";
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		this.Friction = this.phyVerletBend.FrictionConstant;
		this.RotationalStiffness = this.phyVerletBend.RotationalStiffness;
		this.DisplacementStiffness = this.phyVerletBend.DisplacementStiffness;
		this.NormalStiffness = this.phyVerletBend.NormalStiffness;
	}

	private void Start()
	{
	}

	private void Update()
	{
		this.OnUpdate();
	}

	private VerletBend phyVerletBend;

	public float Friction;

	public float RotationalStiffness;

	public float DisplacementStiffness;

	public float NormalStiffness;
}
