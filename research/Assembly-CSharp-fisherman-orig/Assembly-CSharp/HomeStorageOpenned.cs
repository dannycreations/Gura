using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine.UI;

[TriggerName(Name = "Home storage openned")]
[Serializable]
public class HomeStorageOpenned : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return HomeStorageOpenned._helpers.MenuPrefabsList != null && HomeStorageOpenned._helpers.MenuPrefabsList.inventoryFormAS != null && HomeStorageOpenned._helpers.MenuPrefabsList.inventoryFormAS.isActive && HomeStorageOpenned._homeStorage != null && HomeStorageOpenned._homeStorage.isOn;
	}

	public override void Update()
	{
		base.Update();
		if (HomeStorageOpenned._homeStorage == null && HomeStorageOpenned._helpers.MenuPrefabsList != null && HomeStorageOpenned._helpers.MenuPrefabsList.inventoryFormAS != null && HomeStorageOpenned._helpers.MenuPrefabsList.inventoryFormAS.isActive)
		{
			HomeStorageOpenned._homeStorage = HomeStorageOpenned._helpers.MenuPrefabsList.inventoryForm.transform.Find("Main/TabsLayout/tglStorage").GetComponent<Toggle>();
		}
	}

	private static MenuHelpers _helpers = new MenuHelpers();

	private static Toggle _homeStorage = null;
}
