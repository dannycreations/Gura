using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine.UI;

[TriggerName(Name = "Rod Setups Opened")]
[Serializable]
public class RodSetupsOpened : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		bool flag = RodSetupsOpened._helpers.MenuPrefabsList != null && RodSetupsOpened._helpers.MenuPrefabsList.inventoryFormAS != null && RodSetupsOpened._helpers.MenuPrefabsList.inventoryFormAS.isActive && RodSetupsOpened._rodSetups != null;
		if (flag)
		{
			RodSetupsOpened._rodSetups.isOn = true;
		}
		return flag;
	}

	public override void Update()
	{
		base.Update();
		if (RodSetupsOpened._rodSetups == null && RodSetupsOpened._helpers.MenuPrefabsList != null && RodSetupsOpened._helpers.MenuPrefabsList.inventoryFormAS != null && RodSetupsOpened._helpers.MenuPrefabsList.inventoryFormAS.isActive && StaticUserData.CurrentPond == null)
		{
			RodSetupsOpened._rodSetups = RodSetupsOpened._helpers.MenuPrefabsList.inventoryForm.transform.Find("Main/TabsLayout/tglRodPresets").GetComponent<Toggle>();
		}
	}

	private static MenuHelpers _helpers = new MenuHelpers();

	private static Toggle _rodSetups = null;
}
