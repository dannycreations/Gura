using System;

public class DummyTackleBehaviour : TackleBehaviour
{
	public DummyTackleBehaviour(TackleControllerBase owner, IAssembledRod rodAssembly)
		: base(owner, rodAssembly, null)
	{
	}
}
