using System;
using Assets.Scripts.Common.Managers.Helpers;

[TriggerName(Name = "Passed tutorial")]
[Serializable]
public class PassedTutorial : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return StaticUserData.CurrentPond == null;
	}

	private static MenuHelpers _helpers = new MenuHelpers();
}
