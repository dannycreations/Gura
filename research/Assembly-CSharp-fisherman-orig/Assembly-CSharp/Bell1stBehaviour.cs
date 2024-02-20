using System;
using Phy;

public class Bell1stBehaviour : BellBehaviour
{
	public Bell1stBehaviour(BellController controller, IAssembledRod rodAssembly, GameFactory.RodSlot rodSlot, FishingRodSimulation sim = null)
		: base(controller, rodSlot, rodAssembly)
	{
		base.Sim = sim ?? rodSlot.Sim;
		if (!GameFactory.Player.IsTackleThrown)
		{
			base.gameObject.SetActive(false);
		}
	}

	public override void Init()
	{
		base.Init();
	}
}
