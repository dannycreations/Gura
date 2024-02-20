using System;

public class FishShowSmall : FishStateBase
{
	public override TPMFishState State
	{
		get
		{
			return TPMFishState.ShowSmall;
		}
	}

	protected override void onEnter()
	{
		GameFactory.FishSpawner.DestroyFishCam();
	}

	protected override Type onUpdate()
	{
		if (!base.Fish.Tackle.IsShowing)
		{
			return typeof(FishDestroy);
		}
		return null;
	}
}
