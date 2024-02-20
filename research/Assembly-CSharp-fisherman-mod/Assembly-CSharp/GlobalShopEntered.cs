using System;
using Assets.Scripts.Common.Managers.Helpers;

[TriggerName(Name = "Global shop entered")]
[Serializable]
public class GlobalShopEntered : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return StaticUserData.CurrentPond == null && GlobalShopEntered._helpers.MenuPrefabsList != null && GlobalShopEntered._helpers.MenuPrefabsList.shopFormAS != null && GlobalShopEntered._helpers.MenuPrefabsList.shopFormAS.isActive;
	}

	private static MenuHelpers _helpers = new MenuHelpers();
}
