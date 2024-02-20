using System;
using Phy;

public class PhyDebugBend : PhyDebugConnection
{
	public override void Init(ConnectionBase phyConnection)
	{
		this.phyBend = phyConnection as Bend;
		base.Init(phyConnection);
		base.name = "Bend_" + this.phyBend.UID;
	}

	private void Start()
	{
	}

	private void Update()
	{
		this.OnUpdate();
	}

	private Bend phyBend;
}
