using System;

[TriggerName(Name = "Level gained")]
[Serializable]
public class LevelGainedTrigger : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
		if (arguments == null || arguments.Length != 1)
		{
			throw new ArgumentException("arguments array for LevelGainedTrigger has to contain 1 argument");
		}
		this.Level = int.Parse(arguments[0]);
	}

	public override bool IsTriggering()
	{
		return PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.Level >= this.Level;
	}

	[TriggerVariable(Name = "Level")]
	public int Level;
}
