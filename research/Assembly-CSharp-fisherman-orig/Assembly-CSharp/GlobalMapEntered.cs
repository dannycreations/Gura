using System;
using Assets.Scripts.Common.Managers.Helpers;

[TriggerName(Name = "Global map entered")]
[Serializable]
public class GlobalMapEntered : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return StaticUserData.CurrentPond == null && GlobalMapEntered._helpers.MenuPrefabsList != null && GlobalMapEntered._helpers.MenuPrefabsList.globalMapFormAS != null && GlobalMapEntered._helpers.MenuPrefabsList.globalMapFormAS.isActive;
	}

	private static MenuHelpers _helpers = new MenuHelpers();
}
