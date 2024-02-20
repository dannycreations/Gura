using System;

public class HintElementIdActive : HintElementId
{
	protected override void OnDisable()
	{
		this.Remove();
	}
}
