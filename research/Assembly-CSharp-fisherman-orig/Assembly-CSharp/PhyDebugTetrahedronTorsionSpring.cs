using System;
using Phy;
using Phy.Verlet;

public class PhyDebugTetrahedronTorsionSpring : PhyDebugConnection
{
	public override void Init(ConnectionBase phyConnection)
	{
		this.phySpring = phyConnection as TetrahedronTorsionSpring;
		base.Init(phyConnection);
		base.name = "TetrahedronTorsionSpring_" + this.phySpring.UID;
	}

	private void Start()
	{
	}

	private void Update()
	{
		this.OnUpdate();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (this.phySpring != null)
		{
			this.Torsion = this.phySpring.Torsion;
		}
	}

	public float Torsion;

	private TetrahedronTorsionSpring phySpring;
}
