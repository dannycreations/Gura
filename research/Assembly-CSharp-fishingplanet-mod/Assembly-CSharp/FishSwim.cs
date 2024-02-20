using System;
using ObjectModel;
using Phy;

public class FishSwim : FishStateBase
{
	protected override void onEnter()
	{
		base.Fish.Swim();
	}

	protected override Type onUpdate()
	{
		if (base.Fish.Tackle != null && base.Fish.Tackle.IsInWater)
		{
			if (base.Fish.Tackle.IsLureTackle)
			{
				return typeof(FishAttack);
			}
			if (base.Fish.Tackle.TackleType == FishingRodSimulation.TackleType.Float)
			{
				return (!base.Fish.Tackle.IsLiveBait) ? typeof(FishBite) : typeof(FishPredatorSwim);
			}
			if (base.Fish.Tackle.TackleType == FishingRodSimulation.TackleType.Feeder || base.Fish.Tackle.TackleType == FishingRodSimulation.TackleType.CarpClassic || base.Fish.Tackle.TackleType == FishingRodSimulation.TackleType.CarpMethod || base.Fish.Tackle.TackleType == FishingRodSimulation.TackleType.CarpPVABag || base.Fish.Tackle.TackleType == FishingRodSimulation.TackleType.CarpPVAStick)
			{
				return (!base.Fish.Tackle.IsLiveBait) ? typeof(FishBite) : typeof(FishPredatorSwim);
			}
			throw new InvalidOperationException("Tackle is of unknown type - " + base.Fish.Tackle.GetType().Name);
		}
		else
		{
			if (base.Fish.Behavior == FishBehavior.Go)
			{
				return typeof(FishEscape);
			}
			return null;
		}
	}
}
