using System;

public class FishDestroy : FishStateBase
{
	protected override void onEnter()
	{
		if (base.Fish.Tackle.Fish == base.Fish)
		{
			base.Fish.Tackle.EscapeFish();
		}
		base.Fish.Destroy();
	}
}
