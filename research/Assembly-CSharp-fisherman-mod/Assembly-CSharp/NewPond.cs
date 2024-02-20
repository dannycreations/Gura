using System;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;

[TriggerName(Name = "New pond unlocked")]
[Serializable]
public class NewPond : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		bool flag;
		if (CacheLibrary.MapCache != null && CacheLibrary.MapCache.CachedPonds != null)
		{
			flag = CacheLibrary.MapCache.CachedPonds.Where((Pond pond) => !pond.PondLocked()).Count<Pond>() > 2;
		}
		else
		{
			flag = false;
		}
		return flag;
	}

	private static MenuHelpers _helpers = new MenuHelpers();
}
