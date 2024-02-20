using System;
using Assets.Scripts.Common.Managers.Helpers;

[TriggerName(Name = "None")]
[Serializable]
public class None : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return false;
	}

	private static MenuHelpers _helpers = new MenuHelpers();
}
