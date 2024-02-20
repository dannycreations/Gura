using System;
using Nss.Udt.Referee;

public class SuperCube : RefereeBehaviour
{
	protected override void OnEnable()
	{
		RefereeManager<SuperCube>.Instance.Add(this);
	}

	protected override void OnDisable()
	{
		RefereeManager<SuperCube>.Instance.Remove(this);
	}
}
