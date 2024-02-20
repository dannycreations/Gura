using System;

public class Bell3rdBehaviour : BellBehaviour
{
	public Bell3rdBehaviour(BellController controller, GameFactory.RodSlot rodSlot)
		: base(controller, rodSlot, null)
	{
	}

	public void Init(RodBehaviour rod)
	{
		base.Init();
	}
}
