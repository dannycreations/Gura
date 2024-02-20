using System;
using Assets.Scripts.Common.Managers.Helpers;

[TriggerName(Name = "Local map entered")]
[Serializable]
public class LocalMapEntered : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return StaticUserData.CurrentPond != null && LocalMapEntered._helpers.MenuPrefabsList != null && LocalMapEntered._helpers.MenuPrefabsList.globalMapFormAS != null && LocalMapEntered._helpers.MenuPrefabsList.globalMapFormAS.isActive;
	}

	private static MenuHelpers _helpers = new MenuHelpers();
}
