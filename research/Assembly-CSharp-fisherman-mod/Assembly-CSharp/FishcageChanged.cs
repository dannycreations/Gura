using System;

[TriggerName(Name = "Fishcage changed")]
[Serializable]
public class FishcageChanged : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return this._fishcageId != -1 && PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.FishCage != null && PhotonConnectionFactory.Instance.Profile.FishCage.Cage.ItemId != this._fishcageId;
	}

	public override void Update()
	{
		base.Update();
		if (this._fishcageId == -1 && PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Profile != null && PhotonConnectionFactory.Instance.Profile.FishCage != null && PhotonConnectionFactory.Instance.Profile.FishCage.Cage != null)
		{
			this._fishcageId = PhotonConnectionFactory.Instance.Profile.FishCage.Cage.ItemId;
		}
	}

	private int _fishcageId = -1;
}
