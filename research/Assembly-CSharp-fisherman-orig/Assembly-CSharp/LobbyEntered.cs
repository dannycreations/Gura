using System;
using Assets.Scripts.Common.Managers.Helpers;

[TriggerName(Name = "Lobby entered")]
[Serializable]
public class LobbyEntered : TutorialTrigger
{
	public override void AcceptArguments(string[] arguments)
	{
	}

	public override bool IsTriggering()
	{
		return PhotonConnectionFactory.Instance != null && StaticUserData.CurrentPond == null;
	}

	private static MenuHelpers _helpers = new MenuHelpers();
}
