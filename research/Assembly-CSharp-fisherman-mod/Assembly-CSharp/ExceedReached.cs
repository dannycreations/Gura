using System;
using Assets.Scripts.Common.Managers.Helpers;

[TriggerName(Name = "Storage exceed reached")]
[Serializable]
public class ExceedReached : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.Inventory != null && (StaticUserData.CurrentPond == null && ExceedReached._helpers.MenuPrefabsList != null && ExceedReached._helpers.MenuPrefabsList.inventoryFormAS != null && ExceedReached._helpers.MenuPrefabsList.inventoryFormAS.isActive) && PhotonConnectionFactory.Instance.Profile.Inventory.IsStorageOverloaded;
	}

	private static MenuHelpers _helpers = new MenuHelpers();
}
