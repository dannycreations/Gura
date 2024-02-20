using System;
using System.Collections.Generic;

[Serializable]
public abstract class TutorialTrigger
{
	public abstract void AcceptArguments(string[] arguments);

	public abstract bool IsTriggering();

	public virtual void Update()
	{
	}

	public static Dictionary<string, Func<TutorialTrigger>> Activators = new Dictionary<string, Func<TutorialTrigger>>
	{
		{
			typeof(PondEnterTrigger).ToString(),
			() => new PondEnterTrigger()
		},
		{
			typeof(LevelGainedTrigger).ToString(),
			() => new LevelGainedTrigger()
		},
		{
			typeof(PremiumAccountTrigger).ToString(),
			() => new PremiumAccountTrigger()
		},
		{
			typeof(FloatRodArmedTrigger).ToString(),
			() => new FloatRodArmedTrigger()
		},
		{
			typeof(FloatRodCastedTimesTrigger).ToString(),
			() => new FloatRodCastedTimesTrigger()
		},
		{
			typeof(CastingRodCastedTimesTrigger).ToString(),
			() => new CastingRodCastedTimesTrigger()
		},
		{
			typeof(HomeStorageOpenned).ToString(),
			() => new HomeStorageOpenned()
		},
		{
			typeof(GlobalShopEntered).ToString(),
			() => new GlobalShopEntered()
		},
		{
			typeof(InventoryEntered).ToString(),
			() => new InventoryEntered()
		},
		{
			typeof(CastingRodPulledTrigger).ToString(),
			() => new CastingRodPulledTrigger()
		},
		{
			typeof(LocalMapEntered).ToString(),
			() => new LocalMapEntered()
		},
		{
			typeof(PassedTutorial).ToString(),
			() => new PassedTutorial()
		},
		{
			typeof(NotOnPondTrigger).ToString(),
			() => new NotOnPondTrigger()
		},
		{
			typeof(FishCatched).ToString(),
			() => new FishCatched()
		},
		{
			typeof(GlobalMapEntered).ToString(),
			() => new GlobalMapEntered()
		},
		{
			typeof(SameCategoryCaught).ToString(),
			() => new SameCategoryCaught()
		},
		{
			typeof(LobbyEntered).ToString(),
			() => new LobbyEntered()
		},
		{
			typeof(FishcageChanged).ToString(),
			() => new FishcageChanged()
		},
		{
			typeof(DaysAdded).ToString(),
			() => new DaysAdded()
		},
		{
			typeof(FullEquipment).ToString(),
			() => new FullEquipment()
		},
		{
			typeof(FireworksEquipped).ToString(),
			() => new FireworksEquipped()
		},
		{
			typeof(None).ToString(),
			() => new None()
		},
		{
			typeof(NewPond).ToString(),
			() => new NewPond()
		},
		{
			typeof(RodBought).ToString(),
			() => new RodBought()
		},
		{
			typeof(ExceedReached).ToString(),
			() => new ExceedReached()
		},
		{
			typeof(RodSetupsOpened).ToString(),
			() => new RodSetupsOpened()
		}
	};
}
