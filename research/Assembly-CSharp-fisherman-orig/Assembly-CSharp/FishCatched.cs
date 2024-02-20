using System;

[TriggerName(Name = "Fish catched times")]
[Serializable]
public class FishCatched : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
		if (arguments == null || arguments.Length != 1)
		{
			throw new ArgumentException("arguments array for FishCatched has to contain 1 argument");
		}
		this.FishNumber = int.Parse(arguments[0]);
	}

	public override bool IsTriggering()
	{
		return PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.FishCage != null && PhotonConnectionFactory.Instance.Profile.FishCage.Fish != null && PhotonConnectionFactory.Instance.Profile.FishCage.Fish.Count == this.FishNumber;
	}

	[TriggerVariable(Name = "Number")]
	public int FishNumber;
}
