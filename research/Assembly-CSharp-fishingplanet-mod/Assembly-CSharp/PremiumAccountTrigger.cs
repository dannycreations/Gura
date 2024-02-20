using System;

[TriggerName(Name = "Obtaining premium")]
[Serializable]
public class PremiumAccountTrigger : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.HasPremium;
	}
}
