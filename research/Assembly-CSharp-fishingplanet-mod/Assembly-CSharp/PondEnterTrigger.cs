using System;

[TriggerName(Name = "Pond entered")]
[Serializable]
public class PondEnterTrigger : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
		if (arguments == null || arguments.Length != 1)
		{
			throw new ArgumentException("arguments array for PondEnterTrigger has to contain 1 argument");
		}
		this.PondId = int.Parse(arguments[0]);
	}

	public override bool IsTriggering()
	{
		return StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId == this.PondId;
	}

	[TriggerVariable(Name = "Pond ID")]
	public int PondId;
}
