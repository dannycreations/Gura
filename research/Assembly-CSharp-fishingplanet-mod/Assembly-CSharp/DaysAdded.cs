using System;
using Assets.Scripts.Common.Managers.Helpers;

[TriggerName(Name = "Add days button pressed")]
[Serializable]
public class DaysAdded : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return ShowPondInfo.Instance != null && ShowPondInfo.Instance.TravelInit != null && ShowPondInfo.Instance.TravelInit.ResidenceCost.DaysValue > 1;
	}

	private static MenuHelpers _helpers = new MenuHelpers();
}
