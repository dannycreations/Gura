using System;

public class BellOnPodBehaviour : BellBehaviour
{
	public BellOnPodBehaviour(BellController controller, GameFactory.RodSlot rodSlot, IAssembledRod rodAssembly = null)
		: base(controller, rodSlot, rodAssembly)
	{
		base.Sim = RodOnPodBehaviour.RodPodSim;
	}

	public override void Init()
	{
		base.Init();
	}
}
